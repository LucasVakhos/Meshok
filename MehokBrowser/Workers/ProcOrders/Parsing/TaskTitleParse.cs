using GH.Components;
using MeshokBrowser.Models;
using System.Linq;
namespace MeshokBrowser.Workers
{
    public class TaskTitleParse : ParsingTask<Title>
    {
        public TaskTitleParse()
        {
            CaptionOfTask = "Сканирование лота";
        }
        protected override void WorkWithDocument()
        {
            currentObject.ParsingSaccess = false;
            //Field.GetFields<UpdatablePropertyAttribute>(currentObject, new string[] {"id", "Name"});
            Field[] fields = currentObject.GetFields();
            foreach (GhDomElement table in doc.GetElementsByTagName("table").Where(x => x.GetElementsByTagName("tr").Count() == 10
                && x.GetElementsByTagName("td").Any(td => fields.Any(f => f.CaptionText.Contains(td.TextContent)))))
            {
                foreach (GhDomElement tr in table.GetElementsByTagName("tr"))
                {
                    GhDomElement caption = tr.GetElementsByTagName("td").FirstOrDefault() as GhDomElement;
                    GhDomElement data = tr.GetElementsByTagName("td").Where(x => x.TextContent != caption.TextContent).FirstOrDefault() as GhDomElement;
                    if (caption != null)
                    {
                        foreach (Field field in fields.Where(x => x.Caption == caption.TextContent))
                        {
                            currentObject.AsValue(field.Name, data.TextContent);
                            break;
                        }
                    }
                }
            }
        }
    }
}
