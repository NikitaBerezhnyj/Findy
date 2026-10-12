using CommunityToolkit.Mvvm.ComponentModel;
using MauiIcons.Material;

namespace Findy.Models;

public partial class DevicesItem : ObservableObject
{
    bool _hasValue;

    public DevicesItem(ScanSample sample)
    {
        Address = sample.Address;
        BtType = sample.BluetoothType;
        IsConnectable = sample.Connectable;
        Category = sample.Category;
        SetIdentity(null, null);
    }

    public string Address { get; }
    public string BtType { get; }
    public bool IsConnectable { get; }
    public string? BleName { get; private set; }
    public DateTime LastSeen { get; private set; }

    public double Rssi { get; private set; }
    public double SignalLevel { get; private set; }
    public string RssiText { get; private set; } = "";

    public bool HasSubtitle => Subtitle is not null;
    public MaterialIcons KindIcon => DeviceKind.Icon(Category);
    public string KindText => DeviceKind.Text(Category);
    public MaterialIcons LinkIcon => IsConnectable
        ? MaterialIcons.Link
        : MaterialIcons.WifiTethering;

    public string LinkChip => BtType;

    public string QualityText => SignalScale.Quality(SignalLevel);

    public string LinkHelp => LinkInfo.Help(BtType, IsConnectable);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(KindIcon), nameof(KindText))]
    string category = "";

    [ObservableProperty] string title = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSubtitle))]
    string? subtitle;

    [ObservableProperty] bool isPaired;

    public void SetIdentity(string? bleName, BtDevice? paired)
    {
        BleName = bleName;
        IsPaired = paired is not null;

        if (paired is not null)
        {
            Title = paired.Name;
            Subtitle = $"Seen as {bleName}";
            if (Category == DeviceKind.Unknown) Category = paired.Category;
        }
        else if (bleName is not null)
        {
            Title = bleName;
            Subtitle = null;
        }
        else
        {
            var tail = Address.Length >= 5 ? Address[^5..] : Address;
            Title = "Unknown device";
            Subtitle = $"ID · {tail}";
        }
    }

    public void Update(int newRssi)
    {
        Rssi = _hasValue ? 0.3 * newRssi + 0.7 * Rssi : newRssi;
        _hasValue = true;
        LastSeen = DateTime.UtcNow;
    }

    public void Publish()
    {
        var level = SignalScale.Level(Rssi);
        if (Math.Abs(level - SignalLevel) > 0.005)
        {
            SignalLevel = level;
            OnPropertyChanged(nameof(SignalLevel));
            OnPropertyChanged(nameof(QualityText));
        }

        var text = $"{Rssi:0} dBm";
        if (text != RssiText)
        {
            RssiText = text;
            OnPropertyChanged(nameof(RssiText));
        }
    }

    public TrackTarget ToTarget() =>
        new(Address, BleName, Title, Subtitle, KindIcon);
}