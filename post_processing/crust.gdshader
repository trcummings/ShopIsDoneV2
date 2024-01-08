shader_type spatial;
render_mode unshaded;

uniform int crust_amount = 40;
uniform sampler2D screen_texture : hint_screen_texture;

void vertex() {
	POSITION = vec4(VERTEX, 1);
}

void fragment() {
	vec2 grid_uv = round(SCREEN_UV * float(crust_amount)) / float(crust_amount);
	vec4 base_color = texture(screen_texture, SCREEN_UV);
	vec4 crust_color = texture(screen_texture, grid_uv);

	ALBEDO = base_color.rgb * crust_color.rgb;
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
