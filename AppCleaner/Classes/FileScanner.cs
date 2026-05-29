//FileScanner
#nullable enable
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraLayout;
using DevExpress.XtraLayout.Utils;
using System.Xml.Linq;

namespace AppCleaner;

public partial class FileScanner : XtraUserControl
{
    private const int UiUpdateIntervalMs = 500;
    private const int BackupMaxAttempts = 10_000;

    private static readonly StringComparer PathComparer = StringComparer.OrdinalIgnoreCase;

    // Папки, которые нельзя сканировать ни при каких условиях.
    // Они исключаются и при проектном режиме, и при fallback-обходе папок.
    private static readonly HashSet<string> IgnoredFolders = new(PathComparer)
    {
        "bin",
        "obj",
        ".vs",
        ".git",
        "node_modules"
    };

    // Файлы, которые нельзя изменять/удалять.
    private static readonly HashSet<string> IgnoredFiles = new(PathComparer)
    {
        "appsettings.json",
        "web.config"
    };

    private readonly FileScannerStore _store = new();
    private readonly System.Windows.Forms.Timer _uiTimer = new();
    private readonly string _iniFilePath = Path.Combine(Application.StartupPath, "FileScanner.ini");

    private CancellationTokenSource? _operationCts;
    private bool _suppressFolderEditValueChanged;

    private ComboToDoItems TodoType => _todoType;
    private ComboToDoItems _todoType;
    private ComboNetItems NETType => _netType;
    private ComboNetItems _netType;

    public FileScanner()
    {
        InitializeComponent();
        InitializeBindings();
        InitializeComboBoxes();
        InitializeControls();
        InitializeUiTimer();
        SetupLayouts();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        _store.LoadFromIni(_iniFilePath);
        openFileDlg.InitialDirectory = _store.SearchFolder;
        openFolderDlg.InitialDirectory = _store.SearchFolder;

        _todoType = _store.SelectedActionIndex < 0
            ? default
            : (ComboToDoItems)_store.SelectedActionIndex;

        cboNET.SelectedIndex = Enum.IsDefined(typeof(ComboNetItems), _store.NETVersion)
            ? (int)_store.NETVersion
            : 0;
        _netType = _store.NETVersion;

        RefreshPathComboBoxes();
        SetupLayouts();
        SyncPathEditorFromStore();
        RefreshUi();
    }

    private void InitializeBindings()
    {
        bsFileScanner.DataSource = _store;

        txtFind.DataBindings.Add("EditValue", bsFileScanner,
            nameof(FileScannerStore.FindText), true,
            DataSourceUpdateMode.OnPropertyChanged);

        txtReplace.DataBindings.Add("EditValue", bsFileScanner,
            nameof(FileScannerStore.ReplaceText), true,
            DataSourceUpdateMode.OnPropertyChanged);

        cboSearchExt.DataBindings.Add("EditValue", bsFileScanner,
            nameof(FileScannerStore.SearchPattern), true,
            DataSourceUpdateMode.OnPropertyChanged);

        cboSelectToDo.DataBindings.Add("SelectedIndex", bsFileScanner,
            nameof(FileScannerStore.SelectedActionIndex), true,
            DataSourceUpdateMode.OnPropertyChanged);

        cboDRY_RUN.DataBindings.Add("SelectedIndex", bsFileScanner,
            nameof(FileScannerStore.DryRunIndex), true,
            DataSourceUpdateMode.OnPropertyChanged);

        btnBegin.DataBindings.Add("Enabled", bsFileScanner,
            nameof(FileScannerStore.BeginEnabled), true,
            DataSourceUpdateMode.Never);

        btnCancel.DataBindings.Add("Enabled", bsFileScanner,
            nameof(FileScannerStore.CancelEnabled), true,
            DataSourceUpdateMode.Never);

        progressBar.Properties.Maximum = Math.Max(1, _store.ProgressMaximum);

        _store.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(FileScannerStore.ProgressMaximum) &&
                !string.IsNullOrEmpty(e.PropertyName))
            {
                return;
            }

