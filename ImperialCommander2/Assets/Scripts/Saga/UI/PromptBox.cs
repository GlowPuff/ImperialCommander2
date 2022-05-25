using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Saga
{
	public class PromptBox : MonoBehaviour
	{
		public TextMeshProUGUI theText;
		public CanvasGroup cg;
		public PopupBase popupBase;
		public List<Button> buttonList;
		public TextMeshProUGUI cancelText;

		QuestionPrompt questionPrompt;
		Action callback;

		/// <summary>
		/// Parses text for glyphs
		/// </summary>
		public void Show( IEventAction eventAction, Action action = null )
		{
			EventSystem.current.SetSelectedGameObject( null );

			questionPrompt = eventAction as QuestionPrompt;
			cancelText.text = DataStore.uiLanguage.uiSetup.cancel;
			callback = action;

			theText.text = Utils.ReplaceGlyphs( questionPrompt.theText );
			for ( int i = 0; i < buttonList.Count; i++ )
				buttonList[i].gameObject.SetActive( false );
			for ( int i = 0; i < questionPrompt.buttonList.Count; i++ )
			{
				buttonList[i].gameObject.SetActive( true );
				buttonList[i].transform.GetChild( 0 ).GetComponent<TextMeshProUGUI>().text = Utils.ReplaceGlyphs( questionPrompt.buttonList[i].buttonText );
			}

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

		public void OnButton( int index )
		{
			Debug.Log( $"CHOICE: {questionPrompt.buttonList[index].buttonText}, {questionPrompt.buttonList[index].triggerGUID}" );
			OnClose();
			FindObjectOfType<TriggerManager>().FireTrigger( questionPrompt.buttonList[index].triggerGUID );
			FindObjectOfType<SagaEventManager>().DoEvent( questionPrompt.buttonList[index].eventGUID );
		}
	}
}
