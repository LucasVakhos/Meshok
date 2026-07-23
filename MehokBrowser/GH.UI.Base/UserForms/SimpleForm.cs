using DevExpress.XtraEditors;
using System.ComponentModel;
namespace MehokBrowser.Forms.Base
{
    [ToolboxItem(false)]
    public class SimpleForm : XtraForm
    {
        public bool IsDesignMode => DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        public bool IsRuntimeMode => !IsDesignMode;
        public SimpleForm()
        {
        }
        protected override void OnLoad(EventArgs e)
        {
            if (IsRuntimeMode)
            {
                Icon = Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            }
            base.OnLoad(e);
        }
    }
}
