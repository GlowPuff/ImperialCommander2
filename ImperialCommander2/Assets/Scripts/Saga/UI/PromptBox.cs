using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Saga
{
	public class PromptBox : MonoBehaviour
	{
		public TextMeshProUGUI theText;
		public PopupBase popupBase;
		public List<Button> buttonList;
		public TextMeshProUGUI cancelText;
		public GameObject cancelButton;

		RectTransform rect;
		Vector2 ap;
		QuestionPrompt questionPrompt;
		Action callback;
		bool acceptInput = true;
		bool acceptInput2 = true;

		void Awake()
		{
			rect = GetComponent<RectTransform>();
			ap = rect.anchoredPosition;
		}

		/// <summary>
		/// Parses text for glyphs
		/// </summary>
		public void Show( IEventAction eventAction, Action action = null )
		{
			EventSystem.current.SetSelectedGameObject( null );

			questionPrompt = eventAction as QuestionPrompt;
			string c = DataStore.uiLanguage.uiSetup.cancel[0].ToString().ToUpper();
			cancelText.text = c + DataStore.uiLanguage.uiSetup.cancel.Substring( 1 );
			callback = action;

			if ( !questionPrompt.includeCancel )
				cancelButton.SetActive( false );

			for ( int i = 0; i < buttonList.Count; i++ )
				buttonList[i].gameObject.SetActive( false );
			for ( int i = 0; i < questionPrompt.buttonList.Count; i++ )
			{
				buttonList[i].gameObject.SetActive( true );
				buttonList[i].transform.GetChild( 0 ).GetComponent<TextMeshProUGUI>().text = Utils.ReplaceGlyphs( questionPrompt.buttonList[i].buttonText );
			}

			popupBase.Show();

			SetText( Utils.ReplaceGlyphs( questionPrompt.theText ) );

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
			if ( !acceptInput )
				return;
			acceptInput = false;

			callback?.Invoke();
			popupBase.Close( () =>
			{
				Destroy( transform.parent.gameObject );
			} );
		}

		public void OnButton( int index )
		{
			if ( !acceptInput || !acceptInput2 )
				return;
			acceptInput2 = false;

			Debug.Log( $"CHOICE: {questionPrompt.buttonList[index].buttonText}, {questionPrompt.buttonList[index].triggerGUID}" );
			FindObjectOfType<TriggerManager>().FireTrigger( questionPrompt.buttonList[index].triggerGUID );
			FindObjectOfType<SagaEventManager>().DoEvent( questionPrompt.buttonList[index].eventGUID );
			OnClose();
		}
	}
}
