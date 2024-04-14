using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Saga
{
	public partial class SagaEventManager : MonoBehaviour
	{
		public GameObject textBoxPrefab, promptBoxPrefab, toggleVisButton, inputBoxPrefab;
		public Transform infoButtonTX;
		public EnemyActivationPopup enemyActivationPopup;

		bool[] hiddenChildren = new bool[0];

		public bool IsProcessingEvents { get { return processingEvents; } }

		/// <summary>
		/// ALL mission events (global and in each map section)
		/// </summary>
		List<MissionEvent> missionEvents = new List<MissionEvent>();
		Queue<IEventAction> eventActionQueue = new Queue<IEventAction>();
		Queue<MissionEvent> eventQueue = new Queue<MissionEvent>();
		bool processingEvents = false;
		Action endProcessingCallback = null;

		public bool IsUIHidden
		{
			get { return hiddenChildren.Length > 0; }
		}

		public bool UIShowing
		{
			get
			{
				int c = 0;
				foreach ( Transform item in transform )
				{
					if ( item.gameObject.activeInHierarchy )
						c++;
				}
				return c > 0;
			}
		}

		public void Init( Mission mission )
		{
			//SKIP "NONE" EVENTS?
			foreach ( var ev in mission.globalEvents )
			{
				missionEvents.Add( ev );
			}

			foreach ( var section in mission.mapSections )
			{
				foreach ( var ev in section.missionEvents )
				{
					missionEvents.Add( ev );
				}
			}
		}

		public void ResetEndOfEvents()
		{
			//foreach ( var ev in missionEvents )
			//	ev.hasActivatedThisRound = false;
		}

		public void SetEndProcessingCallback( Action callback )
		{
			Debug.Log( "SetEndProcessingCallback()::CALLBACK SET" );
			endProcessingCallback = callback;
		}

		public void ToggleVisibility()
		{
			//hide popups
			if ( hiddenChildren.Length == 0 )
			{
				//store the visibility state of each popup under this parent before hiding them
				hiddenChildren = new bool[transform.childCount];
				for ( int i = 0; i < hiddenChildren.Length; i++ )
				{
					hiddenChildren[i] = transform.GetChild( i ).gameObject.activeInHierarchy;
					transform.GetChild( i ).gameObject.SetActive( false );
				}
			}
			else//unhide popups
			{
				//restore previously hidden popups
				for ( int i = 0; i < transform.childCount; i++ )
				{
					transform.GetChild( i ).gameObject.SetActive( hiddenChildren[i] );
				}
				hiddenChildren = new bool[0];
			}
		}

		/// <summary>
		/// Checks if an event with a trigger condition is triggered, then fires it.
		/// Call this when any of the trigger conditions happen (ie: end of turn, hero wounded, trigger value changed)
		/// </summary>
		public void CheckIfEventsTriggered( Action callback = null )
		{
			Debug.Log( "CheckIfEventsTriggered()::LOOKING FOR EVENT TRIGGERS..." );

			foreach ( var ev in missionEvents.Where( x => x.GUID != Guid.Empty ) )
			{
				//if this event is NOT repeatable AND it has already fired, continue to next
				if ( !ev.isRepeatable && DataStore.sagaSessionData.gameVars.firedEvents.Contains( ev.GUID ) )
				{
					//Debug.Log( "CheckIfEventsTriggered()::NON-REPEAT EVENT, SKIPPING::" + ev.name );
					continue;
				}

				var results = RunChecks( ev );

				if ( results.Count > 0 )
				{
					bool check = false;
					if ( ev.behaviorAll )
						check = results.All( x => x == true );
					else if ( !ev.behaviorAll )
						check = results.Any( x => x == true );

					if ( check )
					{
						if ( ev.useAnyHeroWounded )
						{
							var h = DataStore.deployedHeroes.Where( x => x.characterType == CharacterType.Hero && x.heroState.isWounded );
							foreach ( var item in h )
								DataStore.sagaSessionData.AnyHeroWoundedEventDone.Add( item.id );
						}
						if ( ev.useAnyHeroDefeated )
						{
							var h = DataStore.deployedHeroes.Where( x => x.characterType == CharacterType.Hero && x.heroState.isDefeated );
							foreach ( var item in h )
								DataStore.sagaSessionData.AnyHeroWithdrawnEventDone.Add( item.id );
						}
						//if ( ev.usesEnd )
						//	ev.hasActivatedThisRound = true;
						Debug.Log( "CheckIfEventsTriggered()::EVENT TRIGGERED::" + ev.name );
						//eventTriggered = true;//remove this
						DoEvent( ev );
					}
				}
			}

			Debug.Log( $"CheckIfEventsTriggered()::FINISHED LOOKING FOR EVENT TRIGGERS, FIRING CALLBACK [{callback != null}]" );

			callback?.Invoke();
		}

		List<bool> RunChecks( MissionEvent me, bool checkEndCurrentRound = false )
		{
			EventTriggerChecker checker = new EventTriggerChecker( me );
			List<bool> result = new List<bool>();

			//additional triggers
			if ( me.additionalTriggers.Count > 0 )
				result.Add( checker.CheckAdditionalTriggers() );
			//end each round
			if ( me.useEndOfEachRound )
				result.Add( checker.CheckEndRound() );
			//end of specific round
			if ( me.useEndOfRound )
				result.Add( checker.CheckEndRound( me.endOfRound ) );
			//start of each round
			if ( me.useStartOfEachRound )
				result.Add( checker.CheckStartRound() );
			//start of specific round
			if ( me.useStartOfRound )
				result.Add( checker.CheckStartRound( me.startOfRound ) );
			//particular group activated
			if ( me.useActivation )
				result.Add( checker.CheckGroupActivated() );
			//hero wounded
			if ( me.useHeroWounded )
				result.Add( checker.CheckHeroWounded() );
			//ally defeated
			if ( me.useAllyDefeated )
				result.Add( checker.CheckAllyDefeated() );
			//hero defeated
			if ( me.useHeroWithdraws )
				result.Add( checker.CheckHeroDefeated() );
			//any hero defeated
			if ( me.useAnyHeroDefeated )
				result.Add( checker.CheckAnyHeroDefeated() );
			//any hero wounded
			if ( me.useAnyHeroWounded )
				result.Add( checker.CheckAnyHeroWounded() );
			//all groups defeated
			if ( me.useAllGroupsDefeated )
				result.Add( checker.CheckAllGroupsDefeated() );
			//all heroes wounded
			if ( me.useAllHeroesWounded )
				result.Add( checker.CheckAllHeroesWounded() );
			if ( checkEndCurrentRound && me.isEndOfCurrentRound )
				result.Add( checker.CheckEndOfCurrentRound() );

			return result;
		}

		public MissionEvent EventFromGUID( Guid guid )
		{
			return missionEvents.Where( x => x.GUID == guid ).FirstOr( null );
		}

		public MissionEvent EventFromName( string n )
		{
			//if ( eventActionQueue.Count > 0 )
			//	return null;
			return missionEvents.Where( x => x.name == n ).FirstOr( null );
		}

		/// <summary>
		/// Enqueues an Event, cutInLine makes it fire NEXT instead of LAST, after the currently processing Event
		/// </summary>
		void EnqueueEventAfterCurrent( MissionEvent ev, bool cutInLine )
		{
			//check if this Mission is using the alternate Event system
			if ( DataStore.mission.missionProperties.useAlternateEventSystem )
			{
				//Debug.Log( "EnqueueEventAfterCurrent()::Using Alternate Event System" );
				cutInLine = false;
			}

			if ( !processingEvents )
			{
				Debug.Log( $"EnqueueEventAfterCurrent()::Queue empty, new Event added: [{ev.name}]" );
				eventQueue.Enqueue( ev );
			}
			else if ( !cutInLine )
			{
				Debug.Log( $"EnqueueEventAfterCurrent()::new Event added to END of queue: [{ev.name}], Alternate Event System=[{DataStore.mission.missionProperties.useAlternateEventSystem}]" );
				eventQueue.Enqueue( ev );
			}
			else
			{
				Guid current = eventQueue.Peek().GUID;
				int idx = eventQueue.FindIndexByProperty( x => x.GUID == current );

				//if there are no more Events after the current one, just enqueue the new one
				if ( eventQueue.Count - 1 == idx )
				{
					Debug.Log( $"EnqueueEventAfterCurrent()::Current Event is last in queue, new Event added to END of queue: [{ev.name}]" );
					eventQueue.Enqueue( ev );
				}
				else
				{
					Debug.Log( $"EnqueueEventAfterCurrent()::Queue has many elements, adding new Event..." );

					//with many events queued, we need to recreate the queue so the new event can cut in line
					//create a temp list of all Events already in the current queue
					List<MissionEvent> temp = new List<MissionEvent>();
					temp.AddRange( eventQueue );

					//reset the actual queue to empty, now that it's copied
					eventQueue.Clear();

					//go through each queued Event...
					for ( int i = 0; i < temp.Count; i++ )
					{
						//let the new Event cut in line when we're at the currently indexed Event
						if ( i == idx + 1 )
						{
							Debug.Log( $"EnqueueEventAfterCurrent()::New Event added at index {i} [{ev.name}]" );
							eventQueue.Enqueue( ev );
						}
						//and be sure to also queue the currently indexed Event back into line
						eventQueue.Enqueue( temp[i] );
					}
				}
			}
		}

		/// <summary>
		/// [DEPRECATED] Just show the event's text
		/// </summary>
		public void PreviewEvent( Guid guid )
		{
			MissionEvent ev = EventFromGUID( guid );
			if ( ev != null )
				ShowTextBox( ev.eventText );
			else
				FindObjectOfType<SagaController>().errorPanel.Show( "PreviewEvent()", "GUID not found." );
		}

		/// <summary>
		/// Queues an Event if another Event is in progress, otherwise fires it immediately
		/// </summary>
		public void DoEvent( Guid guid, bool cutInLine = true )
		{
			DoEvent( EventFromGUID( guid ), cutInLine );
		}

		/// <summary>
		/// Queues an Event if another Event is in progress, otherwise fires it immediately
		/// </summary>
		public void DoEvent( MissionEvent ev, bool cutInLine = true )
		{
			if ( ev != null && ev.GUID != Guid.Empty )
			{
				if ( !ev.isRepeatable && DataStore.sagaSessionData.gameVars.firedEvents.Contains( ev.GUID ) )
				{
					Debug.Log( "DoEvent()::EVENT HAS ALREADY FIRED::" + ev.name );
					return;
				}

				DataStore.sagaSessionData.gameVars.AddFiredEvent( ev.GUID );

				//if this is an "end of current round" Event, queue it for later
				if ( ev.isEndOfCurrentRound )
				{
					Debug.Log( "DoEvent()::Queued END OF CURRENT ROUND EVENT::" + ev.name );
					DataStore.sagaSessionData.gameVars.AddEndCurrentRoundEvent( ev.GUID );
				}
				else
				{
					Debug.Log( "DoEvent()::Queued " + ev.name );
					if ( !eventQueue.Contains( ev ) )
						EnqueueEventAfterCurrent( ev, cutInLine );

					//start processing if not busy with an event already
					if ( !processingEvents )
						ProcessEvent( eventQueue.Peek() );
				}
			}
			else
			{
				if ( ev == null )
					FindObjectOfType<SagaController>().ShowError( "ProcessEvent()::MissionEvent is null" );
			}
		}

		public void QueueEndOfCurrentTurnEvents()
		{
			Debug.Log( "QueueEndOfCurrentTurnEvents()::START PROCESSING EVENT QUEUE" );
			if ( DataStore.sagaSessionData.gameVars.endCurrentRoundEvents.Count == 0 )
				Debug.Log( "QueueEndOfCurrentTurnEvents()::NO EVENTS TO QUEUE" );

			foreach ( var evg in DataStore.sagaSessionData.gameVars.endCurrentRoundEvents )
			{
				MissionEvent ev = EventFromGUID( evg.Key );
				//DataStore.sagaSessionData.gameVars.AddFiredEvent( ev.Key );
				var results = RunChecks( ev, true );

				if ( results.Count > 0 )
				{
					bool check = false;
					if ( ev.behaviorAll )
						check = results.All( x => x == true );
					else if ( !ev.behaviorAll )
						check = results.Any( x => x == true );

					if ( check )
					{
						Debug.Log( "QueueEndOfCurrentTurnEvents()::EVENT TRIGGERED::" + ev.name );
						if ( !eventQueue.Contains( ev ) )
							EnqueueEventAfterCurrent( ev, true );
					}
				}
			}

			DataStore.sagaSessionData.gameVars.endCurrentRoundEvents.Clear();
			//start processing events if none in progress
			if ( !processingEvents && eventQueue.Count > 0 )
				ProcessEvent( eventQueue.Peek() );
		}

		/// <summary>
		/// Start processing all the Event Actions in the event
		/// </summary>
		void ProcessEvent( MissionEvent ev )
		{
			Debug.Log( "ProcessEvent()::START PROCESSING EVENT QUEUE::" + ev.name );

			processingEvents = true;

			//create a queue of event actions in this event
			//pop the first EA (event action) off the queue
			//process this EA, ProcessEventAction()
			//when it's done, call NextEventAction()
			eventActionQueue.Clear();
			Debug.Log( "ProcessEvent()::PROCESSING EVENT::" + ev.name );

			//event will never be null at this point
			foreach ( var ea in ev.eventActions )
			{
				eventActionQueue.Enqueue( ea );
			}
			Debug.Log( $"ShowEvent()::Queued {eventActionQueue.Count} event actions" );

			if ( !string.IsNullOrEmpty( ev.eventText ) )
			{
				ShowTextBox( ev.eventText, () =>
				{
					//process event actions, otherwise go to next mission event
					NextEventAction();
				} );
			}
			else
			{
				if ( eventActionQueue.Count >= 0 )
				{
					NextEventAction();
				}
			}
		}

		void ProcessEventAction( IEventAction eventAction )
		{
			switch ( eventAction.eventActionType )
			{
				case EventActionType.G1:
					MissionManagement( eventAction as MissionManagement );
					break;
				case EventActionType.G2:
					ChangeMissionInfo( eventAction as ChangeMissionInfo );
					break;
				case EventActionType.G3:
					ShowChangeObjective( eventAction as ChangeObjective );
					break;
				case EventActionType.G4:
					ModifyVariable( eventAction as ModifyVariable );
					break;
				case EventActionType.G5:
					ModifyThreat( eventAction as ModifyThreat );
					break;
				case EventActionType.G6:
					ShowPromptBox( eventAction as QuestionPrompt, NextEventAction );
					break;
				case EventActionType.G7:
					ShowTextBox( (eventAction as ShowTextBox).theText, NextEventAction );
					break;
				case EventActionType.G8:
					ActivateEventGroup( eventAction as ActivateEventGroup );
					break;
				case EventActionType.G9:
					ShowInputBox( eventAction as InputPrompt );
					break;
				case EventActionType.GM1:
					ChangeGroupInstructions( eventAction as ChangeInstructions );
					break;
				case EventActionType.GM2:
					ChangeGroupTarget( eventAction as ChangeTarget );
					break;
				case EventActionType.GM3:
					ChangeGroupStatus( eventAction as ChangeGroupStatus );
					break;
				case EventActionType.GM4:
					ChangeReposition( eventAction as ChangeReposition );
					break;
				case EventActionType.GM5:
					ResetGroup( eventAction as ResetGroup );
					break;
				case EventActionType.GM6:
					RemoveGroup( eventAction as RemoveGroup );
					break;
				case EventActionType.GM7:
					QueryGroup( eventAction as QueryGroup );
					break;
				case EventActionType.D1:
					EnemyDeployment( eventAction as EnemyDeployment );
					break;
				case EventActionType.D2:
					AllyDeployment( eventAction as AllyDeployment );
					break;
				case EventActionType.D3:
					OptionalDeployment( eventAction as OptionalDeployment );
					break;
				case EventActionType.D4:
					RandomDeployment( eventAction as RandomDeployment );
					break;
				case EventActionType.D5:
					AddGroupToHand( eventAction as AddGroupDeployment );
					break;
				case EventActionType.D6:
					CustomDeployment( eventAction as CustomEnemyDeployment );
					break;
				case EventActionType.M1:
					MapManagement( eventAction as MapManagement );
					break;
				case EventActionType.M2:
					ModifyMapEntity( eventAction as ModifyMapEntity );
					break;
				case EventActionType.CM1:
					ModifyXP( eventAction as CampaignModifyXP );
					break;
				case EventActionType.CM2:
					ModifyCredits( eventAction as CampaignModifyCredits );
					break;
				case EventActionType.CM3:
					ModifyFameAwards( eventAction as CampaignModifyFameAwards );
					break;
				case EventActionType.CM4:
					SetNextMission( eventAction as CampaignSetNextMission );
					break;
				case EventActionType.G10:
					ModifyRndLimit( eventAction as ModifyRoundLimit );
					break;
				case EventActionType.G11:
					SetCountdown( eventAction as SetCountdown );
					break;
				case EventActionType.CM5:
					AddCampaignRewards( eventAction as AddCampaignReward );
					break;
				default:
					Debug.Log( "ProcessEventAction()::EVENT TYPE NOT SUPPORTED: " + eventAction.eventActionType.ToString() + " = " + (int)eventAction.eventActionType );
					NextEventAction();
					break;
			}
		}

		public void NextEventAction()
		{
			if ( eventActionQueue.Count > 0 )
			{
				Debug.Log( "NextEventAction()::PROCESSING NEXT EVENT ACTION" );
				ProcessEventAction( eventActionQueue.Dequeue() );
			}
			else
			{
				Debug.Log( "NextEventAction()::DONE PROCESSING EVENT" );

				//remove the event just processed
				//have to check because the mission may have ended, clearing the queue
				if ( eventQueue.Count > 0 )
					eventQueue.Dequeue();
				processingEvents = false;

				if ( eventQueue.Count > 0 )
				{
					Debug.Log( "NextEventAction()::PROCESSING NEXT EVENT::" + eventQueue.Peek().name );
					//process the next event
					ProcessEvent( eventQueue.Peek() );
				}
				else
				{
					Debug.Log( "NextEventAction()::DONE PROCESSING ALL EVENTS" );
					processingEvents = false;
					//FindObjectOfType<SagaController>().ToggleNavAndEntitySelection( true );
					//toggleVisButton.SetActive( false );

					//Debug.Log( $"NextEventAction()::endProcessingCallback={endProcessingCallback.ToString()}" );
					endProcessingCallback?.Invoke();
					endProcessingCallback = null;
				}
			}
		}

		private void Update()
		{
			bool vis = false;
			int count = 0;
			foreach ( Transform item in transform )
			{
				var popup = item.GetComponent<PopupBase>();
				if ( (popup != null && popup.isActive) )
				{
					count++;
					vis = true;
				}
				if ( enemyActivationPopup.isActive )
					vis = true;
			}

			if ( enemyActivationPopup.isActive && count > 0 )
			{
				//can't hide the UI when it's already hidden AND the activation screen is showing
				vis = false;
			}

			toggleVisButton.SetActive( vis );
		}
	}
}