using System;
using System.Collections.Generic;
using System.Linq;

namespace Saga
{
	public class DeploymentGroupOverride
	{
		public string ID = "";//for identification of override
		public ChangeInstructions changeInstructions;
		public ChangeTarget changeTarget;

		///EnemyDeployment properties
		public string nameOverride;
		public string repositionInstructions;
		public int threatCost;
		public string modification;
		public bool canReinforce;
		public bool canRedeploy;
		public bool canBeDefeated;
		public bool useResetOnRedeployment;
		public bool useThreat;//use threat cost
		public bool showMod;
		public bool useGenericMugshot;
		public Guid setTrigger;
		public Guid setEvent;
		public Guid specificDeploymentPoint;
		public DeploymentSpot deploymentPoint;
		//outline color is only customizable for custom deployments and custom characters
		public string deploymentOutlineColor = "Blue";
		///EnemyGroupData properties
		public List<DPData> pointList = new List<DPData>();
		///Custom properties
		public bool isCustomDeployment;
		public MarkerType customType;
		public string thumbnailGroupImperial;
		public string thumbnailGroupRebel;
		public DeploymentCard customCard;
		public string[] customBonuses;

		public DeploymentGroupOverride() { }//empty constructor for json.net

		public DeploymentGroupOverride( string cardID )
		{
			//set defaults all groups would use
			ID = cardID;
			isCustomDeployment = false;
			repositionInstructions = "";
			nameOverride = "";
			if ( !string.IsNullOrEmpty( cardID ) )
				nameOverride = DataStore.GetEnemy( cardID )?.name ?? DataStore.allyCards.Where( x => x.id == cardID ).FirstOr( null )?.name ?? $"Unknown card::{cardID}";
			//overrides set to null are not used when called upon
			threatCost = 0;
			modification = "";
			canReinforce = true;
			canRedeploy = true;
			canBeDefeated = true;
			useResetOnRedeployment = false;
			useThreat = false;
			showMod = false;
			useGenericMugshot = false;
			setTrigger = Guid.Empty;
			setEvent = Guid.Empty;
			specificDeploymentPoint = Guid.Empty;
			deploymentPoint = DeploymentSpot.Active;
			var card = DataStore.GetEnemy( cardID );
			if ( card != null )
				deploymentOutlineColor = (bool)(DataStore.GetEnemy( cardID )?.isElite) ? "Red" : card.deploymentOutlineColor;

			changeInstructions = null;
			changeTarget = null;
		}

		/// <summary>
		/// From EnemyDeployment event action
		/// </summary>
		public void SetEnemyDeploymentOverride( EnemyDeployment ed )
		{
			threatCost = ed.threatCost;
			modification = ed.modification;
			canReinforce = ed.canReinforce;
			canRedeploy = ed.canRedeploy;
			canBeDefeated = ed.canBeDefeated;
			useResetOnRedeployment = ed.useResetOnRedeployment;
			useThreat = ed.useThreat;
			showMod = ed.showMod;
			setTrigger = ed.enemyGroupData.defeatedTrigger;
			setEvent = ed.enemyGroupData.defeatedEvent;
			repositionInstructions = ed.repositionInstructions;

			//warning - this will overwrite deploymentPoint, specificDeploymentPoint, nameOverride
			SetEnemyDeploymentOverride( ed.enemyGroupData );

			//use the custom name from EnemyDeployment in this case
			if ( !string.IsNullOrEmpty( ed.enemyName ) )
				nameOverride = ed.enemyName;
			//use specific DP from EnemyDeployment in this case
			deploymentPoint = ed.deploymentPoint;
			specificDeploymentPoint = ed.specificDeploymentPoint;
		}

		/// <summary>
		/// (From Reserved/Starting data) Sets groupTraits, custom name, instructions, DP override if they exist in EnemyGroupData parameter
		/// </summary>
		public void SetEnemyDeploymentOverride( EnemyGroupData ed )
		{
			//name
			if ( !string.IsNullOrEmpty( ed.cardName ) )
				nameOverride = ed.cardName;
			//traits
			var ct = new ChangeTarget()
			{
				targetType = PriorityTargetType.Trait,
				groupPriorityTraits = ed.groupPriorityTraits,
				percentChance = 60
			};
			SetTargetOverride( ct );
			//instructions
			if ( !string.IsNullOrEmpty( ed.customText ) )
			{
				SetInstructionOverride( new ChangeInstructions()
				{
					instructionType = ed.customInstructionType,
					theText = ed.customText,
				} );
			}
			//DPs
			specificDeploymentPoint = ed.pointList[0].GUID;//there is always at least 1

			//determine if the deploymentPoint should be Active or Specific
			if ( ed.pointList.All( x => x.GUID == Guid.Empty ) )
				deploymentPoint = DeploymentSpot.Active;
			else if ( ed.pointList.Any( x => x.GUID == Utils.GUIDOne ) )
				deploymentPoint = DeploymentSpot.None;
			else
				deploymentPoint = DeploymentSpot.Specific;
			pointList = ed.pointList;

			//defeated trigger/event
			setTrigger = ed.defeatedTrigger;
			setEvent = ed.defeatedEvent;

			//generic mugshot
			useGenericMugshot = ed.useGenericMugshot;
		}

		public void SetInstructionOverride( ChangeInstructions ci )
		{
			changeInstructions = ci;
		}

		public void SetTargetOverride( ChangeTarget ct )
		{
			changeTarget = ct;
		}

		public Guid[] GetDeploymentPoints()
		{
			if ( deploymentPoint == DeploymentSpot.Active )
				return new Guid[] { Guid.Empty };
			else
			{
				if ( pointList.All( x => x.GUID == specificDeploymentPoint ) )
					return new Guid[] { specificDeploymentPoint };
				else
					return pointList.Select( x => x.GUID ).ToArray();
			}
		}

		public void ResetDP()
		{
			deploymentPoint = DeploymentSpot.Active;
			foreach ( var item in pointList )
			{
				item.GUID = Guid.Empty;
			}
		}
	}
}

/*
Saga will use a deployment point's (DP) "Is Active" status to establish whether to use it for the "Active Deployment Point" used by deployment event actions and any end of turn deployments/reinforcements

Active DPs have a permanent (as long as its Active) visual icon on the map in the Saga app (using its color)

Events with the Modify Map Entity event action can be used to enable/disable a DP

If more than one DP is Active, ONE of them will be randomly chosen for groups to deploy on

If no DPs are Active, the deployment won't happen

All figures in a group will deploy on the same Active DP, unless they are using the "Specific DP" option

If a "Specific DP" is specified, but that DP is inactive, the DP will momentarily become active only for that group to deploy on it (as long as its Map Section is visible), and then become inactive (hidden from the map) after the deployment finishes.  See NOTE below

The app will zoom to the DP the group should deploy on, as well as show a popup naming the group along with the ID

Like every other popup in Saga, a "show map" icon can be hit to temporarily hide the popup so the players can see the map underneath
 * */