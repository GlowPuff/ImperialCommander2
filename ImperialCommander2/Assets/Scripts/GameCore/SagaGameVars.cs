using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Saga
{
	public class SagaGameVars
	{
		public int medPacCount = 1;
		public int round;
		public int eventsTriggered;
		public int currentThreat;
		public int deploymentModifier;
		public int fame;
		public bool pauseDeployment;
		public bool pauseThreatIncrease;
		public bool isNewGame = true;
		public string currentMissionInfo;
		public bool isEndTurn = false;//temporary event condition
		public bool isStartTurn = false;//temporary event condition
		public string currentObjective;
		public bool delayOptionalDeployment = false;
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

				ovrd.isCustomDeployment = true;
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
				//outline color
				ovrd.deploymentOutlineColor = ced.deploymentOutlineColor;

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
				//all custom deployments get an override, do NOT remove these
				if ( !dgOverrides[idx].isCustomDeployment )
				{
					Debug.Log( $"RemoveOverride()::{id}" );
					dgOverrides.RemoveAt( idx );
				}
				else
					Debug.Log( $"RemoveOverride()::Custom Deployment, skipping ({id})" );
			}
		}

		public void RemoveAllOverrides()
		{
			dgOverridesAll = null;
			//only keep overrides that are created from custom deployments
			dgOverrides = dgOverrides.Where( x => x.isCustomDeployment ).ToList();
			Debug.Log( $"RemoveAllOverrides()::{dgOverrides.Count} overrides left over" );
		}
	}
}
