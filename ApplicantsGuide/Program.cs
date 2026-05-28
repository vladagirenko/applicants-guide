using System;
using System.Windows.Forms;
using ApplicantsGuide.Forms;

namespace ApplicantsGuide
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            
            Application.ThreadException += (sender, e) =>
            {
                MessageBox.Show(
                    "Помилка UI-потоку:\n\n" + e.Exception.ToString(),
                    "Критична помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                MessageBox.Show(
                    "Необроблене виключення:\n\n" + e.ExceptionObject.ToString(),
                    "Критична помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };

            try
            {
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Помилка при запуску форми:\n\n" + ex.ToString(),
                    "Критична помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}