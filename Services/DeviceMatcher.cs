using System.Text.RegularExpressions;

namespace Findy.Services;

public static partial class DeviceMatcher
{
    public static bool Similar(string a, string b)
    {
        var na = Normalize(a);
        var nb = Normalize(b);
        if (na.Length > 0 && nb.Length > 0 && (na.Contains(nb) || nb.Contains(na)))
            return true;

        var digitsA = Digits().Matches(a).Select(m => m.Value).ToHashSet();
        return Digits().Matches(b).Any(m => digitsA.Contains(m.Value));
    }

    static string Normalize(string s) =>
        new string(s.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

    [GeneratedRegex(@"\d{3,}")]
    private static partial Regex Digits();
}