using System;
using UnityEngine;
using UnityEditor;

namespace AmplifyShaderEditor
{
	public enum eVectorFromMatrixMode
	{
		Row,
		Column
	}

	[Serializable]
	[NodeAttributes( "Vector From Matrix", "Misc", "Retrieve vector data from a matrix" )]
	public sealed class VectorFromMatrixNode : ParentNode
	{
		private const string IndexStr = "Index";
		private const string ModeStr = "Mode";

		[SerializeField]
		private eVectorFromMatrixMode _mode = eVectorFromMatrixMode.Row;

		[SerializeField]
		private int m_index = 0;

		[SerializeField]
		private int m_maxIndex = 3;

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT4x4, false, Constants.EmptyPortValue );
			AddOutputVectorPorts( WirePortDataType.FLOAT4, "XYZW" );
			m_useInternalPortData = true;
			m_autoWrapProperties = true;
		}
		public override void OnInputPortConnected( int portId, int otherNodeId, int otherPortId, bool activateNode = true )
		{
			base.OnInputPortConnected( portId, otherNodeId, otherPortId, activateNode );
			UpdatePorts();
		}

		public override void OnConnectedOutputNodeChanges( int inputPortId, int otherNodeId, int otherPortId, string name, WirePortDataType type )
		{
			base.OnConnectedOutputNodeChanges( inputPortId, otherNodeId, otherPortId, name, type );
			UpdatePorts();
		}

		void UpdatePorts()
		{
			m_inputPorts[ 0 ].MatchPortToConnection();

			if ( m_inputPorts[ 0 ].DataType == WirePortDataType.FLOAT3x3 )
			{
				m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT3, false );
				m_outputPorts[ 0 ].Name = "XYZ";
				m_maxIndex = 2;
				m_outputPorts[ 4 ].Visible = false;
			}
			else
			{
				m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT4, false );
				m_outputPorts[ 0 ].Name = "XYZW";
				m_maxIndex = 3;
				m_outputPorts[ 4 ].Visible = true;
			}
			m_sizeIsDirty = true;
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			string value = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
			if ( m_inputPorts[ 0 ].DataType != WirePortDataType.FLOAT4x4 &&
				m_inputPorts[ 0 ].DataType != WirePortDataType.FLOAT3x3 )
			{
				value = UIUtils.CastPortType( dataCollector.PortCategory, m_currentPrecisionType, new NodeCastInfo( m_uniqueId, outputId ), value, m_inputPorts[ 0 ].DataType, WirePortDataType.FLOAT4x4, value );
			}
			if ( _mode == eVectorFromMatrixMode.Row )
			{
				value += "[" + m_index + "]";
			}
			else
			{
				string formatStr = value + "[{0}]" + "[" + m_index + "]";
				int count = 4;
				if ( m_inputPorts[ 0 ].DataType != WirePortDataType.FLOAT3x3 )
				{
					value = "float4( ";
				}
				else
				{
					count = 3;
					value = "float3( ";
				}

				for ( int i = 0; i < count; i++ )
				{
					value += string.Format( formatStr, i );
					if ( i != ( count - 1 ) )
					{
						value += ",";
					}
				}
				value += " )";
			}
			return GetOutputVectorItem( 0, outputId, value );
		}

		public override void DrawProperties()
		{
			m_index = EditorGUILayout.IntSlider( IndexStr, m_index, 0, m_maxIndex );
			_mode = ( eVectorFromMatrixMode ) EditorGUILayout.EnumPopup( ModeStr, _mode );
			base.DrawProperties();
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, _mode );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_index );
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			_mode = ( eVectorFromMatrixMode ) Enum.Parse( typeof( eVectorFromMatrixMode ), GetCurrentParam( ref nodeParams ) );
			m_index = Convert.ToInt32( GetCurrentParam( ref nodeParams ) );
		}
	}
}
