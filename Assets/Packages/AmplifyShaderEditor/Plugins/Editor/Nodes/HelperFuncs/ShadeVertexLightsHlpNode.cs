// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "Shade Vertex Lights", "Vertex-lit", "Computes illumination from four per-vertex lights and ambient, given object space position & normal" )]
	public sealed class ShadeVertexLightsHlpNode : HelperParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_funcType = "ShadeVertexLights";
			m_inputPorts[ 0 ].ChangeProperties( "Vertex", WirePortDataType.FLOAT4, false );
			AddInputPort( WirePortDataType.FLOAT3, false, "Normal" );
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT3, false );
		}
	}
}
