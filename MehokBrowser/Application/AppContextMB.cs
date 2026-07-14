using GH.Components;
using System.Windows.Forms;
namespace MeshokBrowser
{
    public class AppContextMB : AppContext<AppContextMB>
    {
        protected override void InitializeSomething()
        {
            // WebView2 is initialized by GhBrowser when its handle is created.
        }
        public override Form GetMainForm()
        {
            return new MainMeshok();
        }
        public override Form GetLoginForm()
        {
            return new LoginFormIShop();
        }
        public override CfgCoreConnection GetConnectionSetting()
        {
            var cfg = IniHelper.Cfg<CfgIShop>();
            if (cfg == null)
                cfg = new CfgIShop();
            return cfg;
        }
        public override CfgForm CreateConnectForm()
        {
            return new CfgFormIShop();
        }
        public override CfgApp GetCfgApp()
        {
            return new CfgApp();
        }
        public override IFactoryCriator GetSqlFactoryCriator()
        {
            return new FactoryCriatorMB();
        }
    }
}
