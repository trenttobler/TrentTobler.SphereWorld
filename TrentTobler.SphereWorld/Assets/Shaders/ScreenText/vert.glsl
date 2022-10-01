#version 330 core

uniform mat4 view;

in vec2 aPos;

out vec2 coord;

void main()
{
    gl_Position = vec4(aPos, 0, 1) * view;

    mat3 coordMap = mat3(
        0.5, 0.0, 0.5,
        0.0, -0.5, 0.5,
        0.0, 0.0, 1.0);

    coord = vec2(
        0.5 + aPos.x * 0.5,
        0.5 - aPos.y * 0.5);
}
