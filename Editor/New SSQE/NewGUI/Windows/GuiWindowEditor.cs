using New_SSQE.Audio;
using New_SSQE.ExternalUtils;
using New_SSQE.Misc.Dialogs;
using New_SSQE.Misc.Static;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;
using New_SSQE.NewGUI.Input;
using New_SSQE.NewMaps;
using New_SSQE.NewMaps.Parsing;
using New_SSQE.Objects;
using New_SSQE.Preferences;
using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using Un4seen.Bass;

namespace New_SSQE.NewGUI.Windows
{
    internal partial class GuiWindowEditor : GuiWindow
    {
        public GuiWindowEditor() : base(BackgroundSquare, StandardMapNavs, SpecialMapNavs, ConstantMapNavs, CopyButton, BackButton, Tempo,
            Timeline, MusicVolume, SfxVolume, PlayPause, ToastLabel, ZoomLabel, ZoomValueLabel, ClickModeLabel, TempoLabel,
            MusicVolumeLabel, MusicVolumeValueLabel, SfxVolumeLabel, SfxVolumeValueLabel, CurrentTimeLabel, CurrentMsLabel,
            MusicMute, SfxMute, TotalTimeLabel, NotesLabel, Grid, Track)
        {
            BackgroundSquare.Color = Color.FromArgb((int)Settings.editorBGOpacity.Value, 30, 30, 30);
            
            LNavPlayer.Visible = PlatformUtils.ExecutableExists("SSQE Player") || Settings.playtestGame.Value != "SSQE Player";

            BeatSnapDivisor.Update();
            QuantumSnapDivisor.Update();
            Timeline.Update();
            Tempo.Update();
        }

        public override void Close()
        {
            base.Close();

            LNavController.Disconnect();
            RNavController.Disconnect();
            SpecialNavController.Disconnect();
        }

