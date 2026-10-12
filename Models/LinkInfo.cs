namespace Findy.Models;

public static class LinkInfo
{
    public static string Help(string btType, bool connectable)
    {
        var kind = btType switch
        {
            "Dual" => "Dual: works as both Classic and BLE. Typical for earbuds and phones.",
            "LE" => "LE: works only over Bluetooth Low Energy. Typical for watches, bands and beacons.",
            _ => "Connection type is unknown."
        };

        var chance = connectable
            ? "The device accepts connections, so it looks like earbuds, a watch or a phone. Its signal is visible, so tracking it should work: good chances."
            : "The device only broadcasts a signal and does not accept connections (beacon, tracker, sensor). You can track it, but it is less likely to be earbuds or a watch.";

        return $"{kind}\n\n{chance}\n\nThis is an estimate based on the signal type, not a guarantee.";
    }
}