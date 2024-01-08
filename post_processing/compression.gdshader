shader_type spatial;
render_mode unshaded;

uniform float uv_scale : hint_range(1, 256) = 1;
uniform float chunk_size : hint_range(1, 100) = 1;
uniform sampler2D depth_texture : hint_depth_texture;
uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap;
uniform float zNear = 0.05;
uniform float zFar = 100;
uniform float color_depth : hint_range(0.0, 255.0) = 100.0;
uniform float color_number : hint_range(0.0, 50.0) = 20.0;
uniform int crust_amount = 40;


void vertex() {
	POSITION = vec4(VERTEX, 1);
}

float calculate_1D_gaussian(float x, int pos, float uv_pos)
{
	float cs = float(chunk_size);
	float gaussian = modf(uv_pos * uv_scale, cs);
	int j = int(round(gaussian)) + pos;
	
	if (j >= 0 && j < 4)
	{
		return exp(-0.5 * pow(PI * x, 2));
	}
	else
	{
		return 0.0;
	}
}

const mat3 rgb_2_ycbcr = mat3(
    vec3(0.299, -0.168736, 0.5), 
    vec3(0.587, -0.331264, -0.418688),   
    vec3(0.114, 0.5, -0.081312)
);
const mat3 ycbcr_2_rgb = mat3(
    vec3(1.0, 1.0, 1.0),
    vec3(0.0, -0.344136, 1.772), 
    vec3(1.402, -0.714136, 0.0)
);

// simulating 8:4:4 compression ratio (16bit)
vec3 compress_ycbcr_844 (vec3 rgb) {
	vec3 ycbcr = rgb_2_ycbcr * rgb;
	ycbcr.r = floor(ycbcr.r * color_depth + 0.5) / color_depth;
	ycbcr.gb += 0.5;
	ycbcr.gb = floor(ycbcr.gb * color_number + 0.5) / color_number;
	ycbcr.gb -= 0.5;    
	return ycbcr_2_rgb * ycbcr;
}

const int CHUNK_SIZE = 8;
const float sigma = float(CHUNK_SIZE) / 3.0;

// Function to calculate Gaussian weight
float gaussian(float x) {
    return exp(-0.5 * pow(x / sigma, 2.0)) / (sigma * sqrt(2.0 * 3.14159265));
}

vec4 overlay(vec4 base, vec4 blend){
	vec4 limit = step(0.5, base);
	return mix(2.0 * base * blend, 1.0 - 2.0 * (1.0 - base) * (1.0 - blend), limit);
}

void fragment() {
    // Calculate chunk coordinates
    ivec2 chunkCoords = ivec2(FRAGCOORD.xy / float(CHUNK_SIZE));
    ivec2 chunkStart = chunkCoords * CHUNK_SIZE;
	vec2 texel_size = 1.0 / VIEWPORT_SIZE;

    vec4 color = vec4(0.0);
    float total_weight = 0.0;
	int radius = CHUNK_SIZE / 2;
	vec2 grid_uv = round(SCREEN_UV * float(crust_amount)) / float(crust_amount);

    // Apply 1D Gaussian blur inside the chunk
    for (int x = -radius; x <= radius; x++) {
		for (int y = -radius; y <= radius; y++) {
			vec2 offset = SCREEN_UV + (vec2(ivec2(x, y)) * texel_size);
			//vec2 offset = (SCREEN_UV + vec2(float(x), float(y)) * texel_size + vec2(chunkStart) * texel_size);
			vec4 sample = texture(screen_texture, offset);
			float x_ratio = float(x) / float(radius);
			float y_ratio = float(y) / float(radius);
			float x_weight = calculate_1D_gaussian(x_ratio, x, SCREEN_UV.x);
			float y_weight = calculate_1D_gaussian(y_ratio, y, SCREEN_UV.y);
			float weight = x_weight * y_weight;
	        color += sample * weight;
	        total_weight += weight;
		}
    }
	
	vec4 crust_color = texture(screen_texture, grid_uv);
    color /= total_weight;
	//vec4 overlaid = overlay(color, crust_color);

	ALBEDO = compress_ycbcr_844(color.rgb);
}

//void fragment() {
	//// Compress the color from rgb to yCbCr
	//vec2 texel_size = 1.0 / VIEWPORT_SIZE;
	//vec3 pixel_sum = texture(screen_texture, SCREEN_UV).rgb;
	//float weight_sum = 0.0;
	//for (int x = -radius; x <= radius; x++)
	//{
		//for (int y = -radius; y <= radius; y++)
		//{
			//// For each pixel within the radius, calculate a gaussian blur
			//vec2 offset = SCREEN_UV + (vec2(ivec2(x, y)) * texel_size);
			//vec3 pixel_color = texture(screen_texture, offset).rgb;
			//float x_ratio = float(x) / float(radius);
			//float y_ratio = float(y) / float(radius);
			//float x_weight = calculate_1D_gaussian(x_ratio, x, SCREEN_UV.x);
			//float y_weight = calculate_1D_gaussian(y_ratio, y, SCREEN_UV.y);
			//float weight = x_weight * y_weight;
			//
			//// Accumulate blur
			//pixel_sum += pixel_color * weight;
			//weight_sum += weight;
		//}
	//}
	//
	//ALBEDO = compress_ycbcr_844(pixel_sum / weight_sum);
//}

//void fragment() {
	////float base_depth = get_depth(SCREEN_UV, FRAGCOORD.z, INV_PROJECTION_MATRIX);
	////float surface_depth = get_depth(SCREEN_UV, texture(depth_texture, SCREEN_UV).x, INV_PROJECTION_MATRIX);
	////float diff = surface_depth - base_depth;
	//
	//float depth = texture(depth_texture, SCREEN_UV).x;
	//vec3 ndc = vec3(SCREEN_UV * 2.0 - 1.0, depth);
	//vec4 view = INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
 	//view.xyz /= view.w;
	//float linear_depth = view.z;
	//
	//float scaled_depth = (zFar * zNear) / (zFar + (linear_depth * (zNear - zFar)));
	//
	//vec2 uv = 1.0 / (VIEWPORT_SIZE).xy;
	//
	//for (int x = -radius; x <= radius; x++)
	//{
		//for (int y = -radius; y <= radius; y++)
		//{
			//// For each pixel within the radius, calculate a gaussian blur
			//vec2 offset = SCREEN_UV + vec2(float(x), float(y)) * uv;
			//vec3 pixel_color = texture(screen_texture, offset).rgb;
			//float x_ratio = float(x) / float(radius);
			//float y_ratio = float(y) / float(radius);
			//float x_weight = calculate_1D_gaussian(x_ratio, x, SCREEN_UV.x);
			//float y_weight = calculate_1D_gaussian(y_ratio, y, SCREEN_UV.y);
			//float weight = x_weight * y_weight;
			//
			//// Accumulate blur
			//pixel_sum += pixel_color * weight;
			//weight_sum += weight;
		//}
	//}
	//
	//ALBEDO = pixel_sum / weight_sum;
//}