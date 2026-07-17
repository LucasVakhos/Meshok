using GH.Components;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MeshokBrowser.Networking;

/// <summary>
/// Authenticated HTTP session that reuses Meshok cookies from WebView2.
/// WebView2 is needed only to sign in; subsequent requests do not navigate or render pages.
/// </summary>
public sealed class MeshokHttpSession : IDisposable
{
    private readonly HttpClient _client;

    private MeshokHttpSession(HttpClient client)
    {
        _client = client;
    }

    public static async Task<MeshokHttpSession> CreateAsync(
        GhBrowser browser,
        Uri baseUri,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(browser);
        ArgumentNullException.ThrowIfNull(baseUri);

        await browser.EnsureCoreWebView2Async();
        cancellationToken.ThrowIfCancellationRequested();

        var core = browser.CoreWebView2
            ?? throw new InvalidOperationException("WebView2 is not initialized.");
        var webViewCookies = await core.CookieManager.GetCookiesAsync(baseUri.AbsoluteUri);
        cancellationToken.ThrowIfCancellationRequested();

        var cookieContainer = new CookieContainer();
        var cookieOrigin = new Uri(baseUri.GetLeftPart(UriPartial.Authority));
        foreach (var webViewCookie in webViewCookies)
        {
            var cookie = new Cookie(
                webViewCookie.Name,
                webViewCookie.Value,
                string.IsNullOrWhiteSpace(webViewCookie.Path) ? "/" : webViewCookie.Path)
            {
                HttpOnly = webViewCookie.IsHttpOnly,
                Secure = webViewCookie.IsSecure
            };
            cookieContainer.Add(cookieOrigin, cookie);
        }

        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.All,
            CookieContainer = cookieContainer,
            UseCookies = true
        };
        var client = new HttpClient(handler)
        {
            BaseAddress = cookieOrigin,
            Timeout = TimeSpan.FromSeconds(60)
        };
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("text/html", 0.9));
        client.DefaultRequestHeaders.UserAgent.ParseAdd("MeshokBrowser/1.0");

        return new MeshokHttpSession(client);
    }

    public Task<MeshokHttpResponse> GetAsync(
        string requestUri,
        CancellationToken cancellationToken = default) =>
        SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri), cancellationToken);

    public Task<MeshokHttpResponse> PostFormAsync(
        string requestUri,
        IEnumerable<KeyValuePair<string, string>> fields,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = new FormUrlEncodedContent(fields)
        };
        return SendAsync(request, cancellationToken);
    }

    public Task<MeshokHttpResponse> PostJsonAsync<T>(
        string requestUri,
        T payload,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(payload)
        };
        return SendAsync(request, cancellationToken);
    }

    public async Task<MeshokHttpResponse> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        using (request)
        using (var response = await _client.SendAsync(
                   request,
                   HttpCompletionOption.ResponseHeadersRead,
                   cancellationToken))
        {
            string body = await response.Content.ReadAsStringAsync(cancellationToken);
            return new MeshokHttpResponse(
                response.StatusCode,
                response.RequestMessage?.RequestUri,
                response.Content.Headers.ContentType?.MediaType,
                body);
        }
    }

    public void Dispose() => _client.Dispose();
}

public sealed record MeshokHttpResponse(
    HttpStatusCode StatusCode,
    Uri? FinalUri,
    string? MediaType,
    string Body)
{
    public bool IsSuccessStatusCode => (int)StatusCode is >= 200 and <= 299;

    public bool LooksLikeJson =>
        MediaType?.Contains("json", StringComparison.OrdinalIgnoreCase) == true ||
        Body.AsSpan().TrimStart().StartsWith("{") ||
        Body.AsSpan().TrimStart().StartsWith("[");
}
