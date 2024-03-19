shader_type canvas_item;

uniform vec2 tiling = vec2(1., 1.);
uniform vec2 offset = vec2(0., 0.);
uniform float rotation = 0.;
uniform float frequency = 5.;
uniform float star_size = 1.;
uniform float sine_speed = 5.;
uniform float sharpness = 1.;
uniform vec4 color_1 : source_color = vec4(1., 1., 1., 1.);
uniform vec4 color_2 : source_color = vec4(0., 0., 0., 0.);

// PI / 5
const float pi5 = 0.628318530718;

float polar_star(in vec2 p)
{
	float m2 = mod(atan(p.y, p.x) / pi5 + 1., 2.);
	float adjust = -sharpness;
	
	return length(p) * cos((pi5 * adjust) * (m2 - 4. * step(1., m2) + 1.)) - 1.;
}

void fragment() {
	vec2 resolution = 1.0 / TEXTURE_PIXEL_SIZE;
	vec2 frag_coord = UV;
	
	vec2 uv = UV - 0.5 + offset;
	uv = uv * (2.0 + tiling);
	
	float t = .94 + rotation;
	uv = mat2(vec2(cos(t), -sin(t)), vec2(sin(t), cos(t))) * uv;
	
	float d = polar_star(uv) * 5.;
	d = sin(d * frequency + TIME * sine_speed) / 10.;
	d = smoothstep(0., 0., d);
	
	float clip = polar_star(uv * 5. * star_size);
	clip = smoothstep(1., 1., clip);
	
	d = d - clip;
	
	vec4 col_1 = vec4(d,d,d,d) * color_1;
	vec4 col_2 = (1. - vec4(d,d,d,d + clip)) * color_2;
	
	COLOR = COLOR * col_1 + col_2;
}