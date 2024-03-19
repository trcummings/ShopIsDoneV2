shader_type canvas_item;

mat2 rotate2D(float r) {
    return mat2(vec2(cos(r), sin(r)), vec2(-sin(r), cos(r)));
}

// based on the follow tweet:
// https://twitter.com/zozuar/status/1621229990267310081
//vec2 n,q,p=(FC.xy-.5*r)/r.y;
//float d=dot(p,p),S=9.,i,a,j;
//for(mat2 m=rotate2D(5.);j++<30.;){
  //p*=m;
  //n*=m;
  //q=p*S+t*4.+sin(t*4.-d*6.)*.8+j+n;
  //a+=dot(cos(q)/S,vec2(.2));
  //n-=sin(q);
  //S*=1.2;
//}
//o+=(a+.2)*vec4(4,2,1,0)+a+a-d;
void fragment()
{
    // Normalized pixel coordinates (from 0 to 1)
	vec2 res = 1. / SCREEN_PIXEL_SIZE;
    vec2 uv = (FRAGCOORD.xy -.5 * res.xy) / res.y;
    vec3 col = vec3(0);
    float t = TIME;
    
    vec2 n = vec2(0);
    vec2 q = vec2(0);
    vec2 p = uv;
    float d = dot(p,p);
    float S = 12.;
    float a = 0.0;
    mat2 m = rotate2D(5.);

    for (float j = 0.; j < 30.; j++) {
        p *= m;
        n *= m;
		float tMod = sin(t * 4. - d * 6.);
        q = p * S + t * 4. + tMod * .8 + j + n; // wtf???
        a += dot(cos(q)/S, vec2(.2));
        n -= sin(q);
        S *= 1.2;
    }

    col = vec3(4, 2, 1) * (a + .2) + a + a - d;

    // Output to screen
    COLOR *= vec4(col,1.0);
}
