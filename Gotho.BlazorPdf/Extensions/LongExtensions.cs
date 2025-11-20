namespace Gotho.BlazorPdf.Extensions;

public static class LongExtensions
{
    /// <summary>
    /// Converts the specified byte count to a human-readable file size string.
    /// </summary>
    /// <param name="bytes">The size in bytes to be converted.</param>
    /// <returns>A string representing the file size with the appropriate unit (B, KB, MB, GB, TB).</returns>
    public static string GetReadableFileSize(this long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        var order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.#} {sizes[order]}";
    }
}