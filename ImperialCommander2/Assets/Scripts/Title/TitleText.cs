using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TitleText : MonoBehaviour
{
	public RectTransform t1, t2, t3;
	public Text i1, i2, i3;

	CanvasGroup cg;

	private void Awake()
	{
		cg = GetComponent<CanvasGroup>();
	}

	public void Show()
	{
		Sequence seq = DOTween.Sequence();
		Tween t1T = t1.DOAnchorPos( new Vector2( 0, 0 ), 2 ).SetDelay( .5f );
		Tween t2T = t2.DOAnchorPos( new Vector2( 0, -108 ), 2 ).SetDelay( .25f );
		Tween t3T = t3.DOAnchorPos( new Vector2( 0, -223 ), 2 ).SetDelay( .25f );

		Tween t1A = i1.DOFade( 1, 2 );
		Tween t2A = i2.DOFade( 1, 2 ).SetDelay( .25f );
		Tween t3A = i3.DOFade( 1, 2 ).SetDelay( .25f );

		seq
			.Join( t1T )
			.Join( t1A )
			.Join( t2T )
			.Join( t2A )
			.Join( t3T )
			.Join( t3A );
		seq.Play();
	}

	public void FlipOut()
	{
		cg.DOFade( 0, .5f ).OnComplete( () => gameObject.SetActive( false ) );
		//transform.DOLocalRotate( new Vector3( 0, -90, 0 ), .5f ).OnComplete( () => gameObject.SetActive( false ) );
	}

	public void FlipIn()
	{
		gameObject.SetActive( true );
		cg.alpha = 0;
		cg.DOFade( 1, .5f );
		//transform.DORotate( new Vector3( 0, 0, 0 ), .5f );
	}
}
