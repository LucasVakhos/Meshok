using DevExpress.XtraLayout;

namespace MehokBrowser.UI.Interfaces
{
    /// <summary>Интерфейс для фреймов с поддержкой страниц (табов).</summary>
    public interface IPagesFrame
    {
        /// <summary>Группа вкладок.</summary>
        TabbedControlGroup PagesGroup { get; set; }
    }
}
