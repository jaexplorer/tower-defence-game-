// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "Decode Float RG", "Generic", "Decodes a previously-encoded RG float" )]
	public sealed class DecodeFloatRGHlpNode : HelperParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_funcType = "DecodeFloatRG";
			m_inputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT2, false );
			m_inputPorts[ 0 ].Name = "RG";
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT, false );
		}
	}
}
