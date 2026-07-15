namespace GH.Components
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