        public override void ConnectEvents()
        {
            LNavController.SelectionChanged += (s, e) =>
            {
                bool options = OptionsNav.Visible;
                bool timing = TimingNav.Visible;
                bool patterns = PatternsNav.Visible;

                OptionsNav.Visible = s == LNavOptions;
                TimingNav.Visible = s == LNavTiming;
                PatternsNav.Visible = s == LNavPatterns;
                PlayerNav.Visible = s == LNavPlayer;

                if (Settings.playtestGame.Value != "SSQE Player" && s == LNavPlayer)
                {
                    if (options)
                        LNavController.UpdateSelection(LNavOptions);
                    else if (timing)
                        LNavController.UpdateSelection(LNavTiming);
                    else if (patterns)
                        LNavController.UpdateSelection(LNavPatterns);
                    else
                        LNavController.ClearSelection();
                }
            };

            LNavPlayer.LeftClick += (s, e) =>
            {
                string audio = Path.Combine(Assets.CACHED, $"{Mapping.Current.SoundID}.asset");

                switch (Settings.playtestGame.Value)
                {
                    case "Rhythia":
                        if (!File.Exists(Settings.rhythiaPath.Value))
                        {
                            Logging.Log($"Invalid Rhythia path - {Settings.rhythiaPath.Value}", LogSeverity.WARN);
                            ShowToast("INVALID RHYTHIA PATH [MENU > SETTINGS > PLAYER]", Settings.color1.Value);
                        }
                        else
                        {
                            try
                            {
                                if (MusicPlayer.IsPlaying)
                                    MusicPlayer.Pause();

                                Settings.Save();

                                string txt = Path.Combine(Assets.TEMP, "tempmap.txt");

                                TXT.Write(txt);

                                string audioPath = Path.GetFullPath(audio).Replace("\\", "/");
                                string txtPath = Path.GetFullPath(txt).Replace("\\", "/");

                                ProcessStartInfo info = new(Settings.rhythiaPath.Value)
                                {
                                    ArgumentList =
                                    {
                                        $"--a={audioPath}",
                                        $"--t={txtPath}"
                                    }
                                };

                                Process.Start(info);
                            }
                            catch (Exception ex)
                            {
                                Logging.Log("Failed to start Rhythia", LogSeverity.WARN, ex);
                                ShowToast("FAILED TO START RHYTHIA", Settings.color1.Value);
                            }
                        }
                        break;

                    case "Novastra":
                        if (!File.Exists(Settings.novaPath.Value))
                        {
                            Logging.Log($"Invalid Novastra path - {Settings.novaPath.Value}", LogSeverity.WARN);
                            ShowToast("INVALID NOVASTRA PATH [MENU > SETTINGS > PLAYER]", Settings.color1.Value);
                        }
                        else
                        {
                            try
                            {
                                if (MusicPlayer.IsPlaying)
                                    MusicPlayer.Pause();

                                Settings.Save();

                                string temp = Path.Combine(Assets.TEMP, "novaPlaytest");
                                Directory.CreateDirectory(temp);
                                foreach (string file in Directory.GetFiles(temp))
                                    File.Delete(file);

                                File.Copy(audio, Path.Combine(temp, $"audio.{(MusicPlayer.ctype == BASSChannelType.BASS_CTYPE_STREAM_MP3 ? "mp3" : "ogg")}"), true);
                                NPK.WriteNCH(Path.Combine(temp, "chart.nch"));
                                NPK.WriteNLR(Path.Combine(temp, "lyrics.nlr"));

                                string tempPath = Path.GetFullPath(temp).Replace("\\", "/");

                                float tempo = Mapping.Current.Tempo;
                                int ctrl = KeybindManager.CtrlHeld ? 1 : 0;
                                int alt = KeybindManager.AltHeld ? 1 : 0;
                                int shift = KeybindManager.ShiftHeld ? 1 : 0;

                                ProcessStartInfo info = new(Settings.novaPath.Value)
                                {
                                    ArgumentList =
                                    {
                                        "--demo-mode",
                                        $"-c={tempPath}",
                                        $"-m={tempo},{ctrl},{alt},{shift}",
                                        $"-f={Mapping.Current.FileID}",
                                        $"-p={Settings.currentTime.Value.Value / 1000}"
                                    }
                                };

                                Process.Start(info);
                            }
                            catch (Exception ex)
                            {
                                Logging.Log("Failed to start Novastra", LogSeverity.WARN, ex);
                                ShowToast("FAILED TO START NOVASTRA", Settings.color1.Value);
                            }
                        }
                        break;
                }
            };

            RNavController.SelectionChanged += (s, e) =>
            {
                SnappingNav.Visible = s == RNavSnapping;
                GraphicsNav.Visible = s == RNavGraphics;
                ExportNav.Visible = s == RNavExport;

                ConvertAudio.Visible = ExportNav.Visible && !PlatformUtils.IsLinux;
            };

            LNavController.Initialize();
            RNavController.Initialize();

            CopyButton.LeftClick += (s, e) =>
            {
                try
                {
                    if (KeybindManager.AltHeld)
                        Clipboard.SetText(TXT.CopyLegacy(Settings.correctOnCopy.Value));
                    else
                        Clipboard.SetText(TXT.Copy(Settings.correctOnCopy.Value));

                    ShowToast("COPIED TO CLIPBOARD", Settings.color1.Value);
                }
                catch (Exception ex)
                {
                    Logging.Log("Failed to copy", LogSeverity.WARN, ex);
                    ShowToast("FAILED TO COPY", Settings.color2.Value);
                }
            };

            BackButton.LeftClick += (s, e) => Windowing.SwitchWindow(new GuiWindowMenu());

            OpenTimings.LeftClick += (s, e) => TimingsWindow.ShowWindow();
            OpenTimings.BindKeybind("openTimings");
            OpenBookmarks.LeftClick += (s, e) => BookmarksWindow.ShowWindow();
            OpenBookmarks.BindKeybind("openBookmarks");

            HFlip.LeftClick += (s, e) => Mapping.Current.Notes.Modify_Edit("HORIZONTAL FLIP", Patterns.HorizontalFlip);
            HFlip.BindKeybind("hFlip");
            VFlip.LeftClick += (s, e) => Mapping.Current.Notes.Modify_Edit("VERTICAL FLIP", Patterns.VerticalFlip);
            VFlip.BindKeybind("vFlip");

            StoreNodes.LeftClick += (s, e) =>
            {
                Mapping.Current.Notes.BezierNodes = [];
                List<Note> selected = [..Mapping.Current.Notes.Selected.OrderBy(n => n.Ms)];
                int selectIndex = 0;
                
                for (int i = 0; i < Mapping.Current.Notes.Count; i++)
                {
                    if (selectIndex >= selected.Count)
                        break;

                    if (Mapping.Current.Notes[i] == selected[selectIndex])
                    {
                        Mapping.Current.BezierNodes.Add(i);
                        selectIndex++;
                    }
                }
            };
            StoreNodes.BindKeybind("storeNodes");
            ClearNodes.LeftClick += (s, e) => Mapping.Current.BezierNodes.Clear();
            BezierButton.LeftClick += (s, e) => Patterns.RunBezier();
            BezierButton.BindKeybind("drawBezier");

            RotateButton.LeftClick += (s, e) =>
            {
                float deg = RotateBox.Value;
                Mapping.Current.Notes.Modify_Edit($"ROTATE {deg}", n => Patterns.Rotate(n, deg));
            };

            ScaleButton.LeftClick += (s, e) =>
            {
                float scale = ScaleBox.Value;
                Mapping.Current.Notes.Modify_Edit($"SCALE {scale}%", n => Patterns.Scale(n, scale));
            };

            ImportIni.LeftClick += (s, e) =>
            {
                DialogResult result = new OpenFileDialog()
                {
                    Title = "Select .ini File",
                    Filter = "Map Property Files (*.ini)|*.ini"
                }.Show(Settings.defaultPath, out string fileName);

                if (result == DialogResult.OK)
                    INI.Read(fileName);
            };

            bool playerRunning = false;

            void Playtest(bool fromStart)
            {
                if (MusicPlayer.IsPlaying)
                    MusicPlayer.Pause();

                if (!playerRunning && PlatformUtils.ExecutableExists("SSQE Player"))
                {
                    Settings.Save();
                    TXT.Write(Path.Combine(Assets.TEMP, "tempmap.txt"));

                    Process process = PlatformUtils.RunExecutable("SSQE Player", $"{fromStart} false {KeybindManager.AltHeld}");
                    playerRunning = process != null;

                    if (process != null)
                    {
                        process.EnableRaisingEvents = true;
                        process.Exited += delegate { playerRunning = false; };
                    }
                }
            }

            PlayMap.LeftClick += (s, e) => Playtest(false);
            FromStart.LeftClick += (s, e) => Playtest(true);

            CopyBookmarks.LeftClick += (s, e) => Mapping.Current.CopyBookmarks();
            PasteBookmarks.LeftClick += (s, e) => Mapping.Current.PasteBookmarks();

            SaveButton.LeftClick += (s, e) =>
            {
                if (Mapping.Save())
                    ShowToast("SAVED", Settings.color1.Value);
            };
            SaveButton.BindKeybind("save");

            SaveAsButton.LeftClick += (s, e) =>
            {
                if (Mapping.SaveAs())
                    ShowToast("SAVED", Settings.color1.Value);
            };
            SaveAsButton.BindKeybind("saveAs");

            SwapClickMode.LeftClick += (s, e) => Settings.selectTool.Value ^= true;
            SwapClickMode.BindKeybind("switchClickTool");

            ReplaceID.LeftClick += (s, e) =>
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(ReplaceIDBox.Text))
                    {
                        DialogResult result = MessageBox.Show("Are you sure you want to replace this ID?\nIf an asset exists with this ID, it will be overwritten and this map will be saved.", MBoxIcon.Warning, MBoxButtons.Yes_No);
                        if (result != DialogResult.Yes)
                            return;

                        string id = ReplaceIDBox.Text;
                        MusicPlayer.Reset();

                        File.Move(Path.Combine(Assets.CACHED, $"{Mapping.Current.SoundID}.asset"), Path.Combine(Assets.CACHED, $"{id}.asset"));
                        Mapping.Current.SoundID = id;

                        Mapping.LoadAudio(id);
                        MusicPlayer.Volume = Settings.masterVolume.Value.Value;

                        if (Mapping.Current.FileName != null)
                            Mapping.Save();
                        else
                            Mapping.Autosave();

                        ShowToast($"REPLACED AUDIO ID WITH {id}", Settings.color1.Value);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Log("Failed to replace audio ID", LogSeverity.WARN, ex);
                    ShowToast("FAILED TO REPLACE ID", Settings.color1.Value);
                }
            };

