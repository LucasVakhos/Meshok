using NHibernate.Mapping.ByCode.Conformist;
using GH.NHibernate;
namespace MeshokBrowser.NHibernate
{
    public class RelBaseSiteStatus : BaseEntity
    {
    }
    public class RelBaseSiteStatusMap : ClassMapping<RelBaseSiteStatus>
    {
        public RelBaseSiteStatusMap()
        {
            Table("z$import_statuses");
            Id(x => x.id, map => map.Column("cs_id"));
            Property(x => x.Name, map => map.Column("cs_name"));
        }
    }
}
