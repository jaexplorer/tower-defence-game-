// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using UnityEditor;
using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "Matrix3X3", "Constants", "Matrix3X3 property" )]
	public sealed class Matrix3X3Node : PropertyNode
	{
		[SerializeField]
		private Matrix4x4 m_defaultValue = Matrix4x4.identity;

		[SerializeField]
		private Matrix4x4 m_materialValue = Matrix4x4.identity;

		public Matrix3X3Node() : base() { }
		public Matrix3X3Node( int uniqueId, float x, float y, float width, float height ) : base( uniqueId, x, y, width, height ) { }
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddOutputPort( WirePortDataType.FLOAT3x3, Constants.EmptyPortValue );
			m_insideSize.Set( Constants.FLOAT_DRAW_WIDTH_FIELD_SIZE * 3 + Constants.FLOAT_WIDTH_SPACING * 2, Constants.FLOAT_DRAW_HEIGHT_FIELD_SIZE * 3 + Constants.FLOAT_WIDTH_SPACING * 2 + Constants.OUTSIDE_WIRE_MARGIN );
			m_defaultValue = new Matrix4x4();
			m_materialValue = new Matrix4x4();
			m_drawPreview = false;
			m_precisionString = UIUtils.FinalPrecisionWirePortToCgType( m_currentPrecisionType, m_outputPorts[ 0 ].DataType );
		}

		public override void CopyDefaultsToMaterial()
		{
			m_materialValue = m_defaultValue;
		}

		public override void DrawSubProperties()
		{
			EditorGUILayout.LabelField( Constants.DefaultValueLabel );
			for ( int row = 0; row < 3; row++ )
			{
				EditorGUILayout.BeginHorizontal();
				for ( int column = 0; column < 3; column++ )
				{
					m_defaultValue[ row, column ] = EditorGUILayout.FloatField( string.Empty, m_defaultValue[ row, column ], GUILayout.MaxWidth( 76 ) );
				}
				EditorGUILayout.EndHorizontal();
			}
		}

		public override void DrawMaterialProperties()
		{
			if ( m_materialMode )
				EditorGUI.BeginChangeCheck();

			EditorGUILayout.LabelField( Constants.MaterialValueLabel );
			for ( int row = 0; row < 3; row++ )
			{
				EditorGUILayout.BeginHorizontal();
				for ( int column = 0; column < 3; column++ )
				{
					m_materialValue[ row, column ] = EditorGUILayout.FloatField( string.Empty, m_materialValue[ row, column ], GUILayout.MaxWidth( 76 ) );
				}
				EditorGUILayout.EndHorizontal();
			}

			if ( m_materialMode && EditorGUI.EndChangeCheck() )
				m_requireMaterialUpdate = true;
		}

		public override void Draw( DrawInfo drawInfo )
		{
			base.Draw( drawInfo );
			if ( m_isVisible )
			{
				m_propertyDrawPos.position = m_remainingBox.position;

				bool currMode = m_materialMode && m_currentParameterType != PropertyType.Constant;
				Matrix4x4 value = currMode ? m_materialValue : m_defaultValue;

				m_propertyDrawPos.width = drawInfo.InvertedZoom * Constants.FLOAT_DRAW_WIDTH_FIELD_SIZE;
				m_propertyDrawPos.height = drawInfo.InvertedZoom * Constants.FLOAT_DRAW_HEIGHT_FIELD_SIZE;

				EditorGUI.BeginChangeCheck();
				for ( int row = 0; row < 3; row++ )
				{
					for ( int column = 0; column < 3; column++ )
					{
						m_propertyDrawPos.position = m_remainingBox.position + Vector2.Scale( m_propertyDrawPos.size, new Vector2( column, row ) ) + new Vector2( Constants.FLOAT_WIDTH_SPACING * drawInfo.InvertedZoom * column, Constants.FLOAT_WIDTH_SPACING * drawInfo.InvertedZoom * row );
						value[ row, column ] = EditorGUI.FloatField( m_propertyDrawPos, string.Empty, value[ row, column ], UIUtils.MainSkin.textField );
					}
				}

				if ( currMode )
				{
					m_materialValue = value;
				}
				else
				{
					m_defaultValue = value;
				}

				if ( EditorGUI.EndChangeCheck() )
				{
					m_requireMaterialUpdate = m_materialMode;
					BeginDelayedDirtyProperty();
				}
			}
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalvar );
			m_precisionString = UIUtils.FinalPrecisionWirePortToCgType( m_currentPrecisionType, m_outputPorts[ 0 ].DataType );
			if ( m_currentParameterType != PropertyType.Constant )
				return PropertyData;

			Matrix4x4 value = m_defaultValue;

			return m_precisionString + "(" + value[ 0, 0 ] + "," + value[ 0, 1 ] + "," + value[ 0, 2 ] + "," +
								+value[ 1, 0 ] + "," + value[ 1, 1 ] + "," + value[ 1, 2 ] + "," +
								+value[ 2, 0 ] + "," + value[ 2, 1 ] + "," + value[ 2, 2 ] + ")";

		}


		public override void UpdateMaterial( Material mat )
		{
			base.UpdateMaterial( mat );
			if ( UIUtils.IsProperty( m_currentParameterType ) )
			{
				mat.SetMatrix( m_propertyName, m_materialValue );
			}
		}

		public override void SetMaterialMode( Material mat )
		{
			base.SetMaterialMode( mat );
			if ( m_materialMode && UIUtils.IsProperty( m_currentParameterType ) && mat.HasProperty( m_propertyName ) )
			{
				m_materialValue = mat.GetMatrix( m_propertyName );
			}
		}

		public override void ForceUpdateFromMaterial( Material material )
		{
			if ( UIUtils.IsProperty( m_currentParameterType ) && material.HasProperty( m_propertyName ) )
				m_materialValue = material.GetMatrix( m_propertyName );
		}


		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			string[] matrixVals = GetCurrentParam( ref nodeParams ).Split( IOUtils.VECTOR_SEPARATOR );
			if ( matrixVals.Length == 9 )
			{
				m_defaultValue[ 0, 0 ] = Convert.ToSingle( matrixVals[ 0 ] );
				m_defaultValue[ 0, 1 ] = Convert.ToSingle( matrixVals[ 1 ] );
				m_defaultValue[ 0, 2 ] = Convert.ToSingle( matrixVals[ 2 ] );

				m_defaultValue[ 1, 0 ] = Convert.ToSingle( matrixVals[ 3 ] );
				m_defaultValue[ 1, 1 ] = Convert.ToSingle( matrixVals[ 4 ] );
				m_defaultValue[ 1, 2 ] = Convert.ToSingle( matrixVals[ 5 ] );

				m_defaultValue[ 2, 0 ] = Convert.ToSingle( matrixVals[ 6 ] );
				m_defaultValue[ 2, 1 ] = Convert.ToSingle( matrixVals[ 7 ] );
				m_defaultValue[ 2, 2 ] = Convert.ToSingle( matrixVals[ 8 ] );
			}
			else
			{
				UIUtils.ShowMessage( "Incorrect number of matrix4x4 values", MessageSeverity.Error );
			}
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );

			IOUtils.AddFieldValueToString( ref	nodeInfo, m_defaultValue[ 0, 0 ].ToString() + IOUtils.VECTOR_SEPARATOR + m_defaultValue[ 0, 1 ].ToString() + IOUtils.VECTOR_SEPARATOR + m_defaultValue[ 0, 2 ].ToString() + IOUtils.VECTOR_SEPARATOR +
												m_defaultValue[ 1, 0 ].ToString() + IOUtils.VECTOR_SEPARATOR + m_defaultValue[ 1, 1 ].ToString() + IOUtils.VECTOR_SEPARATOR + m_defaultValue[ 1, 2 ].ToString() + IOUtils.VECTOR_SEPARATOR +
												m_defaultValue[ 2, 0 ].ToString() + IOUtils.VECTOR_SEPARATOR + m_defaultValue[ 2, 1 ].ToString() + IOUtils.VECTOR_SEPARATOR + m_defaultValue[ 2, 2 ].ToString() );
		}

		public override string GetPropertyValStr()
		{
			return ( m_materialMode && m_currentParameterType != PropertyType.Constant ) ? m_materialValue[ 0, 0 ].ToString( Mathf.Abs( m_materialValue[ 0, 0 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel ) + IOUtils.VECTOR_SEPARATOR + m_materialValue[ 0, 1 ].ToString( Mathf.Abs( m_materialValue[ 0, 1 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel ) + IOUtils.VECTOR_SEPARATOR + m_materialValue[ 0, 2 ].ToString( Mathf.Abs( m_materialValue[ 0, 2 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel ) + IOUtils.MATRIX_DATA_SEPARATOR +
																							m_materialValue[ 1, 0 ].ToString( Mathf.Abs( m_materialValue[ 1, 0 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel ) + IOUtils.VECTOR_SEPARATOR + m_materialValue[ 1, 1 ].ToString( Mathf.Abs( m_materialValue[ 1, 1 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel ) + IOUtils.VECTOR_SEPARATOR + m_materialValue[ 1, 2 ].ToString( Mathf.Abs( m_materialValue[ 1, 2 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel ) + IOUtils.MATRIX_DATA_SEPARATOR +
																							m_materialValue[ 2, 0 ].ToString( Mathf.Abs( m_materialValue[ 2, 0 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel ) + IOUtils.VECTOR_SEPARATOR + m_materialValue[ 2, 1 ].ToString( Mathf.Abs( m_materialValue[ 2, 1 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel ) + IOUtils.VECTOR_SEPARATOR + m_materialValue[ 2, 2 ].ToString( Mathf.Abs( m_materialValue[ 2, 2 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel ) :

																							m_defaultValue[ 0, 0 ].ToString( Mathf.Abs( m_defaultValue[ 0, 0 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel ) + IOUtils.VECTOR_SEPARATOR + m_defaultValue[ 0, 1 ].ToString( Mathf.Abs( m_defaultValue[ 0, 1 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel ) + IOUtils.VECTOR_SEPARATOR + m_defaultValue[ 0, 2 ].ToString( Mathf.Abs( m_defaultValue[ 0, 2 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel ) + IOUtils.MATRIX_DATA_SEPARATOR +
																							m_defaultValue[ 1, 0 ].ToString( Mathf.Abs( m_defaultValue[ 1, 0 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel ) + IOUtils.VECTOR_SEPARATOR + m_defaultValue[ 1, 1 ].ToString( Mathf.Abs( m_defaultValue[ 1, 1 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel ) + IOUtils.VECTOR_SEPARATOR + m_defaultValue[ 1, 2 ].ToString( Mathf.Abs( m_defaultValue[ 1, 2 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel ) + IOUtils.MATRIX_DATA_SEPARATOR +
																							m_defaultValue[ 2, 0 ].ToString( Mathf.Abs( m_defaultValue[ 2, 0 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel ) + IOUtils.VECTOR_SEPARATOR + m_defaultValue[ 2, 1 ].ToString( Mathf.Abs( m_defaultValue[ 2, 1 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel ) + IOUtils.VECTOR_SEPARATOR + m_defaultValue[ 2, 2 ].ToString( Mathf.Abs( m_defaultValue[ 2, 2 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel );
		}

	}
}
