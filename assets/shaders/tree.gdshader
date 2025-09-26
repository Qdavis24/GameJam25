shader_type canvas_item;

uniform float wind_strength : hint_range(0.0, 1.0) = 0.5;
uniform float wind_speed : hint_range(0.0, 5.0) = 1.0;

void vertex() {
    // Sway trees based on their world position and time
    float sway = sin(TIME * wind_speed + VERTEX.x * 0.01) * wind_strength;
    
    // Only sway the top part of trees (higher UV.y values)
    float sway_factor = UV.y; // Trees sway more at the top
    
    VERTEX.x += sway * sway_factor * 10.0;
}