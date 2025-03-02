using UnityEngine;
using UnityEditor;

using System;
namespace AmplifyShaderEditor
{
    [Serializable]
    [NodeAttributes( "Parallax Mapping", "Generic", "Calculates offseted UVs for parallax mapping" )]
    public sealed class ParallaxMappingNode : ParentNode
    {
        private enum ParallaxType { Normal, Planar }

        [SerializeField]
        private int m_selectedParallaxTypeInt = 0;

        [SerializeField]
        private ParallaxType m_selectedParallaxType = ParallaxType.Normal;

        private readonly string[] m_parallaxTypeStr = { "Normal", "Planar" };

		private int m_cachedPropertyId = -1;

		protected override void CommonInit( int uniqueId )
        {
            base.CommonInit( uniqueId );
            AddInputPort( WirePortDataType.FLOAT2, false, "UV" );
            AddInputPort( WirePortDataType.FLOAT, false, "Height" );
            AddInputPort( WirePortDataType.FLOAT, false, "Scale" );
            AddInputPort( WirePortDataType.FLOAT3, false, "ViewDir (tan)" );
            AddOutputPort( WirePortDataType.FLOAT2, "Out" );
            m_useInternalPortData = true;
			m_autoWrapProperties = true;
			m_textLabelWidth = 100;
            UpdateTitle();
			m_forceDrawPreviewAsPlane = true;
			m_previewShaderGUID = "589f12f68e00ac74286815aa56053fcc";
        }

		public override void SetPreviewInputs()
		{
			base.SetPreviewInputs();

			if ( m_cachedPropertyId == -1 )
				m_cachedPropertyId = Shader.PropertyToID( "_ParallaxType" );

			PreviewMaterial.SetFloat( m_cachedPropertyId, ( m_selectedParallaxType == ParallaxType.Normal ? 0 : 1 ) );
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
        {
            base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalvar );

            string textcoords = m_inputPorts[ 0 ].GenerateShaderForOutput( ref dataCollector, WirePortDataType.FLOAT2, false, true );
            string height = m_inputPorts[ 1 ].GenerateShaderForOutput( ref dataCollector, WirePortDataType.FLOAT, false, true );
            string scale = m_inputPorts[ 2 ].GenerateShaderForOutput( ref dataCollector, WirePortDataType.FLOAT, false, true );
            string viewDirTan = m_inputPorts[ 3 ].GenerateShaderForOutput( ref dataCollector, WirePortDataType.FLOAT3, false, true );
            string localVarName = "Offset" + m_uniqueId;
            string calculation = "";

            switch ( m_selectedParallaxType )
            {
                default:
                case ParallaxType.Normal:
                    calculation = "( ( " + height + " - 1.0 ) * " + viewDirTan + ".xy * " + scale + " ) + " + textcoords;
                    break;
                case ParallaxType.Planar:
                    calculation = "( ( " + height + " - 1.0 ) * ( " + viewDirTan + ".xy / " + viewDirTan + ".z ) * " + scale + " ) + " + textcoords;
                    break;
            }

            dataCollector.AddToLocalVariables( m_uniqueId, m_currentPrecisionType, m_outputPorts[ 0 ].DataType, localVarName, calculation );
            return GetOutputVectorItem( 0, outputId, localVarName );
        }

        public override void DrawProperties()
        {
            base.DrawProperties();
            EditorGUILayout.BeginVertical();
            EditorGUI.BeginChangeCheck();
            m_selectedParallaxTypeInt = EditorGUILayout.Popup( "Parallax Type", m_selectedParallaxTypeInt, m_parallaxTypeStr );
            if ( EditorGUI.EndChangeCheck() )
            {
                switch ( m_selectedParallaxTypeInt )
                {
                    default:
                    case 0: m_selectedParallaxType = ParallaxType.Normal; break;
                    case 1: m_selectedParallaxType = ParallaxType.Planar; break;
                }
                UpdateTitle();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.HelpBox( "Normal type does a cheaper approximation thats view dependent while Planar is more accurate but generates higher aliasing artifacts at steep angles.", MessageType.None);
        }


        void UpdateTitle()
        {
            m_additionalContent.text = string.Format( "Type( {0} )", m_parallaxTypeStr[ m_selectedParallaxTypeInt ] );
        }

        public override void ReadFromString( ref string[] nodeParams )
        {
            base.ReadFromString( ref nodeParams );
            m_selectedParallaxType = ( ParallaxType )Enum.Parse( typeof( ParallaxType ), GetCurrentParam( ref nodeParams ) );
            switch ( m_selectedParallaxType )
            {
                default:
                case ParallaxType.Normal: m_selectedParallaxTypeInt = 0; break;
                case ParallaxType.Planar: m_selectedParallaxTypeInt = 1; break;
            }
            UpdateTitle();
        }

        public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
        {
            base.WriteToString( ref nodeInfo, ref connectionsInfo );
            IOUtils.AddFieldValueToString( ref nodeInfo, m_selectedParallaxType );
        }
    }
}
