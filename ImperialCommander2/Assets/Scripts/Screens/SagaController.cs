using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Saga
{
	public class SagaController : MonoBehaviour
	{
		//UI OBJECTS
		public Image faderOverlay;
		public Transform infoBtnTX;
		public Text roundText, currentThreatText, medPacText, timerText;
		public Button activateImperialButton, endTurnButton, fameButton;
		public TextMeshProUGUI missionTitleText;
		public GameObject zoomBar, timerContainer;
		//POPUPS
		public SagaEventPopup eventPopup;
		public SettingsScreen settingsScreen;
		public SagaDeploymentPopup deploymentPopup;
		public ErrorPanel errorPanel;
		public FamePopup famePopup;
		public ObjectivePanel objectivePanel;
		public EnemyActivationPopup enemyActivationPopup;
		public ImperialPopup imperialPopup;
		public MedpacPopup medpacPopup;
		public HeroDashboard heroDashboard;
		public HelpPanel helpPanel;
		public ToastManager toastManager;
		//MANAGERS
		public CameraController cameraController;
		public DeploymentManager dgManager;
		public SagaEventManager eventManager;
		public TileManager tileManager;
		public MapEntityManager mapEntityManager;
		public TriggerManager triggerManager;
		//OTHER
		public SagaLanguageController languageController;
		public VolumeProfile volume;
		public bool isDebugMode = false;//can be toggled within Unity
		public string debugMissionFilename = "test";//quick start a Mission right from "Saga" screen
		public int debugThreatLevel = 3;
		public bool debugAdaptiveDifficulty = false;
		public Difficulty debugDifficulty = Difficulty.Medium;
		public bool debugUseAlly = false;

		[HideInInspector]
		public Sound sound;
		//isError is set locally when there is an exception so the app doesn't try to save a potentially broken state
		bool isError { get; set; } = false;//checked by OnQuitSaga() before trying to save state when quitting

		void LogCallback( string condition, string stackTrace, LogType type )
		{
			//only capture errors, exceptions, asserts
			if ( !isError//only show first error if many happen at once
				&& type != LogType.Warning
				&& type != LogType.Log )
			{
				isError = true;

				errorPanel.Show( $"An Error Occurred of Type <color=green>{type}</color>", $"<color=yellow>{condition}</color>\n\n{stackTrace.Replace( "(at", "\n\n(at" )}", () => isError = false );
				//reset isError only after the inital error has been seen
			}
		}

		private void OnDestroy()
		{
			Application.logMessageReceived -= LogCallback;
		}

		// Start is called before the first frame update
		void Start()
		{
			Debug.Log( "ENTERING SAGA GAME" );

			//Exception handling for any Unity thrown exception, such as from asset management
			Application.logMessageReceived += LogCallback;

			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

			Screen.sleepTimeout = SleepTimeout.NeverSleep;

			//DEBUG BOOTSTRAP A MISSION
#if DEBUG
			//bootstrapDEBUG() is a testing feature that allows you to enter a mission directly within Unity ( from the "Saga" screen) without having to go through the Title screen first
			//Inside Unity, toggle "Is Debug Mode" on the Saga Controller object
			//You can also use an official mission code as a parameter, or leave it empty and modify the method directly to load a custom mission file for testing

			//make sure we bootstrap a debug session ONLY within Unity
			if ( isDebugMode && Application.isEditor )
				bootstrapDEBUG();//optionally pass a string of the official Mission ID, such as CORE1

			//restoreDEBUG();//test restore session, comment this out for production build
#endif

			//apply settings
			sound = FindObjectOfType<Sound>();
			//play menu ambient and music
			sound.PlayMusicAndMenuAmbient();

			//the Mission translation and backup translation have already been set at this point (in Setup)
			//set translated UI
			try
			{
				languageController.SetTranslatedUI();
			}
			catch ( Exception e )
			{
				Utils.LogWarning( $"SetTranslatedUI()\n{e.Message}" );
				errorPanel.Show( "SetTranslatedUI()", e );
			}

			//apply settings
			if ( volume.TryGet<Vignette>( out var vig ) )
				vig.active = PlayerPrefs.GetInt( "vignette" ) == 1;
			OnZoomBarToggle( PlayerPrefs.GetInt( "zoombuttons" ) == 1 );

			if ( DataStore.sagaSessionData.setupOptions.isTutorial )
				StartTutorial();
			else
				StartSaga();

			cameraController.CameraViewToggle( PlayerPrefs.GetInt( "viewToggle" ) == 0 ? CameraView.Normal : CameraView.TopDown );
		}

		void StartTutorial()
		{
			Debug.Log( $"STARTING TUTORIAL {DataStore.sagaSessionData.setupOptions.tutorialIndex}" );

			missionTitleText.text = DataStore.mission.missionProperties.missionName;
			missionTitleText.DOFade( 1, 1 );
			missionTitleText.DOFade( 0, 2 ).SetDelay( 3 );

			DataStore.sagaSessionData.InitGameVars();
			ResetUI( () =>
			{
				//load the Mission parameters
				if ( !ParseMission() )
				{
					ShowError( "Failed to load the tutorial." );
					return;
				}

				StartNewGame();
			} );
		}

		void StartSaga()
		{
			//see if it's a new game or restoring state
			if ( DataStore.sagaSessionData.gameVars.isNewGame )
			{
				Debug.Log( "STARTING NEW GAME" );
				DataStore.sagaSessionData.InitGameVars();

				ResetUI( () =>
				{
					//load the Mission parameters
					if ( !ParseMission() )
					{
						isError = true;
						ShowError( "Failed to load the mission." );
						return;
					}

					var card = DataStore.GetMissionCard( DataStore.sagaSessionData.setupOptions.projectItem.missionID );
					if ( card != null )//official mission
						missionTitleText.text = card.name;
					else//custom mission
						missionTitleText.text = DataStore.sagaSessionData.setupOptions.projectItem.Title;
					missionTitleText.DOFade( 1, 1 );
					missionTitleText.DOFade( 0, 2 ).SetDelay( 3 );

					StartNewGame();
				} );
			}
			else
			{
				Debug.Log( "CONTINUING GAME" );
				ResetUI();
				if ( !ContinueGame() )
				{
					isError = true;
					ShowError( DataStore.uiLanguage.uiMainApp.restoreErrorMsgUC );
					return;
				}
			}
		}

		void restoreDEBUG()
		{
			Debug.Log( "***BOOTSTRAP RESTORE DEBUG***" );
			DataStore.InitData();

			DataStore.StartNewSagaSession( new SagaSetupOptions() );
			DataStore.sagaSessionData.gameVars.isNewGame = false;
		}

		/// <summary>
		/// Start a debug session
		/// </summary>
		/// <param name="missionCode">If empty, defaults to "test.json" in the Documents/ImperialCommander folder. If specified, it's a Mission ID, such as CORE1.</param>
		void bootstrapDEBUG( string missionCode = "" )
		{
			//DEBUG MODE REQUIRES A DUMMY "test.json" MISSION TO ALREADY BE IN THE "Documents/ImperialCommander" FOLDER
			//This dummy Mission is loaded to assist with trying out new features

			//in a non-debug game, the following is already set at the Saga setup screen
			Debug.Log( "***BOOTSTRAP DEBUG***" );
			FileManager.SetupDefaultFolders();
			RunningCampaign.Reset();
			DataStore.InitData();

			if ( string.IsNullOrEmpty( missionCode ) )
			{
				Debug.Log( "BOOSTRAP CUSTOM MISSION" );

				if ( string.IsNullOrEmpty( debugMissionFilename ) )
				{
					Utils.LogError( "bootstrapDEBUG()::debugMissionFilename is empty" );
					return;
				}

				if ( !debugMissionFilename.ToLower().Contains( ".json" ) )
					debugMissionFilename += ".json";

				DataStore.StartNewSagaSession( new SagaSetupOptions()
				{
					projectItem = new ProjectItem() { fullPathWithFilename = Path.Combine( FileManager.baseDocumentFolder, $"{debugMissionFilename}" ) },
					difficulty = debugDifficulty,
					threatLevel = debugThreatLevel,
					useAdaptiveDifficulty = debugAdaptiveDifficulty,
				} );

				//try to load the mission
				DataStore.mission = FileManager.LoadMission( DataStore.sagaSessionData.setupOptions.projectItem.fullPathWithFilename, out string missionStringified );
				DataStore.sagaSessionData.missionStringified = missionStringified;
			}
			else
			{
				Debug.Log( "BOOTSTRAP OFFICIAL MISSION: " + missionCode );
				DataStore.StartNewSagaSession( new SagaSetupOptions()
				{
					projectItem = new ProjectItem()
					{
						missionID = missionCode,
					},
					difficulty = debugDifficulty,
					threatLevel = debugThreatLevel,
					useAdaptiveDifficulty = debugAdaptiveDifficulty,
				} );

				//load the mission
				DataStore.mission = FileManager.LoadMissionFromResource( missionCode, out string missionStringified );
				DataStore.sagaSessionData.missionStringified = missionStringified;
				if ( DataStore.mission == null )
					errorPanel.Show( "StartMission()", $"Could not load mission:\n'{missionCode}'" );
			}

			//DataStore.sagaSessionData.gameVars.pauseDeployment = true;
			//DataStore.sagaSessionData.gameVars.pauseThreatIncrease = true;
			//hero
			DataStore.sagaSessionData.MissionHeroes.Add( DataStore.heroCards[0] );
			DataStore.sagaSessionData.MissionHeroes.Add( DataStore.heroCards[1] );
			//DataStore.sagaSessionData.EarnedVillains.Add( DataStore.villainCards.GetDeploymentCard( "DG072" ) );
			if ( debugUseAlly )
				DataStore.sagaSessionData.selectedAlly = DataStore.allyCards[0];
		}

		public void ShowError( string m )
		{
			Debug.Log( "ShowError()::" + m );
			isError = true;
			errorPanel.Show( m );
		}

		void ResetUI( Action callback = null )
		{
			faderOverlay.gameObject.SetActive( true );
			faderOverlay.DOFade( 0, 1 ).OnComplete( () =>
			{
				faderOverlay.gameObject.SetActive( false );
				callback?.Invoke();
			} );
		}

		public void EndMission()
		{
			sound.PlaySound( 13 );

			//remove the state
			DataStore.sagaSessionData.RemoveState();

			faderOverlay.gameObject.SetActive( true );
			faderOverlay.color = new Color( 0, 0, 0, 0 );
			faderOverlay.DOFade( 1, 2 ).OnComplete( () =>
			{
				//if NOT playing from a campaign, quit to title screen
				if ( RunningCampaign.sagaCampaignGUID == Guid.Empty )
					SceneManager.LoadScene( "Title" );
				else
					//otherwise, quit back to campaign screen
					SceneManager.LoadScene( "Campaign" );
			} );
		}

		bool ParseMission()
		{
			try
			{
				if ( DataStore.mission == null )
				{
					Debug.Log( "ParseMission()::DataStore.mission is null" );
					return false;
				}

				//inject translation, if one exists
				SetMissionTranslation();

				//add embedded custom characters and associated data used within the mission to DataStore
				DataStore.AddEmbeddedImportsToPools();
				DataStore.AddGlobalImportsToPools();

				//add threat if it's a side mission
				if ( DataStore.mission.missionProperties.missionType == MissionType.Side )
				{
					Debug.Log( "ParseMission()::SIDE MISSION DETECTED" );
					DataStore.sagaSessionData.ModifyThreat( 2 * DataStore.sagaSessionData.setupOptions.threatLevel, true );
				}
				//ally
				if ( DataStore.mission.missionProperties.useFixedAlly == YesNoAll.Yes )
				{
					DataStore.sagaSessionData.fixedAlly = DataStore.allyCards.GetDeploymentCard( DataStore.mission.missionProperties.fixedAlly );
				}
				//add threat if player added an ally
				if ( DataStore.sagaSessionData.selectedAlly != null )
				{
					DataStore.sagaSessionData.ModifyThreat( DataStore.sagaSessionData.selectedAlly.cost );
				}
				//initial groups
				foreach ( var g in DataStore.mission.initialDeploymentGroups )
				{
					DeploymentCard card = DataStore.GetEnemy( g.cardID );
					//try setting the translated name if it's not custom
					if ( !g.useInitialGroupCustomName )
						g.cardName = card.name;
					//create override for enemyGroupData
					var ovrd = DataStore.sagaSessionData.gameVars.CreateDeploymentOverride( g.cardID );
					ovrd.SetEnemyDeploymentOverride( g );
					if ( card != null )
						DataStore.sagaSessionData.MissionStarting.Add( card );
				}
				//reserved groups
				foreach ( var g in DataStore.mission.reservedDeploymentGroups )
				{
					DeploymentCard card = DataStore.GetEnemy( g.cardID );
					DataStore.sagaSessionData.MissionReserved.Add( card );
				}
				//banned groups
				Debug.Log( $"FOUND {DataStore.mission.missionProperties.bannedGroups.Count} IGNORED GROUPS" );
				foreach ( var g in DataStore.mission.missionProperties.bannedGroups )
				{
					DataStore.sagaSessionData.MissionIgnored.Add( DataStore.GetEnemy( g ) );
				}
				//current mission info
				DataStore.sagaSessionData.gameVars.currentMissionInfo = DataStore.mission.missionProperties.missionInfo.Trim();
				//set objective
				if ( !string.IsNullOrEmpty( DataStore.mission.missionProperties.startingObjective ) )
				{
					OnChangeObjective( DataStore.mission.missionProperties.startingObjective );
				}
				//change reposition instructions
				if ( DataStore.mission.missionProperties.changeRepositionOverride != null )
				{
					var cr = DataStore.mission.missionProperties.changeRepositionOverride;
					if ( cr.useSpecific )
					{
						foreach ( var item in cr.repoGroups )
						{
							var ovrd = DataStore.sagaSessionData.gameVars.CreateDeploymentOverride( item.id );
							ovrd.repositionInstructions = cr.theText;
						}
					}
					else
					{
						var ovrd = DataStore.sagaSessionData.gameVars.CreateDeploymentOverride();
						ovrd.repositionInstructions = cr.theText;
					}
				}
				//fame button
				fameButton.interactable = DataStore.sagaSessionData.setupOptions.useAdaptiveDifficulty;

				return true;
			}
			catch ( Exception )
			{
				return false;
			}
		}

		/// <summary>
		/// build card decks, Hand, activate optional deployment, activate first map section
		/// </summary>
		void StartNewGame()
		{
			//set round number
			UpdateRoundNumberUI();

			//create deployment hand and manual deploy list
			DataStore.CreateDeploymentHand( DataStore.sagaSessionData.EarnedVillains, DataStore.sagaSessionData.setupOptions.threatLevel );
			DataStore.CreateManualDeployment();
			//deploy heroes
			for ( int i = 0; i < DataStore.sagaSessionData.MissionHeroes.Count; i++ )
				dgManager.DeployHeroAlly( DataStore.sagaSessionData.MissionHeroes[i] );
			if ( DataStore.sagaSessionData.MissionHeroes.Count == 3 )
			{
				Debug.Log( "Creating dummy hero" );
				dgManager.DeployHeroAlly( new DeploymentCard() { isDummy = true, id = "DUMMY", name = "Dummy Hero", mugShotPath = "CardThumbnails/bonus" } );
			}
			//deploy ally
			if ( DataStore.sagaSessionData.selectedAlly != null )
				dgManager.DeployHeroAlly( DataStore.sagaSessionData.selectedAlly );
			//deploy fixed ally
			if ( DataStore.sagaSessionData.fixedAlly != null )
				dgManager.DeployHeroAlly( DataStore.sagaSessionData.fixedAlly );

			//init event manager
			eventManager.Init( DataStore.mission );
			//init trigger manager
			triggerManager.Init( DataStore.mission );
			//initialize tile manager (loads all tiles in mission)
			tileManager.InstantiateTiles( DataStore.mission.mapSections );
			//initialize map entities, handle random entity groups
			mapEntityManager.ConfigureEntityGroups();
			mapEntityManager.InstantiateEntities( DataStore.mission.mapEntities, false );
			//after tiles load, activate first section and process starting Event
			StartCoroutine( "WaitForTilesLoaded" );
		}

		IEnumerator WaitForTilesLoaded()
		{
			while ( !tileManager.tilesLoaded )
				yield return null;
			tileManager.CamToSection( 0, true, () =>
			 {
				 var tiles = tileManager.ActivateAllVisibleSections();
				 DoStartupTasks( tiles );
			 } );
		}

		void DoStartupTasks( Tuple<List<string>, List<string>> tiles )
		{
			//if no tiles are initially shown, skip the placement window
			if ( tiles.Item1.Count > 0 )
			{
				//sort and group tiles by number, i.e. "Core 2A", "Core 11A", "Empire 2B"
				var orderedAndGrouped = tiles.Item1
					.OrderBy( str => str.Split( ' ' )[0] )  // Order alphabetically
					.ThenBy( str => int.Parse( str.Split( ' ' )[1].TrimEnd( 'A', 'B' ) ) ) // Then order by entire numerical values
					.ThenBy( str => str.EndsWith( "A" ) ? 0 : 1 ) // Finally, order by A/B values
					.GroupBy( str => str ) // Group the strings
					.Select( group => new
					{
						Tile = group.Key,
						Count = group.Count()
					} );
				var tilesWithCount = new List<string>();

				foreach ( var item in orderedAndGrouped )
				{
					if ( item.Count > 1 )
					{
						tilesWithCount.Add( $"{item.Tile} x {item.Count}" );
					}
					else
					{
						tilesWithCount.Add( item.Tile );
					}
				}

				var tmsg = string.Join( ", ", tilesWithCount );
				var emsg = DataStore.uiLanguage.sagaMainApp.mmAddEntitiesUC + ":\n\n";
				var emsg2 = string.Join( "\n", tiles.Item2 );
				emsg = string.IsNullOrEmpty( emsg2.Trim() ) ? "" : emsg + emsg2;

				eventManager.ShowTextBox( $"{DataStore.uiLanguage.sagaMainApp.mmAddTilesUC}:\n\n<color=orange>{tmsg}</color>\n\n{emsg}", () =>
				{
					StartupLayoutAndEvents();
				} );
			}
			else
			{
				StartupLayoutAndEvents();
			}
		}

		void StartupLayoutAndEvents()
		{
			//deploy starting groups
			dgManager.DeployStartingGroups( () =>
				 {
					 Action action = () =>
					 {
						 var ev = eventManager.EventFromGUID( DataStore.mission.missionProperties.startingEvent );
						 eventManager.DoEvent( ev );
						 //handle any start of turn events
						 DataStore.sagaSessionData.gameVars.isStartTurn = true;
						 eventManager.CheckIfEventsTriggered( () =>
						 {
							 DataStore.sagaSessionData.gameVars.isStartTurn = false;
						 } );
					 };

					 //perform optional deployment if it's a side mission or ally is present
					 if ( DataStore.mission.missionProperties.missionType == MissionType.Side
					 || DataStore.sagaSessionData.selectedAlly != null )
					 {
						 Debug.Log( "StartupLayoutAndEvents()::SIDE MISSION DETECTED OR ALLY PRESENT (Optional Deployment)" );
						 var dp = mapEntityManager.GetActiveDeploymentPoint( null );
						 if ( dp != Guid.Empty )
							 deploymentPopup.Show( DeployMode.Landing, false, true, action );
						 else
						 {
							 Debug.Log( "DELAYING OPTIONAL DEPLOYMENT" );
							 DataStore.sagaSessionData.gameVars.delayOptionalDeployment = true;
							 action();
						 }
					 }
					 else
						 action();
				 } );
		}

		bool ContinueGame()
		{
			StateManager sm = new StateManager();
			//Restore session, hand, manual deck, deployed enemies, allies/heroes, custom data, and re-translates the data
			if ( !DataStore.sagaSessionData.LoadState( sm ) )
				return false;

			Debug.Log( $"ContinueGame()::{DataStore.mission.fileName}" );

			sound.PlaySound( 12 );

			//inject translation, if one exists
			SetMissionTranslation();

			//skip the card and only add imported instructions and bonus effects (true)
			//the actual global/embedded card data is already loaded from the State Manager
			DataStore.AddEmbeddedImportsToPools( true );
			DataStore.AddGlobalImportsToPools( true );

			var card = DataStore.GetMissionCard( DataStore.sagaSessionData.setupOptions.projectItem.missionID );
			if ( card != null )//official mission
				missionTitleText.text = card.name;
			else//custom mission
				missionTitleText.text = DataStore.sagaSessionData.setupOptions.projectItem.Title;

			missionTitleText.DOFade( 1, 1 );
			missionTitleText.DOFade( 0, 2 ).SetDelay( 3 );

			//init event manager
			eventManager.Init( DataStore.mission );
			//init trigger manager
			triggerManager.RestoreState( sm.managerStates.triggerManagerState );
			//initialize tile manager (loads all tiles in mission)
			tileManager.InstantiateTiles( DataStore.mission.mapSections );
			//initialize map entities
			mapEntityManager.RestoreState( sm.managerStates.entityManagerState );

			//retore UI elements
			//round
			UpdateRoundNumberUI();

			//medpac count
			medPacText.text = DataStore.sagaSessionData.gameVars.medPacCount.ToString();
			//restore deployed enemies and heroes/allies
			dgManager.RestoreState();
			//fame button
			fameButton.interactable = DataStore.sagaSessionData.setupOptions.useAdaptiveDifficulty;
			//set objective
			if ( !string.IsNullOrEmpty( DataStore.sagaSessionData.gameVars.currentObjective ) )
				OnChangeObjective( DataStore.sagaSessionData.gameVars.currentObjective );

			StartCoroutine( "WaitForRestoredTiles", sm );

			return true;
		}

		IEnumerator WaitForRestoredTiles( StateManager state )
		{
			while ( !tileManager.tilesLoaded )
				yield return null;

			tileManager.RestoreState( state.managerStates.tileManagerState );
			tileManager.CamToSection( 0, true );
			tileManager.RestoreTiles();

			GlowEngine.FindUnityObject<QuickMessage>().Show( DataStore.uiLanguage.uiMainApp.restoredMsgUC );
		}

		/// <summary>
		/// shows an Event if RNG says to, calls OnStartTurn() when it's done
		/// An Event is the LAST thing that can happen on a turn
		/// </summary>
		void DoEvent( Action callback = null )
		{
			//EventSystem.current.SetSelectedGameObject( null );
			//1 in 4 chance to do an event
			//int[] rnd = GlowEngine.GenerateRandomNumbers( 4 );
			//int roll1 = rnd[0] + 1;
			//roll1 = 0;//Saga doesn't do end-of-round Events

			//if ( roll1 == 1 && DataStore.sagaSessionData.gameVars.eventsTriggered < 3 )
			//{
			//	DataStore.sagaSessionData.gameVars.eventsTriggered++;
			//	rnd = GlowEngine.GenerateRandomNumbers( DataStore.cardEvents.Count );
			//	//get a random event
			//	var ev = DataStore.cardEvents[rnd[0]];
			//	//remove it from the list of events so it won't activate again
			//	DataStore.cardEvents.Remove( ev );

			//	//activate it
			//	//eventManager.toggleVisButton.SetActive( true );
			//	eventPopup.Show( ev, () =>
			//	{
			//		//eventManager.toggleVisButton.SetActive( false );
			//		callback?.Invoke();
			//	} );
			//}
			//else
			//{
			//eventManager.toggleVisButton.SetActive( false );
			//all turn actions done and shouldn't be any mission events in progress, so start new turn
			callback?.Invoke();
			//}
		}

		void DoDeployment( bool skipThreatIncrease, Action callback = null )
		{
			EventSystem.current.SetSelectedGameObject( null );
			int[] rnd = GlowEngine.GenerateRandomNumbers( 6 );
			int roll1 = rnd[0] + 1;

			rnd = GlowEngine.GenerateRandomNumbers( 6 );
			int roll2 = rnd[0] + 1;

			Debug.Log( "ROLLED: " + (roll1 + roll2).ToString() );
			Debug.Log( "DEP MODIFIER: " + DataStore.sagaSessionData.gameVars.deploymentModifier );

			int total = roll1 + roll2 + DataStore.sagaSessionData.gameVars.deploymentModifier;
			Debug.Log( "TOTAL ROLLED VALUE: " + total );

			//eventManager.toggleVisButton.SetActive( true );
			if ( total <= 4 )
				deploymentPopup.Show( DeployMode.Calm, skipThreatIncrease, false, () =>
				{
					//eventManager.toggleVisButton.SetActive( false );
					callback?.Invoke();
				} );
			else if ( total > 4 && total <= 7 )
				deploymentPopup.Show( DeployMode.Reinforcements, skipThreatIncrease, false, () =>
				{
					DoEvent( callback );
				} );
			else if ( total > 7 && total <= 10 )
				deploymentPopup.Show( DeployMode.Landing, skipThreatIncrease, false, () =>
				{
					DoEvent( callback );
				} );
			else if ( total > 10 )
				deploymentPopup.Show( DeployMode.Onslaught, skipThreatIncrease, false, () =>
				{
					DoEvent( callback );
				} );
		}

		public void ActivateEnemy( DeploymentCard cd )
		{
			if ( cd == null )
				return;

			Debug.Log( $"ActivateEnemy()::{cd.name}::{cd.id}" );
			dgManager.ExhaustGroup( cd.id );
			enemyActivationPopup.Show( cd, DataStore.sagaSessionData.setupOptions.difficulty );
		}

		public void OnActivateImperial()
		{
			EventSystem.current.SetSelectedGameObject( null );
			sound.PlaySound( FX.Click );
			if ( eventManager.IsUIHidden )
				return;

			int[] rnd;
			DeploymentCard toActivate = null;
			//find a non-exhausted group and activate it, bias to priority 1
			var groups = dgManager.GetNonExhaustedGroups();
			if ( groups.Count > 0 )
			{
				var p1 = groups.Where( x => x.priority == 1 ).ToList();
				var others = groups.Where( x => x.priority != 1 ).ToList();
				var all = p1.Concat( others ).ToList();
				//70% chance to priority 1 groups
				if ( p1.Count > 0 && GlowEngine.RandomBool( 70 ) )
				{
					rnd = GlowEngine.GenerateRandomNumbers( p1.Count );
					toActivate = p1[rnd[0]];
				}
				else
				{
					rnd = GlowEngine.GenerateRandomNumbers( all.Count );
					toActivate = all[rnd[0]];
				}

				DataStore.sagaSessionData.gameVars.activatedGroup = toActivate;
				eventManager.CheckIfEventsTriggered( () =>
				{
					DataStore.sagaSessionData.gameVars.activatedGroup = null;
				} );

				//if an event was just fired, don't activate the group until the event is finished
				if ( eventManager.IsProcessingEvents )
				{
					eventManager.SetEndProcessingCallback( () => ActivateEnemy( toActivate ) );
				}
				else
					ActivateEnemy( toActivate );
			}
		}

		public void OnEndRound()
		{
			if ( eventManager.IsUIHidden )
				return;

			Debug.Log( "OnEndRound()::ENDING TURN***************************" );
			EventSystem.current.SetSelectedGameObject( null );
			sound.PlaySound( FX.Vader );

			if ( DataStore.sagaSessionData.gameVars.roundLimit == -1 )
				Debug.Log( "OnEndRound()::Round Limit: DISABLED by Mission" );

			DataStore.sagaSessionData.gameVars.isEndTurn = true;
			//check if any Events with trigger conditions get triggered, then queue and fire them
			eventManager.CheckIfEventsTriggered( () =>
			{
				DataStore.sagaSessionData.gameVars.isEndTurn = false;
			} );
			//queue up and fire any "end of CURRENT round" events that were set aside this round
			eventManager.QueueEndOfCurrentTurnEvents();

			//check for countdown timer events/triggers, then queue and fire them
			var expired = DataStore.sagaSessionData.gameVars.GetExpiredCountdowns();
			foreach ( var ex in expired )
			{
				eventManager.DoEvent( ex.eventGUID );
				triggerManager.FireTrigger( ex.triggerGUID );
			}

			//LAST - check if the round limit (with an Event) has been reached, then add it at the END of the queue to be fired
			if ( Utils.IsRoundLimitReachedWithSetting( 1 ) )
			{
				Debug.Log( $"OnEndRound()::Round Limit: {DataStore.sagaSessionData.gameVars.roundLimit}" );
				//only want the limit event firing ONCE, when round EQUALS limit, NOT >= otherwise it'll keep firing, even though the event manager prevents such things
				if ( DataStore.sagaSessionData.gameVars.round == DataStore.sagaSessionData.gameVars.roundLimit )
				{
					//round limit has been reached, add it to the END of the current Event queue
					if ( DataStore.sagaSessionData.gameVars.roundLimitEvent != Guid.Empty )
					{
						eventManager.DoEvent( DataStore.sagaSessionData.gameVars.roundLimitEvent, false );
					}
				}
			}

			Action endAction = () =>
			{
				//pause dep ON, pause threat OFF = activate with CALM
				//pause dep ON, pause threat ON = just ready all groups
				//pause dep OFF, pause threat ON = normal rold 2D6 but no threat gain

				if ( DataStore.sagaSessionData.gameVars.pauseDeployment
				&& !DataStore.sagaSessionData.gameVars.pauseThreatIncrease )
				{
					deploymentPopup.Show( DeployMode.Calm, false, false, () => { DoEvent( OnStartTurn ); } );
				}
				else if ( DataStore.sagaSessionData.gameVars.pauseDeployment
				&& DataStore.sagaSessionData.gameVars.pauseThreatIncrease )
				{
					DoEvent( OnStartTurn );
				}
				else if ( !DataStore.sagaSessionData.gameVars.pauseDeployment
				&& DataStore.sagaSessionData.gameVars.pauseThreatIncrease )
				{
					DoDeployment( true, OnStartTurn );
				}
				else//normal deployment
					DoDeployment( false, OnStartTurn );

				dgManager.ReadyAllGroups();
				mapEntityManager.EndTurnCleanup();
			};

			//check if a mission event is now in progress as a result of End of Turn
			if ( !eventManager.IsProcessingEvents )
				endAction();
			else
				eventManager.SetEndProcessingCallback( endAction );
		}

		/// <summary>
		/// OnEndRound() callback chain calls this last after all group activations, Events and Mission Events from previous turn are finished
		/// </summary>
		void OnStartTurn()
		{
			Debug.Log( "OnStartTurn()::STARTING NEW TURN============================" );

			//at this point, the previous round is COMPLETELY finished
			if ( !isError )
				DataStore.sagaSessionData.SaveState();

			//increase the round and update the UI
			IncreaseRound();

			//set current countdown timer, if there is one
			DataStore.sagaSessionData.gameVars.SetCurrentCountdownUI( timerContainer, timerText );

			DataStore.sagaSessionData.gameVars.isStartTurn = true;
			eventManager.CheckIfEventsTriggered( () =>
			{
				DataStore.sagaSessionData.gameVars.isStartTurn = false;
			} );
		}

		public void OnFame()
		{
			sound.PlaySound( FX.Click );
			EventSystem.current.SetSelectedGameObject( null );
			if ( !eventManager.IsUIHidden )
			{
				famePopup.Show( DataStore.sagaSessionData.gameVars.fame, DataStore.sagaSessionData.gameVars.round );
			}
		}

		public void OnInfo()
		{
			sound.PlaySound( FX.Click );
			EventSystem.current.SetSelectedGameObject( null );
			if ( !eventManager.IsUIHidden )
			{
				DOTween.Kill( infoBtnTX );
				infoBtnTX.DOScale( 1, .2f );

				heroDashboard.Show( DataStore.sagaSessionData.gameVars.currentMissionInfo );
			}
		}

		public void OnSettings()
		{
			sound.PlaySound( FX.Click );
			EventSystem.current.SetSelectedGameObject( null );
			if ( !eventManager.IsUIHidden )
			{
				GlowEngine.FindUnityObject<SettingsScreen>().Show( OnQuitSaga, tileManager.currentBiometype, () => UpdateRoundNumberUI() );
			}
		}

		public void OnMedPac()
		{
			sound.PlaySound( FX.Click );
			EventSystem.current.SetSelectedGameObject( null );
			if ( !eventManager.IsUIHidden )
			{
				medpacPopup.Show( () =>
				{
					UpdateMedPacCountUI();
				} );
			}
		}

		public void UpdateMedPacCountUI()
		{
			medPacText.text = DataStore.sagaSessionData.gameVars.medPacCount.ToString();
		}

		public void OnImperialPopup()
		{
			sound.PlaySound( FX.Click );
			EventSystem.current.SetSelectedGameObject( null );
			if ( !eventManager.IsUIHidden )
			{
				imperialPopup.Show();
			}
		}

		public void OnZoomBarToggle( bool isOn )
		{
			zoomBar.SetActive( isOn );
		}

		public void OnChangeObjective( string o, Action callback = null )
		{
			if ( string.IsNullOrEmpty( o ) )
				return;

			DataStore.sagaSessionData.gameVars.currentObjective = o;
			objectivePanel.Show( o, callback );
		}

		void OnQuitSaga( SettingsCommand c )
		{
			//save the state on exit
			//OnSettingsClose() can only be called when the game is in a state that can be saved
			if ( !isError )
				DataStore.sagaSessionData.SaveState();

			if ( c == SettingsCommand.ReturnTitles )
			{
				sound.FadeOutMusic();
				faderOverlay.gameObject.SetActive( true );
				faderOverlay.DOFade( 1, 1 ).OnComplete( () =>
				{
					//if NOT playing from a campaign, quit to title screen
					if ( RunningCampaign.sagaCampaignGUID == Guid.Empty )
						SceneManager.LoadScene( "Title" );
					else
						//otherwise, quit back to campaign screen
						SceneManager.LoadScene( "Campaign" );
				} );
			}
			else
				Application.Quit();
		}

		/// <summary>
		/// Toggles map navigation AND map entity selection
		/// </summary>
		public void ToggleNavAndEntitySelection( bool handle )
		{
			//mapEntityManager.HandleObjectSelection = handle;
			cameraController.ToggleNavigation( handle );
		}

		/// <summary>
		/// Does NOT make events check for being triggered, updates the round and countdown UI
		/// </summary>
		public void IncreaseRound()
		{
			DataStore.sagaSessionData.gameVars.round++;
			UpdateRoundNumberUI();

			Debug.Log( $"IncreaseRound()::Current round: {DataStore.sagaSessionData.gameVars.round}" );
		}

		public void OnPauseThreat( Toggle t )
		{
			if ( !t.gameObject.activeInHierarchy )
				return;
			EventSystem.current.SetSelectedGameObject( null );
			sound.PlaySound( FX.Click );
			DataStore.sagaSessionData.gameVars.pauseThreatIncrease = t.isOn;
			string s = t.isOn ? DataStore.uiLanguage.uiMainApp.pauseThreatMsgUC : DataStore.uiLanguage.uiMainApp.UnPauseThreatMsgUC;
			GlowEngine.FindUnityObject<QuickMessage>().Show( s );
		}

		public void OnPauseDeploy( Toggle t )
		{
			if ( !t.gameObject.activeInHierarchy )
				return;
			EventSystem.current.SetSelectedGameObject( null );
			sound.PlaySound( FX.Click );
			DataStore.sagaSessionData.gameVars.pauseDeployment = t.isOn;
			string s = t.isOn ? DataStore.uiLanguage.uiMainApp.pauseDepMsgUC : DataStore.uiLanguage.uiMainApp.unPauseDepMsgUC;
			GlowEngine.FindUnityObject<QuickMessage>().Show( s );
		}

		public void DEBUGsaveState()
		{
			DataStore.sagaSessionData.SaveState();
		}

		public void NotifyThreatUpdated()
		{
			//to avoid calling ToString() every single frame (lots of garbage), only notify when it changes
			currentThreatText.text = DataStore.sagaSessionData.gameVars.currentThreat.ToString();
		}

		public void OnHelpClick()
		{
			helpPanel.Show();
		}

		public void OnSetCountdownTimer()
		{
			DataStore.sagaSessionData.gameVars.SetCurrentCountdownUI( timerContainer, timerText );
		}

		public void UpdateRoundNumberUI()
		{
			int limitSetting = PlayerPrefs.GetInt( "roundLimitToggle" );
			string color = limitSetting == 1 ? "red" : "orange";

			roundText.text = $"{DataStore.uiLanguage.uiMainApp.roundHeading.ToUpper()}\n{DataStore.sagaSessionData.gameVars.round}";

			if ( limitSetting == 1 || limitSetting == 2 )
			{
				if ( DataStore.sagaSessionData.gameVars.roundLimit != -1
					&& DataStore.sagaSessionData.gameVars.round <= DataStore.sagaSessionData.gameVars.roundLimit )
					roundText.text = $"{DataStore.uiLanguage.uiMainApp.roundHeading}\n{DataStore.sagaSessionData.gameVars.round} / <color={color}>{DataStore.sagaSessionData.gameVars.roundLimit}</color>";
			}
		}

		public void SetMissionTranslation()
		{
			//sanity check
			if ( DataStore.sagaSessionData.setupOptions.projectItem == null )
			{
				Debug.Log( $"SetMissionTranslation()::Skipping, projectItem is null" );
				return;
			}

			Debug.Log( $"SetMissionTranslation()::Setting [{DataStore.sagaSessionData.setupOptions.projectItem.pickerMode}] translation..." );
			//SetMissionTranslation() automatically skips if anything is null

			TranslatedMission translation = null;

			//if it's an official mission
			if ( DataStore.sagaSessionData.setupOptions.projectItem.pickerMode == PickerMode.BuiltIn )
			{
				if ( DataStore.Language.ToUpper() == "EN" )
				{
					Debug.Log( $"SetMissionTranslation()::Skipping, Language=EN" );
					return;
				}

				translation = FileManager.GetOfficialMissionTranslation( DataStore.sagaSessionData.setupOptions.projectItem );
				TranslationController.Instance.SetMissionTranslation( translation, DataStore.mission );
			}
			//if it's a campaign game
			else if ( DataStore.sagaSessionData.setupOptions.projectItem.pickerMode == PickerMode.Embedded )
			{
				translation = RunningCampaign.campaignStructure.GetTranslatedMission( DataStore.Language );
				TranslationController.Instance.SetMissionTranslation( translation, DataStore.mission );
			}
			//custom mission
			else
			{
				translation = FileManager.GetCustomMissionTranslation( DataStore.sagaSessionData.setupOptions.projectItem.fullPathWithFilename );
				TranslationController.Instance.SetMissionTranslation( translation, DataStore.mission );
			}

			if ( translation == null )
				Debug.Log( $"SetMissionTranslation()::No translation found for Mission: '{DataStore.mission.missionProperties.missionName}' / [{DataStore.mission.missionProperties.missionID}]" );

			Debug.Log( "SetMissionTranslation()::Finished setting translation" );
		}

		private void Update()
		{
			bool allActivated = false;
			foreach ( Transform enemy in dgManager.enemyContainer )
			{
				SagaDGPrefab dg = enemy.GetComponent<SagaDGPrefab>();
				if ( !dg.IsExhausted )
					allActivated = true;
			}
			if ( allActivated )
			{
				activateImperialButton.interactable = true;
				float s = GlowEngine.SineAnimation( .95f, 1, 10f );
				activateImperialButton.transform.localScale = new Vector3( s, s );
			}
			else
			{
				activateImperialButton.interactable = false;
				activateImperialButton.transform.localScale = Vector3.one;
			}

			endTurnButton.interactable = !activateImperialButton.interactable;
		}
	}
}