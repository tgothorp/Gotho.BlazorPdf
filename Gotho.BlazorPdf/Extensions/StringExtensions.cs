namespace Gotho.BlazorPdf.Extensions;

internal static class StringExtensions
{
    private static readonly Random _random = new();

    public static string GenerateRandomString(this string _, int length = 12)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
    }

    public static bool IsProbablyUrl(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               input.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsProbablyBase64(this string? input)
    {
        var len = input?.Length ?? 0;

        // Base64 strings should be greater than 16 chars and have a length divisible by 4
        if (len < 16 || len % 4 != 0)
            return false;

        // Check upto the first 64 chars for invalid base64 chars
        for (var i = 0; i < Math.Min(64, len); i++)
        {
            var c = input![i];
            if (!(char.IsLetterOrDigit(c) || c == '+' || c == '/' || c == '='))
                return false;
        }

        return true;
    }
}