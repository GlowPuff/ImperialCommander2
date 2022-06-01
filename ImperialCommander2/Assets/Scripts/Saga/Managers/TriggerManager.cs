using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Saga
{
	public class TriggerManager : MonoBehaviour
	{
		public SagaEventManager eventManager;

		List<TriggerState> triggerStateList = new List<TriggerState>();
		List<EventGroup> eventGroupList = new List<EventGroup>();

		public void Init( Mission mission )
		{
			foreach ( var t in mission.eventGroups )
			{
				var eg = new EventGroup()
				{
					name = t.name,
					GUID = t.GUID,
					triggerGUID = t.triggerGUID,
					repeateable = t.repeateable,
					isUnique = t.isUnique,
					//don't want to modify original List
					missionEvents = t.missionEvents.ToArray().ToList(),
				};
				//mark all events in this group as repeatable or not
				foreach ( var ev in eg.missionEvents )
				{
					var mev = DataStore.mission.GetEventFromGUID( ev );
					mev.isRepeatable = t.repeateable;
				}
				eventGroupList.Add( eg );
			}

			//SKIP "NONE" Triggers?
			foreach ( var t in mission.globalTriggers )
				triggerStateList.Add( new TriggerState( t ) { currentValue = t.initialValue } );

			foreach ( var section in mission.mapSections )
			{
				foreach ( var t in section.triggers )
				{
					triggerStateList.Add( new TriggerState( t ) );
				}
			}

			FindObjectOfType<ObjectivePanel>().NotifyValueUpdated();
		}

		/// <summary>
		/// Skips a "None" trigger
		/// </summary>
		public void FireTrigger( Guid guid )
		{
			if ( guid == Guid.Empty )
				return;

			var ts = triggerStateList.Where( x => x.trigger.GUID == guid ).FirstOr( null );
			if ( ts != null )
			{
				Debug.Log( "FireTrigger::" + ts.trigger.name );
				//increase trigger value up to its max value, if specified
				if ( ts.trigger.maxValue != -1 )
					ts.currentValue = Math.Min( ts.currentValue + 1, ts.trigger.maxValue );
				else
					ts.currentValue++;
				//notify objective of a value change
				FindObjectOfType<ObjectivePanel>().NotifyValueUpdated();

				if ( ts.trigger.eventGUID != Guid.Empty )
					eventManager.DoEvent( eventManager.EventFromGUID( ts.trigger.eventGUID ) );

				//make Events check for any matching "additional Triggers" values
				eventManager.CheckIfEventsTriggered( () =>
				{
					//finally, reset the Trigger value back to 0 if necessary
					if ( ts.trigger.useReset )
						ts.ResetValue();
				} );

				//handle Event Groups
				var eg = eventGroupList.Where( x => x.triggerGUID == guid ).FirstOr( null );
				if ( eg != null )
					FireEventGroup( eg.GUID );
			}
		}

		public void FireEventGroup( Guid guid )
		{
			var eg = eventGroupList.Where( x => x.GUID == guid ).FirstOr( null );
			if ( eg != null && eg.missionEvents.Count > 0 )
			{
				Debug.Log( $"FireTrigger::FIRE EVENT GROUP::{eg.name}" );
				//just like non-repeating events, remember this event group so it won't fire again
				//DataStore.sagaSessionData.gameVars.AddFiredEvent( eg.GUID );
				//pick a random event and remove it unless it's repeatable
				//make sure there is still an Event to fire
				if ( eg.missionEvents.Count > 0 )
				{
					int idx = GlowEngine.GenerateRandomNumbers( eg.missionEvents.Count )[0];
					Guid ev = eg.missionEvents[idx];
					//only remove event if it's treating them uniquely (fire only once)
					if ( eg.isUnique )
						eg.missionEvents.RemoveAt( idx );
					eventManager.DoEvent( eventManager.EventFromGUID( ev ) );
				}
			}
			else if ( eg != null && eg.missionEvents.Count == 0 )
				Debug.Log( "FireTrigger::EVENT GROUP IS SPENT" );

			//reset if it's repeatable
			if ( eg != null && eg.missionEvents.Count == 0 && eg.repeateable )
			{
				Debug.Log( "RESET EVENT GROUP::" + eg.name );
				//reset the event group by filling it up with its Events again
				var meg = DataStore.mission.eventGroups.Where( x => x.GUID == eg.GUID ).FirstOr( null );
				if ( meg != null )
					eg.missionEvents = meg.missionEvents.ToArray().ToList();
			}
		}

		/// <summary>
		/// returns all trigger states that the provided event monitors for a value change
		/// </summary>
		public List<TriggerState> GetTriggerStates( MissionEvent ev )
		{
			var tstates = (from trigby in ev.additionalTriggers join ts in triggerStateList on trigby.triggerGUID equals ts.trigger.GUID select ts).ToList();
			return tstates;
		}

		public void ModifyVariable( TriggerModifier modifier )
		{
			var t = triggerStateList.Where( x => x.trigger.GUID == modifier.triggerGUID ).FirstOr( null );
			if ( t != null )
			{
				Debug.Log( $"ModifyVariable::{modifier.triggerName}::OLD VALUE={t.currentValue}" );
				if ( modifier.setValue > -1 )
					t.currentValue = modifier.setValue;
				else
					t.currentValue += modifier.modifyValue;
				//notify objective of a value change
				FindObjectOfType<ObjectivePanel>().NotifyValueUpdated();
				Debug.Log( $"ModifyVariable::{modifier.triggerName}::NEW VALUE={t.currentValue}" );
			}
		}

		public int CurrentTriggerValue( Guid guid )
		{
			foreach ( var item in triggerStateList )
			{
				if ( item.trigger.GUID == guid )
					return item.currentValue;
			}
			return 0;
		}
	}
}
