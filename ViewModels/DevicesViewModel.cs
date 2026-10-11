using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Findy.Models;
using Findy.Services;

namespace Findy.ViewModels;

public partial class DevicesViewModel : ObservableObject
{
    private DateTime _lastScroll;

    static readonly TimeSpan Ttl = TimeSpan.FromSeconds(6);

    readonly BluetoothService _bt;
    readonly Dictionary<string, DevicesItem> _items = [];
    List<BtDevice> _paired = [];
    IDisposable? _subscription;
    IDispatcherTimer? _timer;
    DateTime _scanStartedAt;

    public DevicesViewModel(BluetoothService bt) => _bt = bt;

    public ObservableCollection<DevicesItem> Items { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMessage))]
    string? message;

    [ObservableProperty] bool isSearching;

    public bool HasMessage => !string.IsNullOrEmpty(Message);

    public void NotifyScrolled() => _lastScroll = DateTime.UtcNow;

    [RelayCommand]
    public async Task StartAsync()
    {
        if (_subscription is not null) return;

        if (!_bt.IsSupported) { Message = "Bluetooth не підтримується на цьому пристрої."; return; }
        if (!await _bt.EnsurePermissionsAsync()) { Message = "Потрібен дозвіл на Bluetooth. Дай його в налаштуваннях додатка."; return; }
        if (!_bt.IsEnabled) { Message = "Bluetooth вимкнено. Увімкни його і натисни «Спробувати знову»."; return; }

        if (_subscription is not null) return;

        _scanStartedAt = DateTime.UtcNow;
        _paired = _bt.GetPairedDevices();
        _subscription = _bt.Subscribe(OnSample);
        if (_subscription is null) { Message = "Не вдалося запустити сканування."; return; }

        Message = null;
        IsSearching = true;

        _timer ??= Application.Current!.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick -= OnTick;
        _timer.Tick += OnTick;
        _timer.Start();
    }

    public void Stop()
    {
        _subscription?.Dispose();
        _subscription = null;
        _timer?.Stop();

        IsSearching = false;
    }

    void OnSample(ScanSample s)
    {
        if (!_items.TryGetValue(s.Address, out var item))
            _items[s.Address] = item = new DevicesItem(s);

        if (s.Name is not null && item.BleName != s.Name)
        {
            var paired = _paired.FirstOrDefault(p => DeviceMatcher.Similar(p.Name, s.Name));
            item.SetIdentity(s.Name, paired);
        }

        item.Update(s.Rssi);
    }

    void OnTick(object? sender, EventArgs e) => Refresh();

    void Refresh()
    {
        var now = DateTime.UtcNow;
        var canExpire = now - _scanStartedAt > Ttl;

        if (canExpire)
        {
            foreach (var stale in _items.Values.Where(i => now - i.LastSeen > Ttl).ToList())
                _items.Remove(stale.Address);
        }

        foreach (var item in _items.Values) item.Publish();

        if (now - _lastScroll < TimeSpan.FromMilliseconds(1500)) return;

        var visible = _items.Values.OrderByDescending(i => i.Rssi).ToList();

        var keep = visible.ToHashSet();
        foreach (var gone in Items.Where(i => !keep.Contains(i)).ToList())
            Items.Remove(gone);

        for (var i = 0; i < visible.Count; i++)
        {
            var from = Items.IndexOf(visible[i]);
            if (from < 0) Items.Insert(i, visible[i]);
            else if (from != i) Items.Move(from, i);
        }

        IsSearching = _subscription is not null && Items.Count == 0;
    }
}