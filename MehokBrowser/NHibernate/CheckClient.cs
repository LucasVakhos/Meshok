using NHibernate.Mapping.ByCode.Conformist;
using GH.NHibernate;
namespace MeshokBrowser.NHibernate
{
    public class CheckClient : BaseEntity
    {
        public CheckClient()
        {
        }
        public virtual int c_md_id { get; set; }
        public virtual int c_mp_id { get; set; }
        public virtual string c_email { get; set; }
        public virtual bool c_enabled { get; set; }
        public virtual string c_phone { get; set; }
        public virtual string c_zipcode { get; set; }
        public virtual string c_address { get; set; }
    }
    public class CheckClientMap : ClassMapping<CheckClient>
    {
        public CheckClientMap()
        {
            Table("z$import_clients_inf");
            Id(x => x.id, map => map.Column("c_id"));
            Property(x => x.Name, map => map.Column("c_name"));
            Property(x => x.c_md_id, map => map.Column("c_md_id"));
            Property(x => x.c_mp_id, map => map.Column("c_mp_id"));
            Property(x => x.c_email, map => map.Column("c_email"));
            Property(x => x.c_enabled, map => map.Column("c_enabled"));
            Property(x => x.c_phone, map => map.Column("c_phone"));
            Property(x => x.c_zipcode, map => map.Column("c_zipcode"));
            Property(x => x.c_address, map => map.Column("c_address"));
        }
    }
}
