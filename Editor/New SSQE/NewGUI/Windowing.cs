using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Windows;

namespace New_SSQE.NewGUI
{
    internal static class Windowing
    {
        private static GuiWindow? _current;

        public static GuiWindow Current
        {
            get
            {
                if (_current == null)
                    SwitchWindow(new GuiWindowMenu());
                return _current!;
            }
        }

        public static void SwitchWindow(GuiWindow window)
        {
            _current?.Close();
            _current = window;
        }
    }
}
