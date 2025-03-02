// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>
//
// Custom Node HeightMap Texture Masking
// Donated by Rea

using UnityEngine;
using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "HeightMap Texture Blend", "Textures", "Advanced Texture Blending by using heightMap and splatMask, usefull for texture layering ", null, KeyCode.None, true, false, null, null, true )]
	public sealed class HeightMapBlendNode : ParentNode
	{

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.OBJECT, false, "HeightMap" );
			AddInputPort( WirePortDataType.OBJECT, false, "SplatMask" );
			AddInputPort( WirePortDataType.OBJECT, false, "BlendStrength" );
			AddOutputVectorPorts( WirePortDataType.FLOAT, Constants.EmptyPortValue );
			m_textLabelWidth = 120;
			m_useInternalPortData = true;
			m_inputPorts[ 2 ].FloatInternalData = 1;
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			string HeightMap = m_inputPorts[ 0 ].GenerateShaderForOutput( ref dataCollector, WirePortDataType.FLOAT, false, true );
			string SplatMask = m_inputPorts[ 1 ].GenerateShaderForOutput( ref dataCollector, WirePortDataType.FLOAT, false, true );
			string Blend = m_inputPorts[ 2 ].GenerateShaderForOutput( ref dataCollector, WirePortDataType.FLOAT, false, true );

			string HeightMask = "float HeightMask" + m_uniqueId + " = saturate(pow(((" + HeightMap + "*" + SplatMask + ")*4)+(" + SplatMask + "*2)," + Blend + "));";
			string result = "HeightMask" + m_uniqueId;

			dataCollector.AddToLocalVariables( m_uniqueId, HeightMask );
			return GetOutputVectorItem( 0, outputId, result ); ;
		}
		/*
         A = (heightMap * SplatMask)*4
         B = SplatMask*2
         C = pow(A+B,Blend)
         saturate(C)
         saturate(pow(((heightMap * SplatMask)*4)+(SplatMask*2),Blend));
         */
	}
}
