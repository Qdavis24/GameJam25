shader_type canvas_item;

uniform float blur_amount : hint_range(0.0, 5.0) = 1.0;
uniform float reflection_strength : hint_range(0.0, 1.0) = 0.3;

void fragment() {
    vec2 uv = UV;
    vec4 color = vec4(0.0);
    
    // Simple blur by sampling neighboring pixels
    float blur_size = blur_amount / 100.0;
    
    for (int x = -2; x <= 2; x++) {
        for (int y = -2; y <= 2; y++) {
            vec2 offset = vec2(float(x), float(y)) * blur_size;
            color += texture(TEXTURE, uv + offset);
        }
    }
    
    color /= 25.0; // Average the samples
    
    // Add reflection/glow effect
    float distance_from_center = distance(uv, vec2(0.5, 0.5));
    float glow = 1.0 - smoothstep(0.2, 0.5, distance_from_center);
    color.rgb += color.rgb * glow * reflection_strength;
    
    COLOR = color;
	}