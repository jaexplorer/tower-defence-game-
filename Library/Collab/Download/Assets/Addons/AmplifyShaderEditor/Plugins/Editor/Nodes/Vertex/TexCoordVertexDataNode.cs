// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using UnityEngine;
using UnityEditor;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "Vertex TexCoord", "Vertex Data", "Vertex texture coordinates, can be used in both local vertex offset and fragment outputs" )]
	public sealed class TexCoordVertexDataNode : VertexDataNode
	{
		[SerializeField]
		private int m_texcoordSize = 2;

		[SerializeField]
		private int m_index = 0;
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_currentVertexData = "texcoord";
			ChangeOutputProperties( 0, "UV", WirePortDataType.FLOAT2, false );
			m_outputPorts[ 1 ].Name = "U";
			m_outputPorts[ 2 ].Name = "V";
			m_outputPorts[ 3 ].Visible = false;
			m_outputPorts[ 4 ].Visible = false;
			m_outputPorts[ 3 ].Name = "W";
			m_outputPorts[ 4 ].Name = "T";
			m_autoWrapProperties = true;
			m_previewShaderGUID = "6c1bee77276896041bbb73b1b9e7f8ac";
		}

		public override void DrawProperties()
		{
			base.DrawProperties();
			EditorGUI.BeginChangeCheck();
			m_texcoordSize = EditorGUILayout.IntPopup( Constants.AvailableUVSizesLabel, m_texcoordSize, Constants.AvailableUVSizesStr, Constants.AvailableUVSizes );
			if ( EditorGUI.EndChangeCheck() )
			{
				UpdateOutput();
			}

			EditorGUI.BeginChangeCheck();
			m_index = EditorGUILayout.IntPopup( Constants.AvailableUVChannelLabel, m_index, Constants.AvailableUVChannelsStr, Constants.AvailableUVChannels );
			if ( EditorGUI.EndChangeCheck() )
			{
				m_currentVertexData = ( m_index == 0 ) ? "texcoord" : "texcoord" + Constants.AvailableUVChannelsStr[ m_index ];
			}
		}

		private void UpdateOutput()
		{
			if ( m_texcoordSize == 3 )
			{
				m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT3, false );
				m_outputPorts[ 0 ].Name = "UVW";
				m_outputPorts[ 3 ].Visible = true;
				m_outputPorts[ 4 ].Visible = false;
			}
			else if ( m_texcoordSize == 4 )
			{
				m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT4, false );
				m_outputPorts[ 0 ].Name = "UVWT";
				m_outputPorts[ 3 ].Visible = true;
				m_outputPorts[ 4 ].Visible = true;
			}
			else
			{
				m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT2, false );
				m_outputPorts[ 0 ].Name = "UV";
				m_outputPorts[ 3 ].Visible = false;
				m_outputPorts[ 4 ].Visible = false;
			}
			m_sizeIsDirty = true;
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalVar )
		{
			if ( dataCollector.PortCategory == MasterNodePortCategory.Vertex || dataCollector.PortCategory == MasterNodePortCategory.Tessellation )
			{
				return base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalVar );
			}
			else
			{
				if( m_texcoordSize > 2)
					dataCollector.UsingHigherSizeTexcoords = true;

				WirePortDataType size = ( WirePortDataType )( 1 << ( m_texcoordSize + 1 ) );
				string texcoords = GenerateFragUVs( ref dataCollector, m_uniqueId, m_index, null, size );
				return GetOutputVectorItem( 0, outputId, texcoords );
			}
		}

		/// <summary>
		/// Generates UV properties and uniforms and returns the varible name to use in the fragment shader
		/// </summary>
		/// <param name="dataCollector"></param>
		/// <param name="uniqueId"></param>
		/// <param name="index"></param>
		/// <returns>frag variable name</returns>
		static public string GenerateFragUVs( ref MasterNodeDataCollector dataCollector, int uniqueId, int index, string propertyName = null, WirePortDataType size = WirePortDataType.FLOAT2)
		{
			string dummyPropUV = "_texcoord" + ( index > 0 ? ( index + 1 ).ToString() : "" );
			string dummyUV = "uv" + ( index > 0 ? ( index + 1 ).ToString() : "" ) + dummyPropUV;

			dataCollector.AddToProperties( uniqueId, "[HideInInspector] " + dummyPropUV + "( \"\", 2D ) = \"white\" {}", 100 );
			dataCollector.AddToInput( uniqueId, UIUtils.WirePortToCgType( size )+" " + dummyUV, true );
			
			string result = Constants.InputVarStr + "." + dummyUV;
			if ( !string.IsNullOrEmpty(propertyName) )
			{
				dataCollector.AddToUniforms( uniqueId, "uniform float4 "+ propertyName+"_ST;" );
				dataCollector.AddToLocalVariables( uniqueId, PrecisionType.Float, size, "uv" + propertyName, result + " * " + propertyName +"_ST.xy + "+propertyName+"_ST.zw");
				result = "uv" + propertyName;
			}

			return result;
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			if ( UIUtils.CurrentShaderVersion() > 2502 )
			{
				m_index = Convert.ToInt32( GetCurrentParam( ref nodeParams ) );
			}

			if ( UIUtils.CurrentShaderVersion() > 5111 )
			{
				m_texcoordSize = Convert.ToInt32( GetCurrentParam( ref nodeParams ) );
				UpdateOutput();
			}
		}

		public override void WriteInputDataToString( ref string nodeInfo )
		{
			base.WriteInputDataToString( ref nodeInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_index );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_texcoordSize );
		}
	}
}
