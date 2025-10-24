shader_type canvas_item;
render_mode world_vertex_coords;

// tree sway params
uniform float wind_strength : hint_range(0.0, 1.0) = 0.5;
uniform float wind_speed : hint_range(0.0, 5.0) = 1.0;

// player modulate params
uniform vec2 player_position;
varying vec2 world_position;
uniform float min_dst;

const vec2[] local_uv = {vec2(0,0 ), vec2(0,1), vec2(1,1), vec2(1,0)};
varying vec2 LOCAL_UV;

void vertex() {
    // Sway trees based on their world position and time
    float sway = sin(TIME * wind_speed + VERTEX.x * 0.01) * wind_strength;

    // Only sway the top part of trees (higher UV.y values)
    float sway_factor = UV.y; // Trees sway more at the top
    if (UV.y < .4 ){
		 VERTEX.x += sway * sway_factor * 20.0;
	}

	// vertex id is 0-3 representing one point in each tile
	LOCAL_UV = local_uv[VERTEX_ID];
	world_position = VERTEX;
}

void fragment() {
	// due to texture packing the UV coords start at .39 and end at .65 for tree
	float base_position = (1.0-LOCAL_UV.y) * 450.0 + world_position.y;

	float dist = distance(player_position, world_position);
	if (base_position > player_position.y && dist < min_dst){
		COLOR.a *= mix(0.2, 1.0, dist/min_dst);
	}


}