using System.ComponentModel.DataAnnotations;
using Common;
using Field = LB.Libs.Field;
using BaseEntity = LB.Libs.BaseEntity;
using UpdatablePropertyAttribute = LB.Libs.UpdatablePropertyAttribute;
namespace MeshokBrowser.Models;

public class CheckMesage : BaseEntity
{
    [Display(Name = "ID ����������")]
    [UpdatableProperty(Caption = "ID ����������")]
    public virtual int c_id { get; set; }
    [Display(Name = "��� ����������")]
    [UpdatableProperty(Caption = "��� ����������")]
    public virtual string c_name { get; set; }
    [Display(Name = "e-mail ����������")]
    [UpdatableProperty(Caption = "e-mail ����������")]
    public virtual string c_email { get; set; }
    [Display(Name = "ID ��������")]
    [UpdatableProperty(Caption = "ID ��������")]
    public virtual int md_id { get; set; } = 1;
    [Display(Name = "����� ��������")]
    [UpdatableProperty(Caption = "����� ��������")]
    public virtual string md_name { get; set; }
    [Display(Name = "ID ������")]
    [UpdatableProperty(Caption = "ID ������")]
    public virtual int mp_id { get; set; } = 1;
    [Display(Name = "����� ������")]
    [UpdatableProperty(Caption = "����� ������")]
    public virtual string mp_name { get; set; }
    [Display(Name = "ID ������� ������")]
    [UpdatableProperty(Caption = "ID ������� ������")]
    public virtual int cs_id { get; set; } = 0;
    [Display(Name = "������ ������")]
    [UpdatableProperty(Caption = "������ ������")]
    public virtual string cs_name { get; set; }
    [Display(Name = "����� ������")]
    [UpdatableProperty(Caption = "����� ������")]
    public virtual double dp_totalsumm { get; set; } = 0;
    [Display(Name = "����������� �����")]
    [UpdatableProperty(Caption = "����������� �����")]
    public virtual string dp_totalsumm_info { get; set; }
    [Display(Name = "����� ��������")]
    [UpdatableProperty(Caption = "����� ��������")]
    public virtual bool dp_packed { get; set; } = false;
    [Display(Name = "����� ��������")]
    [UpdatableProperty(Caption = "����� ��������")]
    public virtual string md_address { get; set; }
    [Display(Name = "������� � ��������")]
    [UpdatableProperty(Caption = "������� � ��������")]
    public virtual string md_treck_num { get; set; }
    [Display(Name = "Url ��� � ��������")]
    [UpdatableProperty(Caption = "Url ��� � ��������")]
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
