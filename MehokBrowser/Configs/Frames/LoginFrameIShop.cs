using LB.Libs;
using MehokBrowser.Configs.Cfg;
using MeshokBrowser.Data;
using MeshokBrowser.Models;
using System.Collections;

namespace MehokBrowser.Configs.Frames;

public class LoginFrameIShop : LoginFrameType<CfgIShop, User>
{
    public LoginFrameIShop()
    {
        LoginInputType = LoginInputType.AsSelectFromCombo;
    }

    protected override IList GetAllUsers()
    {
        return DapperLookupRepository.LoadActiveUsers();
    }
}
