using System;
using DG.Tweening;
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
		public CanvasGroup cg;
		public PopupBase popupBase;

		Action callback;

		/// <summary>
		/// Also parses glyphs
		/// </summary>
		public void Show( string text, Action action = null )
		{
			EventSystem.current.SetSelectedGameObject( null );

			theText.text = Utils.ReplaceGlyphs( text );
			continueButton.text = DataStore.uiLanguage.uiMainApp.continueBtn;
			callback = action;

			cg.DOFade( 1, .2f );
			popupBase.Show();

			theText.transform.parent.localPosition = new Vector3( theText.transform.parent.localPosition.x, -3000, 0 );
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
