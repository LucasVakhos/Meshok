using NHibernate.Mapping.ByCode.Conformist;
using GH.Attributes;
using GH.NHibernate;
namespace MeshokBrowser.NHibernate
{
    public class User : BaseUser
    {
        [UpdatableProperty(Caption = "Логин", ToolTip = "Логин", Group = "Данные пользователя")]
        public override string Login
        {
            get { return Name; }
            set { Name = value; }
        }
    }
    public class UserMap : ClassMapping<User>
    {
        public UserMap()
        {
            Table("MANAGERS");
            Id(x => x.id, map => map.Column("MN_ID"));
            Property(x => x.Name, map => map.Column("MN_NAME"));
            Property(x => x.Password, map => map.Column("MN_PASSWORD"));
            Property(x => x.Active, map => map.Column("MN_ACTIVE"));
        }
    }
}
