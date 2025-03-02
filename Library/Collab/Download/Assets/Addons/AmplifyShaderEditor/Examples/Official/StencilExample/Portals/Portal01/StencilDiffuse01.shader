// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ASESampleShaders/StencilDiffuse01"
{
	Properties
	{
		_Color("_Color", Color) = (1,1,1,1)
		[HideInInspector] __dirty( "", Int ) = 1
		_Normal("Normal", 2D) = "bump" {}
		_Albedo("Albedo", 2D) = "white" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		Stencil
		{
			Ref 1
			Comp Equal
			Pass Keep
			Fail Keep
		}
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_Normal;
			float2 uv_Albedo;
		};

		uniform sampler2D _Normal;
		uniform sampler2D _Albedo;
		uniform float4 _Color;

		void surf( Input input , inout SurfaceOutputStandard output )
		{
			output.Normal = UnpackNormal( tex2D( _Normal,input.uv_Normal) );
			output.Albedo = ( tex2D( _Albedo,input.uv_Albedo) * _Color ).rgb;
			output.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=3001
393;92;1091;695;853;154.5;1;False;False
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;True;2;Float;ASEMaterialInspector;Standard;ASESampleShaders/StencilDiffuse01;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;1.2;2.1;Opaque;0.5;True;True;0;False;Opaque;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;1;255;255;5;1;1;0;False;0;4;10;25;True;FLOAT3;0,0,0;FLOAT3;0,0,0;FLOAT3;0,0,0;FLOAT;0.0;FLOAT;0.0;FLOAT;0.0;FLOAT3;0,0,0;FLOAT3;0,0,0;FLOAT;0.0;OBJECT;0.0;OBJECT;0.0;OBJECT;0.0;OBJECT;0.0;FLOAT3;0,0,0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;-198,-39.5;Float;FLOAT4;0.0,0,0,0;COLOR;0.0,0,0,0
Node;AmplifyShaderEditor.ColorNode;2;-549,68.5;Float;Property;_Color;_Color;-1;1,1,1,1
Node;AmplifyShaderEditor.SamplerNode;4;-462,271.5;Float;Property;_Normal;Normal;-1;None;True;0;True;bump;Auto;True;Object;-1;Auto;SAMPLER2D;;FLOAT2;0,0;FLOAT;1.0;FLOAT2;0,0;FLOAT2;0,0;FLOAT;1.0
Node;AmplifyShaderEditor.SamplerNode;1;-560,-141.5;Float;Property;_Albedo;Albedo;-1;None;True;0;False;white;Auto;False;Object;-1;Auto;SAMPLER2D;;FLOAT2;0,0;FLOAT;1.0;FLOAT2;0,0;FLOAT2;0,0;FLOAT;1.0
WireConnection;0;0;3;0
WireConnection;0;1;4;0
WireConnection;3;0;1;0
WireConnection;3;1;2;0
ASEEND*/
//CHKSM=81CDFDC781A699D4B1891DC5013DFEB3C9983BAC
