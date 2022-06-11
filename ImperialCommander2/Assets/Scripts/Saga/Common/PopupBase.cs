using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

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
		isActive = true;
		gameObject.SetActive( true );
		fader.color = new Color( 0, 0, 0, 0 );
		float opacity = popupOpacity == PopupOpacity.Light ? .75f : .95f;
		fader.DOFade( opacity, 1 );//.95
		cg.DOFade( 1, .5f );
		transform.GetChild( 1 ).localScale = new Vector3( .85f, .85f, .85f );
		transform.GetChild( 1 ).DOScale( 1, .5f ).SetEase( Ease.OutExpo ).OnComplete( () => callback?.Invoke() );
	}

	public void Close( Action callback = null )
	{
		isActive = false;
		FindObjectOfType<Sound>().PlaySound( FX.Click );
		fader.DOFade( 0, .5f ).OnComplete( () =>
		{
			gameObject.SetActive( false );
			callback?.Invoke();
		} );
		cg.DOFade( 0, .2f );
		transform.GetChild( 1 ).DOScale( .85f, .5f ).SetEase( Ease.OutExpo );
	}
}
