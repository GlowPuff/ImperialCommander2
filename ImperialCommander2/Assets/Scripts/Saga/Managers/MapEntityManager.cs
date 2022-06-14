using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Saga
{
	public class MapEntityManager : MonoBehaviour
	{
		[HideInInspector]
		public bool HandleObjectSelection = true;

		public GameObject deploymentSpotPrefab, terminalPrefab, cratePrefab, doorPrefab, spacePrefab, tokenPrefab;
		public Camera cam;
		public SagaEventManager eventManager;

		private Vector3 dragOrigin, dragDistance;
		private bool mButtonDown = false;
		private List<IMapEntity> mapEntities = new List<IMapEntity>();
		private float inputTimer = 0;

		/// <summary>
		/// Builds ALL entities but does not SHOW or ACTIVATE them
		/// </summary>
		public void InstantiateEntities()
		{
			HandleEntityGroups();

			foreach ( IMapEntity e in DataStore.mission.mapEntities )
			{
				if ( e.entityType == EntityType.DeploymentPoint )
				{
					AddDP( e as DeploymentPoint, LifeSpan.Manual );
				}
				else if ( e.entityType == EntityType.Terminal )
				{
					AddTerminal( e as Terminal );
				}
				else if ( e.entityType == EntityType.Crate )
				{
					AddCrate( e as Crate );
				}
				else if ( e.entityType == EntityType.Door )
				{
					AddDoor( e as Door );
				}
				else if ( e.entityType == EntityType.Highlight )
				{
					AddHighlight( e as SpaceHighlight );
				}
				else if ( e.entityType == EntityType.Token )
				{
					AddToken( e as Token );
				}
			}
		}

		void HandleEntityGroups()
		{
			//for random entity groups, we need to swap their positions and section owner
			//go through each group
			foreach ( var eg in DataStore.mission.entityGroups )
			{
				List<Vector> positions = new List<Vector>();
				List<Guid> owners = new List<Guid>();
				List<IMapEntity> mapEntities = new List<IMapEntity>();
				//get the entities
				mapEntities = DataStore.mission.mapEntities.Where( x => eg.missionEntities.Contains( x.GUID ) ).ToList();
				if ( mapEntities.Count > 0 )
				{
					//get their positions and section owner
					positions = mapEntities.Select( x => x.entityPosition ).ToList();
					owners = mapEntities.Select( x => x.mapSectionOwner ).ToList();
					//randomly reassign positions and owner
					int[] idx = GlowEngine.GenerateRandomNumbers( positions.Count );
					for ( int i = 0; i < idx.Length; i++ )
					{
						mapEntities[i].entityPosition = positions[idx[i]];
						mapEntities[i].mapSectionOwner = owners[idx[i]];
					}
				}
			}
		}

		/// <summary>
		/// Toggles entity (except DPs, which aren't visible) VISIBILITY (NOT isActive) inside a map section, ONLY if the entity IsActive
		/// </summary>
		public void ToggleSectionEntitiesVisibility( Guid mapSectionGUID, bool visible )
		{
			//Debug.Log( $"ToggleSectionEntitiesVisibility()::VISIBLE={visible}" );
			foreach ( Transform child in transform )
			{
				var pf = child.GetComponent<IEntityPrefab>();
				if ( pf?.mapEntity.mapSectionOwner == mapSectionGUID
					&& FindObjectOfType<SagaController>().tileManager.IsMapSectionActive( mapSectionGUID )
					&& pf.mapEntity.entityType != EntityType.DeploymentPoint )
				{
					if ( visible )
						pf.ShowEntity();
					else
						pf.HideEntity();
				}
			}
		}

		public List<string> GetActiveEntities( Guid ownerGUID )
		{
			var doors = mapEntities.Where( x => x.mapSectionOwner == ownerGUID && x.entityProperties.isActive && x.entityType == EntityType.Door );
			var tokens = mapEntities.Where( x => x.mapSectionOwner == ownerGUID && x.entityProperties.isActive && x.entityType == EntityType.Token );
			var terminals = mapEntities.Where( x => x.mapSectionOwner == ownerGUID && x.entityProperties.isActive && x.entityType == EntityType.Terminal );
			var crate = mapEntities.Where( x => x.mapSectionOwner == ownerGUID && x.entityProperties.isActive && x.entityType == EntityType.Crate );

			var ret = new List<string>();
			if ( doors.Count() > 0 )
				ret.Add( $"<color=orange>{doors.Count()} {DataStore.uiLanguage.sagaMainApp.doorsUC}</color>" );
			if ( tokens.Count() > 0 )
				ret.Add( $"<color=orange>{tokens.Count()} {DataStore.uiLanguage.sagaMainApp.tokensUC}</color>" );
			if ( terminals.Count() > 0 )
				ret.Add( $"<color=orange>{terminals.Count()} {DataStore.uiLanguage.sagaMainApp.terminalsUC}</color>" );
			if ( crate.Count() > 0 )
				ret.Add( $"<color=orange>{crate.Count()} {DataStore.uiLanguage.sagaMainApp.cratesUC}</color>" );

			return ret;
		}

		public void ModifyPrefabs( ModifyMapEntity mm, Action callback )
		{
			StartCoroutine( ModifyEntityCoroutine( mm, callback ) );
		}

		IEnumerator ModifyEntityCoroutine( ModifyMapEntity mm, Action callback )
		{
			foreach ( var mod in mm.entitiesToModify )
			{
				foreach ( Transform child in transform )
				{
					var pf = child.GetComponent<IEntityPrefab>();
					if ( pf?.mapEntity.GUID == mod.sourceGUID )
					{
						//don't show a message for certain types
						if ( pf.mapEntity.entityType != EntityType.DeploymentPoint
							&& pf.mapEntity.entityType != EntityType.Highlight
							&& pf.mapEntity.entityType != EntityType.Tile
							&& pf.mapEntity.entityType != EntityType.Door )
						{
							//if setting a non-active to active
							if ( !pf.mapEntity.entityProperties.isActive && mod.entityProperties.isActive )
							{
								pf.ModifyEntity( mod.entityProperties );
								FindObjectOfType<SagaController>().cameraController.MoveToEntity( pf.mapEntity.GUID );
								var emsg = DataStore.uiLanguage.sagaMainApp.mmAddEntitiesUC + ":\n\n1 " + pf.mapEntity.entityType;
								bool done = false;
								FindObjectOfType<SagaController>().eventManager.ShowTextBox( emsg, () => done = true );
								while ( !done )
									yield return null;
							}
							else
								pf.ModifyEntity( mod.entityProperties );
						}
						else
							pf.ModifyEntity( mod.entityProperties );
					}
				}
			}

			callback?.Invoke();
		}

		private void AddDP( DeploymentPoint dpoint, LifeSpan life )
		{
			GameObject go = Instantiate( deploymentSpotPrefab, transform );

			var dp = go.GetComponent<DPointPrefab>();
			dp.Init( dpoint, life );
			mapEntities.Add( dpoint );
		}

		private void AddTerminal( Terminal t )
		{
			GameObject go = Instantiate( terminalPrefab, transform );

			var dp = go.GetComponent<TerminalPrefab>();
			dp.Init( t );
			mapEntities.Add( t );
		}

		private void AddCrate( Crate c )
		{
			GameObject go = Instantiate( cratePrefab, transform );

			var dp = go.GetComponent<CratePrefab>();
			dp.Init( c );
			mapEntities.Add( c );
		}

		private void AddDoor( Door d )
		{
			GameObject go = Instantiate( doorPrefab, transform );

			var dp = go.GetComponent<DoorPrefab>();
			dp.Init( d );
			mapEntities.Add( d );
		}

		public void AddHighlight( SpaceHighlight s )
		{
			GameObject go = Instantiate( spacePrefab, transform );

			var dp = go.GetComponent<HighlightPrefab>();
			dp.Init( s );
			mapEntities.Add( s );
		}

		public void AddToken( Token t )
		{
			GameObject go = Instantiate( tokenPrefab, transform );

			var dp = go.GetComponent<TokenPrefab>();
			dp.Init( t );
			mapEntities.Add( t );
		}

		public void EndTurnCleanup()
		{
			//iterate each entity type and do cleanup
			foreach ( Transform tf in transform )
			{
				tf.GetComponent<DPointPrefab>()?.EndTurnCleanup();
				tf.GetComponent<TerminalPrefab>()?.EndTurnCleanup();
				tf.GetComponent<CratePrefab>()?.EndTurnCleanup();
				tf.GetComponent<DoorPrefab>()?.EndTurnCleanup();
				tf.GetComponent<HighlightPrefab>()?.EndTurnCleanup();
				tf.GetComponent<TokenPrefab>()?.EndTurnCleanup();
			}
		}

		void ProcessQuestionPrompt( EntityProperties props )
		{
			QuestionPrompt prompt = new QuestionPrompt();
			prompt.theText = props.theText;
			prompt.includeCancel = true;
			foreach ( var item in props.buttonActions )
			{
				prompt.buttonList.Add( new ButtonAction() { buttonText = item.buttonText, triggerGUID = item.triggerGUID, eventGUID = item.eventGUID } );
			}
			eventManager.toggleVisButton.SetActive( true );
			//FindObjectOfType<SagaController>().ToggleNavAndEntitySelection( false );
			eventManager.ShowPromptBox( prompt, () =>
			{
				eventManager.toggleVisButton.SetActive( false );
				//FindObjectOfType<SagaController>().ToggleNavAndEntitySelection( true );
			} );
		}

		private void Update()
		{
			inputTimer = Mathf.Max( inputTimer - Time.deltaTime, 0 );

			if ( FindObjectOfType<SagaEventManager>().IsProcessingEvents || inputTimer != 0 )
				return;

			int pointerID = -1;//mouse
			if ( Input.touchCount > 0 && Input.GetTouch( 0 ).phase == TouchPhase.Began )
			{
				pointerID = Input.GetTouch( 0 ).fingerId;
			}

			//make sure not clicking UI
			if ( Input.GetMouseButtonDown( 0 )
				&& !eventManager.IsUIHidden
				&& !EventSystem.current.IsPointerOverGameObject( pointerID ) )
			{
				dragOrigin = GetMousePosition();
				mButtonDown = true;
				dragDistance = Vector3.zero;
			}
			//get distance between current and saved position while held down
			if ( Input.GetMouseButton( 0 ) && mButtonDown )
			{
				dragDistance = dragOrigin - GetMousePosition();
			}

			Vector3 mousePosition = Input.mousePosition;

			bool touchClick = false;
			if ( Input.touchCount == 1
				&& Input.GetTouch( 0 ).phase == TouchPhase.Began
				&& !eventManager.IsUIHidden
				&& !EventSystem.current.IsPointerOverGameObject( pointerID ) )
			{
				touchClick = true;
				mousePosition = Input.GetTouch( 0 ).position;
			}

			//mouse released, didn't drag
			if ( touchClick
				|| (Input.GetMouseButtonUp( 0 )
				&& mButtonDown
				&& dragDistance.magnitude == 0) )
			{
				LayerMask mask = LayerMask.GetMask( "MapEntities" );
				RaycastHit hit;
				Ray ray = cam.ScreenPointToRay( mousePosition );// Input.mousePosition );
				if ( Physics.Raycast( ray, out hit, 1000, mask ) )
				{
					Transform objectHit = hit.transform;
					//Debug.Log( objectHit.name );
					if ( objectHit.name == "crate" )
					{
						var e = objectHit.parent.GetComponent<CratePrefab>().mapEntity;//.crate;
						ProcessQuestionPrompt( e.entityProperties );
						inputTimer = 2;
					}
					else if ( objectHit.name == "terminal" )
					{
						var e = objectHit.parent.GetComponent<TerminalPrefab>().mapEntity;//terminal;
						ProcessQuestionPrompt( e.entityProperties );
						inputTimer = 2;
					}
					else if ( objectHit.name == "door" )
					{
						var e = objectHit.parent.GetComponent<DoorPrefab>().mapEntity;//door;
						ProcessQuestionPrompt( e.entityProperties );
						inputTimer = 2;
					}
					else if ( objectHit.name.Contains( "Highlight" ) )
					{
						var e = objectHit.GetComponent<HighlightPrefab>().mapEntity;//spaceHighlight;
						ProcessQuestionPrompt( e.entityProperties );
						inputTimer = 2;
					}
					else if ( objectHit.name == "token" )
					{
						var e = objectHit.parent.GetComponent<TokenPrefab>().mapEntity;//token;
						ProcessQuestionPrompt( e.entityProperties );
						inputTimer = 2;
					}
				}
			}

			//reset vars on mouse button up
			if ( Input.GetMouseButtonUp( 0 ) )
			{
				mButtonDown = false;
				dragDistance = Vector3.zero;
			}
		}

		Vector3 GetMousePosition()
		{
			Plane plane = new Plane( Vector3.up, 0 );
			float distance;
			Ray ray = cam.ScreenPointToRay( Input.mousePosition );
			if ( plane.Raycast( ray, out distance ) )
			{
				return ray.GetPoint( distance );
			}
			else
				return Vector3.zero;
		}

		public IMapEntity GetEntity( Guid guid )
		{
			return mapEntities.Where( x => x.GUID == guid ).FirstOr( null );
		}

		/// <summary>
		/// Returns the Active DP, or a random one if >1, or empty GUID if there is no active DP
		/// </summary>
		public Guid GetActiveDeploymentPoint()
		{
			List<IMapEntity> dps = new List<IMapEntity>();
			foreach ( var e in mapEntities )
			{
				//if it's a DP and it's active
				if ( e.entityType == EntityType.DeploymentPoint && e.entityProperties.isActive )
				{
					//now make sure its owner section is active
					if ( FindObjectOfType<TileManager>().IsMapSectionActive( e.mapSectionOwner ) )
						dps.Add( e );
				}
			}
			//if more than 1 active DP, choose one randomly
			if ( dps.Count > 0 )
				return dps[GlowEngine.GenerateRandomNumbers( dps.Count )[0]].GUID;
			else
				return Guid.Empty;
		}

		public void ToggleHighlightDeploymentPoint( Guid guid, bool visible )
		{
			if ( guid == Guid.Empty )
				return;

			foreach ( Transform child in transform )
			{
				var pf = child.GetComponent<IEntityPrefab>();
				if ( pf.mapEntity.GUID == guid )
				{
					if ( visible )
						pf.ShowEntity();
					else
						pf.HideEntity();
				}
			}
		}
	}
}
