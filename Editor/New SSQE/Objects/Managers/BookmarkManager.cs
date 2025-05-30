using New_SSQE.EditHistory;
using New_SSQE.NewGUI;
using New_SSQE.NewMaps;
using New_SSQE.Objects.Other;

namespace New_SSQE.Objects.Managers
{
    internal class BookmarkManager
    {
        private static List<Bookmark> Bookmarks => Mapping.Current.Bookmarks;

        public static void Replace(string label, List<Bookmark> oldBookmarks, List<Bookmark> newBookmarks)
        {
            label = label.Replace("[S]", Math.Max(oldBookmarks.Count, newBookmarks.Count) > 1 ? "S" : "");

            oldBookmarks = [..oldBookmarks];
            newBookmarks = [..newBookmarks];

            UndoRedoManager.Add(label, () =>
            {
                foreach (Bookmark n in newBookmarks)
                    Bookmarks.Remove(n);
                Bookmarks.AddRange(oldBookmarks);

                Mapping.SortBookmarks();
                BookmarksWindow.Instance?.ResetList();
            }, () =>
            {
                foreach (Bookmark n in oldBookmarks)
                    Bookmarks.Remove(n);
                Bookmarks.AddRange(newBookmarks);

                Mapping.SortBookmarks();
                BookmarksWindow.Instance?.ResetList();
            });
        }

        public static void Edit(string label, List<Bookmark> toModify, Action<Bookmark> action)
        {
            if (toModify.Count == 0)
                return;

            List<Bookmark> completed = toModify.Select(n => n.Clone()).ToList();
            completed.ForEach(action);

            Replace(label, toModify, completed);
        }
        public static void Edit(string label, Bookmark toModify, Action<Bookmark> action) => Edit(label, [toModify], action);

        public static void Add(string label, List<Bookmark> toAdd) => Replace(label, [], toAdd);
        public static void Add(string label, Bookmark toAdd) => Replace(label, [], [toAdd]);

        public static void Remove(string label, List<Bookmark> toRemove) => Replace(label, toRemove, []);
        public static void Remove(string label, Bookmark toRemove) => Replace(label, [toRemove], []);
    }
}
