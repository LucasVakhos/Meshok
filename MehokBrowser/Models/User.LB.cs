namespace MeshokBrowser.Models;

public class User : LB.Libs.BaseUser
{
    [LB.Libs.UpdatableProperty(Caption = "Логин", ToolTip = "Логин", Group = "Данные пользователя")]
    public override string Login
    {
        get => Name;
        set => Name = value;
    }
}
