using CommunityToolkit.Mvvm.ComponentModel;
using MauiIcons.Material;
using Findy.Models;
using Findy.Services;
using CommunityToolkit.Mvvm.Input;

namespace Findy.ViewModels;

public partial class RadarViewModel : ObservableObject, IQueryAttributable
{
    const double Alpha = 0.2;
    const int RssiAt1m = -60;
    const double PathLoss = 2.5;
    const int TrendWindowMs = 2000;
    const int TrendThresholdDb = 3;
    const string VibrationKey = "vibration";
    const int BuzzMs = 90;

    static readonly TimeSpan LostAfter = TimeSpan.FromSeconds(5);

    readonly BluetoothService _bt;
    readonly Queue<(DateTime Time, double Rssi)> _history = new();

    private TrackTarget? _target;
    private IDisposable? _subscription;
    private IDispatcherTimer? _timer;
    private double? _smoothed;
    private DateTime _lastSeen;
    private bool _dirty;
    private DateTime _nextBuzz;

    public RadarViewModel(BluetoothService bt) => _bt = bt;
    public bool HasSubtitle => !string.IsNullOrEmpty(Subtitle);
    public string VibrationText => VibrationEnabled ? "Vibration on" : "Vibration off";

    [ObservableProperty]
    string title = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSubtitle))]
    string? subtitle;

    [ObservableProperty]
    MaterialIcons kindIcon = MaterialIcons.HelpOutline;

    [ObservableProperty]
    string status = "Searching…";

    [ObservableProperty]
    string distanceText = "—";

    [ObservableProperty]
    string trendText = "";

    [ObservableProperty]
    string rssiText = "";

    [ObservableProperty]
    double signalLevel;

    [ObservableProperty]
    bool isLost;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VibrationText))]
    bool vibrationEnabled = Preferences.Default.Get(VibrationKey, true);

    [RelayCommand]
    void ToggleVibration() => VibrationEnabled = !VibrationEnabled;

    partial void OnVibrationEnabledChanged(bool value) =>
        Preferences.Default.Set(VibrationKey, value);

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
        if (_subscription is null) { Status = "Could not start scanning."; return; }

        _bt.ScanFailed -= OnScanFailed;
        _bt.ScanFailed += OnScanFailed;
        _timer ??= Application.Current!.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(250);
        _timer.Tick -= OnTick;
        _timer.Tick += OnTick;
        _timer.Start();
    }

    public void Stop()
    {
        _bt.ScanFailed -= OnScanFailed;
        _subscription?.Dispose();
        _subscription = null;
        _timer?.Stop();
    }

    void OnScanFailed()
    {
        Stop();
        ResetReading();
        Status = "Scan failed";
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

        if (IsLost)
        {
            _smoothed = null;
            _history.Clear();
            IsLost = false;
        }

        _lastSeen = now;
        _smoothed = _smoothed is null ? s.Rssi : Alpha * s.Rssi + (1 - Alpha) * _smoothed;

        _history.Enqueue((now, _smoothed.Value));
        while ((now - _history.Peek().Time).TotalMilliseconds > TrendWindowMs)
            _history.Dequeue();

        _dirty = true;
    }

    void Publish()
    {
        var smoothed = _smoothed!.Value;
        var delta = smoothed - _history.Peek().Rssi;

        Status = "Device found";
        DistanceText = FormatDistance(smoothed);
        TrendText = delta > TrendThresholdDb ? "▲ Closer"
                  : delta < -TrendThresholdDb ? "▼ Farther"
                  : "● No change";
        SignalLevel = SignalScale.Level(smoothed);
        RssiText = $"{smoothed:0} dBm";
    }

    void Buzz()
    {
        if (!VibrationEnabled || _smoothed is null || !Vibration.Default.IsSupported) return;

        var now = DateTime.UtcNow;
        if (now < _nextBuzz) return;

        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(BuzzMs));
        _nextBuzz = now + TimeSpan.FromMilliseconds(1000 - 750 * SignalLevel);
    }

    void OnTick(object? sender, EventArgs e)
    {
        if (_smoothed is not null)
        {
            var silence = DateTime.UtcNow - _lastSeen;
            if (silence > LostAfter)
            {
                ShowLost(silence);
                return;
            }
        }

        if (_dirty)
        {
            _dirty = false;
            Publish();
        }

        Buzz();
    }

    void ShowLost(TimeSpan silence)
    {
        IsLost = true;
        Status = "Signal lost";
        TrendText = $"Seen {(int)silence.TotalSeconds} s ago";
        SignalLevel = 0;
    }

    void ResetReading()
    {
        _smoothed = null;
        _dirty = false;
        _history.Clear();
        IsLost = false;
        Status = "Searching…";
        DistanceText = "—";
        TrendText = "";
        RssiText = "";
        SignalLevel = 0;
    }

    static string FormatDistance(double rssi)
    {
        var d = Math.Pow(10, (RssiAt1m - rssi) / (10 * PathLoss));
        return d switch
        {
            < 0.3 => "< 0.3 m",
            < 3 => $"≈ {d:0.0} m",
            < 10 => $"≈ {d:0} m",
            _ => "> 10 m"
        };
    }
}