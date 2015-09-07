Shader "Custom/MainTex"
{
	Properties 
	{
		_Color ("Main Color", Color) = (.5,.5,.5,1)
		_Shading ("Shading", Range(0,1)) = 0.5
		_OutlineColor ("Outline Color", Color) = (0,0,0,1)
		_Outline ("Outline width", Range (0.0, 0.03)) = .005
		_MainTex ("Base (RGB)", 2D) = "white" { }
	}
 
	CGINCLUDE
	#include "UnityCG.cginc"
	 
	struct appdata 
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};
	 
	struct v2f
	{
		float4 pos : POSITION;
		float4 color : COLOR;
	};
	
	uniform fixed4 _OutlineColor;
	uniform float _Outline;
	 
	v2f vert(appdata v)
	{
		// just make a copy of incoming vertex data but scaled according to normal direction
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
	 
		float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
		float2 offset = TransformViewToProjection(norm.xy);
	 
		o.pos.xy += offset * o.pos.z * _Outline / length(ObjSpaceViewDir(v.vertex));
		o.color = _OutlineColor;
		return o;
	}
	ENDCG
 
	SubShader 
	{
		Tags { "Queue" = "Transparent" }
 
		// note that a vertex shader is specified here but its using the one above
		Pass
		{
			Name "OUTLINE"
			Tags { "LightMode" = "Always" }
			Cull Off
			ZWrite Off
			ColorMask RGB // alpha not used
			Blend SrcAlpha OneMinusSrcAlpha
 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			 
			half4 frag(v2f i) : COLOR 
			{
				return i.color;
			}
			
			ENDCG
		}
		
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