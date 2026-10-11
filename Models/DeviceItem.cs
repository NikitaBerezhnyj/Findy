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

    public string QualityText => SignalLevel switch
    {
        >= 0.75 => "Excellent signal",
        >= 0.5 => "Good signal",
        >= 0.25 => "Weak signal",
        _ => "Very weak signal"
    };

    public string LinkHelp
    {
        get
        {
            var kind = BtType switch
            {
                "Dual" => "Dual: works as both Classic and BLE. Typical for earbuds and phones.",
                "LE" => "LE: works only over Bluetooth Low Energy. Typical for watches, bands and beacons.",
                _ => "Connection type is unknown."
            };

            var chance = IsConnectable
                ? "The device accepts connections, so it looks like earbuds, a watch or a phone. Its signal is visible, so tracking it should work: good chances."
                : "The device only broadcasts a signal and does not accept connections (beacon, tracker, sensor). You can track it, but it is less likely to be earbuds or a watch.";

            return $"{kind}\n\n{chance}\n\nThis is an estimate based on the signal type, not a guarantee.";
        }
    }

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
        var level = Math.Clamp((Rssi + 95) / 65.0, 0, 1);
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