shader_type canvas_item;

uniform float speed = 1.5; // how fast shards move

// Simple random helper based on shard ID
float random(vec2 p) {
    return fract(sin(dot(p, vec2(12.9898,78.233))) * 43758.5453);
}

void fragment() {
    vec2 uv = UV;

    // 2x2 grouping: 0 or 1 per axis
    vec2 shard = step(0.5, uv);

    // Each shard gets a random angle
    float angle = random(shard) * 6.28318; // 0 -> 2*PI
    vec2 dir = vec2(cos(angle), sin(angle));

    // Move each shard outward over time
    vec2 displaced_uv = uv + dir * TIME * speed * 0.25; // scaled for visible movement

    // Fade out as shards fly
    float fade = 1.0 - smoothstep(0.0, 1.2, TIME);

    // Sample the texture
    COLOR = texture(TEXTURE, displaced_uv) * fade;
}
