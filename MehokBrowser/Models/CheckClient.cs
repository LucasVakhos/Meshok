using GH.Components;
namespace MeshokBrowser.Models
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
}