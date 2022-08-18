using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Saga
{
	public class SagaSession
	{
		public int stateManagementVersion = 1;
		public SagaSetupOptions setupOptions;
		public DeploymentCard selectedAlly, fixedAlly;
		public SagaGameVars gameVars;

		public List<DeploymentCard> MissionStarting;
		public List<DeploymentCard> MissionReserved;
		public List<DeploymentCard> EarnedVillains;
		public List<DeploymentCard> MissionIgnored;
		public List<DeploymentCard> MissionHeroes;

		public HashSet<string> CannotRedeployList;
		//list of heroes that finish taking part in an Event with "any hero wounded"
		//makes sure they don't keep firing said Event
		public HashSet<string> AnyHeroWoundedEventDone;

		public string missionStringified;

		public class SagaGameVars
		{
			public int round;
			public int eventsTriggered;
			public int currentThreat;
			public int deploymentModifier;
			public int fame;
			public bool pauseDeployment;
			public bool pauseThreatIncrease;
			public bool isNewGame = true;
			public string currentMissionInfo;
			//temporary event conditions
			public bool isEndTurn = false;
			public bool isStartTurn = false;
			public string currentObjective;
			public DeploymentCard activatedGroup = null;
			//keep track of the end of current round events
			//keep track of events that have already fired (for use with certain TriggeredBy)
			//keep track of any enemy group data overrides (instructions, custom enemy deployment event action, etc)
			public Dictionary<Guid, int> endCurrentRoundEvents { get; } = new Dictionary<Guid, int>();
			public List<Guid> firedEvents { get; } = new List<Guid>();
			public List<DeploymentGroupOverride> dgOverrides = new List<DeploymentGroupOverride>();
			public DeploymentGroupOverride dgOverridesAll = null;
			public Dictionary<Guid, int> highlightLifeTimes = new Dictionary<Guid, int>();

			public SagaGameVars()
			{

			}

			public void AddEndCurrentRoundEvent( Guid guid )
			{
				if ( !endCurrentRoundEvents.ContainsKey( guid ) )
				{
					Debug.Log( "AddEndCurrentRoundEvent()::EVENT ADDED" );
					endCurrentRoundEvents.Add( guid, round );
					//return true;
				}
				else//update it
				{
					Debug.Log( "AddEndCurrentRoundEvent()::EVENT UPDATED" );
					endCurrentRoundEvents[guid] = round;
					//return false;
				}

				//Debug.Log( "AddEndCurrentRoundEvent()::END CURRENT ROUND EVENT ALREADY QUEUED" );
				//return false;
			}

			public bool ShouldFireEndCurrentRoundEvent( Guid guid )
			{
				if ( endCurrentRoundEvents.ContainsKey( guid ) )
					return endCurrentRoundEvents[guid] == round;
				else
					return false;
			}

			public void AddFiredEvent( Guid guid )
			{
				if ( !firedEvents.Contains( guid ) )
					firedEvents.Add( guid );
			}

			/// <summary>
			/// Returns the ALL DeploymentGroupOverride unless id is specified, null if it doesn't exist
			/// </summary>
			public DeploymentGroupOverride GetDeploymentOverride( string id = "" )
			{
				if ( !string.IsNullOrEmpty( id ) )
				{
					return dgOverrides.Where( x => x.ID == id ).FirstOr( null );
				}
				else if ( id == null )
					return null;
				else
					return dgOverridesAll;
			}

			/// <summary>
			/// Create and return a new override, otherwise return existing override
			/// </summary>
			public DeploymentGroupOverride CreateDeploymentOverride( string id = "" )
			{
				if ( string.IsNullOrEmpty( id ) )
				{
					return dgOverridesAll ?? (dgOverridesAll = new DeploymentGroupOverride( "" ));
				}
				else if ( !string.IsNullOrEmpty( id ) )
				{
					if ( dgOverrides.Any( x => x.ID == id ) )
						return dgOverrides.Where( x => x.ID == id ).First();
					else
					{
						var ovrd = new DeploymentGroupOverride( id );
						dgOverrides.Add( ovrd );
						return ovrd;
					}
				}
				return null;
			}

			public DeploymentGroupOverride CreateCustomDeploymentOverride( CustomEnemyDeployment ced )
			{
				if ( dgOverrides.Any( x => x.ID == ced.enemyGroupData.cardID ) )
					return dgOverrides.Where( x => x.ID == ced.enemyGroupData.cardID ).First();
				else
				{
					var ovrd = new DeploymentGroupOverride( ced.enemyGroupData.cardID );
					dgOverrides.Add( ovrd );
					ovrd.isCustom = true;
					ovrd.customType = ced.customType;
					//set name
					ovrd.nameOverride = ced.enemyGroupData.cardName;
					//set egd
					ovrd.SetEnemyDeploymentOverride( ced.enemyGroupData );
					//reposition instructions
					ovrd.repositionInstructions = ced.repositionInstructions;
					//set thumbnail
					ovrd.thumbnailGroupImperial = ced.thumbnailGroupImperial;
					ovrd.thumbnailGroupRebel = ced.thumbnailGroupRebel;
					//bonuses
					ovrd.customBonuses = ced.bonuses.Split( '\n' );
					//deployment
					ovrd.canReinforce = ced.canReinforce;
					ovrd.canRedeploy = ced.canRedeploy;
					ovrd.canBeDefeated = ced.canBeDefeated;
					ovrd.useResetOnRedeployment = ced.useResetOnRedeployment;

					return ovrd;
				}
			}

			public void RemoveOverride( string id )
			{
				if ( string.IsNullOrEmpty( id ) )
					return;
				int idx = dgOverrides.FindIndex( x => { return x.ID == id; } );
				if ( idx >= 0 )
				{
					Debug.Log( $"RemoveOverride()::{id}" );
					dgOverrides.RemoveAt( idx );
				}
			}

			public void RemoveAllOverrides()
			{
				dgOverridesAll = null;
				dgOverrides.Clear();
			}
		}

		public SagaSession( SagaSetupOptions opts )
		{
			//DataStore.InitData() has already been called at this point to load data
			setupOptions = opts;

			MissionStarting = new List<DeploymentCard>();
			MissionReserved = new List<DeploymentCard>();
			EarnedVillains = new List<DeploymentCard>();
			MissionIgnored = new List<DeploymentCard>();
			MissionHeroes = new List<DeploymentCard>();
			CannotRedeployList = new HashSet<string>();
			AnyHeroWoundedEventDone = new HashSet<string>();
			selectedAlly = null;
			fixedAlly = null;

			gameVars = new SagaGameVars();
		}

		public void InitGameVars()
		{
			gameVars.round = 1;
			gameVars.eventsTriggered = 0;
			gameVars.fame = 0;

			gameVars.currentThreat = 0;
			//if ( allyThreatCost == YesNo.Yes && selectedAlly != null )
			//	gameVars.currentThreat += selectedAlly.cost;
			gameVars.currentThreat += setupOptions.addtlThreat;

			gameVars.deploymentModifier = 0;
			if ( setupOptions.difficulty == Difficulty.Hard )
				gameVars.deploymentModifier = 2;

			gameVars.pauseDeployment = false;
			gameVars.pauseThreatIncrease = false;
		}

		/// <summary>
		/// Positive or negative number to add/decrease. force(TRUE)=don't use difficulty scale
		/// </summary>
		public void ModifyThreat( int amount, bool force = false )
		{
			Debug.Log( "UpdateThreat()::OLD VALUE = " + gameVars.currentThreat );
			//the only time ModifyThreat has "force=true" is when the user applies a custom amount of threat
			//or threat is added at (Saga) mission start and the mission type = Side
			//in that case, do NOT apply the difficulty modifier - apply the direct amount requested
			//force=true is also used for the ModifyThreat event action
			if ( amount > 0 && !force )
			{
				if ( setupOptions.difficulty == Difficulty.Easy )
					amount = (int)Math.Floor( amount * .7f );
				else if ( setupOptions.difficulty == Difficulty.Hard )
					amount = (int)Math.Floor( amount * 1.3f );
			}
			Debug.Log( "UpdateThreat()::AMOUNT = " + amount );

			//only pause modification of threat when "amount" is POSITIVE
			//threat COSTS (negative) should ALWAYS modify (subtract) threat
			if ( amount > 0 && gameVars.pauseThreatIncrease && !force )
			{
				Debug.Log( "ModifyThreat()::THREAT PAUSED, NO MODIFICATION" );
				return;
			}

			gameVars.currentThreat = Math.Max( 0, gameVars.currentThreat + amount );
			Debug.Log( "UpdateThreat()::NEW VALUE = " + gameVars.currentThreat );
		}

		public void UpdateDeploymentModifier( int amount )
		{
			gameVars.deploymentModifier += amount;
			Debug.Log( "Update DeploymentModifier: " + gameVars.deploymentModifier );
		}

		public void SetDeploymentModifier( int amount )
		{
			gameVars.deploymentModifier = amount;
			Debug.Log( "Set DeploymentModifier: " + gameVars.deploymentModifier );
		}

		public void SaveState()
		{
			if ( setupOptions.isTutorial )
			{
				Debug.Log( "SaveState()::Canceled save state - this is a tutorial" );
				return;
			}

			StateManager.SaveState();
		}
	}
}
