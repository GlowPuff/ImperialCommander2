using Saga;
using UnityEngine;

public class HighlightPrefab : MonoBehaviour, IEndTurnCleanup, IEntityPrefab
{
	//[HideInInspector]
	//public SpaceHighlight spaceHighlight;

	public IMapEntity mapEntity { get; set; }
	public bool isAnimationBusy { get; set; }

	public void Init( SpaceHighlight s, bool restoring )
	{
		isAnimationBusy = false;
		mapEntity = s;
		GetComponent<SpriteRenderer>().color = Utils.String2UnityColor( s.deploymentColor );
		if ( restoring )
			transform.position = new Vector3( s.entityPosition.X, s.entityPosition.Y, s.entityPosition.Z );
		else
			transform.position = new Vector3( (s.entityPosition.X / 10), 0, (-s.entityPosition.Y / 10) );
		transform.localScale = new Vector3( s.Width * .78f, s.Height * .78f, 1 );

		mapEntity.entityPosition = transform.position.ToSagaVector();
		gameObject.SetActive( false );
	}

	public void EndTurnCleanup()
	{
		if ( (mapEntity as SpaceHighlight).Duration == 0 || !mapEntity.entityProperties.isActive )
			return;

		if ( !DataStore.sagaSessionData.gameVars.highlightLifeTimes.ContainsKey( mapEntity.GUID ) )
			DataStore.sagaSessionData.gameVars.highlightLifeTimes.Add( mapEntity.GUID, 0 );

		//increment the highlight timer for this entity IF its owner map section is active
		if ( FindObjectOfType<SagaController>().tileManager.IsMapSectionActive( mapEntity.mapSectionOwner ) )
		{
			Debug.Log( $"Highlight [{mapEntity.name}] timer increased" );
			DataStore.sagaSessionData.gameVars.highlightLifeTimes[mapEntity.GUID]++;
		}

		if ( DataStore.sagaSessionData.gameVars.highlightLifeTimes[mapEntity.GUID] >= (mapEntity as SpaceHighlight).Duration )
		{
			Debug.Log( $"Highlight [{mapEntity.name}] timer EXPIRED and removed from map" );
			HideEntity();
		}
	}

	public void RemoveEntity()
	{
		Destroy( gameObject );
	}

	public void ShowEntity()
	{
		//if highlight is active and older than lifespan, just return
		if ( mapEntity.entityProperties.isActive
			&& DataStore.sagaSessionData.gameVars.highlightLifeTimes.ContainsKey( mapEntity.GUID )
			&& DataStore.sagaSessionData.gameVars.highlightLifeTimes[mapEntity.GUID] >= (mapEntity as SpaceHighlight).Duration )
			return;

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
