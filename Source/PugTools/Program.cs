using System;
using System.Configuration;
using System.Windows.Forms;

namespace PugTools {
  static class Program {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main() {
      if (Environment.OSVersion.Version.Major >= 6) {
        SetProcessDPIAware();
      }

      Config.ConfigFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

      // Make sure an unhandled exception in a UI event handler (e.g. inside one browser window)
      // shows the standard "Continue/Quit" dialog and, on Continue, keeps the whole application
      // (and every other open browser window) running - instead of taking the entire process
      // down with it. This is WinForms' default behaviour for exceptions on the UI thread; the
      // explicit call here just makes sure it stays that way regardless of build configuration.
      // Note this only helps for regular, catchable .NET exceptions. Truly unrecoverable failures
      // (StackOverflowException, most OutOfMemoryException cases, native/AccessViolation faults)
      // can never be caught by any application-level handler - the CLR terminates the process
      // immediately regardless, and AppDomain.UnhandledException below only gets to observe that,
      // not prevent it. Real protection against those requires OS-level process isolation (i.e.
      // running each heavy browser in its own process), which is a separate, bigger change.
      Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
      AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
        if (e.ExceptionObject is Exception ex) {
          try {
            System.IO.File.AppendAllText(
              System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log"),
              string.Format("{0} [terminating={1}]{2}{3}{2}{2}",
                DateTime.Now, e.IsTerminating, Environment.NewLine, ex));
          } catch {
            // Best-effort logging only - never let the crash handler itself throw.
          }
        }
      };

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new Tools());
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern Boolean SetProcessDPIAware();
  }
}
