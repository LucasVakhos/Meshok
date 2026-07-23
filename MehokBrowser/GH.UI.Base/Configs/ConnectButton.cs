using DevExpress.XtraEditors;

namespace MehokBrowser.UI.Config
{
    /// <summary>Кнопка проверки подключения — заглушка для миграции из GH.Components.</summary>
    internal class ConnectButton : SimpleButton
    {
        public ConnectButton()
        {
            Text = "Проверить подключение";
            Name = "btnConnect";
        }
    }
}
