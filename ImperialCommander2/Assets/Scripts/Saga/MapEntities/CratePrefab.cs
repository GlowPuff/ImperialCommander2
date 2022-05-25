using DG.Tweening;
using UnityEngine;

namespace Saga
{
	public class CratePrefab : MonoBehaviour, IEndTurnCleanup, IEntityPrefab
	{
		public MeshRenderer meshRenderer;
		//[HideInInspector]
		//public Crate crate { get; set; }
		[HideInInspector]
		public IMapEntity mapEntity { get; set; }

		public void Init( Crate c )
		{
			mapEntity = c;
			meshRenderer.material.color = Utils.String2UnityColor( c.deploymentColor );
			//meshRenderer.sharedMaterial.color = Color.gray;//CHANGES ALL MATERIALS~!
			transform.position = new Vector3( (c.entityPosition.X / 10) + .5f, 0, (-c.entityPosition.Y / 10) - .5f );

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
				gameObject.SetActive( true );
				transform.localScale = Vector3.zero;
				transform.DOScale( Vector3.one, 1f ).SetEase( Ease.OutBounce );
			}
		}

		public void HideEntity()
		{
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
				ShowEntity();
			}
			meshRenderer.material.color = Utils.String2UnityColor( props.entityColor );
		}
	}
}