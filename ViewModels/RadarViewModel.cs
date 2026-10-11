using CommunityToolkit.Mvvm.ComponentModel;
using MauiIcons.Material;
using Findy.Models;
using Findy.Services;

namespace Findy.ViewModels;

public partial class RadarViewModel : ObservableObject, IQueryAttributable
{
    const double Alpha = 0.2;
    const int RssiAt1m = -60;
    const double PathLoss = 2.5;
    const int TrendWindowMs = 2000;
    const int TrendThresholdDb = 3;
    static readonly TimeSpan LostAfter = TimeSpan.FromSeconds(3);

    readonly BluetoothService _bt;
    readonly Queue<(DateTime Time, double Rssi)> _history = new();
    TrackTarget? _target;
    IDisposable? _subscription;
    IDispatcherTimer? _timer;
    double? _smoothed;
    DateTime _lastSeen;
    bool _dirty;
    int _rawRssi;

    public RadarViewModel(BluetoothService bt) => _bt = bt;

    public bool HasSubtitle => !string.IsNullOrEmpty(Subtitle);

    [ObservableProperty] string title = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSubtitle))]
    string? subtitle;

    [ObservableProperty] MaterialIcons kindIcon = MaterialIcons.HelpOutline;
    [ObservableProperty] string status = "Шукаю…";
    [ObservableProperty] string distanceText = "—";
    [ObservableProperty] string trendText = "";
    [ObservableProperty] string rssiText = "";
    [ObservableProperty] string debugInfo = "";
    [ObservableProperty] double signalLevel;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("target", out var value) || value is not TrackTarget target) return;

        _target = target;
        Title = target.Title;
        Subtitle = target.Subtitle;
        KindIcon = target.KindIcon;
    }

    public void Start()
    {
        if (_subscription is not null || _target is null) return;

        ResetReading();
        _subscription = _bt.Subscribe(OnSample);
        if (_subscription is null) { Status = "Не вдалося запустити сканування."; return; }

        _timer ??= Application.Current!.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(250);
        _timer.Tick -= OnTick;
        _timer.Tick += OnTick;
        _timer.Start();
    }

    public void Stop()
    {
        _subscription?.Dispose();
        _subscription = null;
        _timer?.Stop();
    }

    void OnSample(ScanSample s)
    {
        if (IsTarget(s)) Track(s);
    }

    bool IsTarget(ScanSample s) =>
        _target is not null &&
        (s.Address == _target.Address || (s.Name is not null && s.Name == _target.BleName));

    void Track(ScanSample s)
    {
        var now = DateTime.UtcNow;
        _lastSeen = now;
        _smoothed = _smoothed is null ? s.Rssi : Alpha * s.Rssi + (1 - Alpha) * _smoothed;
        _rawRssi = s.Rssi;

        _history.Enqueue((now, _smoothed.Value));
        while ((now - _history.Peek().Time).TotalMilliseconds > TrendWindowMs)
            _history.Dequeue();

        _dirty = true;
    }

    void Publish()
    {
        var smoothed = _smoothed!.Value;
        var delta = smoothed - _history.Peek().Rssi;

        Status = "Пристрій знайдено";
        DistanceText = FormatDistance(smoothed);
        TrendText = delta > TrendThresholdDb ? "▲ Ближче"
                  : delta < -TrendThresholdDb ? "▼ Далі"
                  : "● Без змін";
        SignalLevel = Math.Clamp((smoothed + 95) / 65.0, 0, 1);
        RssiText = $"{smoothed:0} dBm";
        DebugInfo = $"Сирий RSSI: {_rawRssi} dBm";
    }

    void OnTick(object? sender, EventArgs e)
    {
        if (_smoothed is not null && DateTime.UtcNow - _lastSeen > LostAfter)
        {
            ResetReading();
            Status = "Сигнал втрачено";
            return;
        }

        if (!_dirty) return;
        _dirty = false;
        Publish();
    }

    void ResetReading()
    {
        _smoothed = null;
        _dirty = false;
        _history.Clear();
        Status = "Шукаю…";
        DistanceText = "—";
        TrendText = "";
        RssiText = "";
        DebugInfo = "";
        SignalLevel = 0;
    }

    static string FormatDistance(double rssi)
    {
        var d = Math.Pow(10, (RssiAt1m - rssi) / (10 * PathLoss));
        return d switch
        {
            < 0.3 => "< 0,3 м",
            < 3 => $"≈ {d:0.0} м",
            < 10 => $"≈ {d:0} м",
            _ => "> 10 м"
        };
    }
}