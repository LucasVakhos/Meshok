using NHibernate.Mapping.ByCode.Conformist;
using GH.NHibernate;
namespace MeshokBrowser.NHibernate
{
    public class SiteStatus : BaseEntity
    {
    }
    public class SiteStatusMap : ClassMapping<SiteStatus>
    {
        public SiteStatusMap()
        {
            Table("z$statuses");
            Id(x => x.id, map => map.Column("zs_id"));
            Property(x => x.Name, map => map.Column("zs_status_name"));
        }
    }
}
