using OpenTK.Graphics.OpenGL;
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
using New_SSQE.NewGUI;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Windows;

namespace New_SSQE
{
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

        public static float ScaleX => Instance?.ClientSize.X / 1920f ?? 1f;
        public static float ScaleY => Instance?.ClientSize.Y / 1080f ?? 1f;

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
            ClientSize = (1280, 720),
            MinimumClientSize = (800, 600),
            Title = $"Sound Space Quantum Editor {Program.Version}",
            WindowState = WindowState.Maximized,
            NumberOfSamples = samples,
            Icon = GetWindowIcon(),
            Flags = DebugVersion ? ContextFlags.Debug : 0,

            APIVersion = PlatformUtils.RequestedAPI
        })
        {
            MSAA = samples > 0;
            
            // requires higher OpenGL version
            if (DebugVersion)// && GLFW.GetProcAddress("glDebugMessageCallback") > 0)
            {
                GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
                GL.Enable(EnableCap.DebugOutput);
            }
            
            Logging.Log($"Required OpenGL version: {APIVersion}");
            Logging.Log("Current OpenGL version: " + (GL.GetString(StringName.Version) ?? "N/A"));

            string version = GL.GetString(StringName.Version) ?? "";
            int major = 0, minor = 0;
            GL.GetInteger(GetPName.MajorVersion, out major);
            GL.GetInteger(GetPName.MinorVersion, out minor);
            GL.GetInteger(GetPName.MaxFramebufferSamples, out MaxSamples);

            Logging.Log($"Max samples: {MaxSamples}");

            if (string.IsNullOrWhiteSpace(version) || new Version(major, minor) < APIVersion)
                throw new PlatformNotSupportedException($"Unsupported OpenGL version (Minimum: {APIVersion})");
            if (!PlatformUtils.ValidateOpenGL())
                throw new PlatformNotSupportedException("OpenGL extension check failed");

            Instance = this;
            DefaultWindow = new BackgroundWindow();

            DiscordManager.Init();
            Settings.Init();

            CheckForUpdates();
            MessageBox.Instance?.Close();

            Mapping.LoadCache();

            OnMouseWheel(new MouseWheelEventArgs());
            Windowing.SwitchWindow(new GuiWindowMenu());

            if (!string.IsNullOrWhiteSpace(InitialFile))
                Mapping.Load(InitialFile);
        }

        public void UpdateFPS(VSyncMode mode, float fps)
        {
            if (Context.IsCurrent)
                VSync = mode;

            float max = Settings.fpsLimit.Value.Max;

            UpdateFrequency = Math.Round(fps) == Math.Round(max) ? 0f : fps + 60f;
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

            

            if (PlatformUtils.IsLinux) // because all the other events for this are a key behind on linux (???)
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
            GLState.ResetScissor();

            if (MusicPlayer.IsPlaying)
                Settings.currentTime.Value.Value = (float)MusicPlayer.CurrentTime.TotalMilliseconds;
            
            try
            {
                Windowing.Render(MouseState.X, MouseState.Y, (float)args.Time);
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
                double start = GC.GetTotalPauseDuration().TotalMilliseconds;

                GC.Collect();
                gcTime = 0;

                double duration = GC.GetTotalPauseDuration().TotalMilliseconds - start;

                if (duration > 50)
                {
                    // something is causing the gc to have a noticeable lag spike when it runs, so maybe now its doing more harm than good
                    Logging.Log($"GC took {duration}ms to process! Disabling forced garbage collection to improve performance");
                    gcEnabled = false;
                }
            }

            DiscordManager.Process(args.Time);
        }

        public void ForceRender() => OnRenderFrame(new FrameEventArgs());

        protected override void OnTextInput(TextInputEventArgs e)
        {
            Windowing.TextInput(e.AsString);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            if (e.Width > 0 && e.Height > 0)
            {
                base.OnResize(e);
                GL.Viewport(0, 0, e.Width, e.Height);
                Shader.SetViewports(e.Width, e.Height);

                Windowing.Resize(e);
                OnRenderFrame(new FrameEventArgs());
            }
        }

        public void LockClick() => GuiWindow.LockClick = true;
        public void UnlockClick() => GuiWindow.LockClick = false;

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Windowing.MouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            Windowing.MouseUp(e);
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            CtrlHeld = e.Control;
            AltHeld = e.Alt;
            ShiftHeld = e.Shift;

            Windowing.KeyUp(e.Key);
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

            Windowing.KeyDown(e.Key);
        }

        protected override void OnFileDrop(FileDropEventArgs e)
        {
            foreach (string file in e.FileNames)
            {
                if (File.Exists(file))
                    Windowing.FileDrop(file);
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            KeyboardState keyboard = KeyboardState;

            CtrlHeld = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);
            AltHeld = keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt);
            ShiftHeld = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);

            Windowing.MouseScroll(e.OffsetY);
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
                Windowing.SwitchWindow(new GuiWindowMenu());
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
                        catch (Exception ex) { Logging.Log($"Failed to extract file: {entry.FullName}", LogSeverity.ERROR, ex); }
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

                if (PlatformUtils.ExecutableExists(file))
                {
                    string current = PlatformUtils.IsLinux ? setting.Value : PlatformUtils.GetExecutableVersionInfo(file).FileVersion ?? "";
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

                if (PlatformUtils.ExecutableExists("SSQE Updater") && redirect != "")
                {
                    string version = redirect[(redirect.LastIndexOf('/') + 1)..];

                    Logging.Log("Checking version of editor");
                    if (Version.Parse(version) > Version.Parse(Program.Version))
                    {
                        DialogResult diag = MessageBox.Show($"New Editor version is available ({version}). Would you like to download the new version?", MBoxIcon.Info, MBoxButtons.Yes_No);

                        if (diag == DialogResult.Yes)
                        {
                            Logging.Log("Attempting to run updater");
                            PlatformUtils.RunExecutable("SSQE Updater", "");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Failed to check for updates", LogSeverity.ERROR, ex);
                MessageBox.Show("Failed to check for updates", MBoxIcon.Warning, MBoxButtons.OK);
            }
        }
    }
}