            ExportButton.LeftClick += (s, e) =>
            {
                //Windowing.OpenDialog(new GuiDialogExport());
                //return;

                switch (Settings.exportType.Value.Current)
                {
                    case "Rhythia (SSPM)":
                        ExportSSPM.ShowWindow();
                        break;
                    case "Novastra (NPK)":
                        ExportNOVA.ShowWindow();
                        break;
                }
            };

            Dictionary<string, (float, Dictionary<string, float>)> srCache = [];
            bool calculating = false;

            CalculateSR.LeftClick += (s, e) =>
            {
                if (calculating)
                    return;
                calculating = true;

                Task.Run(() =>
                {
                    string data = TXT.CopyLegacy();
                    byte[] bytes = Encoding.UTF8.GetBytes(data[data.IndexOf(',')..]);
                    string hash = Encoding.UTF8.GetString(SHA512.HashData(bytes));

                    float sr;
                    Dictionary<string, float> rp;

                    if (srCache.TryGetValue(hash, out (float, Dictionary<string, float>) value))
                    {
                        sr = value.Item1;
                        rp = value.Item2;
                    }
                    else
                    {
                        Networking.GetDifficultyMetrics(data, out sr, out rp);
                        srCache.Add(hash, (sr, rp));
                    }

                    string result = sr > 0 ? $"SR: {Math.Round(sr, 2)}\n" : "";
                    string suffix = sr > 0 ? "RP" : "";

                    foreach (KeyValuePair<string, float> kvp in rp)
                        result += $"\n{kvp.Key}: {Math.Round(kvp.Value, 2)} {suffix}";

                    SRLabel.Text = result;
                    calculating = false;
                });
            };

