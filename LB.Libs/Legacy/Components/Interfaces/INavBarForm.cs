using DevExpress.XtraNavBar;
namespace LB.Libs;

public interface INavBarForm
{
    NavBarControl NavBar { get; }
    FrameHolder FrameHolder { get; }
}
