using System.Text.RegularExpressions;

namespace CryptoChat2.Utils;

public static class InputSanitizer
{
    // Remove HTML/script tags
    public static string Sanitize(string input)
    {
        return Regex.Replace(input, "<.*?>", string.Empty); // Strip tags
    }
}