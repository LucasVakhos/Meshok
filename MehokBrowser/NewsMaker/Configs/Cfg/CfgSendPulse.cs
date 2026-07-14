using GH.Components;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
namespace NewsMaker
{
    public class CfgSendPulse : CfgCore
    {
        [DataMember]
        [Display(Name = "User Id", Description = "User Id ??? ???????????")]
        public string UserId { get; set; } = "e0d565029f8bc2ccd620cdca300a5d3e";
        [DataMember]
        [Display(Name = "Secret", Description = "Secret key ??? ???????????")]
        public string Secret { get; set; } = "963d2fd50f83a3c72ee05cd02fabedb3";
        [DataMember]
        [Display(Name = "Back Email", Description = "???????? ?????")]
        public string BackEmail { get; set; } = "info@bridgenote.com";
        [DataMember]
        [Display(Name = "Send Limit", Description = "??????????? ????????")]
        public int SendLimit { get; set; } = 0;
        protected override void CreateSomething()
        {
            //????????
        }
    }
}
