using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using New_SSQE.NewMaps;
using New_SSQE.Preferences;
using System.Collections;
using System.Collections.ObjectModel;
using New_SSQE.Objects.Managers;
using New_SSQE.Misc;
using New_SSQE.Services;
using New_SSQE.Objects;

namespace New_SSQE.NewGUI
{
    public partial class BookmarksWindow : Window
    {
        internal static BookmarksWindow? Instance;
        private static readonly ObservableCollection<Bookmark> Dataset = new();

        public BookmarksWindow()
        {
            Instance = this;
            Icon = new(new Bitmap(Assets.TexturesAt("Empty.png")));

            InitializeComponent();
            BookmarkList.Items = Dataset;
        }

        public static void ShowWindow()
        {
            Instance?.Close();

            BookmarksWindow window = new();

            Instance?.ResetList();

            window.Show();

            if (Platforms.IsLinux)
            {
                window.Topmost = true;
                BackgroundWindow.YieldWindow(window);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (long.TryParse(BookmarkOffsetBox.Text, out long offset) && long.TryParse(BookmarkEndOffsetBox.Text, out long endOffset))
            {
                if (endOffset < offset)
                    endOffset = offset;

                foreach (Bookmark item in Mapping.Current.Bookmarks)
                    if (item.Ms == offset && item.EndMs == endOffset)
                        return;

                BookmarkManager.Add("ADD BOOKMARK", new Bookmark(BookmarkTextBox.Text ?? "", offset, endOffset));
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (BookmarkList.SelectedItems.Count > 0)
            {
                Bookmark bookmark = GetBookmarkFromSelected(0);

                if (long.TryParse(BookmarkOffsetBox.Text, out long offset) && long.TryParse(BookmarkEndOffsetBox.Text, out long endOffset))
                {
                    foreach (Bookmark item in Mapping.Current.Bookmarks)
                        if (item.Text == BookmarkTextBox.Text && item.Ms == offset && item.EndMs == endOffset)
                            return;

                    BookmarkManager.Edit("EDIT BOOKMARK", bookmark, n =>
                    {
                        n.Text = BookmarkTextBox.Text;
                        n.Ms = offset;
                        n.EndMs = endOffset;
                    });
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            List<Bookmark> toRemove = [];

            for (int i = 0; i < BookmarkList.SelectedItems.Count; i++)
                toRemove.Add(GetBookmarkFromSelected(i));

            BookmarkManager.Remove("DELETE BOOKMARK[S]", toRemove);
        }

        private void CurrentPosButton_Click(object sender, RoutedEventArgs e)
        {
            if (BookmarkOffsetBox.Text != BookmarkEndOffsetBox.Text)
                BookmarkOffsetBox.Text = ((int)Settings.currentTime.Value.Value).ToString();
            BookmarkEndOffsetBox.Text = ((int)Settings.currentTime.Value.Value).ToString();
        }

        private void BookmarkSelection_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (BookmarkList.SelectedItems.Count > 0)
            {
                Bookmark bookmark = GetBookmarkFromSelected(0);

                BookmarkTextBox.Text = bookmark.Text;
                BookmarkOffsetBox.Text = bookmark.Ms.ToString();
                BookmarkEndOffsetBox.Text = bookmark.EndMs.ToString();
            }
        }

        private Bookmark GetBookmarkFromSelected(int index)
        {
            IList selected = BookmarkList.SelectedItems;
            List<Bookmark> bookmarks = Mapping.Current.Bookmarks;

            Bookmark? bookmark = selected[index] as Bookmark;

            for (int i = 0; i < bookmarks.Count; i++)
                if (bookmarks[i].Text == bookmark?.Text && bookmarks[i].Ms == bookmark.Ms && bookmarks[i].EndMs == bookmark.EndMs)
                    return bookmarks[i];

            return bookmarks[0];
        }

        public void ResetList()
        {
            List<Bookmark> bookmarks = Mapping.Current.Bookmarks;

            Dataset.Clear();
            for (int i = 0; i < bookmarks.Count; i++)
                Dataset.Add(new(bookmarks[i].Text, bookmarks[i].Ms, bookmarks[i].EndMs));

            if (Dataset.Count > 0)
            {
                BookmarkTextBox.Text = Dataset[0].Text;
                BookmarkOffsetBox.Text = Dataset[0].Ms.ToString();
                BookmarkEndOffsetBox.Text = Dataset[0].EndMs.ToString();
            }
        }
    }
}
