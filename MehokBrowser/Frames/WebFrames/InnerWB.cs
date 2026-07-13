using GH.AppContext;
using GH.Components;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace MeshokBrowser
{
    public partial class InnerWB : BaseFrame
    {
        public GhBrowser webBrowser => wbMain;

        public int StatusCode { get; set; }

        private async void SetBrowser()
        {
            StatusCode = 0;
            wbMain.TabIndex = 0;
            await wbMain.EnsureCoreWebView2Async();
            wbMain.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            wbMain.CoreWebView2.NavigationCompleted += WbMain_NavigationCompleted;
            wbMain.CoreWebView2.NewWindowRequested += WbMain_NewWindowRequested;
            wbMain.SourceChanged += WbMain_SourceChanged;
        }

        private void WbMain_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;
            Navigate(e.Uri);
        }

        private void WbMain_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            StatusCode = e.IsSuccess ? 0 : (int)e.HttpStatusCode;
        }

        private void WbMain_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            textBox1.Text = wbMain.Source?.AbsoluteUri ?? string.Empty;
        }

        private bool ExecuteBool(string script)
        {
            if (DesignMode || wbMain.CoreWebView2 == null)
                return false;

            string json = wbMain.ExecuteScriptAsync(script).GetAwaiter().GetResult();
            return JsonSerializer.Deserialize<bool>(json);
        }

        public bool InProfile()
        {
            return ExecuteBool("document.getElementById('cz2') !== null");
        }

        private readonly Dictionary<string, int> qtyDictionary = new Dictionary<string, int>();

        public bool HasQty(string ident)
        {
            string key = "what=" + ident.ToLowerInvariant();
            if (wbMain.CoreWebView2 == null)
                return qtyDictionary.TryGetValue(key, out int cached) && cached > 0;

            string keyJson = JsonSerializer.Serialize(key);
            string script = $@"(() => {{
                const root = document.getElementById('cz2');
                if (!root) return 0;
                const link = Array.from(root.querySelectorAll('a[href]'))
                    .find(a => a.getAttribute('href').includes({keyJson}));
                if (!link) return 0;
                const values = (link.textContent || '').match(/\d+/g);
                return values && values.length ? parseInt(values[values.length - 1], 10) : 0;
            }})()";

            string json = wbMain.ExecuteScriptAsync(script).GetAwaiter().GetResult();
            int qty = JsonSerializer.Deserialize<int>(json);
            qtyDictionary[key] = qty;
            return qty > 0;
        }

        public System.Threading.Tasks.Task<T?> ExecuteJsonScriptAsync<T>(string script)
        {
            return webBrowser.ExecuteJsonScriptAsync<T>(script);
        }

        public InnerWB()
        {
            InitializeComponent();
        }

        public void Navigate(string urlString)
        {
            if (DesignMode || string.IsNullOrWhiteSpace(urlString))
                return;

            StatusCode = 0;
            RunContext.Invoke(() =>
            {
                if (webBrowser.CoreWebView2 != null)
                    webBrowser.CoreWebView2.Navigate(urlString);
                else
                    webBrowser.Source = new Uri(urlString);
            });
        }

        public void RefreshBrowser()
        {
            RunContext.Invoke(() => webBrowser.CoreWebView2?.Reload());
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (DesignMode)
                return;

            webBrowser.Select();
            if (webBrowser.CoreWebView2?.CanGoBack == true)
                webBrowser.CoreWebView2.GoBack();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (DesignMode)
                return;

            webBrowser.Select();
            RefreshBrowser();
        }

        private void InnerWB_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
                SetBrowser();
        }
    }
}
