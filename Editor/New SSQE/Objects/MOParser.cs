using New_SSQE.ExternalUtils;
using New_SSQE.Misc.Static;
using System.Drawing;

namespace New_SSQE.Objects
{
    internal class MOParser
    {
        public static MapObject? Parse(int? id, params string[] data)
        {
            if (data.Length == 0)
                return null;

            try
            {
                int origin = id == null ? 2 : 1;
                int index = 0;

                id ??= int.Parse(data[index++]);
                long ms = long.Parse(data[index]);

                long duration;
                EasingStyle style;
                EasingDirection direction;
                float intensity;

                MapObject obj;

                switch (id)
                {
                    case 0:
                        float x = float.Parse(data[++index], Program.Culture);
                        float y = float.Parse(data[++index], Program.Culture);

                        obj = new Note(x, y, ms);
                        break;
                    case 1:
                        float bpm = float.Parse(data[++index], Program.Culture);

                        obj = new TimingPoint(bpm, ms);
                        break;
                    case 2:
                        duration = long.Parse(data[++index]);
                        style = string.IsNullOrWhiteSpace(data[++index]) ? EasingStyle.Linear : (EasingStyle)int.Parse(data[index]);
                        direction = string.IsNullOrWhiteSpace(data[++index]) ? EasingDirection.InOut : (EasingDirection)int.Parse(data[index]);
                        intensity = float.Parse(data[++index], Program.Culture);

                        obj = new Brightness(ms, duration, style, direction, intensity);
                        break;
                    case 3:
                        duration = long.Parse(data[++index]);
                        style = string.IsNullOrWhiteSpace(data[++index]) ? EasingStyle.Linear : (EasingStyle)int.Parse(data[index]);
                        direction = string.IsNullOrWhiteSpace(data[++index]) ? EasingDirection.InOut : (EasingDirection)int.Parse(data[index]);
                        intensity = float.Parse(data[++index], Program.Culture);

                        obj = new Contrast(ms, duration, style, direction, intensity);
                        break;
                    case 4:
                        duration = long.Parse(data[++index]);
                        style = string.IsNullOrWhiteSpace(data[++index]) ? EasingStyle.Linear : (EasingStyle)int.Parse(data[index]);
                        direction = string.IsNullOrWhiteSpace(data[++index]) ? EasingDirection.InOut : (EasingDirection)int.Parse(data[index]);
                        intensity = float.Parse(data[++index], Program.Culture);

                        obj = new Saturation(ms, duration, style, direction, intensity);
                        break;
                    case 5:
                        duration = long.Parse(data[++index]);
                        style = string.IsNullOrWhiteSpace(data[++index]) ? EasingStyle.Linear : (EasingStyle)int.Parse(data[index]);
                        direction = string.IsNullOrWhiteSpace(data[++index]) ? EasingDirection.InOut : (EasingDirection)int.Parse(data[index]);
                        intensity = float.Parse(data[++index], Program.Culture);

                        obj = new Blur(ms, duration, style, direction, intensity);
                        break;
                    case 6:
                        duration = long.Parse(data[++index]);
                        style = string.IsNullOrWhiteSpace(data[++index]) ? EasingStyle.Linear : (EasingStyle)int.Parse(data[index]);
                        direction = string.IsNullOrWhiteSpace(data[++index]) ? EasingDirection.InOut : (EasingDirection)int.Parse(data[index]);
                        intensity = float.Parse(data[++index], Program.Culture);

                        obj = new FOV(ms, duration, style, direction, intensity);
                        break;
                    case 7:
                        duration = long.Parse(data[++index]);
                        style = string.IsNullOrWhiteSpace(data[++index]) ? EasingStyle.Linear : (EasingStyle)int.Parse(data[index]);
                        direction = string.IsNullOrWhiteSpace(data[++index]) ? EasingDirection.InOut : (EasingDirection)int.Parse(data[index]);

                        int r = int.Parse(data[++index]);
                        int g = int.Parse(data[++index]);
                        int b = int.Parse(data[++index]);

                        obj = new Tint(ms, duration, style, direction, Color.FromArgb(r, g, b));
                        break;
                    case 8:
                        duration = long.Parse(data[++index]);
                        style = string.IsNullOrWhiteSpace(data[++index]) ? EasingStyle.Linear : (EasingStyle)int.Parse(data[index]);
                        direction = string.IsNullOrWhiteSpace(data[++index]) ? EasingDirection.InOut : (EasingDirection)int.Parse(data[index]);

                        float xp = float.Parse(data[++index], Program.Culture);
                        float yp = float.Parse(data[++index], Program.Culture);
                        float zp = float.Parse(data[++index], Program.Culture);

                        obj = new Position(ms, duration, style, direction, (xp, yp, zp));
                        break;
                    case 9:
                        duration = long.Parse(data[++index]);
                        style = string.IsNullOrWhiteSpace(data[++index]) ? EasingStyle.Linear : (EasingStyle)int.Parse(data[index]);
                        direction = string.IsNullOrWhiteSpace(data[++index]) ? EasingDirection.InOut : (EasingDirection)int.Parse(data[index]);
                        float rotation = float.Parse(data[++index], Program.Culture);

                        obj = new Rotation(ms, duration, style, direction, rotation);
                        break;
                    case 10:
                        duration = long.Parse(data[++index]);
                        style = string.IsNullOrWhiteSpace(data[++index]) ? EasingStyle.Linear : (EasingStyle)int.Parse(data[index]);
                        direction = string.IsNullOrWhiteSpace(data[++index]) ? EasingDirection.InOut : (EasingDirection)int.Parse(data[index]);
                        float factor = float.Parse(data[++index], Program.Culture);

                        obj = new ARFactor(ms, duration, style, direction, factor);
                        break;
                    case 11:
                        duration = long.Parse(data[++index]);
                        string text = data[++index];
                        int strength = int.Parse(data[++index]);

                        obj = new Text(ms, duration, text, strength);
                        break;
                    case 12:
                        obj = new Beat(ms);
                        break;
                    case 13:
                        int glideDirection = int.Parse(data[++index]);

                        obj = new Glide(ms, (GlideDirection)glideDirection);
                        break;
                    case 14:
                        float mx = float.Parse(data[++index], Program.Culture);
                        float my = float.Parse(data[++index], Program.Culture);

                        obj = new Mine(mx, my, ms);
                        break;
                    case 15:
                        string lyric = data[++index];
                        bool fadeIn = int.Parse(data[++index]) == 1;
                        bool fadeOut = int.Parse(data[++index]) == 1;

                        obj = new Lyric(ms, lyric, fadeIn, fadeOut);
                        break;
                    case 16:
                        duration = long.Parse(data[++index]);

                        obj = new Fever(ms, duration);
                        break;
                    default:
                        obj = new(id ?? -1, ms);
                        break;
                }

                string[] extra = new string[data.Length - ++index];

                for (int i = 0; i < extra.Length; i++)
                    extra[i] = data[index + i];

                obj.ExtraData = extra;
                return obj;
            }
            catch (Exception ex)
            {
                Logging.Log($"Failed to parse MapObject - {id} | {string.Join('|', data)}", LogSeverity.WARN, ex);
            }

            return null;
        }
    }
}