            void ApproachRateChanged() => ApproachRateLabel.Text = $"Approach Rate: {Math.Round(Settings.approachRate.Value.Value + 1)}";
            ApproachRate.ValueChanged += (s, e) => ApproachRateChanged();
            ApproachRateChanged();

            void PlayerApproachRateChanged() => PlayerApproachRateLabel.Text = $"Player Approach Rate: {Math.Round(Settings.playerApproachRate.Value.Value + 1)}";
            PlayerApproachRate.ValueChanged += (s, e) => PlayerApproachRateChanged();
            PlayerApproachRateChanged();

            void SnappingChanged() => SnappingLabel.Text = $"Snapping: 3/{Math.Round(Settings.quantumSnapping.Value.Value + 3)}";
            QuantumSnapDivisor.ValueChanged += (s, e) => SnappingChanged();
            SnappingChanged();

            void CursorPosChanged() => CursorPosLabel.Text = $"Cursor Position: {Math.Abs(Math.Round(Settings.cursorPos.Value.Value))}%";
            TrackCursorPos.ValueChanged += (s, e) => CursorPosChanged();
            CursorPosChanged();

            void TempoChanged() => TempoLabel.Text = $"PLAYBACK SPEED - {Math.Round(Mapping.Current.Tempo * 100)}%";
            Tempo.ValueChanged += (s, e) => TempoChanged();
            TempoChanged();

            void MusicVolumeChanged() => MusicVolumeValueLabel.Text = $"{Math.Abs(Math.Round(Settings.masterVolume.Value.Value * 100))}";
            MusicVolume.ValueChanged += (s, e) => MusicVolumeChanged();
            MusicVolumeChanged();

            void SfxVolumeChanged() => SfxVolumeValueLabel.Text = $"{Math.Abs(Math.Round(Settings.sfxVolume.Value.Value * 100))}";
            SfxVolume.ValueChanged += (s, e) => SfxVolumeChanged();
            SfxVolumeChanged();

            void GameChanged()
            {
                switch (Settings.modchartGame.Value.Current)
                {
                    case "Novastra":
                        SpecialNavNova.Visible = true;
                        break;
                    case "Rhythia":
                        SpecialNavNova.Visible = false;
                        break;
                }
            }

            GameSwitch.LeftClick += (s, e) => GameChanged();
            GameSwitch.RightClick += (s, e) => GameChanged();
            GameChanged();

            EditSpecial.LeftClick += (s, e) =>
            {
                StandardMapNavs.Visible = false;
                SpecialMapNavs.Visible = true;

                Mapping.Current.RenderMode = ObjectRenderMode.Special;
            };

            SpecialNavExit.LeftClick += (s, e) =>
            {
                StandardMapNavs.Visible = true;
                SpecialMapNavs.Visible = false;

                Mapping.Current.RenderMode = ObjectRenderMode.Notes;
            };

            Dictionary<GuiButton, IndividualObjectMode> objModes = new()
            {
                {SpecialNavBeat, IndividualObjectMode.Beat },
                {SpecialNavMine, IndividualObjectMode.Mine },
                {SpecialNavGlide, IndividualObjectMode.Glide },
                {SpecialNavLyric, IndividualObjectMode.Lyric },
                {SpecialNavFever, IndividualObjectMode.Fever },
                {SpecialNavNotes, IndividualObjectMode.Note }
            };

