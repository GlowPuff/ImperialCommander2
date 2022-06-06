using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Saga
{
	public class TextBox : MonoBehaviour
	{
		public Text continueButton;
		public TextMeshProUGUI theText;
		public PopupBase popupBase;

		Action callback;
		RectTransform rect;
		Vector2 ap;

		void Awake()
		{
			rect = GetComponent<RectTransform>();
			ap = rect.anchoredPosition;
		}

		/// <summary>
		/// Also parses glyphs
		/// </summary>
		public void Show( string text, Action action = null )
		{
			EventSystem.current.SetSelectedGameObject( null );

			SetText( Utils.ReplaceGlyphs( text ) );
			continueButton.text = DataStore.uiLanguage.uiMainApp.continueBtn;
			callback = action;

			popupBase.Show();

			theText.transform.parent.localPosition = new Vector3( theText.transform.parent.localPosition.x, -3000, 0 );
		}

		void SetText( string t )
		{
			theText.text = t;
			//get size of text for this string and set the text
			Vector2 size = theText.GetPreferredValues( t, 700, 174 );
			//Debug.Log( size.y );
			//adjust size of window
			var windowH = Mathf.Clamp( size.y + 125, 250, 600 );
			rect.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, windowH );
		}

		public void OnClose()
		{
			callback?.Invoke();
			popupBase.Close( () =>
			{
				Destroy( transform.parent.gameObject );
			} );
		}

		private void Update()
		{
			if ( Input.GetKeyDown( KeyCode.Space ) )
				OnClose();
		}
	}
}
