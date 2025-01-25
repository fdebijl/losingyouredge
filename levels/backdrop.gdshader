
shader_type canvas_item;

uniform float swirl_strength : hint_range(-10.0, 10.0) = 5.0;
uniform float zoom_speed : hint_range(0.1, 5.0) = 1.0;

void fragment() {
    // Get UV coordinates centered at 0,0
    vec2 uv = UV - 0.5;

    // Convert to polar coordinates
    float dist = length(uv);
    float angle = atan(uv.y, uv.x);

    // Add rotation based on distance and time
    angle += dist * swirl_strength + TIME * zoom_speed;

    // Convert back to Cartesian coordinates
    vec2 swirled_uv = vec2(
        dist * cos(angle),
        dist * sin(angle)
    );

    // Recenter UV coordinates
    swirled_uv += 0.5;

    // Create color effect using swirled coordinates
    vec3 color = vec3(1.0);
    color.r = sin(TIME * 2.0) * swirled_uv.x;
    color.g = cos(TIME) * swirled_uv.y;
	color.b = tan(TIME) * swirled_uv.y + swirled_uv.x;
	//color.r = sin(TIME * 2.0) * swirled_uv.x;
    //color.g = cos(TIME) * swirled_uv.y;

    COLOR.rgb = color;
    COLOR.a = 1.0;
}