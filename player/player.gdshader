shader_type canvas_item;

uniform sampler2D sprite;
uniform sampler2D shiney;
uniform sampler2D rust;
uniform float health;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec4 spriteColor = texture(sprite, UV);
	vec4 shineyColor = texture(shiney, UV);
	vec4 rustColor = texture(rust, UV);
    vec4 rustage = mix(shineyColor, rustColor, 1.0 - health);
	COLOR.a = spriteColor.a;
	float gray = 1.0-dot(spriteColor.rgb, vec3(0.299, 0.587, 0.114));

    // apply damage coloring
    COLOR.rgb = mix(spriteColor.rgb, rustage.rgb * gray, 1.0 - health);
	//COLOR.rgb = rustage.rgb;
	
	// Called for every pixel the material is visible on.
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
