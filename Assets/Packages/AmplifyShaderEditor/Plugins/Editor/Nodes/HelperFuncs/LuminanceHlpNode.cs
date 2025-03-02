// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "Luminance", "Generic", "Converts color to luminance (grayscale)" )]
	public sealed class LuminanceHlpNode : HelperParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_funcType = "Luminance";
			m_inputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT3, false );
			m_inputPorts[ 0 ].Name = "RGB";
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT, false );
		}
	}
}
