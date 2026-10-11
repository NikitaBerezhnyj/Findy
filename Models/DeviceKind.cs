
using MauiIcons.Material;

namespace Findy.Models;

public static class DeviceKind
{
    public const string Unknown = "Other";

    public static MaterialIcons Icon(string category) => category switch
    {
        "Audio" => MaterialIcons.Headphones,
        "Phone" => MaterialIcons.Smartphone,
        "Watch / Band" => MaterialIcons.Watch,
        "Computer" => MaterialIcons.Laptop,
        _ => MaterialIcons.HelpOutline
    };

    public static string Text(string category) =>
        category == Unknown ? "Unknown type" : category;
}