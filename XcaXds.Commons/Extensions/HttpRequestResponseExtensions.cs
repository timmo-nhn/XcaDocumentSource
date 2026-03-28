public static class HttpRequestResponseExtensions
{
    public static async Task<string> GetStreamAsStringAsync(Stream stream)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);
        var bodyContent = await reader.ReadToEndAsync();
        stream.Position = 0; // Reset stream position for next reader
        return bodyContent;
    }
}
