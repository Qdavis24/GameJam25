shader_type canvas_item;

uniform float amplitude = 3.0; // Wave size in pixels
uniform float frequency = 6.0; // How many waves along the height
uniform float speed = 1.0;     // Wave animation speed

void fragment() {
    vec2 uv = UV;

    // Convert pixel amplitude to UV space
    float pixel_to_uv = TEXTURE_PIXEL_SIZE.x; // assuming roughly uniform pixels
    float offset = sin(uv.y * frequency + TIME * speed) * amplitude * pixel_to_uv;

    // Offset horizontally for a scroll-like sway
    uv.x += offset;

    COLOR = texture(TEXTURE, uv) * texture(TEXTURE, uv).a;
}