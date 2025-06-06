﻿using System.Drawing;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using System.Reflection;
using System.Text.Json;
using New_SSQE.Audio;
using New_SSQE.Misc.Static;
using New_SSQE.ExternalUtils;
using New_SSQE.NewGUI.Base;

namespace New_SSQE.Preferences
{
    internal abstract class SettingBase
    {
        public Type Type;
        public string Name;

        public SettingBase()
        {
            Settings.toInitialize += (s, e) => Settings.settings.Add(this);
        }

        public abstract object GetValue();
        public abstract object GetDefault();
        public abstract void SetValue(object value);
    }

    internal class Setting<T> : SettingBase
    {
        public T Value;
        public readonly T Default;

        public Setting(T value)
        {
            Value = value;
            Default = value;
        }

        public static implicit operator Setting<T>(T value)
        {
            return new(value) { Type = typeof(T) };
        }

        public override object GetValue() => Value;
        public override object GetDefault() => Default;
        public override void SetValue(object value) => Value = (T)value;
    }

    internal partial class Settings
    {
        public static EventHandler? toInitialize;
        public static readonly List<SettingBase> settings = new();

        private static readonly List<Keys> numpadKeys = [Keys.KeyPad7, Keys.KeyPad8, Keys.KeyPad9, Keys.KeyPad4, Keys.KeyPad5, Keys.KeyPad6, Keys.KeyPad1, Keys.KeyPad2, Keys.KeyPad3];
        private static readonly List<Keys> patternKeys = [Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9];

        private static readonly Dictionary<string, object> cache = [];
        private static readonly Dictionary<string, Keybind> keybinds = [];

        static Settings()
        {
            toInitialize?.Invoke(null, new());

            foreach (FieldInfo key in typeof(Settings).GetFields())
            {
                if (key.GetValue(null) is SettingBase setting)
                {
                    setting.Name = key.Name;

                    cache.Add(setting.Name, setting.GetValue());
                    if (setting is Setting<Keybind> keybind)
                        keybinds.Add(keybind.Name, keybind.Value);
                }
            }
        }

        private static bool isLoaded = false;

        public static void Reset()
        {
            HashSet<string> kept =
            [
                "autosavedFile", "autosavedProperties", "lastFile", "defaultPath", "audioPath",
                "exportPath", "coverPath", "importPath", "rhythiaPath", "rhythiaFolderPath", "patterns"
            ];

            foreach (SettingBase setting in settings)
            {
                if (!kept.Contains(setting.Name))
                    setting.SetValue(cache[setting.Name]);
            }
        }

