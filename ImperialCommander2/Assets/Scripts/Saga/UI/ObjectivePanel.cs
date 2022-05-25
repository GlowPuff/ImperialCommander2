using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Saga
{
	public class ObjectivePanel : MonoBehaviour
	{
		public TextMeshProUGUI message;
		public Transform panelTX, rootTX, chevron;

		bool open = true;
		string originalMessage;

		/// <summary>
		/// Do NOT send a ReplaceGlyphs() string
		/// </summary>
		public void Show( string m, Action callback = null )
		{
			originalMessage = m;
			NotifyValueUpdated();

			Hide( () =>
			{
				var rt = panelTX.GetComponent<RectTransform>();
				rt.offsetMax = new Vector2( 900, 56 );
				message.gameObject.SetActive( true );
				chevron.rotation = Quaternion.Euler( 0, 0, 180 );
				rootTX.DOLocalMoveY( -25, .25f );
				callback?.Invoke();
			} );
		}

		public void Hide( Action callback )
		{
			rootTX.DOLocalMoveY( 125, .25f ).OnComplete( () => callback?.Invoke() );
		}

		public void SlideOpen()
		{
			var rt = panelTX.GetComponent<RectTransform>();
			open = !open;
			if ( !open )
			{
				message.gameObject.SetActive( false );
				chevron.DORotate( new Vector3( 0, 0, 0 ), .25f );
				DOTween.To( () => rt.offsetMax, x => rt.offsetMax = x, new Vector2( 100, 56 ), .25f ).SetEase( Ease.InOutCubic );
			}
			else
			{
				chevron.DORotate( new Vector3( 0, 0, 180 ), .25f );
				DOTween.To( () => rt.offsetMax, x => rt.offsetMax = x, new Vector2( 900, 56 ), .25f ).SetEase( Ease.InOutCubic ).OnComplete( () => message.gameObject.SetActive( true ) );
			}
		}

		public void NotifyValueUpdated()
		{
			if ( open )
			{
				if ( !string.IsNullOrEmpty( originalMessage ) )
					message.text = Utils.ReplaceGlyphs( originalMessage );
			}
		}
	}
}
