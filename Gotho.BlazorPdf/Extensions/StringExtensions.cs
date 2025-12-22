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
    public static bool IsNotNullOrEmpty(this string? _) => !string.IsNullOrEmpty(_);

    public static bool IsProbablyBase64(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) 
            return false;

        // A PDF in Base64 will be much longer than a standard URL, base64 strings must be divisible by 4.
        if (input.Length < 150 || input.Length % 4 != 0) 
            return false;

        // These represent the "%PDF-" binary signature which all PDFs should start with.
        if (input.StartsWith("JVBERi0") || input.StartsWith("VBERi0") || input.StartsWith("lBERi0"))
        {
            return true;
        }

        // Check the first 512 chars is usually enough to confirm valid encoding.
        int checkLen = Math.Min(input.Length, 512);
        for (var i = 0; i < checkLen; i++)
        {
            var c = input[i];
            if (!(char.IsLetterOrDigit(c) || c == '+' || c == '/' || c == '='))
                return false;
        }

        // If it can be parsed as a URL, it's not Base64.'
        return !Uri.TryCreate(input, UriKind.RelativeOrAbsolute, out _);
    }
}