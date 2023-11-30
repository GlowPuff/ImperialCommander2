using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// The "Forced" version will respond to clicks regardless of the "close window" Setting
/// </summary>
public class PopupBase : MonoBehaviour
{
	public CanvasGroup cg;
	public Image fader;
	public PopupOpacity popupOpacity = PopupOpacity.Light;

	public enum PopupOpacity { Dark, Light }

	[HideInInspector]
	public bool isActive = false;

	public void Show( Action callback = null )
	{
		ShowPopup( callback, true );
	}

	public void ShowNoZoom( Action callback = null )
	{
		ShowPopup( callback, false );
	}

	public void Close( Action callback = null )
	{
		ClosePopup( callback, true );
	}

	public void CloseNoZoom( Action callback = null )
	{
		ClosePopup( callback, false );
	}

	void ShowPopup( Action callback, bool doZoom )
	{
		isActive = true;
		gameObject.SetActive( true );
		fader.color = new Color( 0, 0, 0, 0 );
		float opacity = popupOpacity == PopupOpacity.Light ? .75f : .95f;
		fader.DOFade( opacity, 1 );//.95
		cg.DOFade( 1, .5f );

		if ( doZoom )
		{
			transform.GetChild( 1 ).localScale = new Vector3( .85f, .85f, .85f );
			transform.GetChild( 1 ).DOScale( 1, .5f ).SetEase( Ease.OutExpo ).OnComplete( () => callback?.Invoke() );
		}
		else
			callback?.Invoke();
	}

	void ClosePopup( Action callback, bool doZoom )
	{
		EventSystem.current.SetSelectedGameObject( null );
		isActive = false;
		FindObjectOfType<Sound>().PlaySound( FX.Click );
		fader.DOFade( 0, .5f ).OnComplete( () =>
		{
			gameObject.SetActive( false );
			callback?.Invoke();
		} );
		cg.DOFade( 0, .2f );

		if ( doZoom )
			transform.GetChild( 1 )?.DOScale( .85f, .5f ).SetEase( Ease.OutExpo );
	}
}
