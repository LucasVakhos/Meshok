using GH.Components;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
namespace NewsMaker
{
    public class CfgPost : CfgCore
    {
        [DataMember]
        [Display(Name = "Smtp", Description = "Smtp ??????")]
        public string Smtp { get; set; } = "smtp.yandex.ru";
        [DataMember]
        [Display(Name = "?????", Description = "????? ??? ???????????")]
        public string User { get; set; } = "bridgenote";
        [DataMember]
        [Display(Name = "??????", Description = "?????? ??? ???????????")]
        public string PassWrd { get; set; } = "Gznsqehj;fq2016";
        [DataMember]
        [Display(Name = "Email", Description = "???????? ?????")]
        public string BridgeEmail { get; set; } = "bridgenote@yandex.ru";
        [DataMember]
        [Display(Name = "????", Description = "????")]
        public int Port { get; set; } = 25;
        [DataMember]
        [Display(Name = "Use SSL", Description = "???????????? ??????????")]
        public bool UseSSL { get; set; } = true;
        protected override void CreateSomething()
        {
            //????????
        }
    }
}
