using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace GH.Components
{
    public class GhBrowser : WebView2, ISupportInitialize
    {
        private const string LoadingHtml = """
            <html>
              <head><title>Wait for loading ...</title></head>
              <body><div style="text-align:center;font: bold 18px Arial">Wait for loading ...</div></body>
            </html>
            """;

        private bool _noDefaultContextMenu;

        public event EventHandler DocumentCompleted;
        public event EventHandler CreateWindow;

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

            await EnsureCoreWebView2Async();
            NavigateToString(LoadingHtml);
        }

        private void OnCoreWebView2InitializationCompleted(
            object sender,
            CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
                return;

            ApplySettings();
            CoreWebView2.NewWindowRequested += (_, args) =>
            {
                args.Handled = true;
                CreateWindow?.Invoke(this, EventArgs.Empty);
                Navigate(args.Uri);
            };
        }

        private void ApplySettings()
        {
            if (CoreWebView2 != null)
                CoreWebView2.Settings.AreDefaultContextMenusEnabled = !_noDefaultContextMenu;
        }

        private void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            DocumentCompleted?.Invoke(this, EventArgs.Empty);
        }

        public async void Navigate(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            await EnsureCoreWebView2Async();
            CoreWebView2.Navigate(url);
        }

        public void ShowMessage(string message)
        {
            string safe = System.Net.WebUtility.HtmlEncode(message ?? string.Empty);
            NavigateToString($"<html><body><div style=\"text-align:center;font: bold 18px Arial\">{safe}</div></body></html>");
        }
    }
}
