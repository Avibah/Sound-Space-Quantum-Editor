using System;
using System.Windows.Forms;

namespace Sound_Space_Editor
{
	class Program
	{

        [STAThread]
        static void Main()
        {
            Application.SetCompatibleTextRenderingDefault(false);

            MainWindow window = new MainWindow();

            using (window)
                window.Run();
        }
    }
}