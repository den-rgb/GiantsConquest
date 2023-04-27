Shader "Custom/grassLOD" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _LOD0Distance ("LOD0 Distance", Range(0, 100)) = 10
        _LOD1Distance ("LOD1 Distance", Range(0, 100)) = 50
    }

    SubShader {
        LOD 10

        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        float _LOD0Distance;
        float _LOD1Distance;

        struct Input {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o) {
            float distance = length(UNITY_MATRIX_MV[3].xyz);
            float lod = 0;

            if (distance > _LOD0Distance) {
                lod = smoothstep(_LOD0Distance, _LOD0Distance + 5, distance);
            }
            if (distance > _LOD1Distance) {
                lod = smoothstep(_LOD1Distance - 5, _LOD1Distance, distance) * 2;
            }

            float3 darkGreen = float3(0, 0.35, 0);
            float3 texColor = tex2D(_MainTex, IN.uv_MainTex).rgb;
            o.Albedo = lerp(texColor, darkGreen, lod);
            o.Specular = 0.2;
            o.Gloss = 0.7;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
