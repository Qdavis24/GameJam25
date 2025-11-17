shader_type canvas_item;

// 0.0 = no darkening
// 1.0 = completely black
uniform float darkness : hint_range(0.0, 1.0) = 0.1;

void fragment() {
    vec4 col = texture(TEXTURE, UV);

    // reduce brightness evenly
    col.rgb *= (1.0 - darkness);

    // respect the node's modulate color
    col *= COLOR;

    COLOR = col;
}