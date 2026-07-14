using GH.Configs;
using MeshokBrowser.Data;
using MeshokBrowser.Models;
using System.Collections;

namespace GH.Configs
{
    public class LoginFrameIShop : LoginFrameType<CfgIShop, User>
    {
        public LoginFrameIShop()
        {
            LoginInputType = GH.Components.LoginInputType.AsSelectFromCombo;
        }

        protected override IList GetAllUsers()
        {
            return DapperLookupRepository.LoadActiveUsers();
        }
    }
}