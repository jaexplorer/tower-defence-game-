// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using System.Collections.Generic;
using System;

namespace AmplifyShaderEditor
{
	public sealed class ContextPalette : PaletteParent
	{
		private Vector3 m_position;
		private Vector2 m_startDropPosition;
		public ContextPalette( List<ContextMenuItem> items ) : base( items, 0, 0, 250, 250, string.Empty, MenuAnchor.NONE, MenuAutoSize.NONE )
		{
			m_isActive = false;
			OnPaletteNodeCreateEvt += OnOptionSelected;
			m_searchFilterControl += "CONTEXTPALETTE";
			SortElements();
		}

		public override void OnEnterPressed()
		{
			if ( m_searchFilter.Length > 0 && m_currentItems.Count > 0 )
			{
				FireNodeCreateEvent( m_currentItems[ 0 ].NodeType, m_currentItems[ 0 ].Name );
			}
			else
			{
				Disable();
			}
		}

		public override void OnEscapePressed()
		{
			Disable();
			if ( UIUtils.ValidReferences() )
			{
				UIUtils.InvalidateReferences();
			}
		}

		public override void Draw( Rect parentPosition, Vector2 mousePosition, int mouseButtonId, bool hasKeyboadFocus )
		{
			//if ( !_isActive )
			//	return;

			if ( Event.current.type == EventType.mouseDown && !IsInside( Event.current.mousePosition ) )
			{
				Disable();
				return;
			}
			base.Draw( parentPosition, mousePosition, mouseButtonId, hasKeyboadFocus );
		}


		public void Show( Vector2 position, Rect cameraInfo )
		{
			m_startDropPosition = position;
			m_maximizedArea.x = ( position.x + m_maximizedArea.width ) > cameraInfo.width ? ( cameraInfo.width - 1.1f * m_maximizedArea.width ) : position.x;
			m_maximizedArea.y = ( position.y + m_maximizedArea.height ) > cameraInfo.height ? ( cameraInfo.height - 1.1f * m_maximizedArea.height ) : position.y;
			m_position = new Vector3( m_maximizedArea.x, m_maximizedArea.y, 0f );
			m_isActive = true;
			m_focusOnSearch = true;
		}

		public override bool CheckButton( GUIContent content, GUIStyle style, int buttonId )
		{
			if ( buttonId != m_validButtonId )
				return false;

			return GUILayout.Button( content, style );
		}

		void OnOptionSelected( Type type, string name )
		{
			Disable();
		}

		public void Disable()
		{
			m_isActive = false;
		}

		public bool IsActive
		{
			get { return m_isActive; }
		}

		public Vector2 StartDropPosition
		{
			get { return m_startDropPosition; }
		}

		public Vector3 CurrentPosition
		{
			get { return m_position; }
		}

		public Vector2 CurrentPosition2D
		{
			get { return new Vector2( m_position.x, m_position.y ); }
		}
	}
}
