using System;
using GH.Configs;
using MySql.Data.MySqlClient;
namespace NewsMaker
{
    public class CfgBridgeNote : GH.Configs.CfgBridgeNote
    {
        protected override void CreateSomething()
        {
            Server = GH.Components.SecretProvider.NewsBridgeServer;
            Database = GH.Components.SecretProvider.NewsBridgeDatabase;
            UserID = GH.Components.SecretProvider.NewsBridgeUserId;
            Password = GH.Components.SecretProvider.NewsBridgePassword;
        }
        public static MySqlConnection CreateConnection()
        {
            string connString = IniHelper.Cfg<CfgBridgeNote>().ConnectionString();
            return new MySqlConnection(connString);
        }
    }
}
