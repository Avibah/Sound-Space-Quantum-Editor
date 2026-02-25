using New_SSQE.NewGUI.Base;
using New_SSQE.Services;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Drawing;

namespace New_SSQE.NewGUI
{
    internal static class GLState
    {
        public static int? testBuffer = null;

        private static int? _program;
        private static int? _texture;
        private static int? _vao;
        private static int? _vbo;
        private static int? _rbo;

        private static readonly Dictionary<FramebufferTarget, int?> _fbo = new()
        {
            {FramebufferTarget.Framebuffer, null },
            {FramebufferTarget.ReadFramebuffer, null },
            {FramebufferTarget.DrawFramebuffer, null }
        };

        public static unsafe void EnableProgram(int program)
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

        public static void EnableTexture(int texture)
        {
            //if (texture != _texture)
                GL.BindTexture(TextureTarget.Texture2d, texture);
            _texture = texture;
        }

        public static void EnableVAO(int vao)
        {
            if (vao != _vao)
                GL.BindVertexArray(vao);
            _vao = vao;
        }

        public static void EnableVBO(int vbo)
        {
            if (vbo != _vbo)
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            _vbo = vbo;
        }

        public static void BufferData(int vbo, float[] data)
        {
            EnableVBO(vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsage.StaticDraw);
        }

        public static void BufferData(int vbo, Vector4[] data)
        {
            EnableVBO(vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float) * 4, data, BufferUsage.StaticDraw);
        }

