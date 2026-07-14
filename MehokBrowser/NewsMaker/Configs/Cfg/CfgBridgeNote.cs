using MySql.Data.MySqlClient;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
namespace GH.Components
{
    public class CfgBridgeNote : CfgCoreConnection
    {
        public static MySqlConnection CreateConnection()
        {
            string connString = IniHelper.Cfg<CfgBridgeNote>().ConnectionString();
            return new MySqlConnection(connString);
        }
        public override object GetDefault(string name)
        {
            switch (name)
            {
                case nameof(Server):
                    return "bd.bridgenote.com";
                case nameof(Database):
                    return "bridgenote";
                case nameof(UserID):
                    return "bridge";
                case nameof(Password):
                    return "1MaB5zIrOndfqfvUJc";
                default:
                    return null;
            }
        }
        [DataMember]
        [DbConnectionProperty(Category = Category.Connection, Caption = "Server", SubGroup = "Server",
            ToolTip = "?????? ??? ??????????? (http://web.firebirdsql.org).\r\n???? ??????????? ? ??????? ?? ?????? ?????????? ?? Remote [V]")]
        public string Server
        {
            get => _server;
            set
            {
                _server = value;
            }
        }
        private string _server;
        [DataMember]
        [DbConnectionProperty(Category = Category.Connection, Caption = "Remote", Default = true, EditorType = EditorType.Check, SubGroup = "Server",
            ToolTip = "???? ??????????? ? ??????? ?? ?????? ?????????? ?? Remote [V]")]
        public bool Remote
        {
            get => _remote;
            set
            {
                _remote = value;
            }
        }
        private bool _remote = true;
        [DataMember]
        [DbConnectionProperty(Category = Category.Connection, Caption = "Database", ToolTip = "??? ???? ??????")]
        public string Database { get; set; }
        [DataMember]
        [DbConnectionProperty(Category = Category.Connection, Caption = "Port", ToolTip = "???? ??? ???????????. ?????? 3306",
            Default = 3306, SubGroup = "Database")]
        public int Port { get; set; } = 3306;
        [DataMember]
        [DbConnectionProperty(Category = Category.Connection, Caption = "Charset", ToolTip = "???? ??? ???????????. ?????? utf8", Default = "utf8", SubGroup = "Database")]
        public string CharacterSet { get; set; } = "utf8";
        [DataMember]
        [DbConnectionProperty(Category = Category.Connection, Caption = "Protocol", ToolTip = "???????? ???????????",
            Default = MySqlProtocol.Tcp, EditorType = EditorType.Combo, SubGroup = "ConnectionProtocol")]
        public MySqlProtocol ConnectionProtocol { get; set; } = MySqlProtocol.Tcp;
        [DataMember]
        [DbConnectionProperty(Category = Category.Connection, Caption = "Use SSL", ToolTip = "???????????? SSL",
            Default = MySqlSsl.None, EditorType = EditorType.Combo, SubGroup = "ConnectionProtocol")]
        public MySqlSsl SslMode { get; set; } = MySqlSsl.None;
        [DataMember]
        [DbConnectionProperty(Category = Category.Security, Caption = "User", ToolTip = "???????????? ???? ??????")]
        public string UserID { get; set; }
        [DataMember]
        [DbConnectionProperty(Category = Category.Security, Caption = "Password", ToolTip = "?????? ??? ??????????? ? ???? ??????"), PasswordPropertyText]
        public string Password { get; set; }
        protected override void CreateSomething()
        {
        }
        public override string ConnectionString()
        {
            var csb = new MySqlConnectionStringBuilder();
            csb.Server = Server;
            csb.Database = Database;
            csb.UserID = UserID;
            csb.Password = Password;
            csb.ConnectionProtocol = (MySqlConnectionProtocol)ConnectionProtocol;
            csb.Port = (uint)Port;
            csb.CharacterSet = CharacterSet;
            csb.SslMode = (MySqlSslMode)SslMode;
            csb.Pooling = true;
            return csb.ConnectionString;
        }
        public override bool TestConnection()
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectionString()))
            {
                try
                {
                    conn.Open();
                    return true;
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
                return false;
            }
        }
        public override bool IsRemoteDataBase()
        {
            return Remote;
            //return !(Server == Environment.MachineName || Server == "127.0.0.1" || Server == "localhost");
        }
    }
    public enum MySqlProtocol
    {
        Tcp = 1,
        //
        // ??????:
        //     Named pipe connection. Works only on Windows systems.
        NamedPipe = 2,
        //
        // ??????:
        //     Unix domain socket connection. Works only with Unix systems.
        UnixSocket = 3,
        //
        // ??????:
        //     Unix domain socket connection. Works only with Unix systems.
        SharedMemory = 4
    }
    public enum MySqlSsl
    {
        //
        // ??????:
        //     Do not use SSL.
        None = 0,
        //
        // ??????:
        //     Use SSL, if server supports it. This option is only available for the classic
        //     protocol.
        Prefered = 1,
        //
        // ??????:
        //     Always use SSL. Deny connection if server does not support SSL. Do not perform
        //     server certificate validation. This is the default SSL mode when the same isn't
        //     specified as part of the connection string.
        Required = 2,
        //
        // ??????:
        //     Always use SSL. Validate server SSL certificate, but different host name mismatch.
        VerifyCA = 3,
        //
        // ??????:
        //     Always use SSL and perform full certificate validation.
        VerifyFull = 4
    }
}
