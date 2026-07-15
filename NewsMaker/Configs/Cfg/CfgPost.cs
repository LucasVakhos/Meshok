using CfgCore = LB.Libs.CfgCore;
using SecretProvider = LB.Libs.SecretProvider;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
namespace NewsMaker
{
    public class CfgPost : CfgCore
    {
        [DataMember]
        [Display(Name = "Smtp", Description = "Smtp сервер")]
        public string Smtp { get; set; } = SecretProvider.NewsSmtpServer;
        [DataMember]
        [Display(Name = "Логин", Description = "Логин для подключения")]
        public string User { get; set; } = SecretProvider.NewsSmtpUser;
        [DataMember]
        [Display(Name = "Пароль", Description = "Пароль для подключения")]
        public string PassWrd { get; set; } = SecretProvider.NewsSmtpPassword;
        [DataMember]
        [Display(Name = "Email", Description = "Обратный адрес")]
        public string BridgeEmail { get; set; } = SecretProvider.NewsBridgeEmail;
        [DataMember]
        [Display(Name = "Телефон", Description = "Контактный телефон в письме")]
        public string ContactPhone { get; set; } = SecretProvider.NewsContactPhone;
        [DataMember]
        [Display(Name = "Порт", Description = "Порт")]
        public int Port { get; set; } = 25;
        [DataMember]
        [Display(Name = "Use SSL", Description = "Использовать шифрование")]
        public bool UseSSL { get; set; } = true;
        protected override void CreateSomething()
        {
            //заглушка
        }
    }
}
