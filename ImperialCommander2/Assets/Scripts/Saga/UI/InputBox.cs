using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Saga
{

	public class InputBox : MonoBehaviour
	{
		public PopupBase popupBase;
		public TextMeshProUGUI theText, readoutText;
		public Text submitText;
		public CanvasGroup cg;

		RectTransform rect;
		Vector2 ap;
		InputPrompt inputPrompt;
		Action callback;
		int inputValue;

		private void Awake()
		{
			rect = GetComponent<RectTransform>();
			ap = rect.anchoredPosition;
		}

		public void Show( IEventAction eventAction, Action action = null )
		{
			EventSystem.current.SetSelectedGameObject( null );

			inputPrompt = eventAction as InputPrompt;
			submitText.text = DataStore.uiLanguage.uiSettings.ok;
			callback = action;

			inputValue = 0;
			readoutText.text = "0";

			cg.DOFade( 1, .2f );
			popupBase.Show();

			SetText( Utils.ReplaceGlyphs( inputPrompt.theText ) );

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

		public void OnIncrease()
		{
			inputValue = Mathf.Clamp( inputValue + 1, 0, 100 );
			readoutText.text = inputValue.ToString();
		}

		public void OnDecrease()
		{
			inputValue = Mathf.Clamp( inputValue - 1, 0, 100 );
			readoutText.text = inputValue.ToString();
		}

		public void OnSubmitValue()
		{
			Debug.Log( "OnSubmitValue()::" + inputValue );
			foreach ( var item in inputPrompt.inputList )
			{
				int max = item.toValue;
				//if toValue=-1, max is infinite
				if ( item.toValue == -1 )
					max = int.MaxValue;

				if ( inputValue >= item.fromValue && inputValue <= max )
				{
					Debug.Log( "OnSubmitValue()::MATCH::" + inputValue );
					FindObjectOfType<TriggerManager>().FireTrigger( item.triggerGUID );
					FindObjectOfType<SagaEventManager>().DoEvent( item.eventGUID );
					popupBase.Close( () =>
					{
						Destroy( transform.parent.gameObject );
					} );
					FindObjectOfType<SagaEventManager>().ShowTextBox( item.theText, callback );
					return;
				}
			}

			//at this point, no match found, do default stuff
			FindObjectOfType<TriggerManager>().FireTrigger( inputPrompt.failTriggerGUID );
			FindObjectOfType<SagaEventManager>().DoEvent( inputPrompt.failEventGUID );
			popupBase.Close( () =>
			{
				Destroy( transform.parent.gameObject );
			} );
			FindObjectOfType<SagaEventManager>().ShowTextBox( inputPrompt.failText, callback );
		}
	}
}
