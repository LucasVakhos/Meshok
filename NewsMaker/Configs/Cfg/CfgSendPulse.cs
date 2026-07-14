using GH.Configs;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
namespace NewsMaker
{
    public class CfgSendPulse : CfgCore
    {
        [DataMember]
        [Display(Name = "User Id", Description = "User Id для плдключения")]
        public string UserId { get; set; } = GH.Components.SecretProvider.SendPulseUserId;
        [DataMember]
        [Display(Name = "Secret", Description = "Secret key для подключения")]
        public string Secret { get; set; } = GH.Components.SecretProvider.SendPulseSecret;
        [DataMember]
        [Display(Name = "Back Email", Description = "обратный адрес")]
        public string BackEmail { get; set; } = GH.Components.SecretProvider.SendPulseBackEmail;
        [DataMember]
        [Display(Name = "Send Limit", Description = "Ограничение рассылки")]
        public int SendLimit { get; set; } = 0;
        protected override void CreateSomething()
        {
            //заглушка
        }
    }
}
