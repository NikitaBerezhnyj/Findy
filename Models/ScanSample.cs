namespace Findy.Models;

public record ScanSample(
    string Address,
    string? Name,
    int Rssi,
    string Category,
    string BluetoothType,
    bool Connectable
);