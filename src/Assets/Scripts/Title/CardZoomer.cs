using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// DEPRECATED
/// </summary>
public class CardZoomer : MonoBehaviour
{
	public Image image, fader;
	public GameObject button;
	public Canvas canvas;
	public CanvasGroup cg;

	Sound sound;

	private void Awake()
	{
		sound = FindObjectOfType<Sound>();
	}

	public void ZoomIn( Sprite sprite )
	{
		canvas.gameObject.SetActive( true );
		image.sprite = sprite;
		image.transform.DOScale( 0.25f, .5f ).SetEase( Ease.OutExpo ).OnComplete( () => button.SetActive( true ) );
		cg.DOFade( 1, .5f );

		fader.gameObject.SetActive( true );
		fader.DOFade( .75f, .5f );
	}

	public void ZoomOut()
	{
		button.SetActive( false );
		image.transform.DOScale( .187f, .5f ).SetEase( Ease.OutExpo );
		cg.DOFade( 0, .2f );

		fader.DOFade( 0, .5f ).OnComplete( () =>
		{
			fader.gameObject.SetActive( false );
			canvas.gameObject.SetActive( false );
		} );
	}

	public void OnClose()
	{
		sound.PlaySound( FX.Click );
		EventSystem.current.SetSelectedGameObject( null );
		ZoomOut();
	}

	private void Update()
	{
		if ( Input.GetKeyDown( KeyCode.Space ) )
			OnClose();
	}
}
