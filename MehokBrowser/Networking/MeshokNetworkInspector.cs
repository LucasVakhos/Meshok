using GH.Components;
using Microsoft.Web.WebView2.Core;

namespace MeshokBrowser.Networking;

/// <summary>
/// Observes request metadata during one browser-driven run so that real data
/// endpoints can be identified before replacing navigation with direct HTTP.
/// Cookies, request headers and request/response bodies are never captured.
/// </summary>
public sealed class MeshokNetworkInspector : IDisposable
{
    private readonly CoreWebView2 _core;
    private readonly Action<MeshokNetworkCall> _onCall;
    private bool _disposed;

    private MeshokNetworkInspector(
        CoreWebView2 core,
        Action<MeshokNetworkCall> onCall)
    {
        _core = core;
        _onCall = onCall;
        _core.WebResourceResponseReceived += OnWebResourceResponseReceived;
    }

    public static async Task<MeshokNetworkInspector> AttachAsync(
        GhBrowser browser,
        Action<MeshokNetworkCall> onCall)
    {
        ArgumentNullException.ThrowIfNull(browser);
        ArgumentNullException.ThrowIfNull(onCall);

        await browser.EnsureCoreWebView2Async();
        var core = browser.CoreWebView2
            ?? throw new InvalidOperationException("WebView2 is not initialized.");
        return new MeshokNetworkInspector(core, onCall);
    }

    private void OnWebResourceResponseReceived(
        object? sender,
        CoreWebView2WebResourceResponseReceivedEventArgs e)
    {
        string? contentType = null;
        if (e.Response.Headers.Contains("Content-Type"))
            contentType = e.Response.Headers.GetHeader("Content-Type");

        Uri? uri = Uri.TryCreate(e.Request.Uri, UriKind.Absolute, out var parsedUri)
            ? parsedUri
            : null;
        bool likelyDataEndpoint =
            contentType?.Contains("json", StringComparison.OrdinalIgnoreCase) == true ||
            uri?.AbsolutePath.Contains("api", StringComparison.OrdinalIgnoreCase) == true ||
            uri?.AbsolutePath.Contains("ajax", StringComparison.OrdinalIgnoreCase) == true ||
            uri?.Query.Contains("json", StringComparison.OrdinalIgnoreCase) == true;

        _onCall(new MeshokNetworkCall(
            e.Request.Method,
            uri,
            e.Response.StatusCode,
            contentType,
            likelyDataEndpoint));
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _core.WebResourceResponseReceived -= OnWebResourceResponseReceived;
        _disposed = true;
    }
}

public sealed record MeshokNetworkCall(
    string Method,
    Uri? Uri,
    int StatusCode,
    string? ContentType,
    bool IsLikelyDataEndpoint);
