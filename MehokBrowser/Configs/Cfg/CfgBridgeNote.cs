using MySql.Data.MySqlClient;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
namespace GH.Configs
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
            ToolTip = "Сервер для подключения (http://web.firebirdsql.org).\r\nЕсли подключение к серверу на другом компьютере то Remote [V]")]
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
            ToolTip = "Если подключение к серверу на другом компьютере то Remote [V]")]
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
        [DbConnectionProperty(Category = Category.Connection, Caption = "Database", ToolTip = "Имя базы данных")]
        public string Database { get; set; }
        [DataMember]
        [DbConnectionProperty(Category = Category.Connection, Caption = "Port", ToolTip = "Порт для подключения. Обычно 3306",
            Default = 3306, SubGroup = "Database")]
        public int Port { get; set; } = 3306;
        [DataMember]
        [DbConnectionProperty(Category = Category.Connection, Caption = "Charset", ToolTip = "Язык для подключения. Обычно utf8", Default = "utf8", SubGroup = "Database")]
        public string CharacterSet { get; set; } = "utf8";
        [DataMember]
        [DbConnectionProperty(Category = Category.Connection, Caption = "Protocol", ToolTip = "Протокол подключения",
            Default = MySqlProtocol.Tcp, EditorType = EditorType.Combo, SubGroup = "ConnectionProtocol")]
        public MySqlProtocol ConnectionProtocol { get; set; } = MySqlProtocol.Tcp;
        [DataMember]
        [DbConnectionProperty(Category = Category.Connection, Caption = "Use SSL", ToolTip = "Использовать SSL",
            Default = MySqlSsl.None, EditorType = EditorType.Combo, SubGroup = "ConnectionProtocol")]
        public MySqlSsl SslMode { get; set; } = MySqlSsl.None;
        [DataMember]
        [DbConnectionProperty(Category = Category.Security, Caption = "User", ToolTip = "Пользователь базы данных")]
        public string UserID { get; set; }
        [DataMember]
        [DbConnectionProperty(Category = Category.Security, Caption = "Password", ToolTip = "Пароль для подключения к базе данных"), PasswordPropertyText]
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
        }
    }
    public enum MySqlProtocol
    {
        Tcp = 1,
        NamedPipe = 2,
        UnixSocket = 3,
        SharedMemory = 4
    }
    public enum MySqlSsl
    {
        None = 0,
        Prefered = 1,
        Required = 2,
        VerifyCA = 3,
        VerifyFull = 4
    }
}
