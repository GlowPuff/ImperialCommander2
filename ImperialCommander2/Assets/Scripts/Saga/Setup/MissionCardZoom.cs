using DG.Tweening;
using UnityEngine;

namespace Saga
{
	public class MissionCardZoom : MonoBehaviour
	{
		public DynamicMissionCardPrefab dynamicMissionCard;
		public CanvasGroup cg;

		public void Show( MissionCard cd )
		{
			dynamicMissionCard.InitCard( cd );
			gameObject.SetActive( true );
			cg.DOFade( .95f, .5f );
			transform.GetChild( 1 ).localScale = new Vector3( .85f, .85f, .85f );
			transform.GetChild( 1 ).DOScale( 1, .5f ).SetEase( Ease.OutExpo );
		}

		public void Close()
		{
			FindObjectOfType<Sound>().PlaySound( FX.Click );
			cg.DOFade( 0, .5f ).OnComplete( () =>
			{
				gameObject.SetActive( false );
			} );
			transform.GetChild( 1 ).DOScale( .85f, .5f ).SetEase( Ease.OutExpo );
		}

		private void Update()
		{
			if ( Input.GetKeyDown( KeyCode.Space ) )
				Close();
		}
	}
}
