// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using UnityEngine;
using UnityEditor;

namespace AmplifyShaderEditor
{
	[Serializable]
	public sealed class InputPort : WirePort
	{
		private const string InputDefaultNameStr = "Input";

		[SerializeField]
		private bool m_typeLocked;

		[SerializeField]
		private string m_internalData = string.Empty;

		[SerializeField]
		private string m_internalDataWrapper = string.Empty;

		[SerializeField]
		private string m_dataName = string.Empty;

		[SerializeField]
		private string m_internalDataPropertyLabel = string.Empty;

		// this will only is important on master node
		[SerializeField]
		private MasterNodePortCategory m_category = MasterNodePortCategory.Fragment;

		private string m_propertyName = string.Empty;
		private int m_cachedPropertyId = -1;

		private int m_cachedFloatShaderID = -1;
		private int m_cachedVectorShaderID = -1;
		private int m_cachedColorShaderID = -1;

		//[SerializeField]
		//private RenderTexture m_inputPreview = null;
		//[SerializeField]
		private RenderTexture m_inputPreviewTexture = null;
		private Material m_inputPreviewMaterial = null;
		private Shader m_inputPreviewShader = null;

		public InputPort() : base( -1, -1, WirePortDataType.FLOAT, string.Empty ) { m_typeLocked = true; UpdateInternalData(); }
		public InputPort( int nodeId, int portId, WirePortDataType dataType, string name, bool typeLocked, int orderId = -1, MasterNodePortCategory category = MasterNodePortCategory.Fragment ) : base( nodeId, portId, dataType, name, orderId )
		{
			m_dataName = name;
			m_internalDataPropertyLabel = ( string.IsNullOrEmpty( name ) || name.Equals( Constants.EmptyPortValue ) ) ? InputDefaultNameStr : name;
			m_typeLocked = typeLocked;
			m_category = category;
			UpdateInternalData();
		}

		public override void FullDeleteConnections()
		{
			UIUtils.DeleteConnection( true, m_nodeId, m_portId, true, true );
		}

		public override void NotifyExternalRefencesOnChange()
		{
			for ( int i = 0; i < m_externalReferences.Count; i++ )
			{
				ParentNode node = UIUtils.GetNode( m_externalReferences[ i ].NodeId );
				if ( node )
				{
					OutputPort port = node.GetOutputPortByUniqueId( m_externalReferences[ i ].PortId );
					port.UpdateInfoOnExternalConn( m_nodeId, m_portId, m_dataType );
					node.OnConnectedInputNodeChanges( m_externalReferences[ i ].PortId, m_nodeId, m_portId, m_name, m_dataType );
				}
			}
		}

