// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>
using System;
using UnityEngine;
using UnityEditor;

namespace AmplifyShaderEditor
{
	[Serializable]
	class ColorMaskHelper
	{
		private GUIContent ColorMaskContent = new GUIContent( "Color Mask", "Sets color channel writing mask, turning all off makes the object completely invisible\nDefault: RGBA" );
		private readonly char[] m_colorMaskChar = { 'R', 'G', 'B', 'A' };

		private GUIStyle m_leftToggleColorMask;
		private GUIStyle m_middleToggleColorMask;
		private GUIStyle m_rightToggleColorMask;


		[SerializeField]
		private bool[] m_colorMask = { true, true, true, true };

		public void Draw()
		{
			if ( m_leftToggleColorMask == null || m_leftToggleColorMask.normal.background == null )
			{
				m_leftToggleColorMask = GUI.skin.GetStyle( "ButtonLeft" );
			}

			if ( m_middleToggleColorMask == null || m_middleToggleColorMask.normal.background == null )
			{
				m_middleToggleColorMask = GUI.skin.GetStyle( "ButtonMid" );
			}

			if ( m_rightToggleColorMask == null || m_rightToggleColorMask.normal.background == null )
			{
				m_rightToggleColorMask = GUI.skin.GetStyle( "ButtonRight" );
			}


			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField( ColorMaskContent, GUILayout.Width( 90 ) );

			m_colorMask[ 0 ] = GUILayout.Toggle( m_colorMask[ 0 ], "R", m_leftToggleColorMask );
			m_colorMask[ 1 ] = GUILayout.Toggle( m_colorMask[ 1 ], "G", m_middleToggleColorMask );
			m_colorMask[ 2 ] = GUILayout.Toggle( m_colorMask[ 2 ], "B", m_middleToggleColorMask );
			m_colorMask[ 3 ] = GUILayout.Toggle( m_colorMask[ 3 ], "A", m_rightToggleColorMask );

			EditorGUILayout.EndHorizontal();
		}


		public void BuildColorMask( ref string ShaderBody, bool customBlendAvailable )
		{
			int count = 0;
			string colorMask = string.Empty;
			for ( int i = 0; i < m_colorMask.Length; i++ )
			{
				if ( m_colorMask[ i ] )
				{
					count++;
					colorMask += m_colorMaskChar[ i ];
				}
			}

			if ( count != m_colorMask.Length && customBlendAvailable )
			{
				MasterNode.AddRenderState( ref ShaderBody, "ColorMask", ( ( count == 0 ) ? "0" : colorMask ) );
			}
		}

		public void ReadFromString( ref uint index, ref string[] nodeParams )
		{
			for ( int i = 0; i < m_colorMask.Length; i++ )
			{
				m_colorMask[ i ] = Convert.ToBoolean( nodeParams[ index++ ] );
			}
		}

		public void WriteToString( ref string nodeInfo )
		{
			for ( int i = 0; i < m_colorMask.Length; i++ )
			{
				IOUtils.AddFieldValueToString( ref nodeInfo, m_colorMask[ i ] );
			}
		}

		public void Destroy()
		{
			m_leftToggleColorMask = null;
			m_middleToggleColorMask = null;
			m_rightToggleColorMask = null;
		}
	}
}
