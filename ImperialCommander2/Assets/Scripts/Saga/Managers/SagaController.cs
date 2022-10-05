using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Saga
{
	public class SagaController : MonoBehaviour
	{
		//UI OBJECTS
		public Image faderOverlay;
		public Transform infoBtnTX;
		public Text roundText;
		public Button activateImperialButton, endTurnButton, fameButton;
		//POPUPS
		public SagaEventPopup eventPopup;
		public SettingsScreen settingsScreen;
		public SagaDeploymentPopup deploymentPopup;
		public ErrorPanel errorPanel;
		public FamePopup famePopup;
		public ObjectivePanel objectivePanel;
		public EnemyActivationPopup enemyActivationPopup;
		public ImperialPopup imperialPopup;
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

		Sound sound;
		bool isError = false;

		// Start is called before the first frame update
		void Start()
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

			Screen.sleepTimeout = SleepTimeout.NeverSleep;

			//DEBUG BOOTSTRAP A MISSION
			//bootstrapDEBUG();//comment this out for production build
			//restoreDEBUG();//comment this out for production build

			//apply settings
			sound = FindObjectOfType<Sound>();
			sound.CheckAudio();

			//set translated UI
			languageController.SetTranslatedUI();

			//apply settings
			//if ( volume.TryGet<Bloom>( out var bloom ) )
			//	bloom.active = PlayerPrefs.GetInt( "bloom" ) == 1;
			//if ( volume.TryGet<Vignette>( out var vig ) )
			//	vig.active = PlayerPrefs.GetInt( "vignette" ) == 1;

			if ( DataStore.sagaSessionData.setupOptions.isTutorial )
				StartTutorial();
			else
				StartSaga();
		}

		void StartTutorial()
		{
			Debug.Log( $"STARTING TUTORIAL {DataStore.sagaSessionData.setupOptions.tutorialIndex}" );

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

		void bootstrapDEBUG()
		{
			//in a non-debug game, the following is already set at the Saga setup screen
			Debug.Log( "***BOOTSTRAP DEBUG***" );
			DataStore.InitData();

			DataStore.StartNewSagaSession( new SagaSetupOptions()
			{
				projectItem = new ProjectItem() { fullPathWithFilename = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), "ImperialCommander", "atest.json" ) },
				difficulty = Difficulty.Medium,
				threatLevel = 3,
				useAdaptiveDifficulty = true,
			} );
			//DataStore.sagaSessionData.gameVars.pauseDeployment = true;
			//DataStore.sagaSessionData.gameVars.pauseThreatIncrease = true;
			//hero
			DataStore.sagaSessionData.MissionHeroes.Add( DataStore.heroCards[0] );
			DataStore.sagaSessionData.MissionHeroes.Add( DataStore.heroCards[1] );
			//DataStore.sagaSessionData.selectedAlly = DataStore.allyCards[0];

			//try to load the mission
			DataStore.mission = FileManager.LoadMission( DataStore.sagaSessionData.setupOptions.projectItem.fullPathWithFilename, out string missionStringified );
			DataStore.sagaSessionData.missionStringified = missionStringified;
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
			//remove the state
			StateManager.RemoveState();

			faderOverlay.gameObject.SetActive( true );
			faderOverlay.color = new Color( 0, 0, 0, 0 );
			faderOverlay.DOFade( 1, 2 ).OnComplete( () =>
			{
				SceneManager.LoadScene( "Title" );
			} );
		}

		bool ParseMission()
		{
			try
			{
				if ( DataStore.mission == null )
					return false;

				//var missionCard = DataStore.GetMissionCard( DataStore.mission.missionProperties.missionID );
				//if this is an official mission, grab the mission type from the card list
				//if ( missionCard != null )
				//{
				//	if ( missionCard.missionType.Contains( global::MissionType.Side ) )
				//	{
				//		Debug.Log( "ParseMission()::SIDE MISSION DETECTED" );
				//		DataStore.sagaSessionData.ModifyThreat( 2 * DataStore.sagaSessionData.setupOptions.threatLevel, true );
				//	}
				//}//else if it's a custom ission, grab mission type from the Properties

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
		/// build card decks, activate optional deployment, activate first map section
		/// </summary>
		void StartNewGame()
		{
			roundText.text = DataStore.uiLanguage.uiMainApp.roundHeading + "\r\n1";
			//create deployment hand and manual deploy list
			DataStore.CreateDeploymentHand( DataStore.sagaSessionData.EarnedVillains, DataStore.sagaSessionData.setupOptions.threatLevel );
			DataStore.CreateManualDeployment();
			//deploy heroes
			for ( int i = 0; i < DataStore.sagaSessionData.MissionHeroes.Count; i++ )
				dgManager.DeployHeroAlly( DataStore.sagaSessionData.MissionHeroes[i] );
			if ( DataStore.sagaSessionData.MissionHeroes.Count == 3 )
			{
				Debug.Log( "Creating dummy hero" );
				dgManager.DeployHeroAlly( new DeploymentCard() { isDummy = true } );
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
				var tmsg = string.Join( ", ", tiles.Item1 );
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
			//layout starting groups
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

					 //perform optional deployment if it's a side mission
					 if ( DataStore.mission.missionProperties.missionType == MissionType.Side
					 || DataStore.sagaSessionData.selectedAlly != null )
					 {
						 Debug.Log( "StartupLayoutAndEvents()::SIDE MISSION DETECTED" );
						 deploymentPopup.Show( DeployMode.Landing, false, true, action );
					 }
					 else
						 action();
				 } );
		}

		bool ContinueGame()
		{
			StateManager sm = new StateManager();
			//Restore session, hand, manual deck, deployed enemies, allies/heroes
			if ( !sm.LoadSession() )
				return false;

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
			roundText.text = DataStore.uiLanguage.uiMainApp.roundHeading + "\r\n" + DataStore.sagaSessionData.gameVars.round;
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
			EventSystem.current.SetSelectedGameObject( null );
			//1 in 4 chance to do an event
			int[] rnd = GlowEngine.GenerateRandomNumbers( 4 );
			int roll1 = rnd[0] + 1;
			roll1 = 0;//Saga doesn't do end-of-round Events
			if ( roll1 == 1 && DataStore.sagaSessionData.gameVars.eventsTriggered < 3 )
			{
				DataStore.sagaSessionData.gameVars.eventsTriggered++;
				rnd = GlowEngine.GenerateRandomNumbers( DataStore.cardEvents.Count );
				//get a random event
				var ev = DataStore.cardEvents[rnd[0]];
				//remove it from the list of events so it won't activate again
				DataStore.cardEvents.Remove( ev );

				//activate it
				//eventManager.toggleVisButton.SetActive( true );
				eventPopup.Show( ev, () =>
				{
					//eventManager.toggleVisButton.SetActive( false );
					callback?.Invoke();
				} );
			}
			else
			{
				//eventManager.toggleVisButton.SetActive( false );
				//all turn actions done and shouldn't be any mission events in progress, so start new turn
				callback?.Invoke();
			}
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

			DataStore.sagaSessionData.gameVars.isEndTurn = true;
			eventManager.CheckIfEventsTriggered( () =>
			{
				DataStore.sagaSessionData.gameVars.isEndTurn = false;
			} );
			//queue up any end of CURRENT turn events that were fired this round
			eventManager.QueueEndOfCurrentTurnEvents();

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

			IncreaseRound();

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

				FindObjectOfType<SagaEventManager>().ShowTextBox( DataStore.sagaSessionData.gameVars.currentMissionInfo );
			}
		}

		public void OnSettings()
		{
			sound.PlaySound( FX.Click );
			EventSystem.current.SetSelectedGameObject( null );
			if ( !eventManager.IsUIHidden )
			{
				GlowEngine.FindUnityObject<SettingsScreen>().Show( OnQuitSaga );
			}
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
					SceneManager.LoadScene( "Title" );
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
		/// Does NOT make events check for being triggered
		/// </summary>
		public void IncreaseRound()
		{
			DataStore.sagaSessionData.gameVars.round++;
			roundText.text = DataStore.uiLanguage.uiMainApp.roundHeading + "\r\n" + DataStore.sagaSessionData.gameVars.round.ToString();
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