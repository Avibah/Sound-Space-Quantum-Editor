using New_SSQE.ExternalUtils;
using New_SSQE.NewGUI.Base;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Drawing;

namespace New_SSQE.NewGUI
{
    internal static class GLState
    {
        private static ProgramHandle? _program;
        private static TextureHandle? _texture;
        private static VertexArrayHandle? _vao;
        private static BufferHandle? _vbo;
        private static RenderbufferHandle? _rbo;

        private static readonly Dictionary<FramebufferTarget, FramebufferHandle?> _fbo = new()
        {
            {FramebufferTarget.Framebuffer, null },
            {FramebufferTarget.ReadFramebuffer, null },
            {FramebufferTarget.DrawFramebuffer, null }
        };

        public static unsafe void EnableProgram(ProgramHandle program)
        {
            if (program != _program)
                GL.UseProgram(program);
            _program = program;
        }

        public static void EnableTextureUnit(Shader shader, TextureUnit texUnit)
        {
            shader.Enable();
            GL.ActiveTexture(texUnit);

            shader.Uniform1i("texture0", (int)texUnit - (int)TextureUnit.Texture0);
        }

        public static void EnableTexture(TextureHandle texture)
        {
            if (texture != _texture)
                GL.BindTexture(TextureTarget.Texture2d, texture);
            _texture = texture;
        }

        public static void EnableVAO(VertexArrayHandle vao)
        {
            if (vao != _vao)
                GL.BindVertexArray(vao);
            _vao = vao;
        }

        public static void EnableVBO(BufferHandle vbo)
        {
            if (vbo != _vbo)
                GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            _vbo = vbo;
        }

        public static void BufferData(BufferHandle vbo, float[] data)
        {
            EnableVBO(vbo);
            GL.BufferData(BufferTargetARB.ArrayBuffer, data, BufferUsageARB.StaticDraw);
        }

        public static void BufferData(BufferHandle vbo, Vector4[] data)
        {
            EnableVBO(vbo);
            GL.BufferData(BufferTargetARB.ArrayBuffer, data, BufferUsageARB.StaticDraw);
        }

        public static void BufferData(BufferHandle vbo, Vector3[] data)
        {
            EnableVBO(vbo);
            GL.BufferData(BufferTargetARB.ArrayBuffer, data, BufferUsageARB.StaticDraw);
        }

        public static (VertexArrayHandle, BufferHandle) NewVAO_VBO(params int[] fieldWidths)
        {
            VertexArrayHandle vao = GL.GenVertexArray();
            EnableVAO(vao);

            BufferHandle vbo = GL.GenBuffer();
            EnableVBO(vbo);

            int stride = fieldWidths.Sum();
            int offset = 0;

            for (int i = 0; i < fieldWidths.Length; i++)
            {
                GL.VertexAttribPointer((uint)i, fieldWidths[i], VertexAttribPointerType.Float, false, stride * sizeof(float), offset * sizeof(float));
                GL.EnableVertexAttribArray((uint)i);

                offset += fieldWidths[i];
            }

            DisableVAO_VBO();

            return (vao, vbo);
        }

        public static BufferHandle ExtendInstancingVAO(VertexArrayHandle vao, int location, int width)
        {
            EnableVAO(vao);

            BufferHandle vbo = GL.GenBuffer();
            EnableVBO(vbo);

            GL.VertexAttribPointer((uint)location, width, VertexAttribPointerType.Float, false, width * sizeof(float), 0);
            GL.EnableVertexAttribArray((uint)location);
            GL.VertexAttribDivisor((uint)location, 1);

            DisableVAO_VBO();

            return vbo;
        }

        public static void DisableVAO_VBO()
        {
            EnableVAO(VertexArrayHandle.Zero);
            EnableVBO(BufferHandle.Zero);
        }

        public static void LoadTexture(TextureHandle texture, int width, int height, nint pixels, TextureUnit texUnit)
        {
            EnableTextureUnit(Shader.Texture, texUnit);
            EnableTexture(texture);

            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, width, height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, pixels);
        }

