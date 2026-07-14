using NHibernate.Mapping.ByCode.Conformist;
using GH.NHibernate;
namespace MeshokBrowser.NHibernate
{
    public class OrderLineCallBack : BaseEntity
    {
    }
    public class OrderLineCallBackMap : ClassMapping<OrderLineCallBack>
    {
        public OrderLineCallBackMap()
        {
            Table("z$import_cod");
            Id(x => x.id, map => map.Column("cod_id"));
        }
    }
}
