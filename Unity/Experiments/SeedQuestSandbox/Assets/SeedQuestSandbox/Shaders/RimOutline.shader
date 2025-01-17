Shader "SeedQuest/RimOutline" {

	Properties {

		[Header(Base Parameters)]
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0

		[Header(Outline Parameters)]
		_OutlineColor("Outline Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_OutlineWidth("Outline Width", Range(0,.1)) = 0.03
        _OutlinePower("Outline Power", Range(0,1)) = 1

        [Header(Highlight Parameters)]
        _HighlightColor("Hightlight Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _HighlightPower("Hightlight Power", Range(0,1)) = 0.0

		[Header(Rim Parameters)]
		_RimColor("Rim Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_RimExponent("Rim Exponent", Range(0.5, 10.0)) = 1.0
        _RimPower("Rim Power", Range(0,1)) = 1.0

        [Header(Flashing Parameters)]
		[MaterialToggle] _UseDynamicColor("Use Flashing Highlights/Rim", Float) = 1
		_DynamicColorSpeed("Flash Speed (Flash per Second)", Range(0.1, 4.0)) = 0.5
	}

	SubShader {
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 200

		Pass {
			CGPROGRAM

			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;

			// Vertex shader object data
			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			// Fragment shader input data 			
			struct v2f {
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			// Vertex shader
			v2f vert(appdata v) {
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			// Fragment shader
			fixed4 frag(v2f i) : SV_TARGET{
				fixed4 col = tex2D(_MainTex, i.uv);
				col *= _Color;
				return col; 
			}

			ENDCG
		}

		Pass {
			Cull front
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM

			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag

			fixed4 _OutlineColor;
			float _OutlineWidth;
            float _OutlinePower;

			// Vertex shader object data
			struct appdata {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			// Fragment shader input data 
			struct v2f {
				float4 position : SV_POSITION;
			};

			// Vertex shader
			v2f vert(appdata v) {
				v2f o;
				//calculate the position of the expanded object
				float3 normal = normalize(v.normal);
				float3 outlineOffset = normal * _OutlineWidth;
				float3 position = v.vertex + outlineOffset;

				o.position = UnityObjectToClipPos(position);

				return o;
			}

			// Fragment shader
			fixed4 frag(v2f i) : SV_TARGET{
                fixed4 outline = _OutlineColor;
                outline.a = _OutlinePower * _OutlineColor.a;
				return outline;
			}

			ENDCG
		}
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 viewDir;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

        float4 _HighlightColor;
        float _HighlightPower;
		float4 _RimColor;
		float _RimExponent;
        float _RimPower;
		float _UseDynamicColor;
		float _DynamicColorSpeed;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		UNITY_INSTANCING_BUFFER_START(Props)
		// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf(Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;

			// Rim Effect 
			const float PI = 3.14159;
			half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
			half rimPower;
			if(_UseDynamicColor)
				rimPower = _RimPower * pow(rim, _RimExponent) * ((1.0 + sin(_DynamicColorSpeed * 2 *PI * _Time.y)) * 0.5);
			else
				rimPower = _RimPower * pow(rim, _RimExponent); 

            half highlightPower;
            if(_UseDynamicColor)
                highlightPower = _HighlightPower * ((1.0 + sin(_DynamicColorSpeed * 2 *PI * _Time.y)) * 0.5);
            else
                highlightPower = _HighlightPower;

			o.Emission = _RimColor.rgb * rimPower + _HighlightPower * highlightPower;

			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	
	FallBack "Diffuse"
}