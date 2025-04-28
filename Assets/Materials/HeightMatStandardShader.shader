// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

Shader "Unlit/HeightMatStandardShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _HeightMap("Texture", 2D) = "white" {}
        _scaleFactor("ScaleFactor", Float) = 1
        _scaleFactor2("ScaleFactor2", Float) = 1
        _uv_min("UVmin", Vector) = (0, 0, 0, 0)
        _uv_max("UVmax", Vector) = (0.5, 1, 0, 0)
        _length("Length", Float) = 830
        _width("Width", Float) = 1553
        _numTiles("NumTiles", Int) = 30
        _offset("Offset", Float) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _HeightMap;
            float4 _MainTex_ST;
            float _width;
            float _length;
            float _scaleFactor;
            float _scaleFactor2;

            float _offset;

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                //clamping fixes artifical terrain walls
                float4 displacementColor = tex2Dlod(_HeightMap, float4(clamp(v.uv.x, 0.001, 0.999), clamp(1 - v.uv.y, 0.001, 0.999), 0, 0));
                //float3 vert = float3(v.vertex.x, 0, v.vertex.z) + v.normal * displacementColor * _scaleFactor;
                float3 vert = float3(v.vertex.x, 0, v.vertex.z) + v.normal * (displacementColor.r) * _scaleFactor; //*_scaleFactor2

                o.vertex = UnityObjectToClipPos(vert);
                o.uv = v.uv;

                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                /*float ratioValidWidth = _width / pow(2, ceil(log2(_width)));
                //float colorU = i.uv.x / ratioValidWidth;
                float colorU = lerp(0, ratioValidWidth, i.uv.x);
                float ratioValidHeight = _length / pow(2, ceil(log2(_length)));
                float colorV = lerp(0, ratioValidHeight, i.uv.y);
                float2 colorUV = {colorU, 1 - colorV};*/

                // sample the texture
                i.uv.y = 1 - i.uv.y;
                fixed4 col = tex2D(_MainTex, i.uv);


                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}