using New_SSQE.EditHistory;
using New_SSQE.NewGUI;
using New_SSQE.NewMaps;

namespace New_SSQE.Objects.Managers
{
    internal class PointManager
    {
        private static List<TimingPoint> Points => Mapping.Current.TimingPoints;

        public static void Replace(string label, List<TimingPoint> oldPoints, List<TimingPoint> newPoints)
        {
            label = label.Replace("[S]", Math.Max(oldPoints.Count, newPoints.Count) > 1 ? "S" : "");

            oldPoints = [.. oldPoints];
            newPoints = [.. newPoints];

            UndoRedoManager.Add(label, () =>
            {
                foreach (TimingPoint n in newPoints)
                    Points.Remove(n);
                Points.AddRange(oldPoints);

                if (oldPoints.Count == 1)
                    Mapping.Current.SelectedPoint = oldPoints[0];
                else
                    Mapping.Current.SelectedPoint = null;

                Mapping.Current.SortTimings();
                TimingsWindow.Instance?.ResetList();
            }, () =>
            {
                foreach (TimingPoint n in oldPoints)
                    Points.Remove(n);
                Points.AddRange(newPoints);

                if (newPoints.Count == 1)
                    Mapping.Current.SelectedPoint = newPoints[0];
                else
                    Mapping.Current.SelectedPoint = null;

                Mapping.Current.SortTimings();
                TimingsWindow.Instance?.ResetList();
            });
        }

        public static void Edit(string label, List<TimingPoint> toModify, Action<TimingPoint> action)
        {
            if (toModify.Count == 0)
                return;

            List<TimingPoint> completed = toModify.Select(n => n.Clone()).ToList();
            completed.ForEach(action);

            Replace(label, toModify, completed);
        }
        public static void Edit(string label, TimingPoint toModify, Action<TimingPoint> action) => Edit(label, [toModify], action);

        public static void Add(string label, List<TimingPoint> toAdd) => Replace(label, [], toAdd);
        public static void Add(string label, TimingPoint toAdd) => Replace(label, [], [toAdd]);

        public static void Remove(string label, List<TimingPoint> toRemove) => Replace(label, toRemove, []);
        public static void Remove(string label, TimingPoint toRemove) => Replace(label, [toRemove], []);
    }
}
