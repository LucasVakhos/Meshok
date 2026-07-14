using NHibernate.Mapping.ByCode.Conformist;
using GH.NHibernate;
namespace MeshokBrowser.NHibernate
{
    public class BaseStatus : BaseEntity
    {
    }
    public class BaseStatusMap : ClassMapping<BaseStatus>
    {
        public BaseStatusMap()
        {
            Table("z$base_statuses");
            Id(x => x.id, map => map.Column("cs_id"));
            Property(x => x.Name, map => map.Column("cs_name"));
        }
    }
}
