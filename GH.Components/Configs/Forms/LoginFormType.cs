namespace GH.Components
{
    public class LoginFormType<T> : CfgFormType<T> where T : CfgBaseFrame
    {
    protected override bool EnterEnabled()
        {
            return CfgFrame.dataSource.Current is LB.Libs.CfgCoreConnection cfg && cfg.UserIsValid;
        }
    }
}
