using GH.Components;
using MeshokBrowser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace MeshokBrowser.Workers
{
    public static class PostMessageHelper
    {
        public static void PostingMessage(GhBrowser browser, OrderLine currentObject)
        {
            GhDocument doc = browser.Document;
            GhTextAreaElement memo = doc.GetElementsByTagName("textarea").Where(x => x.Id == "MESS").FirstOrDefault() as GhTextAreaElement;
            if (memo == null)
                return;
            string bodiText = doc.Body.TextContent;
            foreach (CheckMesage item in currentObject.Infos.Where(x => x.NeedMessaging).ToArray())
            {
                if (bodiText.Contains(item.ticket))
                    item.NeedMessaging = false;
            }
            if (!currentObject.HasMessages)
                return;
            currentObject.ParsingSaccess = false;
            foreach (CheckMesage item in currentObject.Infos.Where(x => x.NeedMessaging))
            {
                memo.Focus();
                string mess = item.ticket + "\r\n" + item.mess_text;
                memo.Value = mess;
                Application.DoEvents();
                foreach (GhInputElement btn in doc.GetElementsByTagName("input").OfType<GhInputElement>().Where(x => x.GetAttribute("type") == "submit" && x.GetAttribute("value") == "Отправить"))
                {
                    btn.Focus();
                    Application.DoEvents();
#if !NOT_POST_MESSAGE
                    btn.Form.Submit();
                    Application.DoEvents();
                    while (browser.IsBusy)
                    {
                        Application.DoEvents();
                    }
#endif
                    goto success_lbl;
                }
                success_lbl:
                item.NeedMessaging = false;
            }
        }
    }
}
