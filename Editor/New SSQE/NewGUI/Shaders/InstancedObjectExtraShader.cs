using New_SSQE.NewGUI.Base;

namespace New_SSQE.NewGUI.Shaders
{
    internal class InstancedObjectExtraShader : Shader
    {
        private const string vertex = @"#version 330 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec4 aColor; // only using alpha but inputting vec4 for compatibility
layout (location = 2) in vec4 aOffset; // x, y, s, c/a, where c/a = 2 * int(color) + a
layout (location = 3) in vec3 aExtra; // w, h, r, where r is in radians

out vec4 vertexColor;

uniform mat4 Projection;
uniform vec4 NoteColors[32];

void main()
{
    int c = int(aOffset.w / 2.0f);
    float a = aOffset.w - c * 2.0f;

    float sinR = sin(aExtra.z);
    float cosR = cos(aExtra.z);

    float x = cosR * aPosition.x - sinR * aPosition.y;
    float y = sinR * aPosition.x + cosR * aPosition.y;

    gl_Position = Projection * vec4(vec2(x * aExtra.x, y * aExtra.y) * aOffset.z + aOffset.xy, 0.0f, 1.0f);
    vertexColor = vec4(NoteColors[c].xyz, NoteColors[c].w * aColor.w * a);
}";

        public InstancedObjectExtraShader() : base(vertex) { }
    }
}
