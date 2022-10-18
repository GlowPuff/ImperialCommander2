using System;
using System.Collections.Generic;
using UnityEngine;

namespace Saga
{
	public class SagaSession
	{
		public int stateManagementVersion = 1;
		public SagaSetupOptions setupOptions;
		public DeploymentCard selectedAlly, fixedAlly;
		public SagaGameVars gameVars;
		public Guid campaignGUID;

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
			campaignGUID = Guid.Empty;

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
				//round to nearest whole number, with X.5 rounding UP
				if ( setupOptions.difficulty == Difficulty.Easy )
					amount = (int)Math.Round( amount * .7f, 0, MidpointRounding.AwayFromZero );
				else if ( setupOptions.difficulty == Difficulty.Hard )
					amount = (int)Math.Round( amount * 1.3f, 0, MidpointRounding.AwayFromZero );
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

			//GlowEngine.FindUnityObject<SagaController>()?.NotifyThreatUpdated();
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
				Debug.Log( "SaveState()::Canceled SaveState() - this is a tutorial" );
				return;
			}

			campaignGUID = RunningCampaign.sagaCampaignGUID;

			if ( RunningCampaign.sagaCampaignGUID == Guid.Empty )
				StateManager.SaveState( "SagaSession" );
			else
				StateManager.SaveState( "CampaignSession" );
		}

		public bool LoadState( StateManager sm )
		{
			if ( setupOptions.isTutorial )
			{
				Debug.Log( "SaveState()::Canceled LoadState() - this is a tutorial" );
				return false;
			}

			if ( RunningCampaign.sagaCampaignGUID == Guid.Empty )
				return sm.LoadState( "SagaSession" );
			else
				return sm.LoadState( "CampaignSession" );
		}

		public void RemoveState()
		{
			if ( setupOptions.isTutorial )
			{
				Debug.Log( "RemoveState()::Canceled RemoveState() - this is a tutorial" );
				return;
			}

			if ( RunningCampaign.sagaCampaignGUID == Guid.Empty )
				StateManager.RemoveState( "SagaSession" );
			else
				StateManager.RemoveState( "CampaignSession" );
		}
	}
}
