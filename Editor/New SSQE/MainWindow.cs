﻿using OpenTK.Graphics.OpenGL;
using System.ComponentModel;
using System.Drawing;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using SkiaSharp;
using OpenTK.Windowing.Common.Input;
using System.Runtime.InteropServices;
using System.IO.Compression;
using Assets = New_SSQE.Misc.Static.Assets;
using New_SSQE.Audio;
using New_SSQE.Preferences;
using New_SSQE.Misc.Dialogs;
using New_SSQE.Misc.Static;
using New_SSQE.ExternalUtils;
using New_SSQE.NewMaps;
using System.Diagnostics;
using New_SSQE.NewGUI;
using New_SSQE.NewGUI.Base;

namespace New_SSQE
{
    /*
     * OpenGL functions used in this program (with their required versions):
     * 
     * 4.3 - glDebugMessageCallback (only used when DebugVersion is true, should be disabled on release)
     * 3.3 - glVertexAttribDivisor
     * 3.1 - glDrawArraysInstanced
     * 3.0 - glBindVertexArray
     * 3.0 - glBindRenderbuffer
     * 3.0 - glBindFramebuffer
     * 3.0 - glBlitFramebuffer
     * 3.0 - glGenRenderbuffers
     * 3.0 - glGenFramebuffers
     * 3.0 - glGenVertexArrays
     * 3.0 - glDeleteVertexArrays
     * 3.0 - glRenderbufferStorageMultisample
     * 3.0 - glFramebufferRenderbuffer
     * 2.0 - glBindBuffer
     * 2.0 - glBindTexture
     * 2.0 - glActiveTexture
     * 2.0 - glGenTextures
     * 2.0 - glGenBuffers
     * 2.0 - glGetUniformLocation
     * 2.0 - glUniform (4f, 3f, 2f, 1i)
     * 2.0 - glUniformMatrix (4f)
     * 2.0 - glDrawArrays
     * 2.0 - glBufferData
     * 2.0 - glDeleteTextures
     * 2.0 - glDeleteBuffers
     * 2.0 - glVertexAttribPointer
     * 2.0 - glUseProgram
     * 2.0 - glTexParameter (i)
     * 2.0 - glTexImage2D
     * 2.0 - glEnableVertexAttribArray
     * 2.0 - glViewport
     * 2.0 - glClear
     * 2.0 - glClearColor
     * 2.0 - glGetString
     * 2.0 - glGetError
     * 2.0 - glGetInteger
     * 2.0 - glEnable
     * 2.0 - glBlendFunc
     * 2.0 - glCreateShader
     * 2.0 - glCreateProgram
     * 2.0 - glShaderSource
     * 2.0 - glLinkProgram
     * 2.0 - glGetShaderInfoLog
     * 2.0 - glDetachShader
     * 2.0 - glDeleteShader
     * 2.0 - glCompileShader
     * 2.0 - glAttachShader
     * 
     * The requested API version should match the version of the highest unconditionally required function in this list.
     * Currently: 3.3 - glVertexAttribDivisor (required for efficient instancing of map objects and text)
     * 
     */

    internal class MainWindow : GameWindow
    {
        public static bool DebugVersion = false;

        public static readonly Vector2i SpriteSize = (4, 4);

        public static string InitialFile = "";

        public static MainWindow Instance;

        public readonly Dictionary<Keys, Tuple<int, int>> KeyMapping = new();
        public static bool Focused = true;

        public Point Mouse = new(-1, -1);
        public Vector2 Delta = (0, 0);

        public bool CtrlHeld;
        public bool AltHeld;
        public bool ShiftHeld;

        public static Avalonia.Controls.Window DefaultWindow;

        private bool isFullscreen = false;



        private static WindowIcon GetWindowIcon()
        {
            byte[] bytes = File.ReadAllBytes(Path.Combine(Assets.TEXTURES, "Icon.ico"));
            SKBitmap bmp = SKBitmap.Decode(bytes, new SKImageInfo(256, 256, SKColorType.Rgba8888));
            OpenTK.Windowing.Common.Input.Image image = new(bmp.Width, bmp.Height, bmp.Bytes);

            return new(image);
        }

        private static void OnDebugMessage(DebugSource source, DebugType type, uint id, DebugSeverity severity, int length, IntPtr pMessage, IntPtr pUserParam)
        {
            string message = Marshal.PtrToStringAnsi(pMessage, length);

            if (type == DebugType.DebugTypeError)
                Logging.Log($"[{severity} source={source} type={type} id={id}] {message}", LogSeverity.WARN);
        }
        private static readonly GLDebugProc DebugMessageDelegate = OnDebugMessage;
        
        public static int MaxSamples = 0;

        public static bool MSAA;
        public static string FileToLoad = "";

