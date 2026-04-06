using New_SSQE.NewGUI.Shaders;
using OpenTK.Mathematics;
using System.Drawing;

namespace New_SSQE.NewGUI.Base
{
    internal abstract class Shader
    {
        public static readonly Shader Default = new DefaultShader();
        public static readonly Shader Texture = new TextureShader();
        public static readonly Shader FBOTexture = new FBOTextureShader();
        public static readonly Shader FBOMain = new FBOMainShader();
        public static readonly Shader FBOObject = new FBOObjectShader();
        public static readonly Shader InstancedMain = new InstancedMainShader();
        public static readonly Shader InstancedObject = new InstancedObjectShader();
        public static readonly Shader InstancedMainExtra = new InstancedMainExtraShader();
        public static readonly Shader InstancedObjectExtra = new InstancedObjectExtraShader();
        public static readonly Shader Sprite = new SpriteShader();
        public static readonly Shader Waveform = new WaveformShader();
        public static readonly Shader Font = new FontShader();
        public static readonly Shader Unicode = new UnicodeShader();

        public static void SetViewports(float width, float height)
        {
            Default.SetViewport(width, height);
            Texture.SetViewport(width, height);
            InstancedMain.SetViewport(width, height);
            InstancedObject.SetViewport(width, height);
            InstancedMainExtra.SetViewport(width, height);
            InstancedObjectExtra.SetViewport(width, height);
            Sprite.SetViewport(width, height);
            Waveform.SetViewport(width, height);
            Font.SetViewport(width, height);
            Unicode.SetViewport(width, height);
        }

        private const string defaultVertex = @"#version 330 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec4 aColor;
out vec4 vertexColor;

uniform mat4 Projection;
                                                
void main()
{
    gl_Position = Projection * vec4(aPosition, 0.0f, 1.0f);
    vertexColor = aColor;
}";

        private const string defaultFragment = @"#version 330 core
out vec4 FragColor;
in vec4 vertexColor;
                                                
void main()
{
    FragColor = vertexColor;
}";

        protected string Vertex;
        protected string Fragment;

        private int program;
        private bool compiled;

        protected Shader(string? vertex = null, string? fragment = null, bool lazy = false)
        {
            Vertex = vertex ?? defaultVertex;
            Fragment = fragment ?? defaultFragment;

            if (!lazy)
                Compile();
        }

        public virtual void Compile()
        {
            if (!compiled)
                program = GLState.CompileShader(Vertex, Fragment);
            compiled = true;
        }

        public void Invalidate() => compiled = false;

        public void Enable()
        {
            if (!compiled)
                Compile();
            GLState.EnableProgram(program);
        }

        public void SetViewport(float width, float height)
        {
            Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, width, height, 0, 0, 1);

            UniformMatrix4("Projection", projection);
        }

        public void Uniform1(string uniform, float[] values)
        {
            if (!compiled)
                Compile();
            GLState.Uniform1(program, uniform, values);
        }
        public void Uniform1(string uniform, float x)
        {
            if (!compiled)
                Compile();
            GLState.Uniform1(program, uniform, x);
        }
        public void Uniform1i(string uniform, int[] values)
        {
            if (!compiled)
                Compile();
            GLState.Uniform1i(program, uniform, values);
        }
        public void Uniform1i(string uniform, int x)
        {
            if (!compiled)
                Compile();
            GLState.Uniform1i(program, uniform, x);
        }
        public void Uniform2(string uniform, Vector2[] values)
        {
            if (!compiled)
                Compile();
            GLState.Uniform2(program, uniform, values);
        }
        public void Uniform2(string uniform, Vector2 value)
        {
            if (!compiled)
                Compile();
            GLState.Uniform2(program, uniform, value);
        }
        public void Uniform2(string uniform, float x, float y)
        {
            if (!compiled)
                Compile();
            GLState.Uniform2(program, uniform, x, y);
        }
        public void Uniform3(string uniform, Vector3[] values)
        {
            if (!compiled)
                Compile();
            GLState.Uniform3(program, uniform, values);
        }
        public void Uniform3(string uniform, Vector3 value)
        {
            if (!compiled)
                Compile();
            GLState.Uniform3(program, uniform, value);
        }
        public void Uniform3(string uniform, float x, float y, float z)
        {
            if (!compiled)
                Compile();
            GLState.Uniform3(program, uniform, x, y, z);
        }
        public void Uniform3(string uniform, Color value)
        {
            if (!compiled)
                Compile();
            GLState.Uniform3(program, uniform, value);
        }
        public void Uniform4(string uniform, Vector4[] values)
        {
            if (!compiled)
                Compile();
            GLState.Uniform4(program, uniform, values);
        }
        public void Uniform4(string uniform, Vector4 value)
        {
            if (!compiled)
                Compile();
            GLState.Uniform4(program, uniform, value);
        }
        public void Uniform4(string uniform, float x, float y, float z, float w)
        {
            if (!compiled)
                Compile();
            GLState.Uniform4(program, uniform, x, y, z, w);
        }
        public void UniformMatrix4(string uniform, Matrix4 value)
        {
            if (!compiled)
                Compile();
            GLState.UniformMatrix4(program, uniform, value);
        }
    }
}
