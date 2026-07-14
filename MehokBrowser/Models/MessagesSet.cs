using GH.Components;
using System.ComponentModel.DataAnnotations;
namespace MeshokBrowser.Models
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
}