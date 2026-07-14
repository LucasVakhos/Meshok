using GH.Components;
using MeshokBrowser.NHibernate;
using System.Linq;
using System.Windows.Forms;
namespace MeshokBrowser.Workers
{
    public class TaskUnionDeals : ParsingTask<NeedUnion>
    {
        public TaskUnionDeals()
        {
            CaptionOfTask = "Объединение сделок";
        }
        protected override void WorkWithDocument()
        {
            if (TaskFinished)
                return;
            currentObject.ParsingSaccess = false;
            int cnt = currentObject.Packet.OrderLines.Count;
            foreach (GhDomElement row in doc.GetElementsByTagName("tr"))
            {
                if (row.InnerHtml.Contains(@"<td class=""list"" style=""padding: 4px;""><a href=""/item/"))
                {
                    foreach (OrderLine deal in currentObject.Packet.OrderLines)
                    {
                        if (row.InnerHtml.Contains("/deal/" + deal.deal_id))
                        {
                            deal.HtmlRow = row;
                            deal.CheckRow();
                            //deal.Union = true;
                            cnt--;
                            break;
                        }
                    }
                    if (cnt == 0)
                        goto apply_lbl;
                }
            }
        apply_lbl:
            GhDomElement div = doc.GetElementById("saleBAction") as GhDomElement;
            foreach (GhSelectElement sel in div.GetElementsByTagName("select").OfType<GhSelectElement>())
            {
                if (sel.Name == "do_work")
                {
                    sel.SetAttribute("value", "J");
                    Application.DoEvents();
                    foreach (GhInputElement btn in div.GetElementsByTagName("input").OfType<GhInputElement>())
                    {
                        if (btn.GetAttribute("value") == "Применить")
                        {
                            TaskFinished = true;
                            btn.Click();
                            Application.DoEvents();
                            currentObject.ParsingSaccess = true;
                            return;
                        }
                    }
                }
            }
        }
    }
}