        public static void BufferData(int vbo, Vector3[] data)
        {
            EnableVBO(vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float) * 3, data, BufferUsage.StaticDraw);
        }

        public static (int, int) NewVAO_VBO(params int[] fieldWidths)
        {
            int vao = GL.GenVertexArray();
            EnableVAO(vao);

            int vbo = GL.GenBuffer();
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

        public static int ExtendInstancingVAO(int vao, int location, int width)
        {
            EnableVAO(vao);

            int vbo = GL.GenBuffer();
            EnableVBO(vbo);

            GL.VertexAttribPointer((uint)location, width, VertexAttribPointerType.Float, false, width * sizeof(float), 0);
            GL.EnableVertexAttribArray((uint)location);
            GL.VertexAttribDivisor((uint)location, 1);

            DisableVAO_VBO();

            return vbo;
        }

        public static void DisableVAO_VBO()
        {
            EnableVAO(0);
            EnableVBO(0);
        }

        public static void LoadTexture(int texture, int width, int height, nint pixels, TextureUnit texUnit)
        {
            EnableTextureUnit(Shader.Texture, texUnit);
            EnableTexture(texture);

            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, width, height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, pixels);
        }

        public static int NewTexture(TextureUnit texUnit, bool smooth = false)
        {
            int texture = GL.GenTexture();
            EnableTextureUnit(Shader.Texture, texUnit);
            EnableTexture(texture);

            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)(smooth ? TextureMinFilter.Linear : TextureMinFilter.Nearest));
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)(smooth ? TextureMagFilter.Linear : TextureMagFilter.Nearest));

            return texture;
        }

        public static int NewTexture(int width, int height, nint pixels, TextureUnit texUnit)
        {
            int texture = NewTexture(texUnit);
            LoadTexture(texture, width, height, pixels, texUnit);

            return texture;
        }

        public static void DrawTriangles(int first, int count) => GL.DrawArrays(PrimitiveType.Triangles, first, count);

        public static void DrawTriangles(int vao, int first, int count)
        {
            EnableVAO(vao);
            DrawTriangles(first, count);
        }

        public static void DrawInstances(int first, int count, int instances) => GL.DrawArraysInstanced(PrimitiveType.Triangles, first, count, instances);

        public static void DrawInstances(int vao, int first, int count, int instances)
        {
            EnableVAO(vao);
            DrawInstances(first, count, instances);
        }

        public static void DrawArrays(PrimitiveType type, int first, int count) => GL.DrawArrays(type, first, count);

        public static void DrawArrays(int vao, PrimitiveType type, int first, int count)
        {
            EnableVAO(vao);
            DrawArrays(type, first, count);
        }

        public static void CleanVAO(int vao)
        {
            if (_vao == vao)
                _vao = null;
            GL.DeleteVertexArray(vao);
        }

        public static void CleanVBO(int vbo)
        {
            if (_vbo == vbo)
                _vbo = null;
            GL.DeleteBuffer(vbo);
        }

        public static void CleanTex(int texture)
        {
            if (_texture == texture)
                _texture = null;
            GL.DeleteTexture(texture);
        }

        public static void CleanFBO(int fbo)
        {
            foreach (KeyValuePair<FramebufferTarget, int?> _fboTemp in _fbo)
            {
                if (_fboTemp.Value == fbo)
                    _fbo[_fboTemp.Key] = null;
            }

            GL.DeleteFramebuffer(fbo);
        }

        public static void CleanRBO(int rbo)
        {
            if (_rbo == rbo)
                _rbo = null;
            GL.DeleteRenderbuffer(rbo);
        }

        public static void Uniform1(int program, string uniform, float[] values)
        {
            EnableProgram(program);

            int location = GL.GetUniformLocation(program, uniform);
            GL.Uniform1f(location, values.Length, values);
        }
        public static void Uniform1(int program, string uniform, float x) => Uniform1(program, uniform, [x]);

        public static void Uniform1i(int program, string uniform, int[] values)
        {
            EnableProgram(program);

            int location = GL.GetUniformLocation(program, uniform);
            GL.Uniform1i(location, values.Length, values);
        }
        public static void Uniform1i(int program, string uniform, int x) => Uniform1i(program, uniform, [x]);

        public static void Uniform2(int program, string uniform, Vector2[] values)
        {
            EnableProgram(program);

            int location = GL.GetUniformLocation(program, uniform);
            GL.Uniform2f(location, values.Length, values);
        }
        public static void Uniform2(int program, string uniform, Vector2 value) => Uniform2(program, uniform, [value]);
        public static void Uniform2(int program, string uniform, float x, float y) => Uniform2(program, uniform, [(x, y)]);

        public static void Uniform3(int program, string uniform, Vector3[] values)
        {
            EnableProgram(program);

            int location = GL.GetUniformLocation(program, uniform);
            GL.Uniform3f(location, values.Length, values);
        }
        public static void Uniform3(int program, string uniform, Vector3 value) => Uniform3(program, uniform, [value]);
        public static void Uniform3(int program, string uniform, float x, float y, float z) => Uniform3(program, uniform, [(x, y, z)]);
        public static void Uniform3(int program, string uniform, Color value) => Uniform3(program, uniform, [(value.R / 255f, value.G / 255f, value.B / 255f)]);

        public static void Uniform4(int program, string uniform, Vector4[] values)
        {
            EnableProgram(program);

            int location = GL.GetUniformLocation(program, uniform);
            GL.Uniform4f(location, values.Length, values);
        }
        public static void Uniform4(int program, string uniform, Vector4 value) => Uniform4(program, uniform, [value]);
        public static void Uniform4(int program, string uniform, float x, float y, float z, float w) => Uniform4(program, uniform, [(x, y, z, w)]);

        public static void UniformMatrix4(int program, string uniform, Matrix4 value)
        {
            EnableProgram(program);

            int location = GL.GetUniformLocation(program, uniform);
            GL.UniformMatrix4f(location, 1, false, ref value);
        }

        public static void EnableFBO(int fbo, FramebufferTarget target = FramebufferTarget.Framebuffer)
        {
            if (fbo != _fbo[target])
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            _fbo[target] = fbo;
        }

        public static void EnableRBO(int rbo)
        {
            if (rbo != _rbo)
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo);
            _rbo = rbo;
        }

        public static void DisableFBO_RBO()
        {
            EnableFBO(0, FramebufferTarget.ReadFramebuffer);
            EnableFBO(0, FramebufferTarget.DrawFramebuffer);
            EnableFBO(0);

            EnableRBO(0);
        }

        public static void CreateFBO(out int fbo, out int msaa_fbo, out int rbo, out int fbo_tex)
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

        public static void AllocateFBO(float vpW, float vpH, int msaa_fbo, int rbo, int fbo_tex)
        {
            EnableRBO(rbo);
            EnableFBO(msaa_fbo);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, Math.Min(MainWindow.MaxSamples, 32), InternalFormat.Rgba8, (int)vpW, (int)vpH);

            DisableFBO_RBO();
            EnableTextureUnit(Shader.FBOTexture, TextureUnit.Texture3);
            EnableTexture(fbo_tex);
            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, (int)vpW, (int)vpH, 0, PixelFormat.Rgba, PixelType.UnsignedByte, nint.Zero);
        }

        public static void BeginFBORender(RectangleF viewport, int msaa_fbo)
        {
            EnableFBO(msaa_fbo);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.StencilBufferBit);
            GL.Viewport((int)viewport.X, (int)viewport.Y, (int)viewport.Width, (int)viewport.Height);
        }
        public static void BeginFBORender(float vpX, float vpY, float vpW, float vpH, int msaa_fbo) => BeginFBORender(new(vpX, vpY, vpW, vpH), msaa_fbo);

        public static void EndFBORender(RectangleF viewport, int fbo, int msaa_fbo)
        {
            EnableFBO(msaa_fbo, FramebufferTarget.ReadFramebuffer);
            EnableFBO(fbo, FramebufferTarget.DrawFramebuffer);

            GL.BlitFramebuffer((int)viewport.X, (int)viewport.Y, (int)viewport.Width, (int)viewport.Height,
                (int)viewport.X, (int)viewport.Y, (int)viewport.Width, (int)viewport.Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
            DisableFBO_RBO();
        }
        public static void EndFBORender(float vpX, float vpY, float vpW, float vpH, int fbo, int msaa_fbo) => EndFBORender(new(vpX, vpY, vpW, vpH), fbo, msaa_fbo);

        public static int CompileShader(string vertex, string fragment)
        {
            int _vertex = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(_vertex, vertex);
            GL.CompileShader(_vertex);

            GL.GetShaderInfoLog(_vertex, out string vertexLog);
            if (!string.IsNullOrWhiteSpace(vertexLog))
                Logging.Log($"Failed to compile vertex shader: {vertexLog}\n{vertex}");

            int _fragment = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(_fragment, fragment);
            GL.CompileShader(_fragment);

            GL.GetShaderInfoLog(_fragment, out string fragmentLog);
            if (!string.IsNullOrWhiteSpace(fragmentLog))
                Logging.Log($"Failed to compile fragment shader: {fragmentLog}\n{fragment}");

            int program = GL.CreateProgram();
            GL.AttachShader(program, _vertex);
            GL.AttachShader(program, _fragment);

            GL.LinkProgram(program);

            GL.DetachShader(program, _vertex);
            GL.DetachShader(program, _fragment);

            GL.DeleteShader(_vertex);
            GL.DeleteShader(_fragment);

            return program;
        }

        private static Stack<Vector4i> scissors = [];
        private static int _scissor = 0;

        public static void ResetScissor()
        {
            Box2i rect = MainWindow.Instance?.ClientRectangle ?? Box2i.Empty;
            Vector4i vec = (0, 0, rect.Width, rect.Height);

            scissors = [];
            scissors.Push(vec);

            GL.Scissor(vec.X, vec.Y, vec.Z, vec.W);
            GL.Disable(EnableCap.ScissorTest);
        }

        public static void EnableScissor(float x, float y, float w, float h)
        {
            y = MainWindow.Instance.ClientSize.Y - y - h;

            Vector4i prev = scissors.Peek();
            if (prev == Vector4i.Zero)
                return;

            w -= Math.Max(0, prev.X - x);
            h -= Math.Max(0, prev.Y - y);
            x = Math.Clamp(x, prev.X, prev.X + prev.Z) + 0.5f;
            y = Math.Clamp(y, prev.Y, prev.Y + prev.W) + 0.5f;
            w = Math.Clamp(x + w, x, prev.X + prev.Z) - x + 0.5f;
            h = Math.Clamp(y + h, y, prev.Y + prev.W) - y + 0.5f;

            Vector4i vec = ((int)x, (int)y, (int)w, (int)h);
            GL.Scissor(vec.X, vec.Y, vec.Z, vec.W);
            scissors.Push(vec);

            if (_scissor++ == 0)
                GL.Enable(EnableCap.ScissorTest);
        }

        public static void EnableScissor(RectangleF rect) => EnableScissor(rect.X, rect.Y, rect.Width, rect.Height);

        public static void DisableScissor()
        {
            if (scissors.Count > 0)
            {
                if (scissors.Count == 1 && scissors.Peek() == Vector4i.Zero)
                    return;

                scissors.Pop();
                Vector4i vec = scissors.Peek();
                GL.Scissor(vec.X, vec.Y, vec.Z, vec.W);
            }
            if (--_scissor == 0)
                GL.Disable(EnableCap.ScissorTest);
        }

        public static void PreStencilMask()
        {
            GL.Clear(ClearBufferMask.StencilBufferBit);
            GL.Enable(EnableCap.StencilTest);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            GL.StencilMask(0xFF);
        }

        public static void StencilMask()
        {
            GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
            GL.StencilMask(0x00);
        }

        public static void PostStencilMask()
        {
            GL.Disable(EnableCap.StencilTest);
        }
    }
}