        public MainWindow(int samples) : base(GameWindowSettings.Default, new()
        {
            Size = (1280, 720),
            Title = $"Sound Space Quantum Editor {Program.Version}",
            WindowState = WindowState.Maximized,
            NumberOfSamples = samples,
            Icon = GetWindowIcon(),
            Flags = DebugVersion ? ContextFlags.Debug : 0,

            APIVersion = new(3, 3)
        })
        {
            MSAA = samples > 0;
            
            // requires higher OpenGL version
            if (DebugVersion)
            {
                GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
                GL.Enable(EnableCap.DebugOutput);
            }
            
            Logging.Log($"Required OpenGL version: {APIVersion}");
            Logging.Log("Current OpenGL version: " + (GL.GetString(StringName.Version) ?? "N/A"));

            string version = GL.GetString(StringName.Version) ?? "";
            int major = 0, minor = 0;
            GL.GetInteger(GetPName.MajorVersion, ref major);
            GL.GetInteger(GetPName.MinorVersion, ref minor);
            GL.GetInteger(GetPName.MaxFramebufferSamples, ref MaxSamples);

            if (string.IsNullOrWhiteSpace(version) || new Version(major, minor) < APIVersion)
                throw new Exception($"Unsupported OpenGL version (Minimum: {APIVersion})");

            Instance = this;
            DefaultWindow = new BackgroundWindow();

            DiscordManager.Init();

            Settings.Init();

            CheckForUpdates();
            MessageBox.Instance?.Close();

            Mapping.LoadCache();

            OnMouseWheel(new MouseWheelEventArgs());
            Windowing.SwitchWindow(new NewGUI.Windows.GuiWindowMenu());

            if (!string.IsNullOrWhiteSpace(InitialFile))
                Mapping.Load(InitialFile);
        }

        public void UpdateFPS(VSyncMode mode, float fps)
        {
            if (Context.IsCurrent)
                VSync = mode;

            float max = Settings.fpsLimit.Value.Max;

            RenderFrequency = Math.Round(fps) == Math.Round(max) ? 0f : fps + 60f;
        }

        protected override void OnLoad()
        {
            GL.ClearColor(0f, 0f, 0f, 1f);
            
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        private double frameTime;
        private const double updateFrequency = 1 / 10.0;

        private double gcTime;
        private bool gcEnabled = true;

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            if (attemptClose)
                RunClose();
            if (closed)
                return;

            if (Platform.IsLinux) // because all the other events for this are a key behind on linux (???)
            {
                KeyboardState keyboard = KeyboardState;

                CtrlHeld = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);
                AltHeld = keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt);
                ShiftHeld = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
            }

            frameTime += args.Time;
            if (frameTime > updateFrequency)
            {
                ExportSSPM.UpdateID();
                frameTime = 0;

                if (!string.IsNullOrWhiteSpace(FileToLoad))
                {
                    Mapping.Load(FileToLoad);
                    FileToLoad = "";
                }
            }

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (MusicPlayer.IsPlaying)
                Settings.currentTime.Value.Value = (float)MusicPlayer.CurrentTime.TotalMilliseconds;
            
            try
            {
                Windowing.Current?.Render(MouseState.X, MouseState.Y, (float)args.Time);
            }
            catch (Exception ex)
            {
                Logging.Log($"Failed to render frame", LogSeverity.ERROR, ex);
            }

            OpenTK.Graphics.OpenGL.ErrorCode err = GL.GetError();
            while (err != OpenTK.Graphics.OpenGL.ErrorCode.NoError)
            {
                Logging.Log($"OpenGL Error: '{err}'", LogSeverity.WARN);
                err = GL.GetError();
            }

            SwapBuffers();

            // apparently the garbage collector doesnt want to deal with everything on its own .-.
            gcTime += args.Time;

            if (gcTime >= 2 && gcEnabled)
            {
                Stopwatch sw = Stopwatch.StartNew();

                GC.Collect();
                gcTime = 0;

                double duration = sw.Elapsed.TotalMilliseconds;

                if (duration > 50)
                {
                    // something is causing the gc to have a noticeable lag spike when it runs, so maybe now its doing more harm than good
                    Logging.Log($"GC took {duration}ms to process! Disabling forced garbage collection to improve performance");
                    gcEnabled = false;
                }

                sw.Stop();
            }

