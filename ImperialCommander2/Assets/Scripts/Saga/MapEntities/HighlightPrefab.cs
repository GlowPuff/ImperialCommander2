using Saga;
using UnityEngine;

public class HighlightPrefab : MonoBehaviour, IEndTurnCleanup, IEntityPrefab
{
	//[HideInInspector]
	//public SpaceHighlight spaceHighlight;

	public IMapEntity mapEntity { get; set; }

	public void Init( SpaceHighlight s )
	{
		mapEntity = s;
		GetComponent<SpriteRenderer>().color = Utils.String2UnityColor( s.deploymentColor );
		transform.position = new Vector3( (s.entityPosition.X / 10), 0, (-s.entityPosition.Y / 10) );
		transform.localScale = new Vector3( s.Width * .78f, s.Height * .78f, 1 );

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
		if ( mapEntity.entityProperties.isActive && FindObjectOfType<SagaController>().tileManager.IsMapSectionActive( mapEntity.mapSectionOwner ) )
		{
			gameObject.SetActive( true );
		}
	}

	public void HideEntity()
	{
		gameObject.SetActive( false );
	}

	public void ModifyEntity( EntityProperties props )
	{
		mapEntity.entityProperties = props;

		SpaceHighlight s = mapEntity as SpaceHighlight;
		if ( !mapEntity.entityProperties.isActive )
		{
			gameObject.SetActive( false );
		}
		else
		{
			gameObject.SetActive( true );
		}
		GetComponent<SpriteRenderer>().color = Utils.String2UnityColor( mapEntity.entityProperties.entityColor );
	}
}
