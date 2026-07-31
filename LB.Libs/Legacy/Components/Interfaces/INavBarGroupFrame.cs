using DevExpress.XtraNavBar;
namespace LB.Libs;

public interface INavBarGroupFrame
{
    NavBarGroup Group { get; }
    bool IsBase { get; }
}