            SpecialNavController.SelectionChanged += (s, e) =>
            {
                IndividualObjectMode mode = IndividualObjectMode.Disabled;
                if (s is GuiButton button)
                    objModes.TryGetValue(button, out mode);

                LyricNav.Visible = s == SpecialNavLyric;
                LyricPreview.Visible = LyricNav.Visible || mode == IndividualObjectMode.Disabled;
                FeverPreview.Visible = s == SpecialNavFever || mode == IndividualObjectMode.Disabled;
                NoteNav.Visible = s == SpecialNavNotes;

                Mapping.Current.ObjectMode = mode;
            };

            SpecialNavController.Initialize();

            Numpad.LeftClick += (s, e) => Settings.RefreshKeyMapping();

            void CreateLyric(string text)
            {
                List<MapObject> selected = Mapping.Current.SpecialObjects.Selected;
                long ms = Timing.GetClosestBeat(Settings.currentTime.Value.Value);
                ms = (long)Math.Clamp(ms >= 0 ? ms : Settings.currentTime.Value.Value, 0, Settings.currentTime.Value.Max);
                bool fadeIn = LyricFadeIn.Toggle;
                bool fadeOut = LyricFadeOut.Toggle;

                if (selected.Count == 1 && selected[0] is Lyric lyric)
                {
                    Mapping.Current.SpecialObjects.Modify_Edit("EDIT LYRIC", [lyric], n =>
                    {
                        if (n is Lyric lyric)
                        {
                            lyric.Text = text;
                            lyric.FadeIn = fadeIn;
                            lyric.FadeOut = fadeOut;
                        }
                    });
                }
                else
                {
                    Lyric toAdd = new(ms, text, fadeIn, fadeOut);
                    Mapping.Current.SpecialObjects.Modify_Add("ADD LYRIC", toAdd);

                    if (Settings.autoAdvance.Value)
                        Timing.Advance();
                }

                Mapping.Current.ClearSelected();
                LyricBox.Text = "";
                LyricFadeIn.Toggle = false;
                LyricFadeOut.Toggle = false;
            }

            LyricCreate.LeftClick += (s, e) => CreateLyric(LyricBox.Text);
            LyricBox.TextEntered += (s, e) =>
            {
                CreateLyric(e.Text);
                LyricBox.Focused = true;
            };
            LyricFadeIn.LeftClick += (s, e) => LyricFadeOut.Toggle = false;
            LyricFadeOut.LeftClick += (s, e) => LyricFadeIn.Toggle = false;

            NoteApplyModifiers.LeftClick += (s, e) =>
            {
                ListSetting style = Settings.modchartStyle.Value;
                ListSetting direction = Settings.modchartDirection.Value;

                Mapping.Current.Notes.Modify_Edit("MODIFY NOTE[S]", n =>
                {
                    n.EnableEasing = NoteEnableEasing.Toggle;
                    n.Style = (EasingStyle)Array.IndexOf(style.Possible, style.Current);
                    n.Direction = (EasingDirection)Array.IndexOf(direction.Possible, direction.Current);
                });
            };

            MusicMute.LeftClick += (s, e) => MusicPlayer.Volume = Settings.masterVolume.Value.Value;
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            SliderSetting currentTime = Settings.currentTime.Value;

            ZoomValueLabel.Text = $"{Math.Round(Mapping.Current.Zoom * 100)}%";
            ClickModeLabel.Text = $"Click: {(Settings.ClickMode == ClickMode.Select ? "Select" : "Place")}";
            ClickModeLabel.Visible = Settings.ClickMode != ClickMode.Both;

            BeatDivisorLabel.Text = $"Beat Divisor: {Math.Round(Settings.beatDivisor.Value.Value * 10) / 10 + 1}";

            CurrentTimeLabel.Text = $"{(int)(currentTime.Value / 60000)}:{(int)(currentTime.Value % 60000 / 1000):0#}";
            TotalTimeLabel.Text = $"{(int)(currentTime.Max / 60000)}:{(int)(currentTime.Max % 60000 / 1000):0#}";

            float progress = currentTime.Value / currentTime.Max;
            RectangleF timelineRect = Timeline.GetRect();
            RectangleF currentMsRect = CurrentMsLabel.GetRect();

