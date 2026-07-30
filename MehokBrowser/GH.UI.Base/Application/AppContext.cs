using DevExpress.Skins;
using DevExpress.XtraSplashScreen;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using DlgHelper = GH.Components.DlgHelper;
using MeshokBrowser;
namespace MehokBrowser.Application
{
    public class AppContext<T> : RunContext where T : RunContext
    {
        internal static Mutex _mutex;
        static NamedPipeManager namedPipe;
        private static void NamedPipeManager_ReceiveString(string obj)
        {
            switch (obj)
            {
                case NamedPipeManager.ACTIVE_STRING:
                    if (Instance != null && Instance.MainForm != null)
                    {
                        if (Instance.MainForm.WindowState == FormWindowState.Minimized)
                            Instance.MainForm.WindowState = FormWindowState.Normal;
                        Instance.MainForm.Activate();
                    }
                    break;
                default:
                    break;
            }
        }
        public static void RunInstance()
        {
            LB.Libs.IniFile.MigrateLegacyFiles();
            FileVersionInfo.GetVersionInfo(ExeFullName);
            string m_name = "Mutex_" + Path.GetFileNameWithoutExtension(System.Windows.Forms.Application.ExecutablePath);
            _mutex = new Mutex(true, m_name, out bool RuningNow);
            if (RuningNow)
            {
                namedPipe = new NamedPipeManager();
                namedPipe.ReceiveString += NamedPipeManager_ReceiveString;
                namedPipe.Start();
                System.Windows.Forms.Application.EnableVisualStyles();
                System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
                SkinManager.EnableFormSkins();
                SyncContext = new WindowsFormsSynchronizationContext();
                if (CreateAppContext())
                {
                    AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs e)
                    {
                        Trace.TraceError(e.ExceptionObject.ToString());
                        DlgHelper.DlgError(e.ExceptionObject.ToString());
                    };
                    System.Windows.Forms.Application.ThreadException += delegate (Object sender, ThreadExceptionEventArgs e)
                    {
                        Trace.TraceError(e.Exception.ToString());
                        DlgHelper.DlgError(e.Exception.ToString());
                        Environment.Exit(0);
                    };
                }
                else
                    System.Windows.Forms.Application.Exit();
            }
            else
            {
                NamedPipeManager.Write(NamedPipeManager.ACTIVE_STRING);
                System.Windows.Forms.Application.Exit();
            }
            SplashScreenManager.CloseForm(false, 250, AppMainForm, false);
            System.Windows.Forms.Application.Run(Instance);
        }
        private static bool CreateAppContext()
        {
            try
            {
                Assembly.GetEntryAssembly().CreateInstance(typeof(T).FullName);
#pragma warning disable CS0618
                DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
#pragma warning restore CS0618
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                Instance = null;
            }
            return Instance != null;
        }

        public AppContext()
        {
            Instance = this;
            SplashScreenManager.ShowForm(null, GetSplashScreen(), true, true, false, 1000);
            CfgConnection = GetConnectionSetting();

            bool needSettings = false;
            if (CfgConnection != null)
            {
                if (!CfgConnection.TestConnection())
                    needSettings = true;
                else if (!LogIn())
                    needSettings = true;
            }

            MainForm = GetMainForm();

            if (needSettings && MainForm is MainMeshok meshok)
            {
                meshok.BeginInvoke(new Action(() =>
                {
                    meshok.btnProgramSetting.PerformClick();
                }));
            }
        }
        protected bool LogIn()
        {
            using (Form form = GetLoginForm())
            {
                if (form == null || (CfgConnection.AutoEntering && CfgConnection.UserIsValid))
                    return true;
                return form.ShowDialog() == DialogResult.OK;
            }
        }
    }
}
