using System;
using System.IO;
using System.Windows.Forms;

namespace WindroseServerManager.Desktop
{
    internal static class Program
    {
        private const string AppTitle = "Windrose Captain's Console";

        private static string LogPath
        {
            get
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WindroseServerManager.log");
            }
        }

        [STAThread]
        private static void Main()
        {
            try
            {
                Log("Application start");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += delegate(object sender, System.Threading.ThreadExceptionEventArgs args)
                {
                    Log("UI thread exception: " + args.Exception);
                    MessageBox.Show(args.Exception.ToString(), AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                };
                AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs args)
                {
                    Log("Unhandled exception: " + args.ExceptionObject);
                };

                var form = new MainForm();
                Log("Main form constructed");
                form.Load += delegate { Log("Main form load"); };
                form.Shown += delegate { Log("Main form shown"); };
                form.FormClosed += delegate { Log("Main form closed"); };
                Application.Run(form);
                Log("Application run completed");
            }
            catch (Exception ex)
            {
                Log("Fatal startup error: " + ex);
                MessageBox.Show(ex.ToString(), AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void Log(string message)
        {
            try
            {
                File.AppendAllText(LogPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + Environment.NewLine);
            }
            catch
            {
            }
        }
    }
}
