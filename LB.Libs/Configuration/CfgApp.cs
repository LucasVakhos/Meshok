using DevExpress.LookAndFeel;
using DevExpress.XtraNavBar;
using System.Runtime.Serialization;

namespace LB.Libs;

/// <summary>
/// Shared application and window settings stored in the executable INI.
/// </summary>
public class CfgApp : CfgCore
{
    private const string DefaultKeyName = "Application.key";
    private const string DefaultExportPath = "Exports";
    private const string DefaultCfgPath = "Configs";
    private const string DefaultLogsPath = "Logs";
    private const string DefaultDownloadPath = "DownloadWebFolder";
    private const string DefaultImportPath = "ImportSources";
    private const string DefaultOrdersPath = "Orders";

    private static readonly DefaultLookAndFeel LookAndFeel = new();
    private Form? _form;

    public event Action<Form>? OnRestore;

    [DataMember]
    [UpdatableProperty(Caption = "Version", ToolTip = "Текущая версия программы", ReadOnly = true)]
    public string ProductVersion { get; set; } = "0.0.0.0";

    [DataMember]
    public string SkinName { get; set; } = "DevExpress Style";

    [DataMember]
    [UpdatableProperty(Caption = "Key Name", ToolTip = "Ключ приложения", Group = "Основная", Required = true)]
    public string KeyName { get; set; } = DefaultKeyName;

    [DataMember]
    [UpdatableProperty(Caption = "Configs Path", ToolTip = "Место хранения конфигурации", Group = "Основная", Required = true)]
    public string CfgPath { get; set; } = DefaultCfgPath;

    [DataMember]
    [UpdatableProperty(Caption = "Logs Path", ToolTip = "Место хранения лог-файлов", Group = "Основная", Required = true, Default = DefaultLogsPath)]
    public string LogsPath { get; set; } = DefaultLogsPath;

    [DataMember]
    [UpdatableProperty(Caption = "Export Path", ToolTip = "Место создания файлов экспорта", Group = "Основная", Required = true, Default = DefaultExportPath)]
    public string ExportPath { get; set; } = DefaultExportPath;

    [DataMember]
    [UpdatableProperty(Caption = "Download Folder", ToolTip = "Папка загрузки обновлений", Group = "Дополнительная", Required = true, Default = DefaultDownloadPath)]
    public string DownloadWebFolder { get; set; } = DefaultDownloadPath;

    [DataMember]
    [UpdatableProperty(Caption = "Import Folder", ToolTip = "Папка импорта", Group = "Дополнительная", Required = true, Default = DefaultImportPath)]
    public string ImportSourceFolder { get; set; } = DefaultImportPath;

    [DataMember]
    [UpdatableProperty(Caption = "Orders Folder", ToolTip = "Папка заказов", Group = "Дополнительная", Required = true, Default = DefaultOrdersPath)]
    public string OrdersFolder { get; set; } = DefaultOrdersPath;

    [DataMember]
    public Size FormSize { get; set; } = new(800, 600);

    [DataMember]
    public Point Location { get; set; } = Point.Empty;

    [DataMember]
    public FormWindowState WindowState { get; set; } = FormWindowState.Normal;

    [DataMember]
    public int NavBarExpandedWidth { get; set; } = 185;

    [DataMember]
    public NavPaneState NavBarPaneState { get; set; } = NavPaneState.Expanded;

    public CfgApp()
    {
        EnsureLoaded();
        LookAndFeel.LookAndFeel.SkinName = SkinName;
    }

    public void AttachForm(Form? form)
    {
        if (ReferenceEquals(_form, form))
            return;

        if (_form is not null)
        {
            _form.Load -= MainForm_Load;
            _form.FormClosing -= MainForm_FormClosing;
            _form.ResizeEnd -= MainForm_Resize;
        }

        _form = form;
        if (_form is not null)
        {
            _form.Load += MainForm_Load;
            _form.FormClosing += MainForm_FormClosing;
            _form.ResizeEnd += MainForm_Resize;
        }
    }

    public void Restore(Form form)
    {
        ArgumentNullException.ThrowIfNull(form);

        Point location = Location;
        Size size = FormSize;
        if (size.Width <= 0 || size.Height <= 0)
            size = new Size(800, 600);

        if (location.X <= 0 && location.Y <= 0)
        {
            Rectangle workingArea = Screen.GetWorkingArea(form);
            location = new Point(
                workingArea.Left + Math.Max(0, (workingArea.Width - size.Width) / 2),
                workingArea.Top + Math.Max(0, (workingArea.Height - size.Height) / 2));
        }

        form.StartPosition = FormStartPosition.Manual;
        form.Location = location;
        form.Size = size;
        if (WindowState != FormWindowState.Minimized && form.ShowInTaskbar)
            form.WindowState = WindowState;

        LookAndFeel.LookAndFeel.SkinName = SkinName;
        OnRestore?.Invoke(form);
    }

    public bool TestConnection()
    {
        return !string.IsNullOrWhiteSpace(DownloadWebFolder);
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        if (sender is Form form)
            Restore(form);
    }

    private void MainForm_Resize(object? sender, EventArgs e)
    {
        if (sender is not Form form || form.WindowState != FormWindowState.Normal)
            return;

        FormSize = form.Size;
        Location = form.Location;
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_form is not null)
        {
            WindowState = _form.WindowState;
            SkinName = LookAndFeel.LookAndFeel.ActiveSkinName;
        }

        Save(true);
        IniHelper.SaveAll();
    }
}
