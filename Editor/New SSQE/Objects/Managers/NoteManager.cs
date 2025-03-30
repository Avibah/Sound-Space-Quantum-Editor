using New_SSQE.EditHistory;
using New_SSQE.NewMaps;

namespace New_SSQE.Objects.Managers
{
    internal class NoteManager : IObjectManager<Note>
    {
        private static ObjectList<Note> Notes => Mapping.Current.Notes;
        private static List<Note> BezierNodes => Mapping.Current.BezierNodes;

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
                    for (int i = 0; i < newNotes.Count; i++)
                    {
                        if (BezierNodes.Remove(newNotes[i]))
                            BezierNodes.Add(oldNotes[i]);
                    }

                    Mapping.Current.BezierNodes = [..BezierNodes.OrderBy(n => n.Ms)];
                }
            }, () =>
            {
                Notes.RemoveAll(oldNotes);
                Notes.AddRange(newNotes);

                Notes.Sort();
                Notes.Selected = new(newNotes);

                if (bezier)
                {
                    for (int i = 0; i < oldNotes.Count; i++)
                    {
                        if (BezierNodes.Remove(oldNotes[i]))
                            BezierNodes.Add(newNotes[i]);
                    }

                    Mapping.Current.BezierNodes = [..BezierNodes.OrderBy(n => n.Ms)];
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