            CurrentMsLabel.SetRect(timelineRect.X + timelineRect.Height / 2 + (timelineRect.Width - timelineRect.Height) * progress - currentMsRect.Width / 2, currentMsRect.Y, currentMsRect.Width, currentMsRect.Height);
            CurrentMsLabel.Text = $"{(long)currentTime.Value:##,##0}ms";

            NotesLabel.Text = Mapping.Current.RenderMode switch
            {
                ObjectRenderMode.Notes => $"{Mapping.Current.Notes.Count} Notes",
                ObjectRenderMode.VFX => $"{Mapping.Current.VfxObjects.Count} Objects",
                ObjectRenderMode.Special => $"{Mapping.Current.SpecialObjects.Count} Objects",
                _ => ""
            };

            if (LyricPreview.Visible)
            {
                LyricCreate.Text = Mapping.Current.SpecialObjects.Selected.Count == 1 ? "EDIT" : "CREATE";

                Lyric? prev = null;
                Lyric? current = null;
                Lyric? next = null;

                string text = "";
                float alpha = 1;
                bool fadingIn = false;

                Lyric[] lyrics = Mapping.Current.SpecialObjects.Where(n => n is Lyric).Cast<Lyric>().ToArray();

                for (int i = 0; i < lyrics.Length; i++)
                {
                    prev = current;
                    current = lyrics[i];
                    if (current.Ms > currentTime.Value)
                        break;

                    if (i + 1 < lyrics.Length)
                        next = lyrics[i + 1];
                    else
                        next = null;


                    if (current.FadeOut)
                    {
                        alpha = 1 - Math.Clamp((currentTime.Value - current.Ms) / 1000, 0, 1);
                        fadingIn = false;
                    }
                    else if (current.FadeIn)
                    {
                        alpha = Math.Clamp((currentTime.Value - current.Ms) / 1000, 0, 1);
                        fadingIn = true;
                    }
                    else if (!fadingIn)
                        alpha = 1;

                    if (!string.IsNullOrWhiteSpace(current.Text))
                    {
                        if (prev != null && string.IsNullOrWhiteSpace(prev.Text))
                        {
                            text = current.Text;
                            if (text.StartsWith('-'))
                                text = text[1..];
                        }
                        else if (current.Text.StartsWith('-'))
                            text = current.Text[1..];
                        else if (text.EndsWith('-'))
                            text = text[..^1] + current.Text;
                        else if (!string.IsNullOrWhiteSpace(text))
                            text += ' ' + current.Text;
                        else
                            text = current.Text;
                    }
                    else if (!current.FadeOut)
                        text = current.Text;
                }

                if (text.EndsWith('-'))
                    text = text[..^1];

                LyricPreview.Text = text;
                LyricPreview.Alpha = alpha;
            }

            bool feverVisible = Mapping.Current.ObjectMode == IndividualObjectMode.Disabled || Mapping.Current.ObjectMode == IndividualObjectMode.Fever;
            FeverPreview.Visible = false;

            if (feverVisible)
            {
                Fever[] fevers = Mapping.Current.SpecialObjects.Where(n => n is Fever).Cast<Fever>().ToArray();

                for (int i = 0; i < fevers.Length; i++)
                {
                    Fever fever = fevers[i];

                    FeverPreview.Visible |= currentTime.Value >= fever.Ms && currentTime.Value <= fever.Ms + fever.Duration;
                }
            }

            base.Render(mousex, mousey, frametime);
        }

        public static void ShowToast(string text, Color? color = null)
        {
            ToastLabel.Show(text, color);
        }

        public override void MouseScroll(float delta)
        {
            base.MouseScroll(delta);

            if (KeybindManager.ShiftHeld)
            {
                SliderSetting setting = Settings.beatDivisor.Value;
                float step = setting.Step * (KeybindManager.CtrlHeld ? 1 : 2) * delta;

                setting.Value = Math.Clamp(setting.Value + step, 0f, setting.Max);
                BeatSnapDivisor.Update();
            }
            else if (KeybindManager.CtrlHeld)
                Mapping.Current.IncrementZoom(delta);
            else
                Timing.Scroll(delta < 0 ^ Settings.reverseScroll.Value, Math.Abs(delta));
        }

        public override void FileDrop(string file)
        {
            if (Path.GetExtension(file) == ".ini")
                INI.Read(file);
            else
                base.FileDrop(file);
        }
    }
}
