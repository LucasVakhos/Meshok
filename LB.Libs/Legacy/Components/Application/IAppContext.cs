using System.Windows.Forms;
namespace LB.Libs;

public interface IAppContext
{
    Form MaitForm { get; set; }
    Form GetMaitForm();
}
