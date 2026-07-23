using MehokBrowser.Frames.Base;
using AbstractFrame = MehokBrowser.Frames.Base.AbstractFrame;
namespace MehokBrowser.UI.Config
{
    public partial class CfgBaseFrame : AbstractFrame
    {
        public CfgBaseFrame()
        {
            InitializeComponent();
        }
        private void dataSource_OnPost(object sender, EventArgs e)
        {
            Save();
        }
        public void Save()
        {
            if (dataSource.Current is LB.Libs.CfgCore cfgCore)
            {
                cfgCore.Save(true);
            }
        }
    }
}
