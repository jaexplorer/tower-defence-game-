// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "Break To Components", "Misc", "Break a data connection into its components" )]
	public sealed class BreakToComponentsNode : ParentNode
	{
		private WirePortDataType m_currentType = WirePortDataType.FLOAT;

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT, false, Constants.EmptyPortValue );
			for ( int i = 0; i < 16; i++ )
			{
				AddOutputPort( WirePortDataType.FLOAT, Constants.EmptyPortValue );
				m_outputPorts[ i ].IndexPreviewOffset = 1;
				if ( i != 0 )
				{
					m_outputPorts[ i ].Visible = false;
				}
			}
			m_previewShaderGUID = "5f58f74a202ba804daddec838b75207d";
		}

		void UpdateOutputs( WirePortDataType newType )
		{
			//this only happens when on initial load
			if ( newType == WirePortDataType.OBJECT )
				return;

			m_currentType = newType;
			switch ( newType )
			{
				case WirePortDataType.OBJECT:
				{
					m_outputPorts[ 0 ].ChangeProperties( Constants.EmptyPortValue, WirePortDataType.OBJECT, false );
					m_outputPorts[ 0 ].Visible = true;
					for ( int i = 1; i < m_outputPorts.Count; i++ )
					{
						m_outputPorts[ i ].Visible = false;
					}
				}
				break;
				case WirePortDataType.FLOAT:
				{
					m_outputPorts[ 0 ].ChangeProperties( Constants.EmptyPortValue, WirePortDataType.FLOAT, false );
					m_outputPorts[ 0 ].Visible = true;
					for ( int i = 1; i < m_outputPorts.Count; i++ )
					{
						m_outputPorts[ i ].Visible = false;
					}
				}
				break;
				case WirePortDataType.FLOAT2:
				{
					for ( int i = 0; i < 2; i++ )
					{
						m_outputPorts[ i ].ChangeProperties( "[" + i + "]", WirePortDataType.FLOAT, false );
						m_outputPorts[ i ].Visible = true;
					}
					for ( int i = 2; i < m_outputPorts.Count; i++ )
					{
						m_outputPorts[ i ].Visible = false;
					}
				}
				break;
				case WirePortDataType.FLOAT3:
				{
					for ( int i = 0; i < 3; i++ )
					{
						m_outputPorts[ i ].ChangeProperties( "[" + i + "]", WirePortDataType.FLOAT, false );
						m_outputPorts[ i ].Visible = true;
					}
					for ( int i = 3; i < m_outputPorts.Count; i++ )
					{
						m_outputPorts[ i ].Visible = false;
					}
				}
				break;
				case WirePortDataType.FLOAT4:
				{
					for ( int i = 0; i < 4; i++ )
					{
						m_outputPorts[ i ].ChangeProperties( "[" + i + "]", WirePortDataType.FLOAT, false );
						m_outputPorts[ i ].Visible = true;
					}
					for ( int i = 4; i < m_outputPorts.Count; i++ )
					{
						m_outputPorts[ i ].Visible = false;
					}
				}
				break;
				case WirePortDataType.FLOAT3x3:
				{
					for ( int i = 0; i < 9; i++ )
					{
						m_outputPorts[ i ].ChangeProperties( "[" + ( int ) ( i / 3 ) + "][" + i % 3 + "]", WirePortDataType.FLOAT, false );
						m_outputPorts[ i ].Visible = true;
					}
					for ( int i = 9; i < m_outputPorts.Count; i++ )
					{
						m_outputPorts[ i ].Visible = false;
					}
				}
				break;
				case WirePortDataType.FLOAT4x4:
				{
					for ( int i = 0; i < 16; i++ )
					{
						m_outputPorts[ i ].ChangeProperties( "[" + ( int ) ( i / 4 ) + "][" + i % 4 + "]", WirePortDataType.FLOAT, false );
						m_outputPorts[ i ].Visible = true;
					}
				}
				break;
				case WirePortDataType.COLOR:
				{
					for ( int i = 0; i < 4; i++ )
					{
						m_outputPorts[ i ].ChangeProperties( "[" + i + "]", WirePortDataType.FLOAT, false );
						m_outputPorts[ i ].Visible = true;
					}
					for ( int i = 4; i < m_outputPorts.Count; i++ )
					{
						m_outputPorts[ i ].Visible = false;
					}
				}
				break;
				case WirePortDataType.INT:
				{
					m_outputPorts[ 0 ].Visible = true;
					m_outputPorts[ 0 ].ChangeProperties( "", WirePortDataType.INT, false );
					for ( int i = 1; i < m_outputPorts.Count; i++ )
					{
						m_outputPorts[ i ].Visible = false;
					}
				}
				break;
			}
			m_sizeIsDirty = true;
		}

		public override void OnConnectedOutputNodeChanges( int outputPortId, int otherNodeId, int otherPortId, string name, WirePortDataType type )
		{
			base.OnConnectedOutputNodeChanges( outputPortId, otherNodeId, otherPortId, name, type );
			m_inputPorts[ 0 ].MatchPortToConnection();
			UpdateOutputs( m_inputPorts[ 0 ].DataType );
		}

		public override void OnInputPortConnected( int portId, int otherNodeId, int otherPortId, bool activateNode = true )
		{
			base.OnInputPortConnected( portId, otherNodeId, otherPortId, activateNode );
			m_inputPorts[ 0 ].MatchPortToConnection();
			UpdateOutputs( m_inputPorts[ 0 ].DataType );
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_currentType );
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			UpdateOutputs( ( WirePortDataType ) Enum.Parse( typeof( WirePortDataType ), GetCurrentParam( ref nodeParams ) ) );
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			string value = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
			switch ( m_inputPorts[ 0 ].DataType )
			{
				case WirePortDataType.OBJECT:
				case WirePortDataType.FLOAT:
				case WirePortDataType.INT:
				{
					return value;
				}
				case WirePortDataType.FLOAT2:
				case WirePortDataType.FLOAT3:
				case WirePortDataType.FLOAT4:
				{
					return GetOutputVectorItem( 0, outputId + 1, value );
				}
				case WirePortDataType.COLOR:
				{
					return GetOutputColorItem( 0, outputId + 1, value );
				}
				case WirePortDataType.FLOAT3x3:
				{
					return value + "[ " + ( ( int ) ( outputId / 3 ) ) + " ][ " + ( outputId % 3 ) + " ]";
				}
				case WirePortDataType.FLOAT4x4:
				{
					return value + "[ " + ( ( int ) ( outputId / 4 ) ) + " ][ " + ( outputId % 4 ) + " ]";
				}
			}
			return value;
		}
	}
}
