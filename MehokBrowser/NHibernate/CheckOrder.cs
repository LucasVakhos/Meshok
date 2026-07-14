using NHibernate.Mapping.ByCode.Conformist;
using GH.NHibernate;
using System;
namespace MeshokBrowser.NHibernate
{
    public class CheckOrder : BaseEntity
    {
        public static CheckOrder NewCheckOrder(OrderLine orderLine)
        {
            CheckOrder check = new CheckOrder();
            check.co_c_id = orderLine.Client.base_id;
            check.co_creation_date = orderLine.date;
            check.co_md_id = orderLine.Client.md_id;
            check.co_mp_id = orderLine.Client.mp_id;
            check.co_status = orderLine.base_status;
            return check;
        }
        public virtual int co_id { get; set; } = 0;
        public virtual int co_c_id { get; set; } = 0;
        public virtual DateTime co_creation_date { get; set; }
        public virtual int co_md_id { get; set; } = 1;
        public virtual int co_mp_id { get; set; } = 1;
        public virtual int co_status { get; set; } = 0;
        public virtual int dp_id { get; set; } = 0;
        public virtual int dp_md_id { get; set; } = 1;
        public virtual int dp_mp_id { get; set; } = 1;
        public virtual int dp_c_id { get; set; } = 0;
        public virtual double dp_totalsumm { get; set; } = 0;
        public virtual int dp_status { get; set; } = 0;
        public virtual bool dp_packed { get; set; } = false;
        public virtual DateTime? dp_creation_date { get; set; } = null;
    }
    public class CheckOrderMap : ClassMapping<CheckOrder>
    {
        public CheckOrderMap()
        {
            Table("z$check_order");
            Id(x => x.id, map => map.Column("cod_id"));
            Property(x => x.co_id, map => map.Column("co_id"));
            Property(x => x.co_c_id, map => map.Column("co_c_id"));
            Property(x => x.co_creation_date, map => map.Column("co_creation_date"));
            Property(x => x.co_md_id, map => map.Column("co_md_id"));
            Property(x => x.co_mp_id, map => map.Column("co_mp_id"));
            Property(x => x.co_status, map => map.Column("co_status"));
            Property(x => x.dp_id, map => map.Column("dp_id"));
            Property(x => x.dp_c_id, map => map.Column("dp_c_id"));
            Property(x => x.dp_md_id, map => map.Column("dp_md_id"));
            Property(x => x.dp_mp_id, map => map.Column("dp_mp_id"));
            Property(x => x.dp_totalsumm, map => map.Column("dp_totalsumm"));
            Property(x => x.dp_status, map => map.Column("dp_status"));
            Property(x => x.dp_packed, map => map.Column("dp_packed"));
            Property(x => x.dp_creation_date, map => map.Column("dp_creation_date"));
        }
    }
}
