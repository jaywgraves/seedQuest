// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Amplify/WorldSpace"
{
	Properties
	{
		_RockAlbedo("Rock Albedo", 2D) = "white" {}
		_RockNormal("Rock Normal", 2D) = "bump" {}
		_RockSpecular("Rock Specular", 2D) = "white" {}
		_SnowAlbedo("Snow Albedo", 2D) = "white" {}
		_SnowNormal("Snow Normal", 2D) = "bump" {}
		_SnowSpecular("Snow Specular", 2D) = "white" {}
		_SnowAmount("SnowAmount", Range( 0 , 2)) = 0.13
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		ZTest LEqual
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform sampler2D _RockNormal;
		uniform float4 _RockNormal_ST;
		uniform sampler2D _SnowNormal;
		uniform float4 _SnowNormal_ST;
		uniform float _SnowAmount;
		uniform sampler2D _RockAlbedo;
		uniform float4 _RockAlbedo_ST;
		uniform sampler2D _SnowAlbedo;
		uniform float4 _SnowAlbedo_ST;
		uniform sampler2D _RockSpecular;
		uniform float4 _RockSpecular_ST;
		uniform sampler2D _SnowSpecular;
		uniform float4 _SnowSpecular_ST;

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float2 uv_RockNormal = i.uv_texcoord * _RockNormal_ST.xy + _RockNormal_ST.zw;
			float2 uv_SnowNormal = i.uv_texcoord * _SnowNormal_ST.xy + _SnowNormal_ST.zw;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_normWorldNormal = normalize( ase_worldNormal );
			float temp_output_21_0 = ( ase_normWorldNormal.y * _SnowAmount );
			float3 lerpResult15 = lerp( UnpackNormal( tex2D( _RockNormal, uv_RockNormal ) ) , UnpackNormal( tex2D( _SnowNormal, uv_SnowNormal ) ) , ( ase_normWorldNormal.z + temp_output_21_0 + ase_normWorldNormal.x ));
			o.Normal = lerpResult15;
			float2 uv_RockAlbedo = i.uv_texcoord * _RockAlbedo_ST.xy + _RockAlbedo_ST.zw;
			float2 uv_SnowAlbedo = i.uv_texcoord * _SnowAlbedo_ST.xy + _SnowAlbedo_ST.zw;
			float temp_output_11_0 = ( (WorldNormalVector( i , lerpResult15 )).y * _SnowAmount );
			float4 lerpResult10 = lerp( tex2D( _RockAlbedo, uv_RockAlbedo ) , tex2D( _SnowAlbedo, uv_SnowAlbedo ) , temp_output_11_0);
			o.Albedo = lerpResult10.rgb;
			float2 uv_RockSpecular = i.uv_texcoord * _RockSpecular_ST.xy + _RockSpecular_ST.zw;
			float2 uv_SnowSpecular = i.uv_texcoord * _SnowSpecular_ST.xy + _SnowSpecular_ST.zw;
			float4 lerpResult17 = lerp( tex2D( _RockSpecular, uv_RockSpecular ) , tex2D( _SnowSpecular, uv_SnowSpecular ) , temp_output_11_0);
			o.Specular = lerpResult17.rgb;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardSpecular keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandardSpecular o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandardSpecular, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16200
18;-1123;1906;1124;2457.612;1283.211;1.807001;True;True
Node;AmplifyShaderEditor.WorldNormalVector;20;-1767.906,-340.7951;Float;False;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;12;-1773.599,-115.1001;Float;False;Property;_SnowAmount;SnowAmount;6;0;Create;True;0;0;False;0;0.13;0.97;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-1345.505,-290.8951;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;14;-1548.101,267.1998;Float;True;Property;_SnowNormal;Snow Normal;4;0;Create;True;0;0;False;0;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;25;-1205.589,-178.6183;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;4;-1548.101,72.7999;Float;True;Property;_RockNormal;Rock Normal;1;0;Create;True;0;0;False;0;None;0a1c9fdd66c90b343a3927a13f01617a;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;15;-1067.3,73.59992;Float;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;19;-865.5047,-194.2967;Float;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;1;-829.4701,-741.9772;Float;True;Property;_RockAlbedo;Rock Albedo;0;0;Create;True;0;0;False;0;None;d33948e9f1c25b740ae2f7429ea48167;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-693.0005,253.9001;Float;True;Property;_RockSpecular;Rock Specular;2;0;Create;True;0;0;False;0;None;9fb53ba31f9de1b42a5e82eaf4bc5856;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;-651.7996,-116.3;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;16;-704.8019,470.7009;Float;True;Property;_SnowSpecular;Snow Specular;5;0;Create;True;0;0;False;0;None;9fb53ba31f9de1b42a5e82eaf4bc5856;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;9;-824.2003,-500.8003;Float;True;Property;_SnowAlbedo;Snow Albedo;3;0;Create;True;0;0;False;0;None;8da443880cf71da4d9500d4947f974be;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;17;-223.4019,317.8009;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;22;-1111.806,-307.4951;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;18;-409.7049,-70.1974;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;10;-127.1,-308.6001;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;146.9001,42.89999;Float;False;True;2;Float;ASEMaterialInspector;0;0;StandardSpecular;Amplify/WorldSpace;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;3;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;1;False;-1;1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;21;0;20;2
WireConnection;21;1;12;0
WireConnection;25;0;20;3
WireConnection;25;1;21;0
WireConnection;25;2;20;1
WireConnection;15;0;4;0
WireConnection;15;1;14;0
WireConnection;15;2;25;0
WireConnection;19;0;15;0
WireConnection;11;0;19;2
WireConnection;11;1;12;0
WireConnection;17;0;2;0
WireConnection;17;1;16;0
WireConnection;17;2;11;0
WireConnection;22;0;21;0
WireConnection;18;0;11;0
WireConnection;10;0;1;0
WireConnection;10;1;9;0
WireConnection;10;2;11;0
WireConnection;0;0;10;0
WireConnection;0;1;15;0
WireConnection;0;3;17;0
ASEEND*/
//CHKSM=DBF2D931D38E51DBFE53E39D09103C367B433F06