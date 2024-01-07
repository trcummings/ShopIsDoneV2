shader_type canvas_item;

uniform int crust_amount = 40;
uniform sampler2D screen_texture : hint_screen_texture;

vec4 overlay(vec4 base, vec4 blend){
	vec4 limit = step(0.9, base);
	return mix(2.0 * base * blend, 1.0 - 2.0 * (1.0 - base) * (1.0 - blend), limit);
}

void fragment() {
	vec2 grid_uv = round(SCREEN_UV * float(crust_amount)) / float(crust_amount);
	vec4 base_color = texture(screen_texture, SCREEN_UV);
	vec4 crust_color = texture(screen_texture, grid_uv);
	
	COLOR = base_color * crust_color;
	//COLOR = overlay(base_color, crust_color);
}