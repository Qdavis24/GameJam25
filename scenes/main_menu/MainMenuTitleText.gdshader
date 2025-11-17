shader_type canvas_item;

// Wavy wobble (haunted breathing)
uniform float wobble_strength : hint_range(0.0, 10.0) = 2.0; // pixels
uniform float wobble_speed    : hint_range(0.0, 20.0) = 8.0;
uniform float wobble_freq     : hint_range(0.0, 40.0) = 20.0;

// Subtle shaky movement
uniform float shake_strength  : hint_range(0.0, 5.0) = 1.0; // pixels
uniform float shake_speed     : hint_range(0.0, 50.0) = 25.0;

void fragment() {
    vec2 uv = UV;
    float t = TIME;

    // Convert pixel strengths to UV scale
    vec2 px = TEXTURE_PIXEL_SIZE;

    // Haunted wobble (vertical wave)
    float wobble = sin(uv.x * wobble_freq + t * wobble_speed)
                    * wobble_strength * px.y;
    uv.y += wobble;

    // Subtle jittery shake
    vec2 shake_offset = vec2(
        sin(t * shake_speed),
        cos(t * shake_speed * 1.37)
    ) * shake_strength * px;

    uv += shake_offset;

    // Sample original text — no color changes
    vec4 col = texture(TEXTURE, uv);
    col *= COLOR; // keep Label's modulate working normally

    COLOR = col;
}