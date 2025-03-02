// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "Summed Blend", "Misc", "Mix all channels through weighted sum", null, KeyCode.None, true )]
	public sealed class SummedBlendNode : WeightedAvgNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_inputData = new string[ 6 ];
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			GetInputData( ref dataCollector, ignoreLocalvar );

			string result = string.Empty;
			string localVarName = "weightedBlendVar" + m_uniqueId;
			dataCollector.AddToLocalVariables( m_uniqueId, m_currentPrecisionType, m_inputPorts[ 0 ].DataType, localVarName, m_inputData[ 0 ] );

			if ( m_activeCount == 0 )
			{
				result = m_inputData[ 0 ];
			}
			else if ( m_activeCount == 1 )
			{
				result += localVarName + "*" + m_inputData[ 1 ];
			}
			else
			{
				for ( int i = 0; i < m_activeCount; i++ )
				{
					result += localVarName + Constants.VectorSuffixes[ i ] + "*" + m_inputData[ i + 1 ];
					if ( i != ( m_activeCount - 1 ) )
					{
						result += " + ";
					}
				}
			}

			result = UIUtils.AddBrackets( result );
			return CreateOutputLocalVariable( 0, result, ref dataCollector );
		}
	}
}
