using New_SSQE.ExternalUtils;
using New_SSQE.NewMaps;
using New_SSQE.NewGUI.Windows;
using New_SSQE.Preferences;
using System.Drawing;

namespace New_SSQE.EditHistory
{
    internal class UndoRedoManager
    {
        public static readonly List<URAction> actions = new();

        public static int _index = -1;

        private static DateTime? prevTime;

        public static void Add(string label, Action undo, Action redo, bool runRedo = true)
        {
            try
            {
                while (_index + 1 < actions.Count)
                    actions.RemoveAt(_index + 1);

                actions.Add(new URAction(label, undo, redo));
                _index++;

                if (runRedo && _index < actions.Count && _index >= 0)
                    actions[_index].Redo?.Invoke();


                // cause people hate bpm now?
                if (label == "ADD NOTE" && runRedo && CurrentMap.TimingPoints.Count == 0)
                {
                    DateTime curTime = DateTime.Now;

                    if (prevTime == null || (curTime - prevTime)?.TotalMilliseconds >= 5000)
                    {
                        GuiWindowEditor.ShowToast("Please use BPM!", Settings.color2.Value);
                        prevTime = curTime;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Register($"Failed to register action: {label} - {ex.Message}", LogSeverity.WARN);
            }
        }

        public static void Undo()
        {
            if (_index >= 0)
            {
                URAction action = actions[_index];
                GuiWindowEditor.ShowToast($"UNDONE: {action.Label}", Color.FromArgb(255, 109, 0));

                action.Undo?.Invoke();
                _index--;
            }
        }

        public static void Redo()
        {
            if (_index + 1 < actions.Count)
            {
                URAction action = actions[_index + 1];
                GuiWindowEditor.ShowToast($"REDONE: {action.Label}", Color.FromArgb(255, 109, 0));

                action.Redo?.Invoke();
                _index++;
            }
        }

        public static void Clear()
        {
            actions.Clear();
            _index = -1;
        }

        public static void ResetActions(IEnumerable<URAction> newActions, int newIndex)
        {
            Clear();
            foreach (URAction action in newActions)
                Add(action.Label, action.Undo, action.Redo, false);
            _index = newIndex;
        }
    }
}
