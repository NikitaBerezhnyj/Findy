
using MauiIcons.Material;

namespace Findy.Models;

public static class DeviceKind
{
    public const string Unknown = "Інше";

    public static MaterialIcons Icon(string category) => category switch
    {
        "Аудіо" => MaterialIcons.Headphones,
        "Телефон" => MaterialIcons.Smartphone,
        "Годинник / браслет" => MaterialIcons.Watch,
        "Комп'ютер" => MaterialIcons.Laptop,
        _ => MaterialIcons.HelpOutline
    };

    public static string Text(string category) =>
        category == Unknown ? "Тип невідомий" : category;
}