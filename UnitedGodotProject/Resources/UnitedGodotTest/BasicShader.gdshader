shader_type spatial;
render_mode unshaded, skip_vertex_transform, shadows_disabled;

#define float2 vec2
#define float3 vec3
#define float4 vec4
#define tex2D texture

struct data
{
    float4 vertex;                
    float2 uv;
    float3 normal;                
    float4 color;
    float4 worldPos;
};

#include "BasicShader.gdshaderinc"

#ifdef NEED_FRAG_WORLDPOS
varying vec3 world_position;
#endif

void vertex() {
	
    data o;
    o.uv = float2(0.0, 0.0);
    o.normal = float3(0.0, 0.0, 0.0);
    o.color = float4(0.0, 0.0, 0.0, 0.0);
    o.worldPos = float4(0.0, 0.0, 0.0, 0.0);
	
	o.vertex = float4(VERTEX, 1.0);	
					
#ifdef NEED_VERT_UV 	
    o.uv = UV;
#endif	
#ifdef NEED_VERT_NORMAL
	#ifdef FRAG_NORMAL_IN_OBJSPACE	
    	o.normal = NORMAL;
	#else
		// todo
	#endif
#endif	
#ifdef NEED_VERT_COLOR		
    o.color = COLOR;
#endif	

#if defined(NEED_VERT_WORLDPOS) || defined(NEED_FRAG_WORLDPOS)
	o.worldPos = (MODEL_MATRIX * o.vertex).xyz;
#endif	

#ifdef CALL_VERT
	vert(o);
#endif

#ifdef CALL_VERT_WORLDSPACE
  #if (defined(NEED_VERT_WORLDPOS) || defined(NEED_FRAG_WORLDPOS)) && !defined(CALL_VERT)
    o.vertex = o.worldPos;
  #else
    o.vertex = (MODEL_MATRIX * o.vertex);
  #endif
    vert_worldspace(o);
    o.vertex = (VIEW_MATRIX * o.vertex);
#else
    o.vertex = (MODELVIEW_MATRIX * o.vertex);
    o.normal = (MODELVIEW_MATRIX * vec4(o.normal, 0.0)).xyz;
#endif



	VERTEX = o.vertex.xyz;	
#ifdef NEED_FRAG_UV 	
	UV = o.uv;
#endif		
#ifdef NEED_FRAG_NORMAL	
	NORMAL = o.normal;
#endif		
#ifdef NEED_FRAG_COLOR	
	COLOR = o.color;
#endif		
#ifdef NEED_FRAG_WORLDPOS
	world_position = o.worldPos;
#endif	
}

void fragment() {
	data fragData;
#ifdef NEED_FRAG_UV	
	fragData.uv = UV;
#endif	
#ifdef NEED_FRAG_NORMAL	
	fragData.normal = NORMAL; // UNTESTED
#endif	
#ifdef NEED_FRAG_COLOR
	fragData.color = COLOR; // UNTESTED
#endif
#ifdef NEED_FRAG_WORLDPOS
	fragData.worldPos = world_position; // UNTESTED
#endif
	float4 col = frag(fragData);
	col.rgb -= FOG.rgb; // would be better to turn off fog (but only seems possible for skybox?)
	ALBEDO = col.rgb;

	//COLOR = vec4(1.0, 0.0, 0.0, 1.0);
	//ALBEDO  = vec3(sq(1.0), sq(0.5) * _Scaling, 0.0);
}
