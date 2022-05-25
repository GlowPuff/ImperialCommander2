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
			bool found = false;
			var tstates = Object.FindObjectOfType<TriggerManager>().GetTriggerStates( missionEvent );
			if ( missionEvent.additionalTriggers.Count > 0 )
			{
				foreach ( var ts in tstates )
				{
					foreach ( var triggerby in missionEvent.additionalTriggers )
					{
						if ( triggerby.triggerGUID == ts.trigger.GUID && ts.currentValue == triggerby.triggerValue )
						{
							Debug.Log( $"CheckAdditionalTriggers()::TRIGGER MATCH for Event '{missionEvent.name}'::" + triggerby.triggerName );
							found = true;
						}
					}
				}
			}

			return found;
		}

		/// <summary>
		/// -1 = every round, otherwise checks for specific round
		/// </summary>
		public bool CheckEndRound( int r = -1 )
		{
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
			return DataStore.deployedHeroes.Any( x => x.isHero && x.heroState.heroHealth == HeroHealth.Wounded );
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
