using System.Linq;
using UnityEngine;

namespace Saga
{
	public class EventTriggerChecker
	{
		MissionEvent missionEvent;

		public EventTriggerChecker( MissionEvent me )
		{
			missionEvent = me;
		}

		public bool CheckAdditionalTriggers()
		{
			var tstates = Object.FindObjectOfType<TriggerManager>().GetTriggerStates( missionEvent );

			var count = from ts in tstates
									join triggeredby in missionEvent.additionalTriggers
									on ts.trigger.GUID equals triggeredby.triggerGUID
									select ts;
			var states = from ts in tstates
									 join triggeredby in missionEvent.additionalTriggers
									 on new { g = ts.trigger.GUID, cv = ts.currentValue }
									 equals new { g = triggeredby.triggerGUID, cv = triggeredby.triggerValue }
									 select ts;

			if ( missionEvent.behaviorAll && count.Count() == states.Count() )
			{
				Debug.Log( $"CheckAdditionalTriggers()::'ALL' TRIGGER MATCH for Event '{missionEvent.name}'" );
				return true;
			}
			else if ( !missionEvent.behaviorAll && states.Count() > 0 )
			{
				Debug.Log( $"CheckAdditionalTriggers()::'ANY' TRIGGER MATCH for Event '{missionEvent.name}'" );
				return true;
			}

			return false;
		}

		/// <summary>
		/// -1 = every round, otherwise checks for specific round
		/// </summary>
		public bool CheckEndRound( int r = -1 )
		{
			//if ( missionEvent.hasActivatedThisRound )
			//	return false;

			if ( DataStore.sagaSessionData.gameVars.isEndTurn )
			{
				if ( r == -1 )
					return true;
				else if ( DataStore.sagaSessionData.gameVars.round == r )
					return true;
			}

			return false;
		}

		/// <summary>
		/// -1 = every round, otherwise checks for specific round
		/// </summary>
		public bool CheckStartRound( int r = -1 )
		{
			//if ( missionEvent.hasActivatedThisRound )
			//	return false;

			if ( DataStore.sagaSessionData.gameVars.isStartTurn )
			{
				if ( r == -1 )
					return true;
				else if ( DataStore.sagaSessionData.gameVars.round == r )
					return true;
			}

			return false;
		}

		public bool CheckEndOfCurrentRound()
		{
			//if ( missionEvent.hasActivatedThisRound )
			//	return false;
			//since this check is ONLY called at the end of a round, no need to check if this is the end of the round first
			return DataStore.sagaSessionData.gameVars.ShouldFireEndCurrentRoundEvent( missionEvent.GUID );
		}

		public bool CheckGroupActivated()
		{
			return missionEvent.activationOf == DataStore.sagaSessionData.gameVars.activatedGroup?.id;
		}

		public bool CheckAllyDefeated()
		{
			var player = DataStore.deployedHeroes.Where( x => x.id == missionEvent.allyDefeated ).FirstOr( null );
			return player != null && player.heroState.heroHealth == HeroHealth.Defeated;
		}

		public bool CheckHeroWounded()
		{
			var player = DataStore.deployedHeroes.Where( x => x.id == missionEvent.heroWounded ).FirstOr( null );
			return player != null && player.heroState.heroHealth == HeroHealth.Wounded;
		}

		public bool CheckHeroDefeated()
		{
			var player = DataStore.deployedHeroes.Where( x => x.id == missionEvent.heroWithdraws ).FirstOr( null );
			return player != null && player.heroState.heroHealth == HeroHealth.Defeated;
		}

		public bool CheckAnyHeroWounded()
		{
			//get list of deployed heroes that have NOT participated in an "any hero wounded" Event yet
			var h = from dh in DataStore.deployedHeroes
							where !DataStore.sagaSessionData.AnyHeroWoundedEventDone.Contains( dh.id )
							select dh;

			//return DataStore.deployedHeroes.Any( x => x.isHero && x.heroState.heroHealth == HeroHealth.Wounded );
			return h.Any( x => x.isHero && x.heroState.heroHealth == HeroHealth.Wounded );
		}

		public bool CheckAllGroupsDefeated()
		{
			return DataStore.deployedEnemies.Count == 0;
		}

		public bool CheckAllHeroesWounded()
		{
			return DataStore.sagaSessionData.MissionHeroes.All( x => x.heroState.heroHealth == HeroHealth.Wounded );
		}
	}
}
