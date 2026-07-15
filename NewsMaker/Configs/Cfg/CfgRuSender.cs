using CfgCore = LB.Libs.CfgCore;
using SecretProvider = LB.Libs.SecretProvider;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
namespace NewsMaker
{
    public class CfgRuSender : CfgCore
    {
        [DataMember]
        [Display(Name = "User Id", Description = "User Id для плдключения")]
        public int ID { get; set; } = SecretProvider.RuSenderId;
        [DataMember]
        [Display(Name = "API Key", Description = "API key для подключения")]
        public string ApiKey { get; set; } = SecretProvider.RuSenderApiKey;
        [DataMember]
        [Display(Name = "Back Email", Description = "Обратный адрес")]
        public string BackEmail { get; set; } = SecretProvider.RuSenderBackEmail;
        [DataMember]
        [Display(Name = "Send Limit In 1 Second", Description = "Ограничение рассылки за 1 секунду")]
        public int SendLimit { get; set; } = 10;
        protected override void CreateSomething()
        {
            //заглушка
        }
    }
}
