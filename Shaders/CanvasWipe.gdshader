shader_type canvas_item;

uniform float percentage : hint_range(0.0,1,.01);
uniform float masked_alpha : hint_range(0,1,.01) = 1;
uniform float unmasked_alpha : hint_range(0,1,.01);

uniform bool horizontal = false;
uniform bool invert = true;

void fragment() {
	vec4 main_tex = texture(TEXTURE, UV);
	COLOR = main_tex;
	
	float uvPOS = UV.y;
	if (horizontal) {
		uvPOS = UV.x;
	}
	
	// Calculate step value for the fill
	float step_mod = step(uvPOS, percentage);
	float step_value = unmasked_alpha;
	
	if (invert) {
		step_value += (1.0 - step_mod);
	}
	else {
		step_value += step_mod;
	}
	
	COLOR.a = main_tex.a * step_value * masked_alpha;
}