		public void UpdateInternalData()
		{
			string[] data = String.IsNullOrEmpty( m_internalData ) ? null : m_internalData.Split( IOUtils.VECTOR_SEPARATOR );
			switch ( m_dataType )
			{
				case WirePortDataType.OBJECT:
				case WirePortDataType.FLOAT:
				{
					m_internalData = ( data == null ) ? "0.0" : data[ 0 ];
					m_internalDataWrapper = string.Empty;
				}
				break;
				case WirePortDataType.INT:
				{
					if ( data == null )
					{
						m_internalData = "0";
					}
					else
					{
						string[] intData = data[ 0 ].Split( IOUtils.FLOAT_SEPARATOR );
						m_internalData = ( intData.Length == 0 ) ? "0" : intData[ 0 ];
					}
					m_internalDataWrapper = string.Empty;
				}
				break;
				case WirePortDataType.FLOAT2:
				{
					if ( data == null )
					{
						m_internalData = "0" + IOUtils.VECTOR_SEPARATOR + "0";
					}
					else
					{
						m_internalData = ( data.Length < 2 ) ? ( data[ 0 ] + IOUtils.VECTOR_SEPARATOR + "0" ) : ( data[ 0 ] + IOUtils.VECTOR_SEPARATOR + data[ 1 ] );
					}
					m_internalDataWrapper = "float2( {0} )";
				}
				break;
				case WirePortDataType.FLOAT3:
				{
					if ( data == null )
					{
						m_internalData = "0" + IOUtils.VECTOR_SEPARATOR +
										"0" + IOUtils.VECTOR_SEPARATOR +
										"0";
					}
					else
					{
						if ( data.Length < 3 )
						{
							if ( data.Length == 1 )
							{
								m_internalData = data[ 0 ] + IOUtils.VECTOR_SEPARATOR +
												"0" + IOUtils.VECTOR_SEPARATOR +
												"0";
							}
							else
							{
								m_internalData = data[ 0 ] + IOUtils.VECTOR_SEPARATOR +
												data[ 1 ] + IOUtils.VECTOR_SEPARATOR +
												"0";
							}
						}
						else if ( data.Length > 3 )
						{
							m_internalData = data[ 0 ] + IOUtils.VECTOR_SEPARATOR +
											data[ 1 ] + IOUtils.VECTOR_SEPARATOR +
											data[ 2 ];
						}
					}

					m_internalDataWrapper = "float3( {0} )";
				}
				break;
				case WirePortDataType.FLOAT4:
				case WirePortDataType.COLOR:
				{
					if ( data == null )
					{
						m_internalData = "0" + IOUtils.VECTOR_SEPARATOR +
										"0" + IOUtils.VECTOR_SEPARATOR +
										"0" + IOUtils.VECTOR_SEPARATOR +
										"0";
					}
					else
					{
						if ( data.Length > 4 )
						{
							m_internalData = string.Empty;
							for ( int i = 0; i < 3; i++ )
							{
								m_internalData += data[ i ] + IOUtils.VECTOR_SEPARATOR;
							}
							m_internalData += data[ 3 ];
						}
						else if ( data.Length < 4 )
						{
							m_internalData = string.Empty;
							int i;
							for ( i = 0; i < data.Length; i++ )
							{
								m_internalData += data[ i ] + IOUtils.VECTOR_SEPARATOR;
							}

							for ( ; i < 3; i++ )
							{
								m_internalData += "0" + IOUtils.VECTOR_SEPARATOR;
							}
							m_internalData += "0";
						}
					}
					m_internalDataWrapper = "float4( {0} )";
				}
				break;
				case WirePortDataType.FLOAT3x3:
				case WirePortDataType.FLOAT4x4:
				{
					if ( data == null )
					{
						for ( int i = 0; i < 15; i++ )
						{
							m_internalData += "0" + IOUtils.VECTOR_SEPARATOR;
						}
						m_internalData += "0";
					}
					else
					{
						if ( data.Length < 16 )
						{
							m_internalData = string.Empty;
							int i;

							for ( i = 0; i < data.Length; i++ )
							{
								m_internalData += data[ i ] + IOUtils.VECTOR_SEPARATOR;
							}

							for ( ; i < 15; i++ )
							{
								m_internalData += "0" + IOUtils.VECTOR_SEPARATOR;
							}
							m_internalData += "0";
						}
					}
					m_internalDataWrapper = "float4x4( {0} )";
				}
				break;
			}
		}

		//TODO: Replace GenerateShaderForOutput(...) calls to this one
		// This is a new similar method to GenerateShaderForOutput(...) which always autocasts
		public string GeneratePortInstructions( ref MasterNodeDataCollector dataCollector )
		{
			string result = string.Empty;
			if ( m_externalReferences.Count > 0 && !m_locked )
			{
				result = UIUtils.GetNode( m_externalReferences[ 0 ].NodeId ).GenerateShaderForOutput( m_externalReferences[ 0 ].PortId, ref dataCollector, false );
				if ( m_externalReferences[ 0 ].DataType != m_dataType )
				{
					result = UIUtils.CastPortType( dataCollector.PortCategory, UIUtils.GetNode( m_nodeId ).CurrentPrecisionType, new NodeCastInfo( m_externalReferences[ 0 ].NodeId, m_externalReferences[ 0 ].PortId ), null, m_externalReferences[ 0 ].DataType, m_dataType, result );
				}
			}
			else
			{
				result = !String.IsNullOrEmpty( m_internalDataWrapper ) ? String.Format( m_internalDataWrapper, m_internalData ) : m_internalData;
			}
			return result;
		}

		public string GenerateShaderForOutput( ref MasterNodeDataCollector dataCollector, bool ignoreLocalVar )
		{
			string result = string.Empty;
			if ( m_externalReferences.Count > 0 && !m_locked )
			{
				result = UIUtils.GetNode( m_externalReferences[ 0 ].NodeId ).GenerateShaderForOutput( m_externalReferences[ 0 ].PortId, ref dataCollector, ignoreLocalVar );
			}
			else
			{
				if ( !String.IsNullOrEmpty( m_internalDataWrapper ) )
				{
					result = String.Format( m_internalDataWrapper, m_internalData );
				}
				else
				{
					result = m_internalData;
				}
			}
			return result;
		}

