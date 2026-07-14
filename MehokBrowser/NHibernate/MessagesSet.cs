using NHibernate.Mapping.ByCode.Conformist;
using GH.NHibernate;
using System.ComponentModel.DataAnnotations;
namespace MeshokBrowser.NHibernate
{
    public class MessagesSet : BaseEntity
    {
        [Display(Name = "Статус в базе")]
        public virtual int zsc_cs_id { get; set; }
        [Display(Name = "Статус на мешке")]
        public virtual int zsc_zs_id { get; set; }
        [Display(Name = "Способ доставки")]
        public virtual int? zsc_md_id { get; set; }
        [Display(Name = "Применять если")]
        public virtual int zsc_case { get; set; }
        [Display(Name = "Текст сообщения")]
        public virtual string zsc_message { get; set; }
    }
    public class MessagesSetMap : ClassMapping<MessagesSet>
    {
        public MessagesSetMap()
        {
            Table("z$statuses_cod");
            Id(x => x.id, map => map.Column("zsc_id"));
            Property(x => x.zsc_cs_id, map => map.Column("zsc_cs_id"));
            Property(x => x.zsc_zs_id, map => map.Column("zsc_zs_id"));
            Property(x => x.zsc_md_id, map => map.Column("zsc_md_id"));
            Property(x => x.zsc_case, map => map.Column("zsc_case"));
            Property(x => x.zsc_message, map => map.Column("zsc_message"));
        }
    }
}
