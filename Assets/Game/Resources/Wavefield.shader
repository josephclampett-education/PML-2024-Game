Shader "Game/Wavefield"
{
    Properties
    {
        _MainTex ("Texture", any) = "" {}
    }

    SubShader
    {
        Pass
        {
            Name "Blit"
            ZWrite Off ZTest Always Blend Off Cull Off

            HLSLPROGRAM
            #pragma vertex vs_main
            #pragma fragment ps_main

            sampler2D _MainTex;
            sampler1D _BesselLUT;

            cbuffer Material
            {
                float Droplet0Scale;
                float2 Droplet0Position; // (mm, mm)

                float Droplet1Scale;
                float2 Droplet1Position; // (mm, mm)
                
                float2 DomainSize; // (mm, mm)
            }

            static const float kPi = 3.14159265359f;
            static const float lambda_F = 4.75; // (mm)
            static const float I_W = 2.1; // (__) for gamma/gamma_F = 0.8
            
            void vs_main
            (
                in uint inVertexID : SV_VertexID,
                out float2 outTexcoord : TEXCOORD0,
                out float2 outPositionWS : TEXCOORD1,
                out float4 outPositionCS : SV_POSITION
            )
            {
                float2 texcoord = float2((inVertexID << 1) & 2, inVertexID & 2);

                float2 positionCS = texcoord * 2.0f - 1.0f;
                float2 positionWS = positionCS * DomainSize;

                outTexcoord = texcoord;
                outTexcoord.y = 1.0f - outTexcoord.y;

                outPositionWS = positionWS;
                outPositionCS = float4(positionCS, 0.0f, 1.0f);
            }
            
            void ps_main
            (
                in float2 inTexcoord : TEXCOORD0,
                in float2 inPositionWS : TEXCOORD1,
                in float4 inPositionSS : SV_POSITION,
                out float outColor : SV_Target
            )
            {
                float history = tex2D(_MainTex, inTexcoord);
                outColor = exp(-0.02) * history;

                // Droplet 0
                if (Droplet0Scale > 0.0f)
                {
                    float rWS = length(inPositionWS - Droplet0Position);

                    float lookupR = 2 * kPi * rWS / lambda_F;
                    lookupR /= 80.89755587113763f;
                    float h = tex1D(_BesselLUT, lookupR + 0.5 / 2048) * 2.0f - 1.0f;
                    h *= exp(-rWS / (I_W * lambda_F));

                    outColor += exp(-0.2) * h;
                }

                // Droplet 1
                if (Droplet1Scale > 0.0f)
                {
                    float rWS = length(inPositionWS - Droplet1Position);

                    float lookupR = 2 * kPi * rWS / lambda_F;
                    lookupR /= 80.89755587113763f;
                    float h = tex1D(_BesselLUT, lookupR + 0.5 / 2048) * 2.0f - 1.0f;
                    h *= exp(-rWS / (I_W * lambda_F));

                    outColor += exp(-0.2) * h;
                }
            }
            ENDHLSL
        }
    }
}