            DiscordManager.Process(args.Time);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            if (e.Width > 0 && e.Height > 0)
            {
                int w = Math.Max(e.Width, 800);
                int h = Math.Max(e.Height, 600);
                Size = new Vector2i(w, h);

                base.OnResize(new(w, h));
                GL.Viewport(0, 0, w, h);
                Shader.SetViewports(w, h);

                Windowing.Current?.Resize(new(w, h));
                OnRenderFrame(new FrameEventArgs());
            }
        }

        public void LockClick() => GuiWindow.LockClick = true;
        public void UnlockClick() => GuiWindow.LockClick = false;

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Windowing.Current?.MouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            Windowing.Current?.MouseUp(e);
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            CtrlHeld = e.Control;
            AltHeld = e.Alt;
            ShiftHeld = e.Shift;

            Windowing.Current?.KeyUp(e.Key);
        }

        private void SwitchFullscreen()
        {
            isFullscreen ^= true;
            WindowState = isFullscreen ? WindowState.Fullscreen : WindowState.Maximized;
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            CtrlHeld = e.Control;
            AltHeld = e.Alt;
            ShiftHeld = e.Shift;

            if (e.Key == Keys.F11)
            {
                SwitchFullscreen();
                return;
            }

            if (e.Key == Keys.F4 && AltHeld)
                Close();

            Windowing.Current?.KeyDown(e.Key);
        }

        protected override void OnFileDrop(FileDropEventArgs e)
        {
            foreach (string file in e.FileNames)
            {
                if (File.Exists(file))
                    Windowing.Current?.FileDrop(file);
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            KeyboardState keyboard = KeyboardState;

            CtrlHeld = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);
            AltHeld = keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt);
            ShiftHeld = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);

            Windowing.Current?.MouseScroll(e.OffsetY);
        }

        private bool forceClose = false;
        private bool attemptClose = false;
        private bool closed = false;

        private void RunClose()
        {
            attemptClose = false;
            forceClose = Mapping.Quit();

            if (forceClose)
                Close();
            else
                Windowing.SwitchWindow(new NewGUI.Windows.GuiWindowMenu());
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!forceClose)
            {
                attemptClose = true;
                e.Cancel = true;
            }
            else
            {
                closed = true;

                Settings.Save();

                TimingsWindow.Instance?.Close();
                BookmarksWindow.Instance?.Close();

                MusicPlayer.Dispose();
                DiscordManager.Dispose();
            }
        }

        protected override void OnFocusedChanged(FocusedChangedEventArgs e)
        {
            Focused = e.IsFocused || !Settings.lowerBackgroundFPS.Value;

            if (Focused)
                UpdateFPS(Settings.useVSync.Value ? VSyncMode.On : VSyncMode.Off, Settings.fpsLimit.Value.Value);
            else
                UpdateFPS(VSyncMode.Off, -45f);
        }



        public static void CheckForUpdates()
        {
            if (!Settings.checkUpdates.Value)
                return;

            static void ExtractFile(string path)
            {
                using (ZipArchive archive = ZipFile.OpenRead(path))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        try
                        {
                            entry.ExtractToFile(entry.FullName, true);
                        }
                        catch (Exception ex) { Logging.Log($"Failed to extract file: {entry.FullName}", LogSeverity.WARN, ex); }
                    }
                }

                File.Delete(path);
            }

            void Download(string file)
            {
                Logging.Log($"Attempting to download file '{file}'");
                Networking.DownloadFile(Links.ALL[$"{file} Zip"], Path.Combine(Assets.THIS, $"{file}.zip"));
                ExtractFile(Path.Combine(Assets.THIS, $"{file}.zip"));
            }

            void Run(string file, string tag, Setting<string> setting)
            {
                Logging.Log($"Searching for file '{file}'");

                if (Platform.ExecutableExists(file))
                {
                    string current = Platform.IsLinux ? setting.Value : Platform.GetExecutableVersionInfo(file).FileVersion ?? "";
                    string version = Networking.DownloadString(Links.ALL[$"{file} Version"]).Trim();

                    if (string.IsNullOrWhiteSpace(current) || Version.Parse(current) < Version.Parse(version))
                    {
                        Logging.Log($"Current and latest versions differ! Current: {current} | Latest: {version}");

                        DialogResult diag = MessageBox.Show($"New {tag} version is available ({version}). Would you like to download the new version?", MBoxIcon.Info, MBoxButtons.Yes_No);

                        if (diag == DialogResult.Yes)
                        {
                            Download(file);
                            setting.Value = version;
                        }
                    }
                }
                else
                {
                    DialogResult diag = MessageBox.Show($"{tag} is not present in this directory. Would you like to download it?", MBoxIcon.Info, MBoxButtons.Yes_No);

                    if (diag == DialogResult.Yes)
                    {
                        string version = Networking.DownloadString(Links.ALL[$"{file} Version"]).Trim();
                        
                        Download(file);
                        setting.Value = version;
                    }
                }
            }
            
            try
            {
                Run("SSQE Player", "Map Player", Settings.SSQE_Player_Version);
                Run("SSQE Updater", "Auto Updater", Settings.SSQE_Updater_Version);

                string redirect = Networking.GetRedirect(Links.EDITOR_REDIRECT);

                if (Platform.ExecutableExists("SSQE Updater") && redirect != "")
                {
                    string version = redirect[(redirect.LastIndexOf('/') + 1)..];

                    Logging.Log("Checking version of editor");
                    if (Version.Parse(version) > Version.Parse(Program.Version))
                    {
                        DialogResult diag = MessageBox.Show($"New Editor version is available ({version}). Would you like to download the new version?", MBoxIcon.Info, MBoxButtons.Yes_No);

                        if (diag == DialogResult.Yes)
                        {
                            Logging.Log("Attempting to run updater");
                            Platform.RunExecutable("SSQE Updater", "");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Failed to check for updates", LogSeverity.WARN, ex);
                MessageBox.Show("Failed to check for updates", MBoxIcon.Warning, MBoxButtons.OK);
            }
        }
    }
}
