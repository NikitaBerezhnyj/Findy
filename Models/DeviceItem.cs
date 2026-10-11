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
        >= 0.75 => "Відмінний сигнал",
        >= 0.5 => "Добрий сигнал",
        >= 0.25 => "Слабкий сигнал",
        _ => "Дуже слабкий сигнал"
    };

    public string LinkHelp
    {
        get
        {
            var kind = BtType switch
            {
                "Dual" => "Dual: працює і як Classic, і як BLE. Типово для навушників і телефонів.",
                "LE" => "LE: працює лише через Bluetooth Low Energy. Типово для годинників, браслетів і маячків.",
                _ => "Тип зв'язку невідомий."
            };

            var chance = IsConnectable
                ? "Пристрій приймає підключення, тож схожий на навушники, годинник чи телефон. Сигнал видно, отже відстежувати його можна: шанси хороші."
                : "Пристрій лише розсилає сигнал і не приймає підключення (маячок, трекер, датчик). Відстежувати його можна, але на навушники чи годинник він схожий менше.";

            return $"{kind}\n\n{chance}\n\nЦе оцінка за типом сигналу, а не гарантія.";
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
            Subtitle = $"Видно як {bleName}";
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
            Title = "Невідомий пристрій";
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