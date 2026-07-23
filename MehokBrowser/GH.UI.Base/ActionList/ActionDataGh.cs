using LB.Libs;

namespace MehokBrowser.Controls
{
    /// <summary>Действие данных — заглушка для совместимости.</summary>
    public class ActionDataGh : ActionGh
    {
        /// <summary>Тип кнопки.</summary>
        public EditTypes ButtonType { get; }

        /// <summary>Создаёт действие данных.</summary>
        public ActionDataGh(EditTypes buttonType)
        {
            ButtonType = buttonType;
            Category = buttonType.ToString();
        }
    }
}