		public string GenerateShaderForOutput( ref MasterNodeDataCollector dataCollector, WirePortDataType inputPortType, bool ignoreLocalVar, bool autoCast = false )
		{
			string result = string.Empty;
			if ( m_externalReferences.Count > 0 && !m_locked )
			{
				result = UIUtils.GetNode( m_externalReferences[ 0 ].NodeId ).GenerateShaderForOutput( m_externalReferences[ 0 ].PortId, ref dataCollector, ignoreLocalVar );
				if ( autoCast && m_externalReferences[ 0 ].DataType != inputPortType )
				{
					result = UIUtils.CastPortType( dataCollector.PortCategory, UIUtils.GetNode( m_nodeId ).CurrentPrecisionType, new NodeCastInfo( m_externalReferences[ 0 ].NodeId, m_externalReferences[ 0 ].PortId ), null, m_externalReferences[ 0 ].DataType, inputPortType, result );
				}
			}
			else
			{
				if ( !String.IsNullOrEmpty( m_internalDataWrapper ) )
				{
					result = String.Format( m_internalDataWrapper, m_internalData );
				}
				else
				{
					result = m_internalData;
				}
			}

			return result;
		}

		public OutputPort GetOutputConnection( int connID = 0 )
		{
			if ( connID < m_externalReferences.Count )
			{
				return UIUtils.GetNode( m_externalReferences[ connID ].NodeId ).OutputPorts[ m_externalReferences[ connID ].PortId ];
			}
			return null;
		}

		public ParentNode GetOutputNode( int connID = 0 )
		{
			if ( connID < m_externalReferences.Count )
			{
				return UIUtils.GetNode( m_externalReferences[ connID ].NodeId );
			}
			return null;
		}

		public bool TypeLocked
		{
			get { return m_typeLocked; }
		}

		public void WriteToString( ref string myString )
		{
			if ( m_externalReferences.Count != 1 )
			{
				return;
			}

			IOUtils.AddTypeToString( ref myString, IOUtils.WireConnectionParam );
			IOUtils.AddFieldValueToString( ref myString, m_nodeId );
			IOUtils.AddFieldValueToString( ref myString, m_portId );
			IOUtils.AddFieldValueToString( ref myString, m_externalReferences[ 0 ].NodeId );
			IOUtils.AddFieldValueToString( ref myString, m_externalReferences[ 0 ].PortId );
			IOUtils.AddLineTerminator( ref myString );
		}

		public void ShowInternalData( bool useCustomLabel = false, string customLabel = null )
		{
			string label = ( useCustomLabel == true && customLabel != null ) ? customLabel : m_internalDataPropertyLabel;
			switch ( m_dataType )
			{
				case WirePortDataType.OBJECT:
				case WirePortDataType.FLOAT:
				{
					FloatInternalData = EditorGUILayout.FloatField( label, FloatInternalData );
				}
				break;
				case WirePortDataType.FLOAT2:
				{
					Vector2InternalData = EditorGUILayout.Vector2Field( label, Vector2InternalData );
				}
				break;
				case WirePortDataType.FLOAT3:
				{
					Vector3InternalData = EditorGUILayout.Vector3Field( label, Vector3InternalData );
				}
				break;
				case WirePortDataType.FLOAT4:
				{
					Vector4InternalData = EditorGUILayout.Vector4Field( label, Vector4InternalData );
				}
				break;
				case WirePortDataType.FLOAT3x3:
				case WirePortDataType.FLOAT4x4:
				{
					Matrix4x4 matrix = Matrix4x4InternalData;
					for ( int i = 0; i < 4; i++ )
					{
						Vector4 currVec = matrix.GetRow( i );
						EditorGUI.BeginChangeCheck();
						currVec = EditorGUILayout.Vector4Field( label + "[ " + i + " ]", currVec );
						if ( EditorGUI.EndChangeCheck() )
						{
							matrix.SetRow( i, currVec );
						}
					}
					Matrix4x4InternalData = matrix;
				}
				break;
				case WirePortDataType.COLOR:
				{
					ColorInternalData = EditorGUILayout.ColorField( label, ColorInternalData );
				}
				break;
				case WirePortDataType.INT:
				{
					IntInternalData = EditorGUILayout.IntField( label, IntInternalData );
				}
				break;
			}
		}

