Shader "Game/Wavefield"
{
    Properties
    {
        _MainTex ("WavefieldRenderTexture", any) = "" {}
        _BesselLUT ("BesselLUT", any) = "" {}
        
        _WavefieldScale ("WavefieldScale", Range(0.0, 2.0)) = 1
        _DecayRate ("DecayRate", Range(0.5, 2.0)) = 1
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

            Texture2D<float> _MainTex;
            Texture2D<float> _BesselLUT;
            SamplerState Sampler_Linear_Clamp;

            cbuffer Material
            {
                // Set per-frame
                float Droplet0Scale;
                float2 Droplet0Position; // (mm, mm)

                float Droplet1Scale;
                float2 Droplet1Position; // (mm, mm)
                
                float2 DomainSize; // (mm, mm)

                // Set in settings
                float _WavefieldScale;
                float _DecayRate;
            }

            // Unity values
            float4 unity_DeltaTime;

            // Constants
            static const float PI = 3.14159265359f;
            static const float lambda_F = 4.7511; // (mm)
            static const float k_F = 2 * PI / lambda_F; // (1/mm)
            
            static const float BesselInputMax = 80.89755587113763f; // J(BesselInputMax) == 0
            
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

            // Bessel LUT maps [0, BesselInputMax] -> [-1, 1]
            float LookupJ0(float r)
            {
                uint texWidth, texHeight;
                _BesselLUT.GetDimensions(texWidth, texHeight);

                // Clamp r at a very distant zero (wavefield beyond will be flat)
                float tableLookupU = saturate(r / BesselInputMax);
                
                // Adjust values to sample centers of LUT texel centers
                tableLookupU = (tableLookupU * (texWidth - 1) + 0.5f) / texWidth;

                // Map from packed [0, 1] to full [-1, 1]
                return _BesselLUT.Sample(Sampler_Linear_Clamp, tableLookupU) * 2.0f - 1.0f; 
            }

            float EvaluateWavefield(float2 position, float2 dropletPosition)
            {
                float dist = length(position - dropletPosition);

                float h = _WavefieldScale * LookupJ0(k_F * dist);

                // Spatial decay
                h *= exp(-dist / (I_W * lambda_F));
                
                return h;
            }
            
            void ps_main
            (
                in float2 inTexcoord : TEXCOORD0,
                in float2 inPositionWS : TEXCOORD1,
                in float4 inPositionSS : SV_POSITION,
                out float outColor : SV_Target
            )
            {
                float history = _MainTex.Sample(Sampler_Linear_Clamp, inTexcoord);
                outColor = exp(-unity_DeltaTime.x * _DecayRate) * history;

                // Droplet 0
                if (Droplet0Scale > 0.0f)
                    outColor += EvaluateWavefield(inPositionWS, Droplet0Position);

                // Droplet 1
                if (Droplet1Scale > 0.0f)
                    outColor += EvaluateWavefield(inPositionWS, Droplet1Position);
            }
            ENDHLSL
        }
    }
}