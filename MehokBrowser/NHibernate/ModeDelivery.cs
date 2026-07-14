using NHibernate.Mapping.ByCode.Conformist;
using GH.NHibernate;
namespace MeshokBrowser.NHibernate
{
    public class ModeDelivery : BaseEntity
    {
    }
    public class ModeDeliveryMap : ClassMapping<ModeDelivery>
    {
        public ModeDeliveryMap()
        {
            Table("mode_delivery");
            Id(x => x.id, map => map.Column("md_id"));
            Property(x => x.Name, map => map.Column("md_name"));
        }
    }
}
