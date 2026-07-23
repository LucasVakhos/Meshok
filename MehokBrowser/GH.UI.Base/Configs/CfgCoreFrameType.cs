using System.Collections;
namespace MehokBrowser.UI.Config
{
    public class CfgCoreFrameType<T> : CfgCoreFrame where T : LB.Libs.CfgCore
    {
        public CfgCoreFrameType()
        {
            dataSource.DataSource = typeof(T);
            if (!IsDesignMode)
            {
                dataSource.OnOpen += DataSource_OnOpen;
                dataSource.Open();
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }
        private void DataSource_OnOpen(out IList list)
        {
            list = new List<T>();
            list.Add(LB.Libs.IniHelper.CoreCfg<T>());
        }
    }
}
