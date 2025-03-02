// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "Normalize", "Vector", "Normalizes a vector", null, KeyCode.N )]
	public sealed class NormalizeNode : SingleInputOp
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_selectedLocation = PreviewLocation.TopCenter;
			m_inputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT4, false );
			m_inputPorts[ 0 ].CreatePortRestrictions( WirePortDataType.FLOAT, WirePortDataType.FLOAT2, WirePortDataType.FLOAT3, WirePortDataType.FLOAT4, WirePortDataType.COLOR, WirePortDataType.OBJECT, WirePortDataType.INT );
			m_previewShaderGUID = "a51b11dfb6b32884e930595e5f9defa8";
		}
		public override string GenerateShaderForOutput( int outputId,  ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{

			string result = string.Empty;
			switch ( m_inputPorts[ 0 ].DataType )
			{
				case WirePortDataType.FLOAT:
				case WirePortDataType.FLOAT2:
				case WirePortDataType.FLOAT3:
				case WirePortDataType.FLOAT4:
				case WirePortDataType.OBJECT:
				case WirePortDataType.COLOR:
				{
					result =  "normalize( " + m_inputPorts[ 0 ].GenerateShaderForOutput( ref dataCollector, m_inputPorts[ 0 ].DataType, ignoreLocalvar ) + " )";
				}break;
				case WirePortDataType.INT:
				{
					return m_inputPorts[ 0 ].GenerateShaderForOutput( ref dataCollector, m_inputPorts[ 0 ].DataType, ignoreLocalvar );
				}
				case WirePortDataType.FLOAT3x3:
				case WirePortDataType.FLOAT4x4:
				{
					result = UIUtils.InvalidParameter( this );
				}
				break;
			}
			return CreateOutputLocalVariable( 0, result, ref dataCollector );
		}
	}
}
