using System.Diagnostics;
using System.Runtime.InteropServices;
using New_SSQE.Misc;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace New_SSQE.ExternalUtils
{
    internal class Platforms
    {
        public static readonly bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static readonly string Extension = IsLinux ? "" : ".exe";
        public static readonly string ExecutableFilter = IsLinux ? "Executable Files|*.*" : "Executable Files (*.exe)|*.exe";

        private static readonly string[] dependencies_Windows = [];
        private static readonly string[] dependencies_Linux = [];

        public static void OpenDirectory(string directory)
        {
            if (IsLinux)
                Process.Start("xdg-open", directory);
            else
                Process.Start("explorer.exe", directory);
        }
        public static void OpenDirectory() => OpenDirectory(Assets.THIS);

        public static Process? RunExecutable(string path, string exe, string args)
        {
            try
            {
                return Process.Start(Path.Combine(path, GetExecutableName(exe)), args);
            }
            catch (Exception ex)
            {
                Logging.Log($"Failed to run executable: {path} | {exe} | {args}", LogSeverity.ERROR, ex);
            }

            return null;
        }
        public static Process? RunExecutable(string path, string exe, params string[] args) => RunExecutable(path, exe, string.Join(" ", args));
        public static Process? RunExecutable(string exe, string args) => RunExecutable(Assets.THIS, exe, args);

        public static bool ExecutableExists(string path, string exe) => File.Exists(Path.Combine(path, GetExecutableName(exe)));
        public static bool ExecutableExists(string exe) => ExecutableExists(Assets.THIS, exe);

        public static FileVersionInfo GetExecutableVersionInfo(string path, string exe) => FileVersionInfo.GetVersionInfo(Path.Combine(path, $"{exe}{Extension}"));
        public static FileVersionInfo GetExecutableVersionInfo(string exe) => GetExecutableVersionInfo(Assets.THIS, exe);

        public static string GetExecutableName(string exe) => $"{exe}{Extension}";

        public static bool CheckDependency(string path, string dependency)
        {
            dependency = IsLinux ? $"lib{dependency}.so" : $"{dependency}.dll";

            return File.Exists(Path.Combine(path, dependency));
        }
        public static bool CheckDependency(string dependency) => CheckDependency(Assets.THIS, dependency);

        public static void CheckDependencies()
        {
            string[] deps = IsLinux ? dependencies_Linux : dependencies_Windows;

            foreach (string file in deps)
            {
                if (!CheckDependency(file))
                    throw new DllNotFoundException($"Missing dependency '{Path.GetFileName(file)}' - Reinstall the editor to repair");
            }
        }

        public static Process? OpenLink(string url)
        {
            ProcessStartInfo ps = new(url)
            {
                UseShellExecute = true,
                Verb = "open"
            };

            return Process.Start(ps);
        }

        public static readonly Version RequestedAPI = new(3, 3);

        private static readonly string[] requiredAPI =
        [
            "glVertexAttribDivisor",            // 3.3

            "glDrawArraysInstanced",            // 3.1

            "glBindVertexArray",                // 3.0
            "glBindRenderbuffer",               // 3.0
            "glBindFramebuffer",                // 3.0
            "glBlitFramebuffer",                // 3.0
            "glGenRenderbuffers",               // 3.0
            "glGenFramebuffers",                // 3.0
            "glGenVertexArrays",                // 3.0
            "glDeleteVertexArrays",             // 3.0
            "glRenderbufferStorageMultisample", // 3.0
            "glFramebufferRenderbuffer",        // 3.0

            "glBindBuffer",                     // 2.0
            "glBindTexture",                    // 2.0
            "glActiveTexture",                  // 2.0
            "glGenTextures",                    // 2.0
            "glGenBuffers",                     // 2.0
            "glGetUniformLocation",             // 2.0
            "glUniform4f",                      // 2.0
            "glUniform3f",                      // 2.0
            "glUniform2f",                      // 2.0
            "glUniform1i",                      // 2.0
            "glUniformMatrix4fv",               // 2.0
            "glDrawArrays",                     // 2.0
            "glBufferData",                     // 2.0
            "glDeleteTextures",                 // 2.0
            "glDeleteBuffers",                  // 2.0
            "glVertexAttribPointer",            // 2.0
            "glUseProgram",                     // 2.0
            "glTexParameteri",                  // 2.0
            "glTexImage2D",                     // 2.0
            "glEnableVertexAttribArray",        // 2.0
            "glViewport",                       // 2.0
            "glClear",                          // 2.0
            "glClearColor",                     // 2.0
            "glGetString",                      // 2.0
            "glGetError",                       // 2.0
            "glGetIntegerv",                    // 2.0
            "glEnable",                         // 2.0
            "glDisable",                        // 2.0
            "glBlendFunc",                      // 2.0
            "glCreateShader",                   // 2.0
            "glCreateProgram",                  // 2.0
            "glShaderSource",                   // 2.0
            "glLinkProgram",                    // 2.0
            "glGetShaderInfoLog",               // 2.0
            "glDetachShader",                   // 2.0
            "glDeleteShader",                   // 2.0
            "glCompileShader",                  // 2.0
            "glAttachShader",                   // 2.0
            "glScissor"                         // 2.0
        ];

        private static readonly string[] optionalAPI =
        [
            "glDebugMessageCallback"            // 4.3
        ];

        public static bool ValidateOpenGL()
        {
            bool result = true;

            foreach (string func in requiredAPI)
            {
                if (GLFW.GetProcAddress(func) <= 0)
                {
                    result = false;
                    Logging.Log($"Missing OpenGL API: {func}", LogSeverity.FATAL);
                }
            }

            foreach (string func in optionalAPI)
            {
                if (GLFW.GetProcAddress(func) <= 0)
                {
                    Logging.Log($"Missing OpenGL API: {func} - Some features may be unavailable", LogSeverity.WARN);
                }
            }

            return result;
        }
    }
}
