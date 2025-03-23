using New_SSQE.Objects;

namespace New_SSQE.ExternalUtils
{
    internal class Clipboard
    {
        public static void SetText(string text)
        {
            try
            {
                if (MainWindow.Instance != null)
                    MainWindow.Instance.ClipboardString = text;
            }
            catch { }
        }

        public static void SetData(List<MapObject> mapObjects)
        {
            string[] objStr = new string[mapObjects.Count + 1];
            objStr[0] = "copy";

            for (int i = 0; i < mapObjects.Count; i++)
                objStr[i + 1] = $"{mapObjects[i].ID}|{mapObjects[i].ToString()}";

            SetText(string.Join(',', objStr));
        }

        public static string GetText()
        {
            try
            {
                return MainWindow.Instance.ClipboardString ?? "";
            }
            catch { }

            return "";
        }

        public static List<MapObject> GetData()
        {
            string text = GetText();
            string[] split = text.Split(',');

            List<MapObject> objects = new();

            if (split.FirstOrDefault() == "copy")
            {
                for (int i = 1; i < split.Length; i++)
                {
                    string item = split[i];
                    MapObject? obj = MOParser.Parse(null, item.Split('|'));

                    if (obj != null)
                        objects.Add(obj);
                }
            }
            else
            {
                for (int i = 0; i < split.Length; i++)
                {
                    string item = split[i];
                    string[] subsplit = item.Split('|');

                    try
                    {
                        float x = float.Parse(subsplit[0], Program.Culture);
                        float y = float.Parse(subsplit[1], Program.Culture);
                        long time = long.Parse(subsplit[2]);

                        objects.Add(new Note(x, y, time));
                    }
                    catch { }
                }
            }

            return objects;
        }
    }
}
