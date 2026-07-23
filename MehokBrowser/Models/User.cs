using UpdatableProperty = LB.Libs.UpdatablePropertyAttribute;
namespace MeshokBrowser.Models
{
    public class User : BaseUser
    {
        [UpdatableProperty(Caption = "�����", ToolTip = "�����", Group = "������ ������������")]
        public override string Login
        {
            get { return Name; }
            set { Name = value; }
        }
    }
}