		public float FloatInternalData
		{
			set
			{
				InternalData = value.ToString();
				if ( value % 1 == 0 )
				{
					m_internalData += ".0";
				}
			}
			get
			{
				float data = 0f;
				try
				{
					data = Convert.ToSingle( m_internalData );
				}
				catch ( Exception e )
				{
					data = 0f;
					FloatInternalData = data;
					if ( DebugConsoleWindow.DeveloperMode )
						Debug.LogError( e );
				}
				return data;
			}
		}

		public int IntInternalData
		{
			set { InternalData = value.ToString(); }
			get
			{
				int data = 0;
				try
				{
					data = Convert.ToInt32( m_internalData );
				}
				catch ( Exception e)
				{
					data = 0;
					IntInternalData = data;
					if ( DebugConsoleWindow.DeveloperMode )
						Debug.LogError( e );
				}
				return data;
			}
		}

		public Vector2 Vector2InternalData
		{
			set { InternalData = value.x.ToString() + IOUtils.VECTOR_SEPARATOR + value.y.ToString(); }
			get
			{
				Vector2 data = new Vector2();
				string[] components = m_internalData.Split( IOUtils.VECTOR_SEPARATOR );
				if ( components.Length >= 2 )
				{
					try
					{
						data.x = Convert.ToSingle( components[ 0 ] );
						data.y = Convert.ToSingle( components[ 1 ] );
					}
					catch ( Exception e )
					{
						data = Vector2.zero;
						Vector2InternalData = data;
						if ( DebugConsoleWindow.DeveloperMode )
							Debug.LogError( e );
					}
				}
				else
				{
					Vector2InternalData = data;
					if ( DebugConsoleWindow.DeveloperMode )
						Debug.LogError( "Length smaller than 2" );
				}
				return data;
			}

		}

		public Vector3 Vector3InternalData
		{
			set
			{
				InternalData = value.x.ToString() + IOUtils.VECTOR_SEPARATOR +
								value.y.ToString() + IOUtils.VECTOR_SEPARATOR +
								value.z.ToString();
			}
			get
			{
				Vector3 data = new Vector3();
				string[] components = m_internalData.Split( IOUtils.VECTOR_SEPARATOR );
				if ( components.Length >= 3 )
				{
					try
					{
						data.x = Convert.ToSingle( components[ 0 ] );
						data.y = Convert.ToSingle( components[ 1 ] );
						data.z = Convert.ToSingle( components[ 2 ] );
					}
					catch ( Exception e)
					{
						data = Vector3.zero;
						Vector3InternalData = data;
						if ( DebugConsoleWindow.DeveloperMode )
							Debug.LogError( e );
					}
				}
				else
				{
					Vector3InternalData = data;
					if ( DebugConsoleWindow.DeveloperMode )
						Debug.LogError( "Length smaller than 3" );
				}
				return data;
			}
		}

		public Vector4 Vector4InternalData
		{
			set
			{
				InternalData = value.x.ToString() + IOUtils.VECTOR_SEPARATOR +
								value.y.ToString() + IOUtils.VECTOR_SEPARATOR +
								value.z.ToString() + IOUtils.VECTOR_SEPARATOR +
								value.w.ToString();
			}
			get
			{
				Vector4 data = new Vector4();
				string[] components = m_internalData.Split( IOUtils.VECTOR_SEPARATOR );
				if ( components.Length >= 4 )
				{
					try
					{
						data.x = Convert.ToSingle( components[ 0 ] );
						data.y = Convert.ToSingle( components[ 1 ] );
						data.z = Convert.ToSingle( components[ 2 ] );
						data.w = Convert.ToSingle( components[ 3 ] );
					}
					catch ( Exception e)
					{
						data = Vector4.zero;
						Vector4InternalData = data;
						if ( DebugConsoleWindow.DeveloperMode )
							Debug.LogError( e );
					}
				}
				else
				{
					Vector4InternalData = data;
					if ( DebugConsoleWindow.DeveloperMode )
						Debug.LogError( "Length smaller than 4" );
				}
				return data;
			}
		}