            SafeInvoke(UpdateProgressMaximum);
        };
    }

    private void InitializeComboBoxes()
    {
        RefreshPathComboBoxes();

        cboSearchExt.Properties.Items.Clear();
        cboSearchExt.Properties.Items.AddRange(FilePatterns.AllPatterns);

        cboSelectToDo.Properties.Items.Clear();

        foreach (ComboToDoItems item in Enum.GetValues(typeof(ComboToDoItems)))
            cboSelectToDo.Properties.Items.Add(GetDisplayName(item));

        cboSelectToDo.SelectedIndex = 0;
        _todoType = cboSelectToDo.SelectedIndex < 0
            ? default
            : (ComboToDoItems)cboSelectToDo.SelectedIndex;

        cboNET.Properties.Items.Clear();

        foreach (ComboNetItems item in Enum.GetValues(typeof(ComboNetItems)))
            cboNET.Properties.Items.Add(GetDisplayName(item));

        cboNET.SelectedIndex = 0;
        _netType = cboNET.SelectedIndex < 0
            ? default
            : (ComboNetItems)cboNET.SelectedIndex;
    }

    private void RefreshPathComboBoxes()
    {
        var pathes = _store.Pathes.ToArray();

        cboSearchFolder.Properties.Items.Clear();
        cboSearchFolder.Properties.Items.AddRange(pathes);

        cboPlaceFolder.Properties.Items.Clear();
        cboPlaceFolder.Properties.Items.AddRange(pathes);
    }

    private void InitializeControls()
    {
        openFileDlg.InitialDirectory = Application.StartupPath;
        openFolderDlg.InitialDirectory = Application.StartupPath;
        _store.RefreshCommandStates();
    }

    private void InitializeUiTimer()
    {
        _uiTimer.Interval = UiUpdateIntervalMs;
        _uiTimer.Tick += (_, _) => RefreshUi();
    }

    internal async Task StartAsync()
    {
        _store.SaveToIni(_iniFilePath);

        if (!ValidateSelected())
            return;

        BeginOperation();

        try
        {
            LogOperationHeader();
            await RunSelectedOperationAsync(CurrentToken).ConfigureAwait(true);
            AddToLog("Операция завершена.");
        }
        catch (OperationCanceledException)
        {
            AddToLog("Операция отменена пользователем.");
        }
        catch (Exception ex)
        {
            AddToLog($"[Критическая ошибка] {ex.Message}");
            AddToLog(ex.ToString());
        }
        finally
        {
            EndOperation();
        }
    }

    public void Cancel()
    {
        _operationCts?.Cancel();
        _store.RefreshCommandStates();
    }

    private CancellationToken CurrentToken => _operationCts?.Token ?? CancellationToken.None;

    private void BeginOperation()
    {
        _store.AddPathes(cboSearchFolder.EditValue?.ToString());
        _store.AddPathes(cboPlaceFolder.EditValue?.ToString());
        RefreshPathComboBoxes();

        _todoType = _store.SelectedActionIndex < 0
            ? default
            : (ComboToDoItems)_store.SelectedActionIndex;

        _uiTimer.Start();
        _store.IsWorking = true;

        var newCts = new CancellationTokenSource();
        var oldCts = Interlocked.Exchange(ref _operationCts, newCts);

        oldCts?.Cancel();
        oldCts?.Dispose();

        _store.Reset();

        EnsureDefaultTokenIfNeeded();
        _store.RefreshCommandStates();
        RefreshUi();
    }

    private void EndOperation()
    {
        var oldCts = Interlocked.Exchange(ref _operationCts, null);

        oldCts?.Cancel();
        oldCts?.Dispose();

        _store.IsWorking = false;
        _store.RefreshCommandStates();

        _uiTimer.Stop();
        RefreshUi();
    }

    private Task RunSelectedOperationAsync(CancellationToken cancellationToken)
    {
        return TodoType switch
        {
            ComboToDoItems.DeleteEmpty
                or ComboToDoItems.DeleteRegionRows
                or ComboToDoItems.FindAndReplace
                => Task.Run(() => ScanAndProcessFiles(cancellationToken), cancellationToken),
            ComboToDoItems.FindValueOrClassAddScaveToProject
                => Task.Run(() => FindAndAddClassToProject(cancellationToken), cancellationToken),
            ComboToDoItems.ClearNameSpace
                => Task.Run(() => NormalizeNamespacesInDirectory(_store.DryRun, cancellationToken), cancellationToken),
            ComboToDoItems.CollectAllNameSpaces
                => Task.Run(() => CollectAllNamespaces(cancellationToken), cancellationToken),
            ComboToDoItems.CollectUsingPackages
                => Task.Run(() => CollectRequiredPackagesFromUsings(cancellationToken), cancellationToken),
            ComboToDoItems.DeleteBakFiles
                => Task.Run(() => DeleteBakFiles(cancellationToken), cancellationToken),
            ComboToDoItems.DeleteNonProjectFiles
                => Task.Run(() => RemoveNonProjectFiles(_store.DryRun, cancellationToken), cancellationToken),
            ComboToDoItems.SyncProjectFileWithSample
                => Task.Run(() => SyncProjectFileWithSample(cancellationToken), cancellationToken),
            ComboToDoItems.ConvertOldCsprojToSdkStyle
                => Task.Run(() => ConvertOldCsprojToSdkStyle(_store.ProjectFile, _store.SampleProjectFile), cancellationToken),
            ComboToDoItems.TranslateEnglishToRussian
                => Task.Run(() => TranslateEnglishInFolderAsync(cancellationToken), cancellationToken),
            _ => Task.CompletedTask
        };
    }

    private ComboToDoItems GetSelectedAction()
    {
        return _store.SelectedActionIndex < 0
            ? default
            : (ComboToDoItems)_store.SelectedActionIndex;
    }

    private void LogOperationHeader()
    {
        switch (TodoType)
        {
            case ComboToDoItems.DeleteNonProjectFiles:
                AddToLog($"Файл проекта: {_store.ProjectFile}");
                break;
            case ComboToDoItems.SyncProjectFileWithSample:
                AddToLog($"Файл проекта: {_store.ProjectFile}");
                AddToLog($"Образец файла проекта: {_store.SampleProjectFile}");
                break;
            default:
                AddToLog($"Папка: {_store.SearchFolder}");
                break;
        }


        if (TodoType != ComboToDoItems.FindValueOrClassAddScaveToProject)
            AddToLog($"Маска файлов: {_store.SearchPattern}");

        if (TodoType == ComboToDoItems.FindValueOrClassAddScaveToProject)
            AddToLog($"Папка назначения: {_store.PlaceFolder}");
    }

    #region UI events

    private void cboSearchExt_EditValueChanged(object sender, EventArgs e)
    {
        _store.SearchPattern = cboSearchExt.EditValue?.ToString() ?? "*.cs";
    }

    private void cboSelectToDo_SelectedIndexChanged(object sender, EventArgs e)
    {
        _store.SelectedActionIndex = cboSelectToDo.SelectedIndex;
        _todoType = cboSelectToDo.SelectedIndex < 0
            ? default
            : (ComboToDoItems)cboSelectToDo.SelectedIndex;

        SetupLayouts();
        SyncPathEditorFromStore();
        _store.RefreshCommandStates();
    }

    private void cboDRY_RUN_EditValueChanged(object sender, EventArgs e)
    {
        _store.DryRunIndex = cboDRY_RUN.SelectedIndex;
    }

    private async void btnBegin_Click(object sender, EventArgs e)
    {
        try
        {
            await StartAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            AddToLog($"[Критическая ошибка] {ex.Message}");
        }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        Cancel();
    }

    private void maskSymbol_EditValueChanged(object sender, EventArgs e)
    {
        _store.RefreshCommandStates();
    }

    private void txtFolder_EditValueChanged(object sender, EventArgs e)
    {
        UpdatePathsFromEditor();
        _store.RefreshCommandStates();
    }

    private void searchFolder_EditValueChanged(object sender, EventArgs e)
    {
        UpdatePathsFromEditor();
        _store.RefreshCommandStates();
    }

    private void searchFolder_BtnClick(object sender, ButtonPressedEventArgs e)
    {
        SelectPath(sender as ButtonEdit);
    }

    private void searchFolder_Click(object sender, EventArgs e)
    {
        SelectPath(sender as ButtonEdit);
    }

    #endregion

    #region UI helpers

    private void SelectPath(ButtonEdit selector)
    {
        if (selector == null)
            return;

        string? selectedPath = ShowPathDialog();

        if (!string.IsNullOrWhiteSpace(selectedPath))
            selector.EditValue = selectedPath;
    }

    private string? ShowPathDialog()
    {
        bool useFileDialog =
            TodoType == ComboToDoItems.DeleteNonProjectFiles ||
            TodoType == ComboToDoItems.SyncProjectFileWithSample;

        if (useFileDialog)
        {
            return openFileDlg.ShowDialog() == DialogResult.OK
                ? openFileDlg.FileName
                : null;
        }

        return openFolderDlg.ShowDialog() == DialogResult.OK
            ? openFolderDlg.SelectedPath
            : null;
    }

    private void SyncPathEditorFromStore()
    {
        _suppressFolderEditValueChanged = true;


        try
        {
            cboSearchFolder.EditValue = TodoType switch
            {
                ComboToDoItems.DeleteNonProjectFiles => _store.ProjectFile,
                ComboToDoItems.SyncProjectFileWithSample => _store.ProjectFile,
                ComboToDoItems.ConvertOldCsprojToSdkStyle => _store.ProjectFile,
                _ => _store.SearchFolder
            };

            cboPlaceFolder.EditValue = TodoType switch
            {
                ComboToDoItems.SyncProjectFileWithSample => _store.SampleProjectFile,
                ComboToDoItems.ConvertOldCsprojToSdkStyle => _store.SampleProjectFile,
                ComboToDoItems.FindValueOrClassAddScaveToProject => _store.PlaceFolder,
                _ => string.Empty
            };
        }
        finally
        {
            _suppressFolderEditValueChanged = false;
        }
    }

    private void UpdatePathsFromEditor()
    {
        if (_suppressFolderEditValueChanged)
            return;

        string searchValue = cboSearchFolder.EditValue?.ToString() ?? string.Empty;
        string placeValue = cboPlaceFolder.EditValue?.ToString() ?? string.Empty;

        switch (TodoType)
        {
            case ComboToDoItems.DeleteNonProjectFiles:
                _store.ProjectFile = searchValue;
                break;

            case ComboToDoItems.SyncProjectFileWithSample:
            case ComboToDoItems.ConvertOldCsprojToSdkStyle:
                _store.ProjectFile = searchValue;
                _store.SampleProjectFile = placeValue;
                break;

            case ComboToDoItems.FindValueOrClassAddScaveToProject:
                _store.SearchFolder = searchValue;
                _store.PlaceFolder = placeValue;
                break;

            default:
                _store.SearchFolder = searchValue;
                break;
        }

        _store.RefreshCommandStates();
    }
    private void SetupLayouts()
    {

        var attr = TodoType.GetAttribute<ComboItemAttribute>();

        bool isFindReplace = TodoType == ComboToDoItems.FindAndReplace;
        bool isFindAdd = TodoType == ComboToDoItems.FindValueOrClassAddScaveToProject;
        bool isSync = TodoType == ComboToDoItems.SyncProjectFileWithSample;
        bool isConvert = TodoType == ComboToDoItems.ConvertOldCsprojToSdkStyle;
        bool isProjectMode = isFindAdd || isSync || isConvert;


        cboSearchFolder.Properties.NullValuePrompt = isConvert
            ? "Установите старый файл проекта..."
            : isProjectMode
                ? "Установите файл проекта для сравнения..."
                : "Установите папку для сканирования...";

        cboPlaceFolder.Properties.NullValuePrompt = isConvert
            ? "Установите путь нового SDK-style проекта..."
            : isSync
                ? "Установите образец файла проекта..."
                : "Установите папку куда копировать найденное...";
        lcSearchFolder.Text = attr?.SearchLabel ?? "Cканировать папку:";
        lcPlaceFolder.Text = attr?.PlaceLabel ?? "Папка для найденного:";

        SetVisibility(lcNET_Version, isConvert);
        SetVisibility(emptySearchExt, !isProjectMode);
        SetVisibility(lcSearchExt, !isProjectMode);
        SetVisibility(lgFindReplace, isFindReplace || isFindAdd || isConvert);
        SetVisibility(lcDRY_RUN, TodoType is ComboToDoItems.ClearNameSpace or ComboToDoItems.DeleteNonProjectFiles);
        SetVisibility(lcFind, isFindReplace || isFindAdd);
        SetVisibility(lcReplace, isFindReplace);
        SetVisibility(lcPlaceFolder, isFindAdd || isSync || isConvert);

        cboSearchFolder.Properties.NullValuePrompt = isProjectMode
            ? "Установите файл проекта для сравнения..."
            : "Установите папку для сканирования...";

        cboPlaceFolder.Properties.NullValuePrompt = isSync
            ? "Установите образец файла проекта..."
            : "Установите папку куда копировать найденное...";
    }

    private static void SetVisibility(BaseLayoutItem item, bool visible)
    {
        item.Visibility = visible
            ? LayoutVisibility.Always
            : LayoutVisibility.Never;
    }
    private void UpdateProgressMaximum()
    {
        progressBar.Properties.Maximum = Math.Max(1, _store.ProgressMaximum);

        if (progressBar.Position > progressBar.Properties.Maximum)
            progressBar.Position = progressBar.Properties.Maximum;
    }

    private void SafeInvoke(Action action)
    {
        if (IsDisposed || Disposing)
            return;

        try
        {
            if (InvokeRequired)
            {
                BeginInvoke(action);
                return;
            }

            action();
        }
        catch (InvalidOperationException)
        {
        }
    }

    private void ScrollToEnd()
    {
        if (!logMemo.IsHandleCreated)
            return;

        var text = logMemo.EditValue?.ToString();

        if (string.IsNullOrEmpty(text))
            return;

        logMemo.SelectionStart = text.Length;
        logMemo.ScrollToCaret();
    }

    private void RefreshUi()
    {
        SafeInvoke(() =>
        {
            foundFiles.EditValue = _store.TotalFiles;

            if (!Equals(foundFolders.EditValue, _store.TotalFolders))
                foundFolders.EditValue = _store.TotalFolders;

            progressBar.EditValue = _store.ProgressValue;
            logMemo.EditValue = _store.LogText;

            ScrollToEnd();
        });
    }
    #endregion


    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _uiTimer.Stop();
            _uiTimer.Dispose();

            var oldCts = Interlocked.Exchange(ref _operationCts, null);

            oldCts?.Cancel();
            oldCts?.Dispose();

            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void cboNET_SelectedIndexChanged(object sender, EventArgs e)
    {
        _store.NETVersion = cboNET.SelectedIndex < 0
            ? default
            : (ComboNetItems)cboNET.SelectedIndex;

        _netType = _store.NETVersion;
    }
}
