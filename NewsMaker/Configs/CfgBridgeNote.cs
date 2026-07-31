using System;
using NewsMaker.Configs;
using MySql.Data.MySqlClient;
namespace NewsMaker;
public class CfgBridgeNote : NewsMaker.Configs.CfgBridgeNote
{
    protected override void CreateSomething()
    {
        Server = LB.Libs.SecretProvider.NewsBridgeServer;
        Database = LB.Libs.SecretProvider.NewsBridgeDatabase;
        UserID = LB.Libs.SecretProvider.NewsBridgeUserId;
        Password = LB.Libs.SecretProvider.NewsBridgePassword;
    }
    public static MySqlConnection CreateConnection()
    {
        string connString = IniHelper.Cfg<CfgBridgeNote>().ConnectionString();
        return new MySqlConnection(connString);
    }
}
