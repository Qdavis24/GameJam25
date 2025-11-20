shader_type canvas_item;

uniform float diamond_size : hint_range(0.1, 0.5) = 0.3; // How big the diamond is
uniform float fade_width; // Very small fade zone
uniform vec4 border_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);

void fragment() {
    vec2 uv = UV;
    vec2 centered = uv - 0.5;

    // Diamond distance
    float iso_dist = abs(centered.x) + abs(centered.y);

    // Distance from diamond edge (adjusted by diamond_size)
    float dist = diamond_size - iso_dist;

    // Very sharp fade - use higher power for more aggressive transition
    float alpha = 1.0 - smoothstep(-fade_width, 0.0, dist);
    alpha = pow(alpha, 0.3); // Makes it snap to black faster

    COLOR = vec4(border_color.rgb, border_color.a * alpha);
}