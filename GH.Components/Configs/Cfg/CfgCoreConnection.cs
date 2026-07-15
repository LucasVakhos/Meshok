namespace GH.Components;

/// <summary>
/// Temporary UI compatibility layer. Configuration data and persistence live
/// in LB.Libs.CfgCoreConnection; this class only keeps the legacy dialog hook.
/// </summary>
[Obsolete("Use LB.Libs.CfgCoreConnection directly.")]
public class CfgCoreConnection : LB.Libs.CfgCoreConnection
{
    protected CfgConnectFrame? Frame { get; private set; }

    public bool ConnectIsOK()
    {
        CfgForm? connectForm = RunContext.GetConnectForm();
        if (connectForm is null)
            return true;

        using (connectForm)
        {
            if (IsComplete && TestConnection())
                return true;
            return connectForm.ShowDialog() == DialogResult.OK;
        }
    }

    internal void SetFrame(CfgConnectFrame? frame)
    {
        Frame = frame;
    }
}