		public Color ColorInternalData
		{
			set
			{
				InternalData = value.r.ToString() + IOUtils.VECTOR_SEPARATOR +
								value.g.ToString() + IOUtils.VECTOR_SEPARATOR +
								value.b.ToString() + IOUtils.VECTOR_SEPARATOR +
								value.a.ToString();
			}
			get
			{
				Color data = new Color();
				string[] components = m_internalData.Split( IOUtils.VECTOR_SEPARATOR );
				if ( components.Length >= 4 )
				{
					try
					{
						data.r = Convert.ToSingle( components[ 0 ] );
						data.g = Convert.ToSingle( components[ 1 ] );
						data.b = Convert.ToSingle( components[ 2 ] );
						data.a = Convert.ToSingle( components[ 3 ] );
					}
					catch ( Exception e )
					{
						data = Color.clear;
						ColorInternalData = data;
						if ( DebugConsoleWindow.DeveloperMode )
							Debug.LogError( e );
					}
				}
				else
				{
					ColorInternalData = data;
					if ( DebugConsoleWindow.DeveloperMode )
						Debug.LogError( "Length smaller than 4" );
				}
				return data;
			}
		}

		public Matrix4x4 Matrix4x4InternalData
		{
			set
			{
				InternalData = value[ 0, 0 ].ToString() + IOUtils.VECTOR_SEPARATOR + value[ 0, 1 ].ToString() + IOUtils.VECTOR_SEPARATOR + value[ 0, 2 ].ToString() + IOUtils.VECTOR_SEPARATOR + value[ 0, 3 ].ToString() + IOUtils.VECTOR_SEPARATOR +
									value[ 1, 0 ].ToString() + IOUtils.VECTOR_SEPARATOR + value[ 1, 1 ].ToString() + IOUtils.VECTOR_SEPARATOR + value[ 1, 2 ].ToString() + IOUtils.VECTOR_SEPARATOR + value[ 1, 3 ].ToString() + IOUtils.VECTOR_SEPARATOR +
									value[ 2, 0 ].ToString() + IOUtils.VECTOR_SEPARATOR + value[ 2, 1 ].ToString() + IOUtils.VECTOR_SEPARATOR + value[ 2, 2 ].ToString() + IOUtils.VECTOR_SEPARATOR + value[ 2, 3 ].ToString() + IOUtils.VECTOR_SEPARATOR +
									value[ 3, 0 ].ToString() + IOUtils.VECTOR_SEPARATOR + value[ 3, 1 ].ToString() + IOUtils.VECTOR_SEPARATOR + value[ 3, 2 ].ToString() + IOUtils.VECTOR_SEPARATOR + value[ 3, 3 ].ToString();
			}
			get
			{
				Matrix4x4 data = new Matrix4x4();
				string[] components = m_internalData.Split( IOUtils.VECTOR_SEPARATOR );
				if ( components.Length >= 16 )
				{
					try
					{
						data[ 0, 0 ] = Convert.ToSingle( components[ 0 ] );
						data[ 0, 1 ] = Convert.ToSingle( components[ 1 ] );
						data[ 0, 2 ] = Convert.ToSingle( components[ 2 ] );
						data[ 0, 3 ] = Convert.ToSingle( components[ 3 ] );
						data[ 1, 0 ] = Convert.ToSingle( components[ 4 ] );
						data[ 1, 1 ] = Convert.ToSingle( components[ 5 ] );
						data[ 1, 2 ] = Convert.ToSingle( components[ 6 ] );
						data[ 1, 3 ] = Convert.ToSingle( components[ 7 ] );
						data[ 2, 0 ] = Convert.ToSingle( components[ 8 ] );
						data[ 2, 1 ] = Convert.ToSingle( components[ 9 ] );
						data[ 2, 2 ] = Convert.ToSingle( components[ 10 ] );
						data[ 2, 3 ] = Convert.ToSingle( components[ 11 ] );
						data[ 3, 0 ] = Convert.ToSingle( components[ 12 ] );
						data[ 3, 1 ] = Convert.ToSingle( components[ 13 ] );
						data[ 3, 2 ] = Convert.ToSingle( components[ 14 ] );
						data[ 3, 3 ] = Convert.ToSingle( components[ 15 ] );
					}
					catch ( Exception e )
					{
						Matrix4x4InternalData = data;
						if ( DebugConsoleWindow.DeveloperMode )
							Debug.LogError( e );
					}
				}
				else
				{
					Matrix4x4InternalData = data;
					if ( DebugConsoleWindow.DeveloperMode )
						Debug.LogError( "Length smaller than 16" );
				}
				return data;
			}
		}

