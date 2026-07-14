using NHibernate.Mapping.ByCode.Conformist;
using GH.NHibernate;
namespace MeshokBrowser.NHibernate
{
    public class OrderCallBack : BaseEntity
    {
    }
    public class OrderCallBackMap : ClassMapping<OrderCallBack>
    {
        public OrderCallBackMap()
        {
            Table("z$import_co");
            Id(x => x.id, map => map.Column("co_id"));
        }
    }
}
