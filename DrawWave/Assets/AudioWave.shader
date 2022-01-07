Shader "Hidden/Editor/AudioWave"
{
    Properties
    {            
        _ColorBg ("ColorBG", Color) = (1, 1, 1, 1)
        _ColorWave ("ColorWave", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        //LOD 100

        Pass
        {
            Name "AudioWave"
            
            ZTest Always ZWrite Off
            Cull Off
            
            HLSLPROGRAM            
            #pragma target 4.5
            #pragma vertex Vertex
            #pragma fragment Fragment
            
            //#include "Packages/com.unity.render-pipelines.papegame/ShaderLibrary/Core.hlsl"
            
            float _sampleArray[750];
            float4 _ColorBg;
            float4 _ColorWave;

         
            struct VertexInput{
                float4 vertex:POSITION;
                float2 uv: TEXCOORD0;
            };
            
            struct VertexOutput{
                half4 pos: SV_POSITION;
                half2 uv: TEXCOORD0;
            };
            
            VertexOutput Vertex(VertexInput i)
            {
               VertexOutput o;

                o.pos = i.vertex;
                o.pos.xy = o.pos.xy * 2 - 1;
                o.uv = i.uv;
                //先求反，然后上下分为正负0-0.5 *2 变为 (-1)-0-1
                //o.uv.y = abs((1.0 - o.uv.y)-0.5)*2; 
                o.uv.y = ((1.0 - o.uv.y)-0.5)*2;
                return o;             
            }
                     
            
            half4 Fragment(VertexOutput i):SV_Target
            {
                float value =_sampleArray[floor(i.uv.x *750)];
                // abs must set in fragment
                if (abs(i.uv.y)< max(value,0.05))
                {
                    return half4(_ColorWave);
                }                
                else{
                    return half4(_ColorBg);
                } 
                //return half4(i.uv,0,1);                                                                                                                                                     
            }
            
                       
            ENDHLSL
          
        }
    }
}