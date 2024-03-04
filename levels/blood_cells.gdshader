shader_type canvas_item;

/**
 Just fooling around basicly. Some sort of bloodstream. 
*/


// https://iquilezles.org/articles/smin
float smin( float a, float b, float k )
{
    float h = clamp( 0.5+0.5*(b-a)/k, 0.0, 1.0 );
    return mix( b, a, h ) - k*h*(1.0-h);
}

float cells(vec2 uv) {
    uv = mix(sin(uv + vec2(1.57, 0)), sin(uv.yx*1.4 + vec2(1.57, 0)), .75);
    return uv.x*uv.y*.3 + .7;
}

const float BEAT = 4.0;
float fbm(vec2 uv)
{
    
    float f = 200.0;
    vec2 r = (vec2(.9, .45));    
    vec2 tmp;
    float T = 100.0 + TIME * 1.3;
    T += sin(TIME * BEAT) * .1;
    // layers of cells with some scaling and rotation applied.
    for (int i = 1; i < 8; ++i)
    {
        float fi = float(i);
        uv.y -= T * .5;
        uv.x -= T * .4;
        tmp = uv;
        
        uv.x = tmp.x * r.x - tmp.y * r.y; 
        uv.y = tmp.x * r.y + tmp.y * r.x; 
        float m = cells(uv);
        f = smin(f, m, .07);
    }
    return 1. - f;
}

vec3 g(vec2 uv)
{
    vec2 off = vec2(0.0, .03);
    float t = fbm(uv);
    float x = t - fbm(uv + off.yx);
    float y = t - fbm(uv + off);
    float s = .0025;
    vec3 xv = vec3(s, x, 0);
    vec3 yv = vec3(0, y, s);
    return normalize(cross(xv, -yv)).xzy;
}

const vec3 ld = normalize(vec3(1.0, 2.0, 3.));

void fragment()
{
	vec2 uv = UV;
    uv -= vec2(0.5);
	vec2 res = 1. / SCREEN_PIXEL_SIZE;
    float a = res.x / res.y;
    uv.y /= a;
    vec2 ouv = uv;
    float B = sin(TIME * BEAT);
    uv = mix(uv, uv * sin(B), .035);
    vec2 _uv = uv * 25.;
    float f = fbm(_uv);
    
    // base color
    COLOR = vec4(f);
    COLOR.rgb *= vec3(1., .3 + B * .05, 0.1 + B * .05);
    
    vec3 v = normalize(vec3(uv, 1.));
    vec3 grad = g(_uv);
    
    // spec
    vec3 H = normalize(ld + v);
    float S = max(0., dot(grad, H));
    S = pow(S, 4.0) * .2;
    COLOR.rgb += S * vec3(.4, .7, .7);
	
    // rim
    float R = 1.0 - clamp(dot(grad, v), .0, 1.);
    COLOR.rgb = mix(COLOR.rgb, vec3(.8, .8, 1.), smoothstep(-.2, 2.9, R));
	
    // edges
    COLOR.rgb = mix(COLOR.rgb, vec3(0.), smoothstep(.45, .55, (max(abs(ouv.y * a), abs(ouv.x)))));
    
    // contrast
    COLOR = smoothstep(.0, 1., COLOR);
	COLOR.a = 1.;
}