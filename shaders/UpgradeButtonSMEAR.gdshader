shader_type canvas_item;

// How far the erosion moves inward (0 = just at edge, 0.4 = deep)
uniform float erosion_amount : hint_range(0.0, 0.5) = 0.4;

// How soft the transition band is
uniform float edge_softness : hint_range(0.0, 0.3) = 0.12;

// How noisy / jagged the erosion edge is
uniform float noise_strength : hint_range(0.0, 0.5) = 0.25;
uniform float scale = 20.0;
uniform vec2 scroll_speed = vec2(0.25, 0.12);

// simple hash-based noise
float hash(vec2 p) {
    return fract(sin(dot(p, vec2(27.1, 83.1))) * 43758.5453);
}

float noise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);

    float a = hash(i);
    float b = hash(i + vec2(1.0, 0.0));
    float c = hash(i + vec2(0.0, 1.0));
    float d = hash(i + vec2(1.0, 1.0));

    vec2 u = f * f * (3.0 - 2.0 * f);

    return mix(a, b, u.x) +
           (c - a) * u.y * (1.0 - u.x) +
           (d - b) * u.x * u.y;
}

void fragment() {
    vec4 base = COLOR; // original button (bg + text)

    // Distance to nearest edge in UV space
    float dist_to_edge = min(min(UV.x, UV.y), min(1.0 - UV.x, 1.0 - UV.y));

    // Animated noisy offset so the border is not perfectly straight
    vec2 animated_uv = UV * scale + scroll_speed * TIME;
    float n = noise(animated_uv); // 0..1
    float noisy_dist = dist_to_edge + (n - 0.5) * noise_strength * erosion_amount;

    // We want:
    // - fully cut out at dist ~ 0 (at the very edge)
    // - fully visible after "erosion_amount" inward
    float inner = erosion_amount; // where we are fully opaque

    float alpha_factor = smoothstep(0.0, inner + edge_softness, noisy_dist);

    base.a *= alpha_factor;

    COLOR = base;
}