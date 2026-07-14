using System.ComponentModel.DataAnnotations;
using Common;
using GH.Components;
using GH.Components;
namespace MeshokBrowser.Models
{
    public class CheckMesage : BaseEntity
    {
        [Display(Name = "ID покупателя")]
        [UpdatableProperty(Caption = "ID покупателя")]
        public virtual int c_id { get; set; }
        [Display(Name = "Имя покупателя")]
        [UpdatableProperty(Caption = "Имя покупателя")]
        public virtual string c_name { get; set; }
        [Display(Name = "e-mail покупателя")]
        [UpdatableProperty(Caption = "e-mail покупателя")]
        public virtual string c_email { get; set; }
        [Display(Name = "ID доставки")]
        [UpdatableProperty(Caption = "ID доставки")]
        public virtual int md_id { get; set; } = 1;
        [Display(Name = "Метод доставки")]
        [UpdatableProperty(Caption = "Метод доставки")]
        public virtual string md_name { get; set; }
        [Display(Name = "ID оплаты")]
        [UpdatableProperty(Caption = "ID оплаты")]
        public virtual int mp_id { get; set; } = 1;
        [Display(Name = "Метод оплаты")]
        [UpdatableProperty(Caption = "Метод оплаты")]
        public virtual string mp_name { get; set; }
        [Display(Name = "ID статуса заказа")]
        [UpdatableProperty(Caption = "ID статуса заказа")]
        public virtual int cs_id { get; set; } = 0;
        [Display(Name = "Статус заказа")]
        [UpdatableProperty(Caption = "Статус заказа")]
        public virtual string cs_name { get; set; }
        [Display(Name = "Сумма заказа")]
        [UpdatableProperty(Caption = "Сумма заказа")]
        public virtual double dp_totalsumm { get; set; } = 0;
        [Display(Name = "Расшифровка суммы")]
        [UpdatableProperty(Caption = "Расшифровка суммы")]
        public virtual string dp_totalsumm_info { get; set; }
        [Display(Name = "Заказ упакован")]
        [UpdatableProperty(Caption = "Заказ упакован")]
        public virtual bool dp_packed { get; set; } = false;
        [Display(Name = "Адрес доставки")]
        [UpdatableProperty(Caption = "Адрес доставки")]
        public virtual string md_address { get; set; }
        [Display(Name = "Трекинг № доставки")]
        [UpdatableProperty(Caption = "Трекинг № доставки")]
        public virtual string md_treck_num { get; set; }
        [Display(Name = "Url для № доставки")]
        [UpdatableProperty(Caption = "Url для № доставки")]
        public virtual string md_tracking_url { get; set; }
        public virtual int zsc_case { get; set; } = 0;
        private string _mess_text = "";
        public virtual string mess_text
        {
            get => _mess_text.Replace("  ", " ");
            set
            {
                if (value != null)
                {
                    while (value.Contains("  "))
                        value = value.Replace("  ", " ");
                    _mess_text = value;
                }
            }
        }
        public virtual string ticket
        {
            get
            {
                if (string.IsNullOrEmpty(mess_text.Trim()))
                    return "";
                int hash = mess_text.GetHashCode();
                return $"message ID:{hash.ToString("X")}";
            }
        }
        public virtual MessageCase message_case => (MessageCase)zsc_case;
        private bool _needMessaging = true;
        public virtual bool NeedMessaging
        {
            get
            {
#if !TEST_EMAIL_MESSAGE
                if (_needMessaging && message_case == MessageCase.EmailCheck)
                    _needMessaging = string.IsNullOrEmpty(c_email);
#endif
                return _needMessaging;
            }
            set => _needMessaging = value;
        }
        private OrderLine _orderLine;
        public virtual void SetDealMessagesFor(OrderLine orderLine)
        {
            if (string.IsNullOrEmpty(mess_text.Trim()))
                return;
            _orderLine = orderLine;
            if (orderLine.Client != null && string.IsNullOrEmpty(c_email))
            {
                c_email = orderLine.Client.c_email;
            }
            foreach (Field item in GetFields())
            {
                string s_from = $"#{item.Name.ToLower()}#";
                if (mess_text.Contains(s_from))
                {
                    string s_for = item.Value == null ? "" : item.Value.ToString().Trim();
                    mess_text = mess_text.Replace(s_from, s_for);
                }
            }
            if (string.IsNullOrEmpty(mess_text.Trim()))
                return;
            orderLine.Infos.Add(this);
        }
    }
}