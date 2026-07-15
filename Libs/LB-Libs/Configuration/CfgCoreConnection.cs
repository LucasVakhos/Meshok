using System.ComponentModel;
using System.Runtime.Serialization;

namespace LB.Libs;

public delegate void GetBaseUserEvent(ref BaseUser? user);

/// <summary>
/// Connection settings without dependencies on the legacy configuration UI.
/// </summary>
public class CfgCoreConnection : CfgCore
{
    private BaseUser? _user;

    [DataMember]
    [DbConnectionProperty(Category = Category.User, Caption = "Login", ToolTip = "Логин", EditorType = EditorType.Combo)]
    public virtual string UserLogin { get; set; } = string.Empty;

    [DataMember]
    [DbConnectionProperty(Category = Category.User, Caption = "Password", ToolTip = "Пароль")]
    public virtual string UserPassword { get; set; } = string.Empty;

    [DataMember]
    [DbConnectionProperty(Category = Category.User, Caption = "Auto entering", ToolTip = "Автовход если доступ разрешён", Default = false)]
    public bool AutoEntering { get; set; }

    public event GetBaseUserEvent? GetBaseUser;

    public bool IsComplete
    {
        get
        {
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(this, false))
            {
                if (property.Attributes[typeof(DbConnectionProperty)] is not DbConnectionProperty)
                    continue;
                if (property.Name is nameof(UserLogin) or nameof(UserPassword) or nameof(AutoEntering))
                    continue;
                if (property.GetValue(this) is null)
                    return false;
            }

            return true;
        }
    }

    public bool UserIsValid =>
        User is not null && User.Login == UserLogin && User.Password == UserPassword;

    protected BaseUser? User
    {
        get
        {
            if (_user is null || _user.Login != UserLogin || _user.Password != UserPassword)
                GetBaseUser?.Invoke(ref _user);
            return _user;
        }
        set => _user = value;
    }

    public BaseUser? GetUser() => User;

    public void CheckIdentity()
    {
        BaseUser? user = User;
        if (user is null || (UserLogin == user.Login && UserPassword == user.Password))
            return;

        UserLogin = user.Login;
        UserPassword = user.Password;
        Save(true);
    }

    protected override void LoadDefauls()
    {
        foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(this, false))
        {
            if (property.Attributes[typeof(DbConnectionProperty)] is DbConnectionProperty attribute)
                Default(property, attribute.Default);
        }
    }

    public virtual string ConnectionString()
    {
        throw new NotImplemented(nameof(ConnectionString), this);
    }

    public virtual bool TestConnection()
    {
        throw new NotImplemented(nameof(TestConnection), this);
    }

    public virtual bool IsRemoteDataBase()
    {
        throw new NotImplemented(nameof(IsRemoteDataBase), this);
    }

    public virtual string GetRemoteServer()
    {
        throw new NotImplemented(nameof(GetRemoteServer), this);
    }
}
