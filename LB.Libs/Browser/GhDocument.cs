using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LB.Libs
{
    /// <summary>
    /// Synchronous compatibility facade over the live WebView2 DOM.
    /// It lets the existing WinForms workflow migrate without carrying any
    /// legacy browser runtime or assemblies.
    /// </summary>
    public sealed class GhDocument
    {
        private readonly GhBrowser _browser;

        internal GhDocument(GhBrowser browser)
        {
            _browser = browser;
        }

        public GhDomElement Body => QuerySelector("body");

        public string Cookie
        {
            get
            {
                string json = Evaluate("document.cookie || ''");
                return JsonSerializer.Deserialize<string>(json) ?? string.Empty;
            }
        }

        public IEnumerable<GhFormElement> Forms =>
            GetElementsByTagName("form").OfType<GhFormElement>();

        public GhDomElement GetElementById(string id) =>
            QuerySelector("#" + CssEscape(id));

        public GhDomElement GetHtmlElementById(string id) => GetElementById(id);

        public IEnumerable<GhDomElement> GetElementsByTagName(string tagName) =>
            QueryAll(tagName);

        public IEnumerable<GhDomElement> GetElementsByClassName(string className) =>
            QueryAll("." + CssEscape(className));

        internal GhDomElement QuerySelector(string selector) => QueryAll(selector).FirstOrDefault();

        internal IReadOnlyList<GhDomElement> QueryAll(string selector)
        {
            string safeSelector = JsonSerializer.Serialize(selector ?? string.Empty);
            string script =
                "(() => {" +
                "const selector = " + safeSelector + ";" +
                "const css = e => {" +
                " if (!e || e.nodeType !== 1) return null;" +
                " if (e.id) return '#' + CSS.escape(e.id);" +
                " const parts = [];" +
                " while (e && e.nodeType === 1) {" +
                "  let p = e.tagName.toLowerCase();" +
                "  if (e.parentElement) {" +
                "   const same = Array.from(e.parentElement.children).filter(x => x.tagName === e.tagName);" +
                "   if (same.length > 1) p += ':nth-of-type(' + (same.indexOf(e) + 1) + ')';" +
                "  }" +
                "  parts.unshift(p); e = e.parentElement;" +
                " }" +
                " return parts.join(' > ');" +
                "};" +
                "return Array.from(document.querySelectorAll(selector)).map(e => ({" +
                " selector: css(e), parentSelector: css(e.parentElement)," +
                " tagName: e.tagName || '', id: e.id || '', className: e.className || ''," +
                " type: e.type || '', name: e.name || '', value: 'value' in e ? e.value : ''," +
                " checked: 'checked' in e && !!e.checked," +
                " textContent: e.textContent || '', innerHtml: e.innerHTML || ''" +
                "}));" +
                "})()";

            string json = Execute(script);
            var rows = JsonSerializer.Deserialize<List<ElementData>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            return rows.Select(CreateElement).ToList();
        }

        internal string Evaluate(string script) => Execute(script);

        private string Execute(string script)
        {
            Task<string> task = _browser.ExecuteScriptAsync(script);
            while (!task.IsCompleted)
            {
                Application.DoEvents();
                Thread.Sleep(1);
            }
            return task.GetAwaiter().GetResult();
        }

        private GhDomElement CreateElement(ElementData data)
        {
            return (data.TagName ?? string.Empty).ToUpperInvariant() switch
            {
                "FORM" => new GhFormElement(this, data),
                "INPUT" => new GhInputElement(this, data),
                "SELECT" => new GhSelectElement(this, data),
                "TEXTAREA" => new GhTextAreaElement(this, data),
                _ => new GhDomElement(this, data)
            };
        }

        internal static string CssEscape(string value) =>
            (value ?? string.Empty).Replace("\\", "\\\\").Replace("'", "\\'");

        internal sealed class ElementData
        {
            public string Selector { get; set; }
            public string ParentSelector { get; set; }
            public string TagName { get; set; }
            public string Id { get; set; }
            public string ClassName { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public bool Checked { get; set; }
            public string TextContent { get; set; }
            public string InnerHtml { get; set; }
        }
    }

    public class GhDomElement
    {
        private readonly GhDocument _document;
        private readonly GhDocument.ElementData _data;

        internal GhDomElement(GhDocument document, GhDocument.ElementData data)
        {
            _document = document;
            _data = data;
        }

        protected string Selector => _data.Selector;
        public string Id => _data.Id ?? string.Empty;
        public string ClassName => _data.ClassName ?? string.Empty;
        public string Type => _data.Type ?? string.Empty;
        public string Name => _data.Name ?? string.Empty;
        public string TagName => _data.TagName ?? string.Empty;
        public string TextContent => _data.TextContent ?? string.Empty;
        public string InnerHtml => _data.InnerHtml ?? string.Empty;
        public GhDomElement Parent => string.IsNullOrEmpty(_data.ParentSelector)
            ? null
            : _document.QuerySelector(_data.ParentSelector);

        public virtual string Value
        {
            get => ReadString("value");
            set => SetProperty("value", value);
        }

        public virtual bool Checked
        {
            get => ReadBoolean("checked");
            set => SetProperty("checked", value);
        }

        public GhFormElement Form => ClosestForm();

        public IEnumerable<GhDomElement> GetElementsByTagName(string tagName) =>
            _document.QueryAll(Selector + " " + tagName);

        public string GetAttribute(string name)
        {
            string js = ElementExpression() + "?.getAttribute(" + JsonSerializer.Serialize(name) + ") ?? ''";
            return DeserializeString(_document.Evaluate(js));
        }

        public bool HasAttribute(string name)
        {
            string js = "!!" + ElementExpression() + "?.hasAttribute(" + JsonSerializer.Serialize(name) + ")";
            return DeserializeBoolean(_document.Evaluate(js));
        }

        public void SetAttribute(string name, string value)
        {
            Run("e.setAttribute(" + JsonSerializer.Serialize(name) + ", " + JsonSerializer.Serialize(value) + ")");
        }

        public void Focus() => Run("e.focus()");
        public void Click() => Run("e.click()");
        public void Submit() => Run("e.requestSubmit ? e.requestSubmit() : e.submit()");

        public void RaiseEvent(string eventName) =>
            Run("e.dispatchEvent(new Event(" + JsonSerializer.Serialize(eventName) + ", { bubbles: true }))");

        private GhFormElement ClosestForm()
        {
            string js =
                "(() => { const e = " + ElementExpression() + ";" +
                "const f = e ? e.closest('form') : null; return f ? f.id : null; })()";
            string id = DeserializeString(_document.Evaluate(js));
            if (!string.IsNullOrEmpty(id))
                return _document.GetElementById(id) as GhFormElement;
            return _document.GetElementsByTagName("form").OfType<GhFormElement>().FirstOrDefault();
        }

        private string ReadString(string property)
        {
            string js = ElementExpression() + "?.[" + JsonSerializer.Serialize(property) + "] ?? ''";
            return DeserializeString(_document.Evaluate(js));
        }

        private bool ReadBoolean(string property)
        {
            string js = "!!" + ElementExpression() + "?.[" + JsonSerializer.Serialize(property) + "]";
            return DeserializeBoolean(_document.Evaluate(js));
        }

        private void SetProperty(string property, object value)
        {
            Run("e[" + JsonSerializer.Serialize(property) + "] = " + JsonSerializer.Serialize(value));
        }

        private void Run(string operation)
        {
            string js = "(() => { const e = " + ElementExpression() + "; if (!e) return false; " + operation + "; return true; })()";
            _document.Evaluate(js);
        }

        private string ElementExpression() =>
            "document.querySelector(" + JsonSerializer.Serialize(Selector) + ")";

        private static string DeserializeString(string json) =>
            JsonSerializer.Deserialize<string>(json) ?? string.Empty;

        private static bool DeserializeBoolean(string json) =>
            JsonSerializer.Deserialize<bool>(json);
    }

    public sealed class GhFormElement : GhDomElement
    {
        internal GhFormElement(GhDocument document, GhDocument.ElementData data) : base(document, data) { }
    }

    public sealed class GhInputElement : GhDomElement
    {
        internal GhInputElement(GhDocument document, GhDocument.ElementData data) : base(document, data) { }
    }

    public sealed class GhSelectElement : GhDomElement
    {
        internal GhSelectElement(GhDocument document, GhDocument.ElementData data) : base(document, data) { }
    }

    public sealed class GhTextAreaElement : GhDomElement
    {
        internal GhTextAreaElement(GhDocument document, GhDocument.ElementData data) : base(document, data) { }
    }
}
