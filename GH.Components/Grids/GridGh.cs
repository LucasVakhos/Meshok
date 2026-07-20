using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Registrator;
using DevExpress.XtraGrid.Views.Base;
using System.ComponentModel;
namespace GH.Components
{
    [ToolboxItem(true)]
    public class GridGh : GridControl, ISavedControl
    {
        private bool _saveLayout = true;
    public override object DataSource
        {
            get => base.DataSource;
            set
            {
                base.DataSource = value;
                if (value is GH.Components.DataSource data && data.Grid != this)
                    data.Grid = this;
            }
        }

    public GridGh()
        {
            SetStandardProperties();
        }
    private void SetStandardProperties()
        {
            Dock = DockStyle.Fill;
            EmbeddedNavigator.Buttons.Append.Visible = false;
            EmbeddedNavigator.Buttons.CancelEdit.Visible = false;
            EmbeddedNavigator.Buttons.Edit.Visible = false;
            EmbeddedNavigator.Buttons.EndEdit.Visible = false;
            EmbeddedNavigator.Buttons.Remove.Visible = false;
            EmbeddedNavigator.Visible = false;
        }
    public override void BeginInit()
        {
            base.BeginInit();
        }
    public override void EndInit()
        {
            base.EndInit();
        }
        [GHProperty, DefaultValue(true)]
        public bool SaveLayout { get => _saveLayout; set => _saveLayout = value; }
    public void LoadControls()
        {
            if (!_saveLayout)
                return;
            string file_name = RunContext.GetConfigsPath(this);
            string key = Path.GetFileNameWithoutExtension(file_name);
            LB.Libs.IniFile ini = LB.Libs.IniFile.DefaultInstance();
            string layout = ini.Read("Layouts", key);
            if (string.IsNullOrEmpty(layout) && File.Exists(file_name))
            {
                layout = Convert.ToBase64String(File.ReadAllBytes(file_name));
                ini.Write("Layouts", key, layout);
                ini.Save();
            }
            if (!string.IsNullOrEmpty(layout))
            {
                using MemoryStream stream = new MemoryStream(Convert.FromBase64String(layout));
                MainView.RestoreLayoutFromStream(stream);
            }
        }
    public void SaveControls()
        {
            if (!_saveLayout)
                return;
            string file_name = RunContext.GetConfigsPath(this);
            string key = Path.GetFileNameWithoutExtension(file_name);
            using MemoryStream stream = new MemoryStream();
            MainView.SaveLayoutToStream(stream);
            LB.Libs.IniFile ini = LB.Libs.IniFile.DefaultInstance();
            ini.Write("Layouts", key, Convert.ToBase64String(stream.ToArray()));
            ini.Save();
        }
    protected override BaseView CreateDefaultView()
        {
            return CreateView(nameof(ViewGh));
        }
    protected override void RegisterAvailableViewsCore(InfoCollection collection)
        {
            base.RegisterAvailableViewsCore(collection);
            collection.Add(new ViewGhInfoRegistrator());
            collection.Add(new HLViewGhInfoRegistrator());
        }
    protected override void OnGotFocus(EventArgs e)
        {
            if (!IsDesignMode)
            {
                if (DataSource is DataSource dataSource)
                {
                    if (MainView is HighlightingViewGh highlightingView)
                    {
                        highlightingView.SelectHighlightr(dataSource.Repository.ConcreteType);
                    }
                }
            }
            base.OnGotFocus(e);
        }
    protected override void OnLostFocus(EventArgs e)
        {
            if (!IsDesignMode)
            {
                if (DataSource is DataSource dataSource)
                {
                    if (MainView is HighlightingViewGh highlightingView)
                    {
                        highlightingView.DeSelectHighlightr();
                    }
                }
            }
            base.OnLostFocus(e);
        }
    protected override void OnEnter(EventArgs e)
        {
            //if (!IsDesignMode)
            //{
            //    if (DataSource is DataSource source)
            //    {
            //        AppContext.RegDataSource(source);
            //    }
            //}
            base.OnEnter(e);
        }
    }
}
