using DevExpress.XtraBars;
namespace LB.Libs;

public interface IBarsForm
{
    BarManager BarManager { get; }
    Bar StatusBar { get; }
}
