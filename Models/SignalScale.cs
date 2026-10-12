namespace Findy.Models;

public static class SignalScale
{
    const double FloorDbm = -95;
    const double RangeDb = 65;

    public static double Level(double rssi) =>
        Math.Clamp((rssi - FloorDbm) / RangeDb, 0, 1);

    public static string Quality(double level) => level switch
    {
        >= 0.75 => "Excellent signal",
        >= 0.5 => "Good signal",
        >= 0.25 => "Weak signal",
        _ => "Very weak signal"
    };
}