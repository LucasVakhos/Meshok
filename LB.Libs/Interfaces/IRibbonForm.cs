using DevExpress.XtraBars.Ribbon;
namespace LB.Libs;

public interface IRibbonForm
{
    RibbonControl Ribbon { get; }
    RibbonStatusBar StatusBar { get; }
    RibbonPageGroup FrameGroup { get; }
}
