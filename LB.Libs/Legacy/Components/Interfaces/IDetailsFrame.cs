using DevExpress.XtraLayout;
namespace LB.Libs;

public interface IDetailsFrame
{
    LayoutControlGroup Page { get; }
    TabbedGroup PageControl { get; }
}
