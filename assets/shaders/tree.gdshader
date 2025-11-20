shader_type canvas_item;

uniform float wind_strength : hint_range(0.0, 1.0) = 0.5;
uniform float wind_speed : hint_range(0.0, 5.0) = 1.0;
uniform vec2 player_position = vec2(0.0, 0.0);
uniform float transparency_radius = 150.0;
uniform float fade_width = 50.0;
varying vec2 world_position;

void vertex() {
    // Sway trees based on their world position and time
    float sway = sin(TIME * wind_speed + VERTEX.x * 0.01) * wind_strength;
    
    // Only sway the top part of trees (higher UV.y values)
    float sway_factor = UV.y; // Trees sway more at the top
    
    VERTEX.x += sway * sway_factor * 10.0;
	world_position = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
}

void fragment() {
    float dist = distance(world_position, player_position);
    
    float inner_radius = transparency_radius;
    float outer_radius = transparency_radius + fade_width;
    
    float alpha_multiplier = smoothstep(inner_radius, outer_radius, dist);
    
    // Mix between min_alpha and 1.0 instead of 0.0 and 1.0
    alpha_multiplier = mix(.5, 1.0, alpha_multiplier);
    
    COLOR = texture(TEXTURE, UV);
    COLOR.a *= alpha_multiplier;
}