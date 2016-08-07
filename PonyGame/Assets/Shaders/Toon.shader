Shader "Custom/Toon"
{
	Properties
	{
		_Color("Main Color", Color) = (.5,.5,.5,1)
		_Shading("Shading", Range(0,1)) = 0.5
		_MainTex("Base (RGB)", 2D) = "white" { }
	}

	SubShader
	{
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		float _Shading;

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb * _Shading;
			o.Emission = c.rgb * (1 - _Shading);
			o.Metallic = 0;
			o.Smoothness = 0;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}