using System.Collections;
using LB.Libs;

namespace MehokBrowser.Configs.Frames;

/// <summary>
/// Абстрактный фрейм логина с дженерик-параметрами конфигурации и пользователя.
/// </summary>
/// <typeparam name="TConfig">Тип конфигурации подключения (наследник CfgCoreConnection).</typeparam>
/// <typeparam name="TUser">Тип пользователя (наследник BaseUser).</typeparam>
public abstract class LoginFrameType<TConfig, TUser> : LoginFrame
    where TConfig : CfgCoreConnection
    where TUser : BaseUser
{
    private TConfig _config;
    private IList<TUser> _users = null;

    /// <summary>Список пользователей.</summary>
    protected virtual IList<TUser> Users { get => _users; set => _users = value; }

    /// <summary>Тип ввода логина.</summary>
    [GHProperty]
    public LoginInputType LoginInputType { get; set; }

    /// <summary>
    /// Конструктор фрейма логина.
    /// </summary>
    public LoginFrameType()
    {
        FinalInitialize();
    }

    private void FinalInitialize()
    {
        if (!IsDesignMode)
        {
            _config = IniHelper.Cfg<TConfig>();
            dataSource.DataSource = typeof(TConfig);
            dataSource.OnOpen += DataSource_OnOpen;
            Users = GetAllUsers() as IList<TUser>;
            _config.GetBaseUser += LoginFrameType_GetBaseUser;
        }
    }

    /// <summary>Получить всех пользователей системы.</summary>
    protected abstract IList GetAllUsers();

    /// <inheritdoc />
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (!IsDesignMode)
        {
            if (LoginInputType == LoginInputType.AsSelectFromCombo)
            {
                SuspendLayout();
                try
                {
                    userLogin.Properties.Buttons.AddRange(
                        new DevExpress.XtraEditors.Controls.EditorButton[] {
                        new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
                        }
                    );
                    userLogin.Properties.Items.AddRange(
                        Users.Select(u => u.Name).ToArray()
                        );
                }
                finally
                {
                    ResumeLayout(true);
                }
            }
            dataSource.Open();
        }
    }

    private void LoginFrameType_GetBaseUser(ref BaseUser? user)
    {
        user = Users.Where(x => x.Login == _config.UserLogin && x.Password == _config.UserPassword).FirstOrDefault();
    }

    private void DataSource_OnOpen(out IList list)
    {
        list = new List<TConfig>();
        list.Add(_config);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _config.GetBaseUser -= LoginFrameType_GetBaseUser;
            _config = null;
        }
        base.Dispose(disposing);
    }
}