		public override void ForceClearConnection()
		{
			UIUtils.DeleteConnection( true, m_nodeId, m_portId, false, true );
		}

		public string InternalData
		{
			get { return m_internalData; }
			set {
				if ( !value.Equals( m_internalData ) )
				{
					m_internalData = value;
				}
			}
		}

		public string WrappedInternalData
		{
			get { return string.IsNullOrEmpty( m_internalDataWrapper ) ? m_internalData : String.Format( m_internalDataWrapper, m_internalData ); }
		}

		public override WirePortDataType DataType
		{
			get { return base.DataType; }
			// must be set to update internal data. do not delete
			set
			{
				if ( base.DataType != value )
				{
					base.DataType = value;
					if ( m_externalReferences.Count == 0 )
						UpdateInternalData();
				}
			}
		}

		public string DataName
		{
			get { return m_dataName; }
			set { m_dataName = value; }
		}

		public MasterNodePortCategory Category
		{
			set { m_category = value; }
			get { return m_category; }
		}

		private int CachedFloatPropertyID
		{
			get
			{
				if ( m_cachedFloatShaderID == -1 )
					m_cachedFloatShaderID = Shader.PropertyToID( "_InputFloat" );
				return m_cachedFloatShaderID;
			}
		}

		private int CachedVectorPropertyID
		{
			get
			{
				if ( m_cachedVectorShaderID == -1 )
					m_cachedVectorShaderID = Shader.PropertyToID( "_InputVector" );
				return m_cachedVectorShaderID;
			}
		}

		private int CachedColorPropertyID
		{
			get
			{
				if ( m_cachedColorShaderID == -1 )
					m_cachedColorShaderID = Shader.PropertyToID( "_InputColor" );
				return m_cachedColorShaderID;
			}
		}

		public bool InputNodeHasPreview()
		{
			return GetOutputNode( 0 ).HasPreviewShader;
		}

		public void SetPreviewInputTexture()
		{
			if( string.IsNullOrEmpty( m_propertyName) )
				m_propertyName = "_" + Convert.ToChar( PortId + 65 );

			if ( m_cachedPropertyId == -1 )
				m_cachedPropertyId = Shader.PropertyToID( m_propertyName );

			UIUtils.GetNode( NodeId ).PreviewMaterial.SetTexture( m_cachedPropertyId, GetOutputConnection( 0 ).OutputPreviewTexture );
		}

