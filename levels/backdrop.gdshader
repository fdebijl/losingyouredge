
shader_type canvas_item;

uniform float swirl_strength : hint_range(-0.01, 1.0, 0.01) = 0.01;
uniform float zoom_speed : hint_range(0.1, 5.0) = 1.0;

vec3 palette(float t) {
  vec3 a = vec3(0.5);
  vec3 b = vec3(0.5);
  vec3 c = vec3(2.0, 1.0, 0.0);
  vec3 d = vec3(0.5, 0.2, 0.25);

  return a + b * cos(6.28318 * (c * t + d));
}

float sin01 (float value) {
  return (sin(value) + 1.0) * 0.5;
}

void fragment() {
	vec2 uv = UV;
	uv.x = uv.x + sin(TIME * zoom_speed + uv.x * 50.0) * swirl_strength;
	uv.y = uv.y + sin(TIME * zoom_speed + uv.y * 50.0) * swirl_strength;
	float dist_to_center = distance(uv, vec2(0.5, 0.5));
	float curvy = sin(dist_to_center * 50.0);
	vec3 color = palette(curvy);
    COLOR.rgb = color;
    COLOR.a = 1.0;
}