using New_SSQE.ExternalUtils;
using New_SSQE.Misc.Static;
using System.Text.Json;

namespace New_SSQE.NewMaps.Parsing
{
    internal class PHZ : IFormatParser
    {
        public static bool IsValid(string path)
        {
            Dictionary<string, JsonElement> result = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(path)) ?? [];
            bool beat = result.TryGetValue("beat", out JsonElement beats);
            bool song = result.TryGetValue("song", out _);

            return beat && song && JsonSerializer.Deserialize<JsonElement[]>(beats) != null;
        }

        public static bool Read(string path)
        {
            if (!File.Exists(path))
                return false;

            string id = "processing";
            float bpm = 120;

            Dictionary<string, JsonElement> result = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(path)) ?? [];
            JsonElement[] beats = [];
            double offset = 0;

            foreach (string key in result.Keys)
            {
                JsonElement value = result[key];

                switch (key)
                {
                    case "bpm":
                        bpm = value.GetSingle();
                        break;
                    case "song":
                        if (value.ValueKind == JsonValueKind.Number)
                            id = value.GetInt32().ToString();
                        else
                            id = value.GetString() ?? "";
                        break;
                    case "beat":
                        beats = JsonSerializer.Deserialize<JsonElement[]>(value) ?? beats;
                        break;
                    case "songOffset":
                        offset = value.GetDouble() / 1000;
                        break;
                }
            }

            Mapping.Current.SoundID = id;

            foreach (JsonElement beat in beats)
            {
                JsonElement[] values = JsonSerializer.Deserialize<JsonElement[]>(beat) ?? [];

                if (values.Length >= 2)
                {
                    int tile = values[0].GetInt32();
                    double time = values[1].GetDouble() / (bpm / 60) + offset;

                    Mapping.Current.Notes.Add(new(2 - tile % 3, 2 - tile / 3, (long)(time * 1000)));
                }
            }

            try
            {
                if (!File.Exists(Path.Combine(Assets.CACHED, $"{id}.asset")))
                    Networking.DownloadFile($"https://pulsus.cc/play/client/s/{id}.mp3", Path.Combine(Assets.CACHED, $"{id}.asset"), FileSource.Pulsus);
            }
            catch (Exception ex)
            {
                Logging.Log("Failed to download audio (Pulsus)", LogSeverity.WARN, ex);
            }

            return true;
        }

        public static bool Write(string path)
        {
            return false;
        }
    }
}
