using GH.Components;
using GH.Components;
namespace MeshokBrowser.Models
{
    public class User : BaseUser
    {
        [UpdatableProperty(Caption = "Логин", ToolTip = "Логин", Group = "Данные пользователя")]
        public override string Login
        {
            get { return Name; }
            set { Name = value; }
        }
    }
}