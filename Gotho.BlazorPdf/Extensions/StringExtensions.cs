namespace MudBlazorPdf.Extensions;

internal static class StringExtensions
{
    private static readonly Random _random = new();
    
    public static string GenerateRandomString(this string _, int length = 12)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
    }
}