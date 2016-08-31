Shader "Custom/NoOutlineTex"
{
	Properties 
	{
		_Color ("Main Color", Color) = (.5,.5,.5,1)
		_Shading ("Shading", Range(0,1)) = 0.5
		_MainTex ("Base (RGB)", 2D) = "white" { }
	}
 
	SubShader 
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
		Name "BASE"
		ZWrite On
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha
	
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		fixed4 _Color;
		float _Shading;
	
        struct Input
        {
            float4 pos : POSITION;
            float2 uv_MainTex : TEXCOORD1;
        };
        
        void surf (Input IN, inout SurfaceOutput o)
        {
        	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
        	fixed3 col = _Color * (1 - tex.a) + tex.rgb * tex.a;
        
        	o.Albedo = col * (1 - _Shading);
        	o.Emission = col * _Shading;
        }
		
		ENDCG
	}
	Fallback "Diffuse"
}