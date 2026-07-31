using System;
using LB.Libs;
using MySql.Data.MySqlClient;
namespace NewsMaker;
public class CfgBridgeNote : LB.Libs.CfgBridgeNote
{
    protected override void CreateSomething()
    {
        Server = "bd.bridgenote.com";
        Database = "bridgenote";
        UserID = "bridge";
        Password = "1MaB5zIrOndfqfvUJc";
    }
    public static MySqlConnection CreateConnection()
    {
        string connString = IniHelper.Cfg<CfgBridgeNote>().ConnectionString();
        return new MySqlConnection(connString);
    }
}
