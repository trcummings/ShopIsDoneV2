shader_type spatial;

// skip_vertex_transform: skip the conversion from model to view space
// world_vertex_coords: VERTEX/NORMAL/etc are modified in world coordinates instead of local. 
// 		+- fragment(): doesn't seem to change anything in this shader. with inverse(INV_INV_VIEW_MATRIX) below.
// 		+- vertex(): deforms the mesh in a circle, but moves with the camera. with inverse(INV_INV_VIEW_MATRIX) below.
render_mode unshaded;
// 		+- no render_mode specified (or unshaded): makes the mesh deform in a long wave but moves with the cam. with inverse(INV_INV_VIEW_MATRIX) below.

uniform vec4 fogColor: source_color = vec4(0.0, 0.0, 0.0, 1.0);
uniform float lightRadiusEncoder = 80.0;

uniform float innerEdge = 0.91;
uniform float innerEdgeOffset = 0.41;
uniform float outerEdge = 1.0;
uniform float depth = 2.5;
uniform float mapWidth;
uniform float mapLength;
uniform sampler2D lightMapTexture;

void vertex() {
	// transform from view space to world space
	vec3 vertexTransformed = ((MODEL_MATRIX) * vec4(VERTEX, 1.0)).xyz;
//	vec3 vertexTransformed = (inverse(INV_INV_VIEW_MATRIX) * vec4(VERTEX, 1.0)).xyz; 
	
	// Get the fragment coordinates
	float fx = vertexTransformed.x;
	float fy = vertexTransformed.y;
	float fz = vertexTransformed.z;
	
	// Get the linear size of the lightmap texture
	ivec2 lightCount = textureSize(lightMapTexture, 0);
	
	// Get the offset used to read the color from the center of pixel
	float bitmapPixelWidth = 1.0 / float(lightCount.x); 
	
	// Loop through each light source
	for (int i = 0; i < lightCount.x; i++) {
		// Get the pixel that holds the current light source information
		float indexOnBitmap = bitmapPixelWidth * float(i);
		
		// texture() reads positions on a bitmap between 0.0 and 1.0, not int pixels,
		vec4 bitmapPixel = texture(lightMapTexture, vec2((bitmapPixelWidth * 0.5) + indexOnBitmap, 0.5));
		
		// Transform the bitmap pixel coordinates back into world space coordinates using the
		// x and y values of the bitmap pixel (the x and z values of the light's actual position')
		vec3 lightPos = vec3(
			(bitmapPixel.x * mapWidth) - (mapWidth * 0.5), 
			0, 
			(bitmapPixel.y * mapLength) - (mapLength * 0.5)
		); 
		
		// Get the size of the light from the bitmap pixel's z value and re-encode it
		float lightRadius = bitmapPixel.z * lightRadiusEncoder;
		
		// Calculate the squared pythagorean radius from the fragment point to the light point
		float radiusSquared = pow(fx - lightPos.x, 2) + pow(fz - lightPos.z, 2); 
		
		float innerRange = lightRadius * innerEdge;
		float adjustedMaxRange = lightRadius * outerEdge;
		
		// Are we within the outer edge of the light's diameter?
		if (radiusSquared <= pow(adjustedMaxRange, 2)) {
			// Are we within the inner range of the light's diameter?
			if (radiusSquared <= pow(innerRange, 2)) {
				// Hard circular cutoff
				VERTEX.y = -depth;
			}
			else {
				// Take square root to get the depth radius inside here only if we need to because 
				// it's an expensive calculation
				float radius = sqrt(radiusSquared);

				// make the mesh fall off in a gradient. also helps a lot to make the circle appear round, not pixely, where it meets the ground.
				// linear
				float dpct = 1.0 / (adjustedMaxRange - innerRange) * (radius - innerRange); 
				float newDepth = (1.0 - dpct) * depth; 

				if (VERTEX.y > -newDepth) VERTEX.y = -newDepth;
			}
		}
	}
}

void fragment() {
	ALBEDO = fogColor.rgb;
	ALPHA = fogColor.a;

	// transform from view to world space
//	vec3 vertexTransformed = ((MODEL_MATRIX) * vec4(VERTEX, 1.0)).xyz;
	vec3 vertexTransformed = (inverse(VIEW_MATRIX) * vec4(VERTEX, 1.0)).xyz; 

	// Get the fragment coordinates
	float fx = vertexTransformed.x;
	float fy = vertexTransformed.y;
	float fz = vertexTransformed.z;

	// Get the linear size of the lightmap texture
	ivec2 lightCount = textureSize(lightMapTexture, 0);

	// offset used to read color from center of pixel
	float bitmapPixelWidth = 1.0 / float(lightCount.x);

	for (int i = 0; i < lightCount.x; i++) {
		// the pixel that holds current units position info
		float indexOnBitmap = bitmapPixelWidth * float(i);

		// texture() reads positions on a bitmap between 0.0 and 1.0, not int pixels
		vec4 bitmapPixel = texture(lightMapTexture, vec2((bitmapPixelWidth * 0.5) + indexOnBitmap, 0.5));

		// Transform the bitmap pixel coordinates back into world space coordinates using the
		// x and y values of the bitmap pixel (the x and z values of the light's actual position')
		vec3 lightPos = vec3(
			(bitmapPixel.x * mapWidth) - (mapWidth * 0.5), 
			0, 
			(bitmapPixel.y * mapLength) - (mapLength * 0.5)
		); 
		
		// Get the size of the light from the bitmap pixel's z value and re-encode it
		float lightRadius = bitmapPixel.z * lightRadiusEncoder;

		// radius
		float radiusSquared = pow(fx - lightPos.x, 2) + pow(fz - lightPos.z, 2); 

		float innerRange = (lightRadius * innerEdge) - innerEdgeOffset;
		float adjustedMaxRange = lightRadius * outerEdge;

		// If we're inside the outer rim
		if (radiusSquared <= pow(adjustedMaxRange, 2)) {
			
			// If we're inside the inner rim
			if (radiusSquared <= pow(innerRange, 2)) {
				ALPHA = 0.0;
			}
		}
	}

//	// DEBUGGING:
//	// maps the light map texture onto the whole shader mesh
//	ALBEDO = texture(lightMapTexture, UV).xyz; 
//
//	// Show unit count as grayscale block on edge of mesh
//	if (UV.y > 0.99) {
//		for (int i = 0; i < lightCount.x; i++) {
//			float indexNormalized = bitmapPixelWidth * float(i);
//			if (UV.x > indexNormalized) {
//				ALBEDO = vec3(indexNormalized, indexNormalized, indexNormalized);
//			}
//		}
//	}
}