        public static void RefreshColors()
        {
            Vector4[] noteColors = new Vector4[32];

            for (int i = 0; i < Settings.noteColors.Value.Count; i++)
            {
                Color color = Settings.noteColors.Value[i];
                noteColors[i] = (color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
            }

            Shader.FBOObject.Uniform4("NoteColors", noteColors);
            Shader.InstancedObject.Uniform4("NoteColors", noteColors);
            Shader.InstancedObjectExtra.Uniform4("NoteColors", noteColors);
            Shader.Sprite.Uniform4("NoteColors", noteColors);

            Vector4[] colors =
            [
                // 0-3
                (color1.Value.R / 255f, color1.Value.G / 255f, color1.Value.B / 255f, color1.Value.A / 255f), // color1
                (color2.Value.R / 255f, color2.Value.G / 255f, color2.Value.B / 255f, color2.Value.A / 255f), // color2
                (color3.Value.R / 255f, color3.Value.G / 255f, color3.Value.B / 255f, color3.Value.A / 255f), // color3
                (color4.Value.R / 255f, color4.Value.G / 255f, color4.Value.B / 255f, color4.Value.A / 255f), // color4
                // 4-9
                (1f, 1f, 1f, 1f),          // white
                (0f, 1f, 0.25f, 1f),       // lime
                (0f, 0.5f, 1f, 1f),        // teal
                (0.75f, 0.75f, 0.75f, 1f), // light gray
                (1f, 0f, 0f, 1f),          // red
                (0.5f, 0.5f, 0.5f, 1f),    // gray
                // 10
                (color5.Value.R / 255f, color5.Value.G / 255f, color5.Value.B / 255f, color5.Value.A / 255f), // color5
            ];

            Shader.InstancedMain.Uniform4("Colors", colors);
            Shader.InstancedMainExtra.Uniform4("Colors", colors);
        }

        public static void RefreshKeyMapping()
        {
            Dictionary<Keys, Tuple<int, int>> keyMapping = MainWindow.Instance.KeyMapping;
            List<Keys> keys = numpad.Value ? numpadKeys : gridKeys.Value;

            keyMapping.Clear();

            for (int i = 0; i < 9; i++)
            {
                if (keyMapping.ContainsKey(keys[i]))
                    keyMapping[keys[i]] = new(i % 3, i / 3);
                else
                    keyMapping.Add(keys[i], new(i % 3, i / 3));
            }
        }

        public static void Load(bool init)
        {
            try
            {
                Reset();
                Dictionary<string, JsonElement> result = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(Path.Combine(Assets.THIS, "settings.txt"))) ?? [];

                foreach (SettingBase setting in settings)
                {
                    try
                    {
                        if (result.TryGetValue(setting.Name, out JsonElement value) && setting != null)
                        {
                            if (setting is Setting<Color> color)
                            {
                                int[] c = value.Deserialize<int[]>() ?? new int[4];
                                if (c.Length == 3)
                                    color.Value = Color.FromArgb(c[0], c[1], c[2]);
                                else
                                    color.Value = Color.FromArgb(c[3], c[0], c[1], c[2]);
                            }
                            else if (setting is Setting<Keybind> keybind)
                                keybind.Value = ConvertToKeybind(value);
                            else if (setting is Setting<SliderSetting> slider)
                                slider.Value = ConvertToSliderSetting(value, slider.Value.Default, slider.Value.Decimals);
                            else if (setting is Setting<ListSetting> list)
                                list.Value.Current = value.GetString() ?? "";
                            else if (setting.Name == "noteColors" && setting is Setting<List<Color>> colors)
                            {
                                List<Color> temp = [];

                                foreach (int[] c in value.Deserialize<int[][]>() ?? [])
                                {
                                    if (c.Length == 3)
                                        temp.Add(Color.FromArgb(c[0], c[1], c[2]));
                                    else
                                        temp.Add(Color.FromArgb(c[3], c[0], c[1], c[2]));
                                }

                                colors.Value = temp;
                            }
                            else if (setting.Name == "gridKeys" && setting is Setting<List<Keys>> keys)
                            {
                                List<Keys> temp = [];
                                JsonElement[] set = value.Deserialize<JsonElement[]>() ?? new JsonElement[9];

                                for (int i = 0; i < 9; i++)
                                    temp.Add(ConvertToKey(set[i]));

                                keys.Value = temp;
                            }
                            else if (setting.Name == "patterns" && setting is Setting<List<string>> pats)
                            {
                                List<string> temp = [];
                                string[] set = value.Deserialize<string[]>() ?? new string[10];

                                for (int i = 0; i < 10; i++)
                                    temp.Add(set[i]);

                                pats.Value = temp;
                            }
                            else
                                setting.SetValue(value.Deserialize(setting.Type)!);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Log($"Failed to update setting - {setting.Name}", LogSeverity.WARN, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"Failed to load settings", LogSeverity.ERROR, ex);
                Reset();
            }

            if (init)
                Init();
        }

        public static void Init()
        {
            MainWindow.Instance.UpdateFPS(useVSync.Value ? VSyncMode.On : VSyncMode.Off, fpsLimit.Value.Value);

            MusicPlayer.Volume = masterVolume.Value.Value;
            SoundPlayer.Volume = sfxVolume.Value.Value;

            RefreshKeyMapping();
            RefreshColors();

            isLoaded = true;
        }

        public static void Save(bool reload = true)
        {
            if (!isLoaded)
                return;

            try
            {
                Dictionary<string, object> finaljson = [];

                foreach (SettingBase setting in settings)
                {
                    string name = setting.Name;

                    if (setting is Setting<Color> color)
                        finaljson.Add(name, new int[] { color.Value.R, color.Value.G, color.Value.B, color.Value.A });
                    else if (setting is Setting<Keybind> keybind)
                        finaljson.Add(name, new object[] { keybind.Value.Key.ToString(), keybind.Value.Ctrl, keybind.Value.Shift, keybind.Value.Alt });
                    else if (setting is Setting<SliderSetting> slider)
                        finaljson.Add(name, new float[] { slider.Value.Value, slider.Value.Max, slider.Value.Step });
                    else if (setting is Setting<ListSetting> list)
                        finaljson.Add(name, list.Value.Current);
                    else if (name == "noteColors" && setting is Setting<List<Color>> colors)
                    {
                        List<int[]> final = [];

                        foreach (Color c in colors.Value)
                            final.Add([c.R, c.G, c.B, c.A]);

                        finaljson.Add(name, final);
                    }
                    else if (name == "gridKeys" && setting is Setting<List<Keys>> keys)
                    {
                        List<string> final = [];

                        for (int i = 0; i < 9; i++)
                            final.Add(keys.Value[i].ToString());

                        finaljson.Add(name, final);
                    }
                    else if (name == "patterns" && setting is Setting<List<string>> pats)
                    {
                        List<string> final = [];

                        for (int i = 0; i < 10; i++)
                            final.Add(pats.Value[i]);

                        finaljson.Add(name, final);
                    }
                    else
                        finaljson.Add(name, setting.GetValue());
                }

                File.WriteAllText(Path.Combine(Assets.THIS, "settings.txt"), JsonSerializer.Serialize(finaljson));
            }
            catch (Exception ex)
            {
                Logging.Log($"Failed to save settings", LogSeverity.ERROR, ex);
            }
            
            if (reload)
                Load(true);
        }

        private static Keys ConvertToKey(JsonElement value)
        {
            return (Keys)Enum.Parse(typeof(Keys), value.GetString() ?? "", true);
        }

        private static Keybind ConvertToKeybind(JsonElement value)
        {
            Keys key = ConvertToKey(value[0]);
            JsonElement[] flags = value.Deserialize<JsonElement[]>() ?? new JsonElement[4];
            // CTRL SHIFT ALT for backwards compatibility
            return new(key, flags[1].GetBoolean(), flags[3].GetBoolean(), flags[2].GetBoolean());
        }

        private static SliderSetting ConvertToSliderSetting(JsonElement value, float defaultVal, int decimalVal)
        {
            float[] set = value.Deserialize<float[]>() ?? new float[3];
            return new(set[0], set[1], set[2]) { Default = defaultVal, Decimals = decimalVal };
        }

        public static List<string> CompareKeybind(Keys key, bool ctrl, bool alt, bool shift)
        {
            List<string> keys = [];

            try
            {
                if (key == Keys.Backspace)
                    key = Keys.Delete;

                foreach (string setting in keybinds.Keys)
                {
                    Keybind keybind = keybinds[setting];

                    if (keybind.Key == key && keybind.Ctrl == ctrl && keybind.Alt == alt && keybind.Shift == shift)
                        keys.Add(setting);
                }

                if (ctrl || alt || shift)
                    return keys;

                List<Keys> keyCloned = new(MainWindow.Instance.KeyMapping.Keys);

                foreach (Keys gridKey in keyCloned)
                {
                    Tuple<int, int> value = MainWindow.Instance.KeyMapping[gridKey];

                    if (gridKey == key)
                        keys.Add($"gridKey{value.Item1}|{value.Item2}");
                }

                for (int i = 0; i < patternKeys.Count; i++)
                    if (patternKeys[i] == key)
                        keys.Add($"pattern{i}");
            }
            catch { }

            return keys;
        }
    }
}
