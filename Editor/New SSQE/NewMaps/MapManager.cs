using New_SSQE.Audio;
using New_SSQE.ExternalUtils;
using New_SSQE.Misc.Dialogs;
using New_SSQE.Misc.Static;
using New_SSQE.NewGUI;
using New_SSQE.NewGUI.Windows;
using New_SSQE.NewMaps.Parsing;
using New_SSQE.Preferences;

namespace New_SSQE.NewMaps
{
    internal class MapManager
    {
        public static List<Map> Cache = [];
        private static readonly string cacheFile = Path.Combine(Assets.TEMP, "cache.txt");

        public static void SaveCache()
        {
            List<string> data = [];
            foreach (Map map in Cache)
                data.Add(map.ToCache());

            string text = string.Join("\n\n", data);
            File.WriteAllText(cacheFile, text);
        }

        public static void LoadCache()
        {
            if (File.Exists(cacheFile) && !string.IsNullOrWhiteSpace(File.ReadAllText(cacheFile)))
            {
                Cache.Clear();
                string[] data = File.ReadAllText(cacheFile).Split("\n\n");

                for (int i = 0; i < data.Length; i++)
                {
                    Map map = new();

                    if (map.FromCache(data[i]))
                        Cache.Add(map);
                }
            }
        }



        public static bool ImportAudio(string id)
        {
            DialogResult result = new OpenFileDialog()
            {
                Title = "Select Audio File",
                Filter = $"Audio Files ({MusicPlayer.SupportedExtensionsString})|{MusicPlayer.SupportedExtensionsString}"
            }.RunWithSetting(Settings.audioPath, out string fileName);

            if (result == DialogResult.OK)
            {
                if (string.IsNullOrWhiteSpace(id))
                    id = Path.GetFileNameWithoutExtension(fileName);
                id = FormatUtils.FixID(id);

                if (fileName != Path.Combine(Assets.CACHED, $"{id}.asset"))
                    File.Copy(fileName, Path.Combine(Assets.CACHED, $"{id}.asset"), true);

                return MusicPlayer.Load(Path.Combine(Assets.CACHED, $"{id}.asset"));
            }

            return false;
        }

        public static bool LoadAudio(string id)
        {
            try
            {
                if (!File.Exists(Path.Combine(Assets.CACHED, $"{id}.asset")))
                {
                    if (Settings.skipDownload.Value)
                    {
                        DialogResult message = MessageBox.Show($"No asset with id '{id}' is present in cache.\n\nWould you like to import a file with this id?", MBoxIcon.Warning, MBoxButtons.OK_Cancel);

                        return message == DialogResult.OK && ImportAudio(id);
                    }
                    else
                        WebClient.DownloadFile($"https://assetdelivery.roblox.com/v1/asset/?id={id}", Path.Combine(Assets.CACHED, $"{id}.asset"), FileSource.Roblox);
                }

                return MusicPlayer.Load(Path.Combine(Assets.CACHED, $"{id}.asset"));
            }
            catch (Exception e)
            {
                Logging.Register("Failed to load audio", LogSeverity.WARN, e);
                DialogResult message = MessageBox.Show($"Failed to download asset with id '{id}':\n\n{e.Message}\n\nWould you like to import a file with this id instead?", MBoxIcon.Error, MBoxButtons.OK_Cancel);

                if (message == DialogResult.OK)
                    return ImportAudio(id);
            }

            return false;
        }



        public static bool LoadMap(Map map)
        {
            CurrentMap.Map?.Close();
            CurrentMap.Map = map;

            map.Close();
            Cache.Add(map);
            SaveCache();

            if (map.Open())
            {
                Windowing.SwitchWindow(new GuiWindowEditor());
            }

            return Windowing.Current is GuiWindowEditor && CurrentMap.SoundID != "-1";
        }

        public static bool Load(string data)
        {
            foreach (Map map in Cache)
            {
                if (map.FileName == data)
                {
                    map.Open();
                    return true;
                }
            }

            try
            {
                Map map = new();
                
                if (!map.Load(data))
                {
                    MessageBox.Show("Failed to load map data\n\nExit and check '*\\logs.txt' for more info", MBoxIcon.Warning, MBoxButtons.OK);
                    return false;
                }

                return LoadMap(map);
            }
            catch (Exception ex)
            {
                Logging.Register("Failed to load map data", LogSeverity.WARN, ex);
                MessageBox.Show($"Failed to load map data: {ex.Message}\n\nExit and check '*\\logs.txt' for more info", MBoxIcon.Warning, MBoxButtons.OK);
                return false;
            }
        }



        public static bool CloseMap(Map map)
        {
            if (!map.IsSaved)
            {
                DialogResult result = MessageBox.Show($"{map.FileID} ({map.SoundID})\n\nWould you like to save before closing?", MBoxIcon.Warning, MBoxButtons.Yes_No_Cancel);

                if (result == DialogResult.Cancel)
                    return false;
                if (result == DialogResult.Yes)
                    map.Save();
            }

            return true;
        }

        public static bool Close()
        {
            bool close = true;

            foreach (Map map in Cache)
            {
                close &= CloseMap(map);
                if (!close)
                    break;
            }

            return close;
        }
    }
}
