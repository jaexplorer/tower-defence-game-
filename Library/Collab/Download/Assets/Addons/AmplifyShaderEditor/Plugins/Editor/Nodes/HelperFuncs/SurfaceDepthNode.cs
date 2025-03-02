using UnityEngine;
using UnityEditor;

using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "Surface Depth", "Generic", "Returns the surface view depth" )]
	public sealed class SurfaceDepthNode : ParentNode
	{
		[SerializeField]
		private int m_viewSpaceInt = 0;

		private readonly string[] m_viewSpaceStr = { "Eye Space", "0-1 Space" };

		private readonly string[] m_vertexNameStr = { "eyeDepth", "clampDepth" };

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddOutputPort( WirePortDataType.FLOAT, "Depth" );
			m_autoWrapProperties = true;
		}

		public override void DrawProperties()
		{
			base.DrawProperties();
			m_viewSpaceInt = EditorGUILayout.Popup( "View Space", m_viewSpaceInt, m_viewSpaceStr );
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			dataCollector.AddToIncludes( m_uniqueId, Constants.UnityShaderVariables );
			dataCollector.AddToInput( m_uniqueId, "float " + m_vertexNameStr[ m_viewSpaceInt ], true );

			string space = m_viewSpaceInt == 1 ? space = " * _ProjectionParams.w" : "";

			string instruction = "-UnityObjectToViewPos( " + Constants.VertexShaderInputStr + ".vertex.xyz ).z" + space;
			dataCollector.AddVertexInstruction( Constants.VertexShaderOutputStr + "." + m_vertexNameStr[ m_viewSpaceInt ] + " = " + instruction, m_uniqueId );

			return GetOutputVectorItem( 0, outputId, Constants.InputVarStr + "." + m_vertexNameStr[ m_viewSpaceInt ] );
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			m_viewSpaceInt = Convert.ToInt32( GetCurrentParam( ref nodeParams ) );

		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_viewSpaceInt );
		}
	}

}
