using Common;
using GH.Components;
using MeshokBrowser.Models;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
namespace MeshokBrowser.Workers
{
    public class TaskCloseDeals : ParsingTask<OrderLine>
    {
        public TaskCloseDeals()
        {
            CaptionOfTask = "Закрытие сделок";
        }
        protected override string GetFullUrl()
        {
            if (currentObject.CloseUrl != "")
                return currentObject.CloseUrl;
            return currentObject.FullUrl;
        }
        protected override void WorkWithDocument()
        {
            currentObject.ParsingSaccess = false;
            GhTextAreaElement memo = doc.GetElementsByTagName("textarea").Where(x => x.GetAttribute("name") == "comment").FirstOrDefault() as GhTextAreaElement;
            if (memo == null)
                return;
            CheckMesage info = currentObject.Infos.FirstOrDefault();
            if (info == null)
                return;
            string mess = "OK, Thanks !!! :o)";
            if (currentObject.CurrStatus == OrderStatus.DealCanceled)
            {
                mess = info.cs_name.Trim();
            }
            memo.Focus();
            memo.Value = mess;
            Application.DoEvents();
            foreach (GhInputElement item in doc.GetElementsByTagName("input").OfType<GhInputElement>())
            {
                if (item.Name == "did" && ((currentObject.CurrStatus == OrderStatus.DealOK && item.GetAttribute("value") == "Y") ||
                    (currentObject.CurrStatus == OrderStatus.DealCanceled && item.GetAttribute("value") == "O")))
                {
                    item.Focus();
                    item.Click();
                    Application.DoEvents();
                }
                else if (item.Name == "fault" && currentObject.CurrStatus == OrderStatus.DealCanceled && item.GetAttribute("value") == "my")
                {
                    if (info != null)
                        switch (info.cs_id)
                        {
                            case -2:
                            case -3:
                                break;
                            default:
                                continue;
                        }
                    item.Focus();
                    item.Click();
                    Application.DoEvents();
                }
                else if (item.Name == "fault" && currentObject.CurrStatus == OrderStatus.DealCanceled && item.GetAttribute("value") == "counterpart")
                {
                    if (info != null)
                        switch (info.cs_id)
                        {
                            case -2:
                            case -3:
                                continue;
                            default:
                                break;
                        }
                    item.Focus();
                    item.Click();
                    Application.DoEvents();
                }
                else if (item.Name == "stars" && ((currentObject.CurrStatus == OrderStatus.DealOK && item.GetAttribute("value") == "5") ||
                    (currentObject.CurrStatus == OrderStatus.DealCanceled && item.GetAttribute("value") == "3")))
                {
                    item.Focus();
                    item.Click();
                    Application.DoEvents();
                }
                else if (item.Name == "save")
                {
                    item.Focus();
                    if (currentObject.CurrStatus == OrderStatus.DealCanceled)
                    {
                        DoWaitUserReaction();
                    }
                    else
                    {
                        Application.DoEvents();
                        WaitForOperationEnd = true;
                        item.Click();
                        DoWaitForOperationEnd();
                        Thread.Sleep(1000);
                    }
                    return;
                }
            }
        }
        protected override void EndParseCurrent()
        {
            currentObject.ParsingSaccess = true;
            GhDomElement stat = doc.GetElementsByClassName("d_s_e_i").FirstOrDefault() as GhDomElement;
            if (stat == null)
            {
                currentObject.deal_status = currentObject.CurrStatus.GetDisplayValue();
            }
            WaitForOperationEnd = false;
        }
    }
}
