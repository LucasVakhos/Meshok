using NHibernate.Mapping.ByCode.Conformist;
using GH.NHibernate;
namespace MeshokBrowser.NHibernate
{
    public class ClientCallBack : BaseEntity
    {
    }
    public class ClientCallBackMap : ClassMapping<ClientCallBack>
    {
        public ClientCallBackMap()
        {
            Table("z$import_clients");
            Id(x => x.id, map => map.Column("c_id"));
        }
    }
}