		public void SetPreviewInputValue()
		{
			if ( m_inputPreviewTexture == null )
				m_inputPreviewTexture = new RenderTexture( 128, 128, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear );

			switch ( DataType )
			{
				case WirePortDataType.FLOAT:
				{
					if( m_inputPreviewShader != UIUtils.FloatShader )
					{
						m_inputPreviewShader = UIUtils.FloatShader;
						InputPreviewMaterial.shader = m_inputPreviewShader;
					}

					float f = FloatInternalData;
					InputPreviewMaterial.SetFloat( CachedFloatPropertyID, f );
				}
				break;
				case WirePortDataType.FLOAT2:
				{
					if ( m_inputPreviewShader != UIUtils.Vector2Shader )
					{
						m_inputPreviewShader = UIUtils.Vector2Shader;
						InputPreviewMaterial.shader = m_inputPreviewShader;
					}

					Vector2 v2 = Vector2InternalData;
					InputPreviewMaterial.SetVector( CachedVectorPropertyID, new Vector4( v2.x, v2.y, 0, 0 ) );
				}
				break;
				case WirePortDataType.FLOAT3:
				{
					if ( m_inputPreviewShader != UIUtils.Vector3Shader )
					{
						m_inputPreviewShader = UIUtils.Vector3Shader;
						InputPreviewMaterial.shader = m_inputPreviewShader;
					}

					Vector3 v3 = Vector3InternalData;
					InputPreviewMaterial.SetVector( CachedVectorPropertyID, new Vector4( v3.x, v3.y, v3.z, 0 ) );
				}
				break;
				case WirePortDataType.FLOAT4:
				{
					if ( m_inputPreviewShader != UIUtils.Vector4Shader )
					{
						m_inputPreviewShader = UIUtils.Vector4Shader;
						InputPreviewMaterial.shader = m_inputPreviewShader;
					}

					InputPreviewMaterial.SetVector( CachedVectorPropertyID, Vector4InternalData );
				}
				break;
				case WirePortDataType.COLOR:
				{
					if ( m_inputPreviewShader != UIUtils.ColorShader )
					{
						m_inputPreviewShader = UIUtils.ColorShader;
						InputPreviewMaterial.shader = m_inputPreviewShader;
					}

					InputPreviewMaterial.SetColor( CachedColorPropertyID, ColorInternalData );
				}
				break;
				case WirePortDataType.FLOAT3x3:
				case WirePortDataType.FLOAT4x4:
				{
					if ( m_inputPreviewShader != UIUtils.FloatShader )
					{
						m_inputPreviewShader = UIUtils.FloatShader;
						InputPreviewMaterial.shader = m_inputPreviewShader;
					}

					InputPreviewMaterial.SetFloat( CachedFloatPropertyID, 1 );
				}
				break;
				default:
				{
					if ( m_inputPreviewShader != UIUtils.FloatShader )
					{
						m_inputPreviewShader = UIUtils.FloatShader;
						InputPreviewMaterial.shader = m_inputPreviewShader;
					}

					InputPreviewMaterial.SetFloat( CachedFloatPropertyID, 0 );
				}
				break;
			}

			RenderTexture temp = RenderTexture.active;
			RenderTexture.active = m_inputPreviewTexture;
			Graphics.Blit( null, m_inputPreviewTexture, InputPreviewMaterial );
			RenderTexture.active = temp;

			if ( string.IsNullOrEmpty( m_propertyName ) )
				m_propertyName = "_" + Convert.ToChar( PortId + 65 );

			if ( m_cachedPropertyId == -1 )
				m_cachedPropertyId = Shader.PropertyToID( m_propertyName );

			UIUtils.GetNode( NodeId ).PreviewMaterial.SetTexture( m_propertyName, m_inputPreviewTexture );
		}

		public override void Destroy()
		{
			base.Destroy();
			//if ( m_inputPreview != null )
			//	UnityEngine.ScriptableObject.DestroyImmediate( m_inputPreview );
			//m_inputPreview = null;

			if ( m_inputPreviewTexture != null )
				UnityEngine.ScriptableObject.DestroyImmediate( m_inputPreviewTexture );
			m_inputPreviewTexture = null;

			if ( m_inputPreviewMaterial != null )
				UnityEngine.ScriptableObject.DestroyImmediate( m_inputPreviewMaterial );
			m_inputPreviewMaterial = null;

			m_inputPreviewShader = null;
		}

		public Shader InputPreviewShader
		{
			get
			{
				if ( m_inputPreviewShader == null )
					m_inputPreviewShader = AssetDatabase.LoadAssetAtPath<Shader>( AssetDatabase.GUIDToAssetPath( "d9ca47581ac157145bff6f72ac5dd73e" ) ); //ranged float

				if ( m_inputPreviewShader == null )
					m_inputPreviewShader = Shader.Find( "Unlit/Colored Transparent" );

					return m_inputPreviewShader;
			}
			set
			{
				m_inputPreviewShader = value;
			}
		}

		public Material InputPreviewMaterial
		{
			get
			{
				if ( m_inputPreviewMaterial == null )
					m_inputPreviewMaterial = new Material( InputPreviewShader );

				return m_inputPreviewMaterial;
			}
			//set
			//{
			//	m_inputPreviewMaterial = value;
			//}
		}

		public override string Name
		{
			get { return m_name; }
			set
			{
				m_name = value;
				m_internalDataPropertyLabel = ( string.IsNullOrEmpty( value ) || value.Equals( Constants.EmptyPortValue ) ) ? InputDefaultNameStr : value;
				m_dirtyLabelSize = true;
			}
		}

		public RenderTexture InputPreviewTexture
		{
			get {
				if ( IsConnected )
					return GetOutputConnection( 0 ).OutputPreviewTexture;
				else
					return m_inputPreviewTexture;
			}
		}
	}
}
