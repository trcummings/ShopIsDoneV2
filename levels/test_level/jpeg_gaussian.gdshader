shader_type canvas_item;

uniform float uv_scale : hint_range(1, 256) = 1;
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

const int CHUNK_SIZE = 8;
const float sigma = float(CHUNK_SIZE) / 3.0;
const mat3 YCB_CR = mat3(
	vec3(0.299,0.587,0.114),
	vec3(-0.168736,-0.331264,0.5),
	vec3(0.5,-0.418688,-0.081312)
);

vec3 toYCbCr(vec3 rgb)
{
    return rgb * YCB_CR + vec3(0,.5,.5);
}

vec3 pre(vec2 coord, vec2 pixel_size){
    return floor(256.*(toYCbCr(texture(SCREEN_TEXTURE, coord / pixel_size).xyz) - .5));
}

vec3 DCT8x8(vec2 coord, vec2 uv, vec2 pixel_size) {
    vec3 result = vec3(0);

    for (float x = 0.; x < 8.; x++) {
    	for (float y = 0.; y < 8.; y++) {
			vec3 pre_color = pre(coord + vec2(x, y), pixel_size);
			float x_res = cos((2. * x + 1.) * uv.x * PI / 16.);
			float y_res = cos((2. * y + 1.) * uv.y * PI / 16.);
            result += pre_color * x_res * y_res; 
    	}
    }
	
    #define a(x) (x != 0. ? 1. : 1. / sqrt(2.))
	
    return result * .25 * a(uv.x) * a(uv.y);
}

void fragment() {
	vec2 pixel_block = floor(FRAGCOORD.xy / 8.);
	vec2 coord = 8. * pixel_block;
	vec2 uv = floor(FRAGCOORD.xy - 8. * pixel_block);

	// Set color
	COLOR.rgb = floor(.5 + DCT8x8(8. * pixel_block, uv, 1.0 / SCREEN_PIXEL_SIZE));
}
