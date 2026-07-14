using NHibernate.Mapping.ByCode.Conformist;
using GH.NHibernate;
namespace MeshokBrowser.NHibernate
{
    public class TitleCallBack : BaseEntity
    {
        public virtual int ts_id { get; set; }
    }
    public class TitleCallBackMap : ClassMapping<TitleCallBack>
    {
        public TitleCallBackMap()
        {
            Table("z$import_t");
            Id(x => x.id, map => map.Column("t_id"));
            Property(x => x.ts_id, map => map.Column("ts_id"));
        }
    }
}
