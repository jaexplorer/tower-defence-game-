// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>
//
// Custom Node Vertex Tangent World
// Donated by Community Member Kebrus

using UnityEngine;
using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "World Tangent", "Surface Standard Inputs", "Per pixel world tangent vector", null, KeyCode.None, true, false, null, null, true )]
	public sealed class VertexTangentNode : ParentNode
	{
		private const string WorldTangentDefFrag = "WorldNormalVector( {0}, float3(1,0,0) )";
		private const string WorldTangentDefVert = "UnityObjectToWorldDir( {0}.tangent.xyz )";

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddOutputVectorPorts( WirePortDataType.FLOAT3, "XYZ" );
			m_drawPreviewAsSphere = true;
			m_previewShaderGUID = "61f0b80493c9b404d8c7bf56d59c3f81";
		}

		public override void PropagateNodeData( NodeData nodeData )
		{
			base.PropagateNodeData( nodeData );
			UIUtils.CurrentDataCollector.DirtyNormal = true;
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			bool isVertex = ( dataCollector.PortCategory == MasterNodePortCategory.Tessellation || dataCollector.PortCategory == MasterNodePortCategory.Vertex );

			if ( isVertex )
			{
				if ( m_outputPorts[ 0 ].IsLocalValue )
					return GetOutputVectorItem( 0, outputId, m_outputPorts[ 0 ].LocalValue );

				dataCollector.ForceNormal = true;

				dataCollector.AddToInput( m_uniqueId, UIUtils.GetInputDeclarationFromType( m_currentPrecisionType, AvailableSurfaceInputs.WORLD_NORMAL ), true );
				dataCollector.AddToInput( m_uniqueId, Constants.InternalData, false );

				RegisterLocalVariable( 0, string.Format( WorldTangentDefVert, Constants.VertexShaderInputStr ), ref dataCollector, "worldTangentVert" + m_uniqueId );

				return GetOutputVectorItem( 0, outputId, m_outputPorts[ 0 ].LocalValue );
			}
			else
			{
				if ( m_outputPorts[ 0 ].IsLocalValue )
					return GetOutputVectorItem( 0, outputId, m_outputPorts[ 0 ].LocalValue );

				dataCollector.ForceNormal = true;

				dataCollector.AddToInput( m_uniqueId, UIUtils.GetInputDeclarationFromType( m_currentPrecisionType, AvailableSurfaceInputs.WORLD_NORMAL ), true );
				dataCollector.AddToInput( m_uniqueId, Constants.InternalData, false );

				RegisterLocalVariable( 0, string.Format( WorldTangentDefFrag, Constants.InputVarStr ), ref dataCollector, "worldTangentFrag" + m_uniqueId );

				return GetOutputVectorItem( 0, outputId, m_outputPorts[ 0 ].LocalValue );
			}

		}
	}
}
