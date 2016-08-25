Shader "Custom/Magic"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MinAlpha("MinAlpha", Range(0,1)) = 0.1
		_Tess("Tessellation", Range(1,32)) = 4
		_Phong("Phong Strengh", Range(0,1)) = 0.5
		_TessClose("MaxDetaiDistance", Range(1,20)) = 2
		_TessFar("MinDetaiDistance", Range(1,20)) = 10
		_Displacement("Displacement", Range(0, 0.2)) = 0.1
		_Amplitude("Amplitude", Range(0, 0.1)) = 0.1
		_TimeFrequency("Time Frequency", Range(0, 16.0)) = 6
		_SpatialFrequency("Spatial Frequency", Range(0, 64)) = 6
	}

	SubShader
		{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "DisableBatching" = "True"}
		
		CGPROGRAM
		#pragma surface surf Magic alpha:fade fullforwardshadows vertex:disp tessellate:tess tessphong:_Phong nolightmap
		#pragma target 4.6 
		#include "Tessellation.cginc"

		struct appdata
		{
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
		};

		float _Phong;
		float _Tess;
		float _TessClose;
		float _TessFar;

		float4 tess(appdata v0, appdata v1, appdata v2)
		{
			return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, _TessClose, _TessFar, _Tess);
		}

		float _Displacement;
		float _Amplitude; 
		float _TimeFrequency;
		float _SpatialFrequency;

		void disp(inout appdata v)
		{
			v.vertex.xyz += v.normal * (sin(_Time * _TimeFrequency + dot((mul(UNITY_MATRIX_MV, v.vertex - float4(0, 0, 0, 1))), float3(1, 1, 0.2)) * _SpatialFrequency) * _Amplitude + _Displacement);
		}

		half _MinAlpha;

		half4 LightingMagic(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
		{
			half4 c;
			c.rgb = s.Albedo;
			c.a = ((1 - dot(s.Normal, viewDir)) * (1 - _MinAlpha) + _MinAlpha) * s.Alpha;
			return c;
		}

		struct Input
		{
			float2 uv_MainTex;
		};

		fixed4 _Color;

		void surf(Input IN, inout SurfaceOutput o)
		{
			fixed4 c = _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
