using DG.Tweening;
using UnityEngine;

namespace Saga
{
	public class DPointPrefab : MonoBehaviour, IEndTurnCleanup, IEntityPrefab
	{
		public IMapEntity mapEntity { get; set; }
		public bool isAnimationBusy { get; set; }

		public void Init( DeploymentPoint dp, bool restoring )
		{
			isAnimationBusy = false;
			//DPs are only visible at the moment a group deploys on it
			mapEntity = dp;
			GetComponent<SpriteRenderer>().color = Utils.String2UnityColor( dp.deploymentColor );
			if ( restoring )
				transform.position = new Vector3( dp.entityPosition.X, dp.entityPosition.Y, dp.entityPosition.Z );
			else
				transform.position = new Vector3( (dp.entityPosition.X / 10) + .5f, 0, (-dp.entityPosition.Y / 10) - .5f );

			mapEntity.entityPosition = transform.position.ToSagaVector();
			gameObject.SetActive( false );
		}

		public void EndTurnCleanup()
		{
		}

		public void RemoveEntity()
		{
			Destroy( gameObject );
		}

		public void ShowEntity()
		{
			//DPs are ONLY shown when a group deploys, and a DP doesn't have to be ACTIVE to be visible

			//if ( mapEntity.entityProperties.isActive )
			if ( FindObjectOfType<SagaController>().tileManager.IsMapSectionActive( mapEntity.mapSectionOwner ) )
			{
				gameObject.SetActive( true );
				transform.localScale = new Vector3( .75f, .75f, .75f );
				transform.DOKill();
				transform.DOScale( .85f, .2f ).SetLoops( -1, LoopType.Yoyo );
			}
		}

		public void HideEntity()
		{
			transform.DOKill();
			transform.DOScale( Vector3.zero, 1f ).SetEase( Ease.InBounce ).OnComplete( () => gameObject.SetActive( false ) );
		}

		public void ModifyEntity( EntityProperties props )
		{
			mapEntity.entityProperties = props;

			if ( !mapEntity.entityProperties.isActive )
			{
				transform.DOScale( Vector3.zero, 1f ).SetEase( Ease.InBounce ).OnComplete( () => gameObject.SetActive( false ) );
			}
			else
			{
				//ShowEntity();
			}
			GetComponent<SpriteRenderer>().color = Utils.String2UnityColor( mapEntity.entityProperties.entityColor );
		}
	}
}
