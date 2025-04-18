using New_SSQE.NewGUI.Base;

namespace New_SSQE.NewGUI.Shaders
{
    internal class SpriteShader : Shader
    {
        private const string vertex = @"#version 330 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec4 aTexCoord; // only using x/y but inputting vec4 for compatibility
layout (location = 2) in vec4 aOffset; // x, y, s, a
layout (location = 3) in vec3 aExtra; // i, c, unused

out vec2 texCoord;
out vec4 aColor;

uniform vec2 SpriteSize;
uniform mat4 Projection;
uniform vec4 NoteColors[32];

void main()
{
    int yOff = int(aExtra.x * SpriteSize.x + SpriteSize.y / 2.0f);
    int xOff = int(aExtra.x - yOff / SpriteSize.x + SpriteSize.x / 2.0f);

    gl_Position = Projection * vec4(aPosition.xy * aOffset.z + aOffset.xy, 0.0f, 1.0f);

    texCoord = vec2((aTexCoord.x + xOff) * SpriteSize.x, (aTexCoord.y + yOff) * SpriteSize.y);
    aColor = vec4(NoteColors[int(aExtra.y)].xyz, aOffset.w);
}";

        private const string fragment = @"#version 330 core
out vec4 FragColor;

in vec2 texCoord;
in vec4 aColor;

uniform sampler2D texture0;

void main()
{
    vec4 color = texture(texture0, texCoord);
    FragColor = color * aColor;
}";

        public SpriteShader() : base(vertex, fragment) { }
    }
}
