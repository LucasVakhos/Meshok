using NHibernate.Mapping.ByCode.Conformist;
using GH.NHibernate;
namespace MeshokBrowser.NHibernate
{
    public class CheckSite : BaseEntity
    {
    }
    public class CheckSitedMap : ClassMapping<CheckSite>
    {
        public CheckSitedMap()
        {
            Table("z$site_id");
            Id(x => x.id, map => map.Column("z$s_id"));
            Property(x => x.Name, map => map.Column("z$s_name"));
        }
    }
}
