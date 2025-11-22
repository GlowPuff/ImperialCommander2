using DG.Tweening;
using Saga;
using UnityEngine;

public class DoorPrefab : MonoBehaviour, IEndTurnCleanup, IEntityPrefab
{
	public GameObject doorModel, doorOpenModel;
	public MeshRenderer meshRenderer;

	public IMapEntity mapEntity { get; set; }
	public bool isAnimationBusy { get; set; }

	public void Init( Door d, bool restoring )
	{
		isAnimationBusy = false;
		meshRenderer.material.color = Color.green;
		//new Color( 74f / 255f, 125f / 255f, 63f / 255f );
		//rgb(52, 87, 44)
		//rgb(74, 125, 63)

		float xmod = 1;
		float ymod = 1;
		if ( d.entityRotation == 90 )
			xmod = -1;
		if ( d.entityRotation == 180 )
		{
			xmod = -1;
			ymod = -1;
		}
		if ( d.entityRotation == 270 )
		{
			xmod = 1;
			ymod = -1;
		}
		mapEntity = d;

		if ( restoring )
			transform.position = new Vector3( d.entityPosition.X, d.entityPosition.Y, d.entityPosition.Z );
		else
		{
			transform.position = new Vector3( (d.entityPosition.X / 10) + xmod, 0, (-d.entityPosition.Y / 10) - ymod );
			//initially, isActive is used to set the door open status
			(mapEntity as Door).doorOpen = d.entityProperties.isActive;
		}
		//then just set isActive to true so it can show itself when asked
		mapEntity.entityProperties.isActive = true;

		transform.localScale = Vector3.zero;
		doorModel.transform.rotation = Quaternion.Euler( 0, d.entityRotation, 0 );
		doorOpenModel.transform.rotation = Quaternion.Euler( 0, d.entityRotation, 0 );

		mapEntity.entityPosition = transform.position.ToSagaVector();
		gameObject.SetActive( false );

		if ( restoring && (mapEntity as Door).doorOpen )
			OpenDoor();
		else if ( restoring && !(mapEntity as Door).doorOpen )
			CloseDoor();
	}

	public void OpenDoor()
	{
		//Debug.Log( $"OPENING DOOR::{mapEntity.name}" );
		doorModel.SetActive( false );
		doorOpenModel.SetActive( true );
	}

	public void CloseDoor()
	{
		//Debug.Log( $"CLOSING DOOR::{mapEntity.name}" );
		doorModel.SetActive( true );
		doorOpenModel.SetActive( false );
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
		if ( FindObjectOfType<SagaController>().tileManager.IsMapSectionActive( mapEntity.mapSectionOwner ) )
		{
			isAnimationBusy = true;
			gameObject.SetActive( true );
			transform.DOScale( Vector3.one, 1f ).SetEase( Ease.OutBounce ).OnComplete( () => isAnimationBusy = false );
			if ( (mapEntity as Door).doorOpen )
				OpenDoor();
			else
				CloseDoor();
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
			(mapEntity as Door).doorOpen = false;
			ShowEntity();
		}
		else
		{
			(mapEntity as Door).doorOpen = true;
			ShowEntity();
		}
	}
}
