shader_type canvas_item;

uniform vec2 uv_mult = vec2(1., 1.);
uniform vec2 uv_offset = vec2(0., 0.);

void fragment() {
	
	COLOR *= texture(TEXTURE, (UV + uv_offset) * uv_mult);
}
