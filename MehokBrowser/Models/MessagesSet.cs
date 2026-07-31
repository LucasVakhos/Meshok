using System.ComponentModel.DataAnnotations;
using BaseEntity = LB.Libs.BaseEntity;
namespace MeshokBrowser.Models;

public class MessagesSet : BaseEntity
{
    [Display(Name = "������ � ����")]
    public virtual int zsc_cs_id { get; set; }
    [Display(Name = "������ �� �����")]
    public virtual int zsc_zs_id { get; set; }
    [Display(Name = "������ ��������")]
    public virtual int? zsc_md_id { get; set; }
    [Display(Name = "��������� ����")]
    public virtual int zsc_case { get; set; }
    [Display(Name = "����� ���������")]
    public virtual string zsc_message { get; set; }
}
