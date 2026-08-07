using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace LB.Libs;

public class GhBrowser : WebView2, ISupportInitialize
{
    private const string LoadingHtml = """
        <html>
          <head><title>Wait for loading ...</title></head>
          <body><div style="text-align:center;font: bold 18px Arial">Wait for loading ...</div></body>
        </html>
        """;

    private const string DomSubmitMessage = "gh-dom-submit";
    private const string DomSubmitScript = """
        document.addEventListener('submit', () =>
            chrome.webview.postMessage('gh-dom-submit'), true);
        """;

    private bool _noDefaultContextMenu;
    private Task<string> _domSubmitBridgeReady;
    private bool _isDisposed;

    public event EventHandler DocumentCompleted;
    public event EventHandler CreateWindow;
    public event EventHandler DomSubmit;

    protected virtual void OnDomSubmit(EventArgs e) => DomSubmit?.Invoke(this, e);

    public bool IsBusy { get; private set; }

    public Uri Url => Source;

    public GhDocument Document => new GhDocument(this);

    public bool NoDefaultContextMenu
    {
        get => _noDefaultContextMenu;
        set
        {
            _noDefaultContextMenu = value;
            ApplySettings();
        }
    }

    public GhBrowser()
    {
        NavigationStarting += (_, _) =>
        {
            if (!_isDisposed)
                IsBusy = true;
        };
        NavigationCompleted += OnNavigationCompleted;
        CoreWebView2InitializationCompleted += OnCoreWebView2InitializationCompleted;
    }

    public virtual void BeginInit()
    {
    }

    public virtual async void EndInit()
    {
        if (DesignMode)
            return;

        try
        {
            await EnsureDomSubmitBridgeAsync();
            if (!_isDisposed)
                NavigateToString(LoadingHtml);
        }
        catch (ObjectDisposedException)
        {
            // Control was disposed during initialization
        }
        catch (InvalidOperationException)
        {
            // CoreWebView2 was disposed during initialization
        }
    }

    private void OnCoreWebView2InitializationCompleted(
        object sender,
        CoreWebView2InitializationCompletedEventArgs e)
    {
        // Check for disposed state first
        if (_isDisposed)
            return;

        // Check if initialization failed
        if (!e.IsSuccess)
        {
            // Log the error if needed, but don't crash
            if (e.InitializationException != null)
            {
                System.Diagnostics.Trace.TraceError(
                    $"WebView2 initialization failed: {e.InitializationException.Message}");
            }
            return;
        }

        // Check if CoreWebView2 is available and not disposed
        if (CoreWebView2 == null || _isDisposed)
            return;

        try
        {
            ApplySettings();
            CoreWebView2.WebMessageReceived += OnWebMessageReceived;
            _domSubmitBridgeReady = CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(DomSubmitScript);
            CoreWebView2.NewWindowRequested += (_, args) =>
            {
                if (_isDisposed)
                    return;

                args.Handled = true;
                CreateWindow?.Invoke(this, EventArgs.Empty);
                Navigate(args.Uri);
            };
        }
        catch (ObjectDisposedException)
        {
            // Control was disposed during event handler execution
        }
        catch (InvalidOperationException)
        {
            // CoreWebView2 was disposed during event handler execution
        }
    }

    private void OnWebMessageReceived(
        object sender,
        CoreWebView2WebMessageReceivedEventArgs e)
    {
        if (_isDisposed)
            return;

        string message;
        try { message = e.TryGetWebMessageAsString(); }
        catch (ArgumentException) { return; }

        if (message == DomSubmitMessage)
            OnDomSubmit(EventArgs.Empty);
    }

    private async Task EnsureDomSubmitBridgeAsync()
    {
        if (_isDisposed)
            return;

        await EnsureCoreWebView2Async();

        if (_isDisposed)
            return;

        if (_domSubmitBridgeReady != null)
            await _domSubmitBridgeReady;
    }

    private void ApplySettings()
    {
        if (CoreWebView2 != null && !_isDisposed)
            CoreWebView2.Settings.AreDefaultContextMenusEnabled = !_noDefaultContextMenu;
    }

    private void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (_isDisposed)
            return;

        IsBusy = false;
        DocumentCompleted?.Invoke(this, EventArgs.Empty);
    }

    public async void Navigate(string url)
    {
        if (string.IsNullOrWhiteSpace(url) || _isDisposed)
            return;

        try
        {
            await EnsureDomSubmitBridgeAsync();
            if (!_isDisposed && CoreWebView2 != null)
                CoreWebView2.Navigate(url);
        }
        catch (ObjectDisposedException)
        {
            // Control was disposed during navigation
        }
        catch (InvalidOperationException)
        {
            // CoreWebView2 was disposed during navigation
        }
    }

    public void ShowMessage(string message)
    {
        if (_isDisposed)
            return;

        try
        {
            string safe = System.Net.WebUtility.HtmlEncode(message ?? string.Empty);
            NavigateToString($"<html><body><div style=\"text-align:center;font: bold 18px Arial\">{safe}</div></body></html>");
        }
        catch (ObjectDisposedException)
        {
            // Control was disposed during ShowMessage
        }
        catch (InvalidOperationException)
        {
            // CoreWebView2 was disposed during ShowMessage
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_isDisposed)
        {
            _isDisposed = true;

            // Unsubscribe from events to prevent callbacks after disposal
            try
            {
                if (CoreWebView2 != null)
                {
                    CoreWebView2.WebMessageReceived -= OnWebMessageReceived;
                }
            }
            catch (ObjectDisposedException)
            {
                // Already disposed, ignore
            }
            catch (InvalidOperationException)
            {
                // CoreWebView2 already disposed, ignore
            }
        }
        base.Dispose(disposing);
    }
}
