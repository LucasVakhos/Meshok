using NHibernate.Mapping.ByCode.Conformist;
using GH.NHibernate;
namespace NewsMaker.NHibernate
{
    public class User : BaseUser
    {
    }
    public class UserMap : ClassMapping<User>
    {
        public UserMap()
        {
            Table("users");
            Id(x => x.id, map => map.Column("id"));
            Property(x => x.Name, map => map.Column("name"));
            Property(x => x.Password, map => map.Column("password"));
            Property(x => x.Active, map => map.Column("active"));
        }
    }
}
