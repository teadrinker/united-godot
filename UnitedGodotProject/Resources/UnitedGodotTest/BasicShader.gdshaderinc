
#define CALL_VERT_WORLDSPACE
//#define CALL_VERT
#define NEED_VERT_UV
#define NEED_FRAG_UV

uniform sampler2D _MainTex;
uniform sampler2D _Texture2;
uniform float4 _Color;            
uniform float4 _ShaderParams;            
uniform float _Scaling;      
 
//void vert(inout fdata io) { io.vertex += 0.05 * sin(io.vertex.y + _Scaling*30.); }
void vert_worldspace(inout data io) { io.vertex.xyz += 0.05 * sin(io.vertex.yzx*5.0 + _Scaling*30.); }
  
float4 frag(data i)
{
    float4 tex = i.uv.x > 0.5 ? tex2D(_MainTex, i.uv) : tex2D(_Texture2, i.uv+float2(0.0,_Scaling * 0.01));
    float rings = (0.5 + 0.3*sin(length( i.uv - 0.5 ) * 240.0 * _Scaling));
    return tex * _Color * rings * _ShaderParams;
}