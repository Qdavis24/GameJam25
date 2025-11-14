shader_type canvas_item;

// How strong the fire pulse is (0 = off, 0.1–0.2 = nice subtle range)
uniform float intensity : hint_range(0.0, 1.0) = 0.15;

// How fast it animates
uniform float speed : hint_range(0.0, 10.0) = 3.0;

// Vertical “flame band” frequency (higher = more bands)
uniform float vertical_scale : hint_range(0.0, 30.0) = 10.0;

// Small extra flicker
uniform float flicker_speed : hint_range(0.0, 20.0) = 8.0;


void fragment() {
    // Whatever the button was going to draw (background + text etc.)
    vec4 base = COLOR;

    float t = TIME * speed;

    // Vertical wavy pattern like rising heat
    float band = sin(UV.y * vertical_scale + t);

    // A little horizontal/flicker component
    float flicker = sin(TIME * flicker_speed + UV.x * vertical_scale * 0.7);

    // Combine and normalize to -1..1
    float combined = (band * 0.7 + flicker * 0.3);

    // Turn into a brightness multiplier around 1.0
    float factor = 1.0 + combined * intensity;

    // Clamp so it never blows out too hard or goes too dark
    factor = clamp(factor, 1.0 - intensity, 1.0 + intensity);

    vec3 col = base.rgb * factor;

    COLOR = vec4(col, base.a); // keep original alpha
}