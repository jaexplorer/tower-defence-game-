// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
namespace AmplifyShaderEditor
{
	[NodeAttributes( "Unpack Scale Normal", "Textures", "Applies UnpackNormal/UnpackScaleNormal function" )]
	[Serializable]
	public class UnpackScaleNormalNode : ParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT4, false, "Value" );
			AddInputPort( WirePortDataType.FLOAT, false, "Normal Scale" );
			m_inputPorts[ 1 ].FloatInternalData = 1;
			AddOutputVectorPorts( WirePortDataType.FLOAT3, "XYZ" );
			m_useInternalPortData = true;
			m_previewShaderGUID = "8b0ae05e25d280c45af81ded56f8012e";
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			string src = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
			

			bool isScaledNormal = false;
			if ( m_inputPorts[ 1 ].IsConnected )
			{
				isScaledNormal = true;
			}
			else
			{
				if ( m_inputPorts[ 1 ].FloatInternalData != 1 )
				{
					isScaledNormal = true;
				}
			}

			string normalMapUnpackMode = string.Empty;
			if ( isScaledNormal )
			{
				string scaleValue = m_inputPorts[ 1 ].GeneratePortInstructions( ref dataCollector );
				dataCollector.AddToIncludes( m_uniqueId, Constants.UnityStandardUtilsLibFuncs );
				normalMapUnpackMode = "UnpackScaleNormal( " + src + " ," + scaleValue + " )";
			}
			else
			{
				normalMapUnpackMode = "UnpackNormal( " + src + " )";
			}

			int outputUsage = 0;
			for ( int i = 0; i < m_outputPorts.Count; i++ )
			{
				if ( m_outputPorts[ i ].IsConnected )
					outputUsage += 1;
			}


			if ( outputUsage > 1 )
			{
				string varName = "localUnpackNormal" + m_uniqueId;
				dataCollector.AddToLocalVariables( m_uniqueId, "float3 " + varName + " = " + normalMapUnpackMode + ";" );
				return GetOutputVectorItem( 0, outputId, varName );
			}
			else
			{
				return GetOutputVectorItem( 0, outputId, normalMapUnpackMode );
			}
		}
	}
}
