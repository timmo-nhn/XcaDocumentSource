using Microsoft.JSInterop;

namespace XcaXds.Frontend.Cookies;

public interface ICookie
{
    Task SetValue(string key, string value, int? days = null);
    Task<string> GetValue(string key, string def = "");
}

public class Cookie : ICookie
{
    private readonly IJSRuntime _jsRuntime;
    private string _expires = "";

    public Cookie(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        ExpireDays = 300;
    }

    public int ExpireDays
    {
        set => _expires = DateToUTC(value);
    }

    public async Task SetValue(string key, string value, int? days = null)
    {
        var encodedValue = Uri.EscapeDataString(value);
        var curExp = (days != null && days > 0) ? DateToUTC(days.Value) : _expires;

        var cookieString = $"{key}={encodedValue}; expires={curExp}; path=/";

        await _jsRuntime.InvokeVoidAsync("cookieFunctions.setCookie", cookieString);
    }

    public async Task<string> GetValue(string key, string def = "")
    {
        var allCookies = await _jsRuntime.InvokeAsync<string>("cookieFunctions.getCookies");

        if (string.IsNullOrWhiteSpace(allCookies)) return def;

        var cookies = allCookies.Split(';');

        foreach (var cookie in cookies)
        {
            var parts = cookie.Split('=', 2);
            if (parts.Length == 2 && parts[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return Uri.UnescapeDataString(parts[1].Trim());
            }
        }

        return def;
    }

    private static string DateToUTC(int days)
    {
        return DateTime.UtcNow.AddDays(days).ToString("R"); // RFC1123 format
    }
}
