using System;
using System.Windows.Forms;
using ApplicantsGuide.Forms;

namespace ApplicantsGuide
{

    /// <summary>
    /// Головний клас програми, що забезпечує точку входу в додаток.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Головна точка входу для додатка «Довідник абітурієнта».
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            // Перехоплення необроблених помилок в UI-потоці для забезпечення стійкості програми
            Application.ThreadException += (sender, e) =>
            {
                MessageBox.Show(
                    "Помилка UI-потоку:\n\n" + e.Exception.ToString(),
                    "Критична помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };

            // Глобальне перехоплення критичних винятків на рівні всього домену програми
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