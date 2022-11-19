using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using Sound_Space_Editor.Properties;

namespace Sound_Space_Editor
{
	class Program
	{

		[STAThread]
		static void Main()
		{
			Application.SetCompatibleTextRenderingDefault(false);

			EditorWindow w;

			try
			{
				long offset = 0;

				var launcherDir = Environment.CurrentDirectory;

				w = new EditorWindow(offset, launcherDir);
			}
			catch(Exception e)
			{
				MessageBox.Show(e.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			using (w)
			{
				w.Run();
			}

			//INativeWindow window = new OpenTK.NativeWindow(1080, 600, "Timings Setup", GameWindowFlags.Default, new GraphicsMode(32, 8, 0, 8), DisplayDevice.Default);

			
		}
	}
}