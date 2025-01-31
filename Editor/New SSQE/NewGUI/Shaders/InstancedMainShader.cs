using New_SSQE.NewGUI.Base;

namespace New_SSQE.NewGUI.Shaders
{
    internal class InstancedMainShader : Shader
    {
        private const string vertex = @"#version 330 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec4 aColor; // only using alpha but inputting vec4 for compatibility
layout (location = 2) in vec4 aOffset; // x, y, s, c/a where c/a = 2 * int(color) + a

out vec4 vertexColor;

uniform mat4 Projection;
uniform vec4 Colors[11];

void main()
{
    int c = int(aOffset.w / 2.0f);
    float a = aOffset.w - c * 2.0f;

    gl_Position = Projection * vec4(aPosition * aOffset.z + aOffset.xy, 0.0f, 1.0f);
    vertexColor = vec4(Colors[c].xyz * aColor.xyz, aColor.w * a);
}";

        public InstancedMainShader() : base(vertex) { }
    }
}
