using TMPro;
using UnityEngine;
using DG.Tweening;

public class QuickMessage : MonoBehaviour
{
	public TextMeshProUGUI message;
	public CanvasGroup cg;

	Sequence sequence = null;

	public void Show( string m )
	{
		InitTween();
		if ( sequence != null && sequence.IsPlaying() )
			sequence.Restart();
		else
		{
			cg.alpha = 0;
			sequence.Play();
		}

		gameObject.SetActive( true );

		message.text = m;
	}

	void InitTween()
	{
		if ( sequence == null )
		{
			//print( "made seq" );
			sequence = DOTween.Sequence();
			Tween t1 = cg.DOFade( 1, .25f );
			Tween t2 = cg.DOFade( 0, .25f ).SetDelay( 3 ).OnComplete( () => { gameObject.SetActive( false ); } );
			sequence
				.Join( t1 )
				.Join( t2 );
			sequence.OnKill( () => { sequence = null; } );
		}
	}
}
