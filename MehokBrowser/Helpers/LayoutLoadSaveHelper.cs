using DevExpress.XtraGrid;
using DevExpress.XtraLayout;
using System.IO;
using System.Windows.Forms;
namespace MeshokBrowser
{
    public static class LayoutLoadSaveHelper
    {
        public static void LoadControls(Control parent, string prefix)
        {
            string s_file_name = Application.StartupPath + @"\layouts\" + prefix + "{0}.xml";
            string file_name;
            foreach (Control ctrl in parent.Controls)
            {
                ILayoutControl lc = ctrl as ILayoutControl;
                if (lc != null)
                {
                    file_name = string.Format(s_file_name, ctrl.Name);
                    RestoreLayout(file_name, stream => lc.RestoreLayoutFromStream(stream));
                    LoadControls(ctrl, string.Concat(prefix, ".", ctrl.Name));
                    continue;
                }
                GridControl gc = ctrl as GridControl;
                if (gc != null)
                {
                    file_name = string.Format(s_file_name, string.Concat(".", gc.Name, ".", gc.MainView.Name));
                    RestoreLayout(file_name, stream => gc.MainView.RestoreLayoutFromStream(stream));
                    continue;
                }
                //VGridControl vgc = ctrl as VGridControl;
                //if (vgc != null)
                //{
                //    file_name = string.Format(s_file_name, string.Concat(".", vgc.Name));
                //    if (File.Exists(file_name))
                //        vgc.RestoreLayoutFromXml(file_name);
                //    continue;
                //}
            }
        }
        public static void SaveControls(Control parent, string prefix)
        {
            string s_file_name = Application.StartupPath + @"\layouts\" + prefix + "{0}.xml";
            string file_name;
            foreach (Control ctrl in parent.Controls)
            {
                ILayoutControl lc = ctrl as ILayoutControl;
                if (lc != null)
                {
                    file_name = string.Format(s_file_name, ctrl.Name);
                    SaveLayout(file_name, stream => lc.SaveLayoutToStream(stream));
                    SaveControls(ctrl, string.Concat(prefix, ".", ctrl.Name));
                    continue;
                }
                GridControl gc = ctrl as GridControl;
                if (gc != null)
                {
                    file_name = string.Format(s_file_name, string.Concat(".", gc.Name, ".", gc.MainView.Name));
                    SaveLayout(file_name, stream => gc.MainView.SaveLayoutToStream(stream));
                    continue;
                }
                //VGridControl vgc = ctrl as VGridControl;
                //if (vgc != null)
                //{
                //    file_name = string.Format(s_file_name, string.Concat(".", vgc.Name));
                //    Directory.CreateDirectory(Path.GetDirectoryName(file_name));
                //    vgc.SaveLayoutToXml(file_name);
                //    continue;
                //}
            }
        }

        private static void RestoreLayout(string legacyPath, Action<Stream> restore)
        {
             LB.Libs.IniFile ini = LB.Libs.IniFile.DefaultInstance();
            string key = Path.GetFileNameWithoutExtension(legacyPath);
            string layout = ini.Read("Layouts", key);
            if (string.IsNullOrEmpty(layout) && File.Exists(legacyPath))
            {
                layout = Convert.ToBase64String(File.ReadAllBytes(legacyPath));
                ini.Write("Layouts", key, layout);
                ini.Save();
            }
            if (string.IsNullOrEmpty(layout))
                return;
            using MemoryStream stream = new MemoryStream(Convert.FromBase64String(layout));
            restore(stream);
        }

        private static void SaveLayout(string legacyPath, Action<Stream> save)
        {
            using MemoryStream stream = new MemoryStream();
            save(stream);
             LB.Libs.IniFile ini = LB.Libs.IniFile.DefaultInstance();
            ini.Write("Layouts", Path.GetFileNameWithoutExtension(legacyPath), Convert.ToBase64String(stream.ToArray()));
            ini.Save();
        }
    }
}
