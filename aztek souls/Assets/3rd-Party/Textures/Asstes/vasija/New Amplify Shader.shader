// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "shader/ vasija"
{
	Properties
	{
		_vasijaalbedo("vasija albedo", 2D) = "white" {}
		_vasijaao("vasija ao", 2D) = "white" {}
		_vasijametalic("vasija metalic", 2D) = "white" {}
		_vasijanormal("vasija normal", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Off
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _vasijanormal;
		uniform float4 _vasijanormal_ST;
		uniform sampler2D _vasijaalbedo;
		uniform float4 _vasijaalbedo_ST;
		uniform sampler2D _vasijametalic;
		uniform float4 _vasijametalic_ST;
		uniform sampler2D _vasijaao;
		uniform float4 _vasijaao_ST;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_vasijanormal = i.uv_texcoord * _vasijanormal_ST.xy + _vasijanormal_ST.zw;
			o.Normal = UnpackNormal( tex2D( _vasijanormal, uv_vasijanormal ) );
			float2 uv_vasijaalbedo = i.uv_texcoord * _vasijaalbedo_ST.xy + _vasijaalbedo_ST.zw;
			o.Albedo = tex2D( _vasijaalbedo, uv_vasijaalbedo ).rgb;
			float2 uv_vasijametalic = i.uv_texcoord * _vasijametalic_ST.xy + _vasijametalic_ST.zw;
			o.Metallic = tex2D( _vasijametalic, uv_vasijametalic ).r;
			float2 uv_vasijaao = i.uv_texcoord * _vasijaao_ST.xy + _vasijaao_ST.zw;
			o.Occlusion = tex2D( _vasijaao, uv_vasijaao ).r;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16700
781;617;742;384;639.5266;-14.57622;1.3;False;False
Node;AmplifyShaderEditor.SamplerNode;1;-467.189,10.21149;Float;True;Property;_vasijaalbedo;vasija albedo;0;0;Create;True;0;0;False;0;5602dbc02fa725143b45737ace228c56;5602dbc02fa725143b45737ace228c56;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;4;-635.7889,78.9115;Float;True;Property;_vasijanormal;vasija normal;3;0;Create;True;0;0;False;0;2b7882afabd5ff94388b12313ada48ee;2b7882afabd5ff94388b12313ada48ee;True;0;True;white;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-443.489,350.5116;Float;True;Property;_vasijaao;vasija ao;1;0;Create;True;0;0;False;0;1ba41ba30cb8f3a4ab1a596f821688cd;1ba41ba30cb8f3a4ab1a596f821688cd;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;3;-509.6893,169.9114;Float;True;Property;_vasijametalic;vasija metalic;2;0;Create;True;0;0;False;0;0d3dd913d81002b45bbd11a15002b077;0d3dd913d81002b45bbd11a15002b077;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;shader/ vasija;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;0;0;1;0
WireConnection;0;1;4;0
WireConnection;0;3;3;0
WireConnection;0;5;2;0
ASEEND*/
//CHKSM=7432C2387C69D4FBBCF3D801C6B29B07D6D348E9