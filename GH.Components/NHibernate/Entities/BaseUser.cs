using NHibernate.Mapping.ByCode.Conformist;
using System.ComponentModel;
namespace GH.Components
{
    [DisplayName("User"), Description("Информация о пользователе"), Category("Информация")]
    public class BaseUser : BaseEntity
    {
        [UpdatableProperty(Caption = "Ф.И.О.", ToolTip = "Инициалы пользователя", Group = "Данные пользователя")]
        public override string Name { get; set; }
        [UpdatableProperty(Caption = "Логин", ToolTip = "Логин", Group = "Данные пользователя")]
        public virtual string Login { get; set; }
        [UpdatableProperty(Caption = "Пароль", ToolTip = "Пароль от 8 до 25 символов", Group = "Данные пользователя"
            , Required = true, MaxLength = 25)]
        [PasswordPropertyText]
        public virtual string Password
        {
            get
            {
                return _password;
            }
            set
            {
                if (_password == value)
                    return;
                if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                    throw new Exception("Пароль не должен быть пустым!");
                _password = value;
            }
        }

    private string _password = null;
        [UpdatableProperty(Caption = "Активен", ToolTip = "Активен", Group = "Данные пользователя")]
        public virtual bool Active { get; set; }
    }

    public class UserMap : ClassMapping<BaseUser>
    {
        public UserMap()
        {
            Table("users");
            Id(x => x.id, map => map.Column("u_id"));
            Property(x => x.Name, map => map.Column("u_name"));
            Property(x => x.Login, map => map.Column("u_e_mail"));
            Property(x => x.Password, map => map.Column("u_password"));
            Property(x => x.Active, map => map.Column("u_active"));
        }
    }
}
