// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( "Inverse Transpose Model View Matrix", "Transform", "All Transformation types" )]
	public sealed class InverseTranspMVMatrixNode : ConstantShaderVariable
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			ChangeOutputProperties( 0, "Out", WirePortDataType.FLOAT4x4 );
			m_value = "UNITY_MATRIX_IT_MV";
			m_drawPreview = false;
		}
	}
}
