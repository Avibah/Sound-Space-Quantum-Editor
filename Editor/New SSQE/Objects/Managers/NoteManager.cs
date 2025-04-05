using New_SSQE.EditHistory;
using New_SSQE.NewMaps;

namespace New_SSQE.Objects.Managers
{
    internal class NoteManager : IObjectManager<Note>
    {
        private static ObjectList<Note> Notes => Mapping.Current.Notes;
        private static List<int> BezierNodes => Mapping.Current.BezierNodes;

        public static void Replace(string label, List<Note> oldNotes, List<Note> newNotes)
        {
            bool bezier = Mapping.Current.BezierNodes.Count > 0;
            label = label.Replace("[S]", Math.Max(oldNotes.Count, newNotes.Count) > 1 ? "S" : "");

            oldNotes = [..oldNotes];
            newNotes = [..newNotes];

            UndoRedoManager.Add(label, () =>
            {
                Notes.RemoveAll(newNotes);
                Notes.AddRange(oldNotes);

                Notes.Sort();
                Notes.Selected = new(oldNotes);

                if (bezier)
                {
                    List<Note> temp = [];
                    for (int i = 0; i < BezierNodes.Count; i++)
                        temp.Add(newNotes[BezierNodes[i]]);

                    int index = 0;
                    List<int> nodes = [];

                    for (int i = 0; i < oldNotes.Count; i++)
                    {
                        if (index >= temp.Count)
                            break;

                        if (oldNotes[i] == temp[index])
                        {
                            nodes.Add(i);
                            index++;
                        }
                    }

                    Mapping.Current.BezierNodes = nodes;
                }
            }, () =>
            {
                Notes.RemoveAll(oldNotes);
                Notes.AddRange(newNotes);

                Notes.Sort();
                Notes.Selected = new(newNotes);

                if (bezier)
                {
                    List<Note> temp = [];
                    for (int i = 0; i < BezierNodes.Count; i++)
                        temp.Add(oldNotes[BezierNodes[i]]);

                    int index = 0;
                    List<int> nodes = [];

                    for (int i = 0; i < newNotes.Count; i++)
                    {
                        if (index >= temp.Count)
                            break;

                        if (newNotes[i] == temp[index])
                        {
                            nodes.Add(i);
                            index++;
                        }
                    }

                    Mapping.Current.BezierNodes = nodes;
                }
            });
        }

        public static void Edit(string label, List<Note> toModify, Action<Note> action)
        {
            if (toModify.Count == 0)
                return;

            List<Note> completed = toModify.Select(n => n.Clone()).ToList();
            completed.ForEach(action);

            Replace(label, toModify, completed);
        }
        public static void Edit(string label, Action<Note> action) => Edit(label, Notes.Selected, action);

        public static void Add(string label, List<Note> toAdd) => Replace(label, [], toAdd);
        public static void Add(string label, Note toAdd) => Replace(label, [], [toAdd]);

        public static void Remove(string label, List<Note> toRemove) => Replace(label, toRemove, []);
        public static void Remove(string label, Note toRemove) => Replace(label, [toRemove], []);
        public static void Remove(string label) => Replace(label, Notes.Selected, []);
    }
}
