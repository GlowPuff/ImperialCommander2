using DG.Tweening;
using UnityEngine;

namespace Saga
{
	public class TokenPrefab : MonoBehaviour, IEndTurnCleanup, IEntityPrefab
	{
		public MeshRenderer baseMesh, neutral, imperial, rebel;
		//[HideInInspector]
		//public Token token;

		public IMapEntity mapEntity { get; set; }
		public bool isAnimationBusy { get; set; }

		public void Init( Token t )
		{
			isAnimationBusy = false;
			mapEntity = t;
			baseMesh.material.color = Utils.String2UnityColor( t.tokenColor );
			transform.position = new Vector3( (t.entityPosition.X / 10) + .5f, 0, (-t.entityPosition.Y / 10) - .5f );
			switch ( t.markerType )
			{
				case MarkerType.Neutral:
					neutral.gameObject.SetActive( true );
					break;
				case MarkerType.Imperial:
					imperial.gameObject.SetActive( true );
					break;
				case MarkerType.Rebel:
					rebel.gameObject.SetActive( true );
					break;
			}

			mapEntity.entityPosition = transform.position.ToSagaVector();
			gameObject.SetActive( false );
		}

		public void RemoveEntity()
		{
			Destroy( gameObject );
		}

		public void EndTurnCleanup()
		{
		}

		public void ShowEntity()
		{
			if ( mapEntity.entityProperties.isActive && FindObjectOfType<SagaController>().tileManager.IsMapSectionActive( mapEntity.mapSectionOwner ) )
			{
				isAnimationBusy = true;
				gameObject.SetActive( true );
				transform.localScale = Vector3.zero;
				transform.DOScale( Vector3.one, 1f ).SetEase( Ease.OutBounce ).OnComplete( () => isAnimationBusy = false );
			}
		}

		public void HideEntity()
		{
			isAnimationBusy = true;
			transform.DOScale( Vector3.zero, 1f ).SetEase( Ease.InBounce ).OnComplete( () =>
			{
				isAnimationBusy = false;
				gameObject.SetActive( false );
			} );
		}

		public void ModifyEntity( EntityProperties props )
		{
			mapEntity.entityProperties = props;

			if ( !mapEntity.entityProperties.isActive )
			{
				isAnimationBusy = true;
				transform.DOScale( Vector3.zero, 1f ).SetEase( Ease.InBounce ).OnComplete( () =>
				{
					isAnimationBusy = false;
					gameObject.SetActive( false );
				} );
			}
			else
			{
				ShowEntity();
			}
			baseMesh.material.color = Utils.String2UnityColor( mapEntity.entityProperties.entityColor );
		}
	}
}