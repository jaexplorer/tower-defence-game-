// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using System;
using UnityEditor;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "Layered Blend", "Misc", "Mix all channels through interpolation factors", null, KeyCode.None, true )]
	public sealed class LayeredBlendNode : WeightedAvgNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_inputPorts[ 1 ].Name = "Layer Base";
			AddInputPort( WirePortDataType.FLOAT, false, string.Empty );
			for ( int i = 2; i < m_inputPorts.Count; i++ )
			{
				m_inputPorts[ i ].Name = AmountsStr[ i - 2 ];
			}
			m_inputData = new string[ 6 ];
			m_minimumSize = 2;
			UpdateConnection( 0 );
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			GetInputData( ref dataCollector, ignoreLocalvar );

			string result = string.Empty;
			string localVarName = "layeredBlendVar" + m_uniqueId;
			dataCollector.AddToLocalVariables( m_uniqueId, m_currentPrecisionType, m_inputPorts[ 0 ].DataType, localVarName, m_inputData[ 0 ] );
			
			if ( m_activeCount == 1 )
			{
				result = m_inputData[ 0 ];
			}
			else if ( m_activeCount == 2 )
			{
				result += "lerp( " + m_inputData[ 1 ] + "," + m_inputData[ 2 ] + " , " + localVarName + " )";
			}
			else
			{
				result = m_inputData[ 1 ];
				for ( int i = 1; i < m_activeCount; i++ )
				{
					result = "lerp( " + result + " , " + m_inputData[ i + 1 ] + " , " + localVarName + Constants.VectorSuffixes[ i - 1 ] + " )";
				}
			}
			result = UIUtils.AddBrackets( result );
			return CreateOutputLocalVariable( 0, result, ref dataCollector );
		}
	}
}
