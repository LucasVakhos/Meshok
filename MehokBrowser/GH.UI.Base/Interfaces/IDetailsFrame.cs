using DevExpress.XtraLayout;
namespace MehokBrowser.UI.Interfaces
{
    public interface IDetailsFrame
    {
        LayoutControlGroup Page { get; }
        TabbedGroup PageControl { get; }
    }
}