        public static TextureHandle NewTexture(TextureUnit texUnit, bool smooth = false)
        {
            TextureHandle texture = GL.GenTexture();
            EnableTextureUnit(Shader.Texture, texUnit);
            EnableTexture(texture);

            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)(smooth ? TextureMinFilter.Linear : TextureMinFilter.Nearest));
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)(smooth ? TextureMagFilter.Linear : TextureMagFilter.Nearest));

            return texture;
        }

        public static TextureHandle NewTexture(int width, int height, nint pixels, TextureUnit texUnit)
        {
            TextureHandle texture = NewTexture(texUnit);
            LoadTexture(texture, width, height, pixels, texUnit);

            return texture;
        }

        public static void DrawTriangles(int first, int count) => GL.DrawArrays(PrimitiveType.Triangles, first, count);

        public static void DrawTriangles(VertexArrayHandle vao, int first, int count)
        {
            EnableVAO(vao);
            DrawTriangles(first, count);
        }

        public static void DrawInstances(int first, int count, int instances) => GL.DrawArraysInstanced(PrimitiveType.Triangles, first, count, instances);

        public static void DrawInstances(VertexArrayHandle vao, int first, int count, int instances)
        {
            EnableVAO(vao);
            DrawInstances(first, count, instances);
        }

        public static void DrawArrays(PrimitiveType type, int first, int count) => GL.DrawArrays(type, first, count);

        public static void DrawArrays(VertexArrayHandle vao, PrimitiveType type, int first, int count)
        {
            EnableVAO(vao);
            DrawArrays(type, first, count);
        }

        public static void Clean(VertexArrayHandle vao) => GL.DeleteVertexArray(vao);
        public static void Clean(BufferHandle vbo) => GL.DeleteBuffer(vbo);
        public static void Clean(TextureHandle texture) => GL.DeleteTexture(texture);
        public static void Clean(FramebufferHandle fbo) => GL.DeleteFramebuffer(fbo);
        public static void Clean(RenderbufferHandle rbo) => GL.DeleteRenderbuffer(rbo);

        public static void Uniform1(ProgramHandle program, string uniform, float[] values)
        {
            EnableProgram(program);

            int location = GL.GetUniformLocation(program, uniform);
            GL.Uniform1f(location, values.Length, values);
        }
        public static void Uniform1(ProgramHandle program, string uniform, float x) => Uniform1(program, uniform, [x]);

        public static void Uniform1i(ProgramHandle program, string uniform, int[] values)
        {
            EnableProgram(program);

            int location = GL.GetUniformLocation(program, uniform);
            GL.Uniform1i(location, values.Length, values);
        }
        public static void Uniform1i(ProgramHandle program, string uniform, int x) => Uniform1i(program, uniform, [x]);

        public static void Uniform2(ProgramHandle program, string uniform, Vector2[] values)
        {
            EnableProgram(program);

            int location = GL.GetUniformLocation(program, uniform);
            GL.Uniform2f(location, values.Length, values);
        }
        public static void Uniform2(ProgramHandle program, string uniform, Vector2 value) => Uniform2(program, uniform, [value]);
        public static void Uniform2(ProgramHandle program, string uniform, float x, float y) => Uniform2(program, uniform, [(x, y)]);

        public static void Uniform3(ProgramHandle program, string uniform, Vector3[] values)
        {
            EnableProgram(program);

            int location = GL.GetUniformLocation(program, uniform);
            GL.Uniform3f(location, values.Length, values);
        }
        public static void Uniform3(ProgramHandle program, string uniform, Vector3 value) => Uniform3(program, uniform, [value]);
        public static void Uniform3(ProgramHandle program, string uniform, float x, float y, float z) => Uniform3(program, uniform, [(x, y, z)]);
        public static void Uniform3(ProgramHandle program, string uniform, Color value) => Uniform3(program, uniform, [(value.R / 255f, value.G / 255f, value.B / 255f)]);

        public static void Uniform4(ProgramHandle program, string uniform, Vector4[] values)
        {
            EnableProgram(program);

            int location = GL.GetUniformLocation(program, uniform);
            GL.Uniform4f(location, values.Length, values);
        }
        public static void Uniform4(ProgramHandle program, string uniform, Vector4 value) => Uniform4(program, uniform, [value]);
        public static void Uniform4(ProgramHandle program, string uniform, float x, float y, float z, float w) => Uniform4(program, uniform, [(x, y, z, w)]);

        public static void UniformMatrix4(ProgramHandle program, string uniform, Matrix4 value)
        {
            EnableProgram(program);

            int location = GL.GetUniformLocation(program, uniform);
            GL.UniformMatrix4f(location, false, value);
        }

        public static void EnableFBO(FramebufferHandle fbo, FramebufferTarget target = FramebufferTarget.Framebuffer)
        {
            if (fbo != _fbo[target])
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            _fbo[target] = fbo;
        }

        public static void EnableRBO(RenderbufferHandle rbo)
        {
            if (rbo != _rbo)
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo);
            _rbo = rbo;
        }

        public static void DisableFBO_RBO()
        {
            EnableFBO(FramebufferHandle.Zero, FramebufferTarget.ReadFramebuffer);
            EnableFBO(FramebufferHandle.Zero, FramebufferTarget.DrawFramebuffer);
            EnableFBO(FramebufferHandle.Zero);

            EnableRBO(RenderbufferHandle.Zero);
        }

        public static void CreateFBO(out FramebufferHandle fbo, out FramebufferHandle msaa_fbo, out RenderbufferHandle rbo, out TextureHandle fbo_tex)
        {
            rbo = GL.GenRenderbuffer();
            EnableRBO(rbo);

            msaa_fbo = GL.GenFramebuffer();
            EnableFBO(msaa_fbo);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, rbo);

            fbo = GL.GenFramebuffer();
            EnableFBO(fbo);

            fbo_tex = NewTexture(TextureUnit.Texture3);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d, fbo_tex, 0);

            DisableFBO_RBO();
        }

        public static void AllocateFBO(float vpW, float vpH, FramebufferHandle msaa_fbo, RenderbufferHandle rbo, TextureHandle fbo_tex)
        {
            EnableRBO(rbo);
            EnableFBO(msaa_fbo);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, Math.Min(MainWindow.MaxSamples, 32), InternalFormat.Rgba8, (int)vpW, (int)vpH);

            DisableFBO_RBO();
            EnableTextureUnit(Shader.FBOTexture, TextureUnit.Texture3);
            EnableTexture(fbo_tex);
            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, (int)vpW, (int)vpH, 0, PixelFormat.Rgba, PixelType.UnsignedByte, nint.Zero);
        }

        public static void BeginFBORender(RectangleF viewport, FramebufferHandle msaa_fbo)
        {
            EnableFBO(msaa_fbo);

            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Viewport((int)viewport.X, (int)viewport.Y, (int)viewport.Width, (int)viewport.Height);
        }
        public static void BeginFBORender(float vpX, float vpY, float vpW, float vpH, FramebufferHandle msaa_fbo) => BeginFBORender(new(vpX, vpY, vpW, vpH), msaa_fbo);

        public static void EndFBORender(RectangleF viewport, FramebufferHandle fbo, FramebufferHandle msaa_fbo)
        {
            EnableFBO(msaa_fbo, FramebufferTarget.ReadFramebuffer);
            EnableFBO(fbo, FramebufferTarget.DrawFramebuffer);

            GL.BlitFramebuffer((int)viewport.X, (int)viewport.Y, (int)viewport.Width, (int)viewport.Height,
                (int)viewport.X, (int)viewport.Y, (int)viewport.Width, (int)viewport.Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
            DisableFBO_RBO();
        }
        public static void EndFBORender(float vpX, float vpY, float vpW, float vpH, FramebufferHandle fbo, FramebufferHandle msaa_fbo) => EndFBORender(new(vpX, vpY, vpW, vpH), fbo, msaa_fbo);

        public static ProgramHandle CompileShader(string vertex, string fragment)
        {
            ShaderHandle _vertex = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(_vertex, vertex);
            GL.CompileShader(_vertex);

            GL.GetShaderInfoLog(_vertex, out string vertexLog);
            if (!string.IsNullOrWhiteSpace(vertexLog))
                Logging.Register($"Failed to compile vertex shader: {vertexLog}\n{vertex}");

            ShaderHandle _fragment = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(_fragment, fragment);
            GL.CompileShader(_fragment);

            GL.GetShaderInfoLog(_fragment, out string fragmentLog);
            if (!string.IsNullOrWhiteSpace(fragmentLog))
                Logging.Register($"Failed to compile fragment shader: {fragmentLog}\n{fragment}");

            ProgramHandle program = GL.CreateProgram();
            GL.AttachShader(program, _vertex);
            GL.AttachShader(program, _fragment);

            GL.LinkProgram(program);

            GL.DetachShader(program, _vertex);
            GL.DetachShader(program, _fragment);

            GL.DeleteShader(_vertex);
            GL.DeleteShader(_fragment);

            return program;
        }
    }
}
