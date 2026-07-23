using MehokBrowser.Configs.Cfg;
using MehokBrowser.Configs.Forms;
using MehokBrowser.Application;
using MehokBrowser.UI.Config;
using CfgApp = LB.Libs.CfgApp;
using CfgCoreConnection = LB.Libs.CfgCoreConnection;
using IniHelper = LB.Libs.IniHelper;
using System.Windows.Forms;
namespace MeshokBrowser
{
    public class AppContextMB : AppContext<AppContextMB>
    {
        protected override void InitializeSomething()
        {
            // Первый запуск собирает старые разрозненные INI в один файл рядом с exe.
            LB.Libs.IniFile.MigrateLegacyFiles();
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

    }
}
