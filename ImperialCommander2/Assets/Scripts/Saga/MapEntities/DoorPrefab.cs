using DG.Tweening;
using Saga;
using UnityEngine;

public class DoorPrefab : MonoBehaviour, IEndTurnCleanup, IEntityPrefab
{
	public GameObject doorModel, doorOpenModel;
	public MeshRenderer meshRenderer;

	public IMapEntity mapEntity { get; set; }

	public void Init( Door d )
	{
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
		//initially, isActive is used to set the door open status
		(mapEntity as Door).doorOpen = d.entityProperties.isActive;
		//then just set isActive to true so it can show itself when asked
		mapEntity.entityProperties.isActive = true;
		transform.position = new Vector3( (d.entityPosition.X / 10) + xmod, 0, (-d.entityPosition.Y / 10) - ymod );
		transform.localScale = Vector3.zero;
		doorModel.transform.rotation = Quaternion.Euler( -90, d.entityRotation, 0 );
		doorOpenModel.transform.rotation = Quaternion.Euler( -90, d.entityRotation, 0 );

		mapEntity.entityPosition = transform.position.ToSagaVector();
		gameObject.SetActive( false );
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
		//if ( mapEntity.entityProperties.isActive )
		//{
		//Debug.Log( "ShowEntity()::DOOR" );
		if ( FindObjectOfType<SagaController>().tileManager.IsMapSectionActive( mapEntity.mapSectionOwner ) )
		{
			gameObject.SetActive( true );
			transform.DOScale( Vector3.one, 1f ).SetEase( Ease.OutBounce );
			if ( (mapEntity as Door).doorOpen )
				OpenDoor();
			else
				CloseDoor();
		}
		//}
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
