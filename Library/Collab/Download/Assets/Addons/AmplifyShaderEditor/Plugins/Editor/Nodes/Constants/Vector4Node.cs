// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using UnityEditor;
using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "Vector4", "Constants", "Vector4 property", null, KeyCode.Alpha4 )]
	public sealed class Vector4Node : PropertyNode
	{
		[SerializeField]
		private Vector4 m_defaultValue = Vector4.zero;

		[SerializeField]
		private Vector4 m_materialValue = Vector4.zero;

		private const float LabelWidth = 8;

		private int m_cachedPropertyId = -1;

		public Vector4Node() : base() { }
		public Vector4Node( int uniqueId, float x, float y, float width, float height ) : base( uniqueId, x, y, width, height ) { }
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_insideSize.Set( 50, 40 );
			m_selectedLocation = PreviewLocation.BottomCenter;
			AddOutputVectorPorts( WirePortDataType.FLOAT4, "XYZW" );
			m_precisionString = UIUtils.FinalPrecisionWirePortToCgType( m_currentPrecisionType, m_outputPorts[ 0 ].DataType );
			m_previewShaderGUID = "aac241d0e47a5a84fbd2edcd640788dc";
		}

		public override void CopyDefaultsToMaterial()
		{
			m_materialValue = m_defaultValue;
		}

		public override void DrawSubProperties()
		{
			m_defaultValue = EditorGUILayout.Vector4Field( Constants.DefaultValueLabel, m_defaultValue );
		}

		public override void DrawMaterialProperties()
		{
			if ( m_materialMode )
				EditorGUI.BeginChangeCheck();

			m_materialValue = EditorGUILayout.Vector4Field( Constants.MaterialValueLabel, m_materialValue );
			if ( m_materialMode && EditorGUI.EndChangeCheck() )
				m_requireMaterialUpdate = true;

		}

		public override void SetPreviewInputs()
		{
			base.SetPreviewInputs();
			
			if ( m_cachedPropertyId == -1 )
				m_cachedPropertyId = Shader.PropertyToID( "_InputVector" );

			if ( m_materialMode && m_currentParameterType != PropertyType.Constant )
				PreviewMaterial.SetVector( m_cachedPropertyId, new Vector4( m_materialValue[ 0 ], m_materialValue[ 1 ], m_materialValue[ 2 ], m_materialValue[ 3 ] ) );
			else
				PreviewMaterial.SetVector( m_cachedPropertyId, new Vector4( m_defaultValue[ 0 ], m_defaultValue[ 1 ], m_defaultValue[ 2 ], m_defaultValue[ 3 ] ) );
		}

		public override void Draw( DrawInfo drawInfo )
		{
			base.Draw( drawInfo );
			if ( m_isVisible )
			{
				m_propertyDrawPos = m_remainingBox;
				m_propertyDrawPos.x = m_remainingBox.x - LabelWidth * drawInfo.InvertedZoom;
				m_propertyDrawPos.width = drawInfo.InvertedZoom * Constants.FLOAT_DRAW_WIDTH_FIELD_SIZE;
				m_propertyDrawPos.height = drawInfo.InvertedZoom * Constants.FLOAT_DRAW_HEIGHT_FIELD_SIZE;

				EditorGUI.BeginChangeCheck();

				for ( int i = 0; i < 4; i++ )
				{
					m_propertyDrawPos.y = m_outputPorts[ i + 1 ].Position.y - 2 * drawInfo.InvertedZoom;
					if ( m_materialMode && m_currentParameterType != PropertyType.Constant )
					{
						float val = m_materialValue[ i ];
						UIUtils.DrawFloat( ref m_propertyDrawPos, ref val, LabelWidth * drawInfo.InvertedZoom );
						m_materialValue[ i ] = val;
					}
					else
					{
						float val = m_defaultValue[ i ];
						UIUtils.DrawFloat( ref m_propertyDrawPos, ref val, LabelWidth * drawInfo.InvertedZoom );
						m_defaultValue[ i ] = val;
					}
				}
				if ( EditorGUI.EndChangeCheck() )
				{
					m_requireMaterialUpdate = m_materialMode;
					m_propertyNameIsDirty = true;
					//MarkForPreviewUpdate();
				}
			}
		}

		public override void ConfigureLocalVariable( ref MasterNodeDataCollector dataCollector )
		{
			Vector4 value = m_defaultValue;
			if ( dataCollector.PortCategory == MasterNodePortCategory.Vertex || dataCollector.PortCategory == MasterNodePortCategory.Tessellation )
			{
				dataCollector.AddToVertexLocalVariables( m_uniqueId, CreateLocalVarDec( value.x + "," + value.y + "," + value.z + "," + value.w ) );
			}
			else
			{
				dataCollector.AddToLocalVariables( m_uniqueId, CreateLocalVarDec( value.x + "," + value.y + "," + value.z + "," + value.w ) );
			}
			m_outputPorts[ 0 ].SetLocalValue( m_propertyName );
			m_outputPorts[ 1 ].SetLocalValue( m_propertyName + ".x" );
			m_outputPorts[ 2 ].SetLocalValue( m_propertyName + ".y" );
			m_outputPorts[ 3 ].SetLocalValue( m_propertyName + ".z" );
			m_outputPorts[ 4 ].SetLocalValue( m_propertyName + ".w" );
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalvar );
			m_precisionString = UIUtils.FinalPrecisionWirePortToCgType( m_currentPrecisionType, m_outputPorts[ 0 ].DataType );

			if ( m_currentParameterType != PropertyType.Constant )
				return GetOutputVectorItem( 0, outputId, PropertyData );

			if ( m_outputPorts[ outputId ].IsLocalValue )
			{
				return m_outputPorts[ outputId ].LocalValue;
			}

			if ( CheckLocalVariable( ref dataCollector ) )
			{
				return m_outputPorts[ outputId ].LocalValue;
			}

			Vector4 value = m_defaultValue;
			string result = string.Empty;
			switch ( outputId )
			{
				case 0:
				{
					result = m_precisionString+"(" + value.x + "," + value.y + "," + value.z + "," + value.w + ")";
				}
				break;

				case 1:
				{
					result = value.x.ToString();
				}
				break;
				case 2:
				{
					result = value.y.ToString();
				}
				break;
				case 3:
				{
					result = value.z.ToString();
				}
				break;
				case 4:
				{
					result = value.w.ToString();
				}
				break;
			}

			if ( result.Equals( string.Empty ) )
			{
				UIUtils.ShowMessage( "Vector4Node generating empty code", MessageSeverity.Warning );
			}
			return result;
		}

		public override string GetPropertyValue()
		{
			return PropertyAttributes + m_propertyName + "(\"" + m_propertyInspectorName + "\", Vector) = (" + m_defaultValue.x + "," + m_defaultValue.y + "," + m_defaultValue.z + "," + m_defaultValue.w + ")";
		}
		
		public override void UpdateMaterial( Material mat )
		{
			base.UpdateMaterial( mat );
			if ( UIUtils.IsProperty( m_currentParameterType ) )
			{
				mat.SetVector( m_propertyName, m_materialValue );
			}
		}

		public override void SetMaterialMode( Material mat )
		{
			base.SetMaterialMode( mat );
			if ( m_materialMode && UIUtils.IsProperty( m_currentParameterType ) && mat.HasProperty( m_propertyName ) )
			{
				m_materialValue = mat.GetVector( m_propertyName );
			}
		}

		public override void ForceUpdateFromMaterial( Material material )
		{
			if ( UIUtils.IsProperty( m_currentParameterType ) && material.HasProperty( m_propertyName ) )
				m_materialValue = material.GetVector( m_propertyName );
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			string[] components = GetCurrentParam( ref nodeParams ).Split( IOUtils.VECTOR_SEPARATOR );
			if ( components.Length == 4 )
			{
				m_defaultValue.x = Convert.ToSingle( components[ 0 ] );
				m_defaultValue.y = Convert.ToSingle( components[ 1 ] );
				m_defaultValue.z = Convert.ToSingle( components[ 2 ] );
				m_defaultValue.w = Convert.ToSingle( components[ 3 ] );
			}
			else
			{
				UIUtils.ShowMessage( "Incorrect number of float4 values", MessageSeverity.Error );
			}
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_defaultValue.x.ToString() + IOUtils.VECTOR_SEPARATOR +
														m_defaultValue.y.ToString() + IOUtils.VECTOR_SEPARATOR +
														m_defaultValue.z.ToString() + IOUtils.VECTOR_SEPARATOR +
														m_defaultValue.w.ToString() );
		}

		public override string GetPropertyValStr()
		{
			return ( m_materialMode && m_currentParameterType != PropertyType.Constant ) ? m_materialValue.x.ToString( Mathf.Abs( m_materialValue.x ) > 1000 ? Constants.PropertyBigVectorFormatLabel : Constants.PropertyVectorFormatLabel ) + IOUtils.VECTOR_SEPARATOR +
																							m_materialValue.y.ToString( Mathf.Abs( m_materialValue.y ) > 1000 ? Constants.PropertyBigVectorFormatLabel : Constants.PropertyVectorFormatLabel ) + IOUtils.VECTOR_SEPARATOR +
																							m_materialValue.z.ToString( Mathf.Abs( m_materialValue.z ) > 1000 ? Constants.PropertyBigVectorFormatLabel : Constants.PropertyVectorFormatLabel ) + IOUtils.VECTOR_SEPARATOR +
																							m_materialValue.w.ToString( Mathf.Abs( m_materialValue.w ) > 1000 ? Constants.PropertyBigVectorFormatLabel : Constants.PropertyVectorFormatLabel ) :
																							m_defaultValue.x.ToString( Mathf.Abs( m_defaultValue.x ) > 1000 ? Constants.PropertyBigVectorFormatLabel : Constants.PropertyVectorFormatLabel ) + IOUtils.VECTOR_SEPARATOR +
																							m_defaultValue.y.ToString( Mathf.Abs( m_defaultValue.y ) > 1000 ? Constants.PropertyBigVectorFormatLabel : Constants.PropertyVectorFormatLabel ) + IOUtils.VECTOR_SEPARATOR +
																							m_defaultValue.z.ToString( Mathf.Abs( m_defaultValue.z ) > 1000 ? Constants.PropertyBigVectorFormatLabel : Constants.PropertyVectorFormatLabel ) + IOUtils.VECTOR_SEPARATOR +
																							m_defaultValue.w.ToString( Mathf.Abs( m_defaultValue.w ) > 1000 ? Constants.PropertyBigVectorFormatLabel : Constants.PropertyVectorFormatLabel );
		}

		public Vector4 Value
		{
			get { return m_defaultValue; }
			set { m_defaultValue = value; }
		}
	}
}
