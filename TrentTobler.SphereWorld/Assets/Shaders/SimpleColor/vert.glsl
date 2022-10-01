#version 330 core

in vec3 aPos;
in vec2 tPos;
in vec3 norm;

out vec2 texCoord;
out vec4 vertexColor;

uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;

void main()
{
    gl_Position = vec4(aPos,1) * model * view * projection;
    texCoord = tPos;

    float shade = (norm.z + 2) / 3;
    vertexColor = vec4(shade, shade, shade, 1);
}
