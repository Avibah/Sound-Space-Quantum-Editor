using New_SSQE.Misc;
using New_SSQE.NewGUI.Dialogs;
using New_SSQE.Preferences;
using New_SSQE.Services;
using System.IO.Compression;

namespace New_SSQE.NewMaps.Parsing
{
    internal class QEMZ : IFormatParser
    {
        public static bool Read(string path)
        {
            string temp = Assets.TempAt("qemz");
            if (!Directory.Exists(temp))
                Directory.CreateDirectory(temp);
            foreach (string file in Directory.GetFiles(temp))
                File.Delete(file);

            ZipFile.ExtractToDirectory(path, temp);

            foreach (string file in Directory.GetFiles(temp))
            {
                if (Path.GetExtension(file) == ".qem")
                    return QEM.Read(file);
            }

            return true;
        }

        public static bool Write(string path)
        {
            bool hasCover = Settings.useCover.Value && File.Exists(Settings.cover.Value);
            bool hasVideo = Settings.useVideo.Value && File.Exists(Settings.video.Value);

            Settings.useRelativeAudio.Value = true;

            string oldCover = Settings.cover.Value;
            if (hasCover)
                Settings.cover.Value = "cover.png";

            string oldVideo = Settings.video.Value;
            if (hasVideo)
                Settings.video.Value = "video.mp4";

            try
            {
                string temp = Assets.TempAt("qemz");
                if (!Directory.Exists(temp))
                    Directory.CreateDirectory(temp);
                foreach (string file in Directory.GetFiles(temp))
                    File.Delete(file);

                QEM.Write(Path.Combine(temp, "data.qem"));

                if (hasCover)
                    File.Copy(oldCover, Path.Combine(temp, "cover.png"), true);
                if (hasVideo)
                    File.Copy(oldVideo, Path.Combine(temp, "video.mp4"), true);
                File.Copy(Assets.CachedAt($"{Mapping.Current.SoundID}.asset"), Path.Combine(temp, "audio.asset"), true);

                Settings.useRelativeAudio.Value = false;
                Settings.cover.Value = oldCover;
                Settings.video.Value = oldVideo;

                if (File.Exists(path))
                    File.Delete(path);
                ZipFile.CreateFromDirectory(temp, path);
            }
            catch
            {
                Settings.useRelativeAudio.Value = false;
                Settings.cover.Value = oldCover;
                Settings.video.Value = oldVideo;
                throw;
            }

            return true;
        }

        public static bool Export()
        {
            string[] mappers = Settings.mappers.Value.Split('\n');
            string[] artists = Settings.songArtist.Value.Split('\n');

            string firstMapper = mappers.Length > 0 ? mappers[0] : "none";
            string firstArtist = artists.Length > 0 ? artists[0] : "unknown";

            string mapId = $"{firstMapper} - {firstArtist} - {Settings.songTitle.Value}";

            DialogResult result = new SaveFileDialog()
            {
                Title = "Export QEMZ",
                Filter = "Quantum Editor Maps (*.qemz)|*.qemz",
                InitialFileName = FormatUtils.FixID(mapId)
            }.Show(Settings.exportPath, out string fileName);

            if (result == DialogResult.OK)
            {
                try
                {
                    Write(fileName);
                    return true;
                }
                catch (Exception ex)
                {
                    Logging.Log("Failed to export", LogSeverity.WARN, ex);
                    MessageDialog.Show($"Failed to export QEMZ:\n\n{ex.Message}", MBoxIcon.Warning, MBoxButtons.OK);
                }
            }

            return false;
        }
    }
}
