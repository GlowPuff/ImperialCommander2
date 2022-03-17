using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// DEPRECATED
/// </summary>
public class CardZoom : MonoBehaviour
{
	public Image fader, image;
	public CanvasGroup cg;
	public TextMeshProUGUI ignoreText;

	Action callback;

	public void Show( Sprite s, CardDescriptor cd, Action action = null )
	{
		gameObject.SetActive( true );
		cg.DOFade( 1, .5f );
		fader.color = new Color( 0, 0, 0, 0 );
		fader.DOFade( .95f, .5f );

		callback = action;
		image.sprite = s;
		image.transform.localScale = (.85f).ToVector3();
		image.transform.DOScale( 1.25f, .5f ).SetEase( Ease.OutExpo );

		if ( !string.IsNullOrEmpty( cd.ignored ) )
		{
			ignoreText.text = "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">F</font></color>" + cd.ignored;
		}
		else
			ignoreText.text = "";
	}

	public void OnOK()
	{
		cg.DOFade( 0, .2f );
		fader.DOFade( 0, .5f ).OnComplete( () =>
		{
			callback?.Invoke();
			gameObject.SetActive( false );
		} );
		image.transform.DOScale( .85f, .5f ).SetEase( Ease.OutExpo );
	}

	private void Update()
	{
		if ( Input.GetKeyDown( KeyCode.Space ) )
			OnOK();
	}
}
