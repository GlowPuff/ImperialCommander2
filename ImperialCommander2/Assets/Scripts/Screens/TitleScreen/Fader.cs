using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
	public Image image;

	private void Start()
	{
		gameObject.SetActive( true );
		image.color = Color.black;
	}

	public void Fade( float time = .5f )
	{
		gameObject.SetActive( true );
		image.DOFade( 1, time );
	}

	public void UnFade( float time = .5f )
	{
		gameObject.SetActive( true );
		image.DOFade( 0, time );
	}

	public void FadeToBlack( float time )
	{
		image.DOFade( 1, time );
	}
}
