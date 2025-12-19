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
    

    public static bool IsNullOrEmpty(this string? _) => string.IsNullOrEmpty(_);

    public static bool IsProbablyUrl(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               input.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }
}