// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEditor;
using UnityEngine;

namespace AmplifyShaderEditor
{
	[System.Serializable]
	public sealed class OutputPort : WirePort
	{
		[SerializeField]
		private bool m_connectedToMasterNode;

		[SerializeField]
		private bool m_isLocalValue;

		[SerializeField]
		private string m_localOutputValue;

		[SerializeField]
		private int m_isLocalWithPortType = 0;

		private RenderTexture m_outputPreview = null;
		private Material m_outputMaskMaterial = null;

		private int m_indexPreviewOffset = 0;

		public OutputPort( int nodeId, int portId, WirePortDataType dataType, string name ) : base( nodeId, portId, dataType, name ) { LabelSize = Vector2.zero; }

		public bool ConnectedToMasterNode
		{
			get { return m_connectedToMasterNode; }
			set { m_connectedToMasterNode = value; }
		}

		public override void FullDeleteConnections()
		{
			UIUtils.DeleteConnection( false, m_nodeId, m_portId, true, true );
		}

		public override void NotifyExternalRefencesOnChange()
		{
			for ( int i = 0; i < m_externalReferences.Count; i++ )
			{
				ParentNode node = UIUtils.GetNode( m_externalReferences[ i ].NodeId );
				if ( node )
				{
					//node.MarkForPreviewUpdate();
					node.CheckSpherePreview();
					InputPort port = node.GetInputPortByUniqueId( m_externalReferences[ i ].PortId );
					port.UpdateInfoOnExternalConn( m_nodeId, m_portId, m_dataType );
					node.OnConnectedOutputNodeChanges( m_externalReferences[ i ].PortId, m_nodeId, m_portId, m_name, m_dataType );
					//node.MarkForPreviewUpdate();
				}
			}
		}

		public string ConfigOutputLocalValue( PrecisionType precisionType, string value, string customName = null , MasterNodePortCategory category = MasterNodePortCategory.Fragment )
		{
			m_localOutputValue = string.IsNullOrEmpty( customName ) ? ( "temp_output_" + m_nodeId + "_" + PortId ) : customName;
			m_isLocalValue = true;
			m_isLocalWithPortType |= ( int ) category;
			return string.Format( Constants.LocalValueDecWithoutIdent, UIUtils.PrecisionWirePortToCgType( precisionType, DataType ), m_localOutputValue, value );
		}

		public void SetLocalValue( string value, MasterNodePortCategory category = MasterNodePortCategory.Fragment )
		{
			m_isLocalValue = true;
			m_localOutputValue = value;
			m_isLocalWithPortType |= ( int ) category;
		}

		public void ResetLocalValue()
		{
			m_isLocalValue = false;
			m_localOutputValue = string.Empty;
			m_isLocalWithPortType = 0;
		}

		public bool IsLocalOnCategory( MasterNodePortCategory category )
		{
			return ( m_isLocalWithPortType & ( int ) category ) != 0; ;
		}

		public override void ForceClearConnection()
		{
			UIUtils.DeleteConnection( false, m_nodeId, m_portId, false, true );
		}

		public bool IsLocalValue { get { return m_isLocalValue; } }
		public int LocalWithPortType { get { return m_isLocalWithPortType; } }

		public string LocalValue { get { return m_localOutputValue; } }

		public RenderTexture OutputPreviewTexture
		{
			get {
				if ( m_outputPreview == null )
					m_outputPreview = new RenderTexture( 128, 128, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear );

				return m_outputPreview;
			}
			set { m_outputPreview = value; }
		}

		public int IndexPreviewOffset
		{
			get { return m_indexPreviewOffset; }
			set { m_indexPreviewOffset = value; }
		}

		public override void Destroy()
		{
			base.Destroy();
			if ( m_outputPreview != null )
				UnityEngine.ScriptableObject.DestroyImmediate( m_outputPreview );
			m_outputPreview = null;

			if ( m_outputMaskMaterial != null )
				UnityEngine.ScriptableObject.DestroyImmediate( m_outputMaskMaterial );
			m_outputMaskMaterial = null;
		}

		public Material MaskingMaterial
		{
			get
			{
				if ( m_outputMaskMaterial == null )
				{
					m_outputMaskMaterial = new Material( AssetDatabase.LoadAssetAtPath<Shader>( AssetDatabase.GUIDToAssetPath( "9c34f18ebe2be3e48b201b748c73dec0" ) ) );
				}
				return m_outputMaskMaterial;
			}
			//set { m_outputMaskMaterial = value; }
		}
	}
}
