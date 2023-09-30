Shader "Unlit/BasicShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Texture2 ("Texture2", 2D) = "white" {}
        //_Color ("Color", Color) = (1,1,1,1)        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex uvert
            #pragma fragment ufrag
            // make fog work
            //#pragma multi_compile_fog

            struct data
            {
                float4 vertex;                
                float2 uv;
                float3 normal;                
                float4 color;
                float4 worldPos;
            };

            #include "BasicShader.gdshaderinc"

            #include "UnityCG.cginc"
 
            struct appdata
            {
                float4 vertex : POSITION;
#ifdef NEED_VERT_UV   
                float2 uv : TEXCOORD0;
#endif     
#ifdef NEED_VERT_NORMAL        
                float3 normal : NORMAL;
#endif
#ifdef NEED_VERT_COLOR
                float3 color : COLOR;
#endif
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;    
#ifdef NEED_FRAG_UV            
                float2 uv : TEXCOORD0;
#endif
#ifdef NEED_FRAG_NORMAL
                half3 normal : NORMAL;      
#endif
#ifdef NEED_FRAG_COLOR             
                float3 color : COLOR;        
#endif
#ifdef NEED_FRAG_WORLDPOS                                        
                float3 worldPos : TEXCOORD2;
#endif                
            };


            //sampler2D _MainTex;
            //float4 _MainTex_ST;
       

            v2f uvert(appdata v)
            {
                data o;
                o.uv = float2(0.0, 0.0);
                o.normal = float3(0.0, 0.0, 0.0);
                o.color = float4(0.0, 0.0, 0.0, 0.0);
                o.worldPos = float4(0.0, 0.0, 0.0, 0.0);

                o.vertex = v.vertex;
#ifdef NEED_VERT_UV
                o.uv = v.uv;                
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
#endif
#ifdef NEED_VERT_NORMAL
    #ifdef FRAG_NORMAL_IN_OBJSPACE
                o.normal = v.normal;                
    #else
                o.normal = UnityObjectToWorldNormal(v.normal);
    #endif 
#endif
#ifdef NEED_VERT_COLOR
                o.color = v.color;
#endif
#if defined(NEED_VERT_WORLDPOS) || defined(NEED_FRAG_WORLDPOS)
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
#endif

#ifdef CALL_VERT
                vert(o);
#endif

#ifdef CALL_VERT_WORLDSPACE
    #if (defined(NEED_VERT_WORLDPOS) || defined(NEED_FRAG_WORLDPOS)) && !defined(CALL_VERT)
                o.vertex = o.worldPos;
    #else
                o.vertex = mul(unity_ObjectToWorld, o.vertex);
    #endif
                vert_worldspace(o);
                o.vertex = mul(UNITY_MATRIX_VP, o.vertex);
#else
                o.vertex = UnityObjectToClipPos(o.vertex);
#endif


                v2f ro; 
                ro.vertex = o.vertex;
#ifdef NEED_FRAG_UV                
                ro.uv = o.uv;
#endif
#ifdef NEED_FRAG_NORMAL
                ro.normal = o.normal;
#endif
#ifdef NEED_FRAG_COLOR
                ro.color = o.color;
#endif
#ifdef NEED_FRAG_WORLDPOS
                ro.worldPos = o.worldPos;
#endif               
                //UNITY_TRANSFER_FOG(o,o.vertex);
                return ro;
            }

            fixed4 ufrag(v2f i) : SV_Target
            {
                data d;
                d.vertex = i.vertex;
#ifdef NEED_FRAG_UV                
                d.uv = i.uv;
#endif
#ifdef NEED_FRAG_NORMAL
                d.normal = i.normal;
#endif
#ifdef NEED_FRAG_COLOR
                d.color = i.color;
#endif
#ifdef NEED_FRAG_WORLDPOS
                d.worldPos = i.worldPos;
#endif
                float4 col = frag(d);
                // sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
