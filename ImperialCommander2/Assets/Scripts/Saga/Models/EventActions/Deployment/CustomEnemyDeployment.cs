using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Saga
{
	public class CustomEnemyDeployment : EventAction
	{
		//card properties
		public MarkerType customType;
		[DefaultValue( MarkerType.Rebel )]
		[JsonProperty( DefaultValueHandling = DefaultValueHandling.Populate )]
		public MarkerType iconType;
		public AttackType attackType;
		public string thumbnailGroupImperial;//card ID format
		public string thumbnailGroupRebel;//card ID format
		public string repositionInstructions;
		public string groupAttack;
		public string groupDefense;
		public string surges;
		public string bonuses;
		public string keywords;
		public string abilities;
		public int groupCost;
		public int groupRedeployCost;
		public int groupSize;
		public int groupHealth;
		public int groupSpeed;
		[DefaultValue( "Gray" )]
		[JsonProperty( DefaultValueHandling = DefaultValueHandling.Populate )]
		public string deploymentOutlineColor;
		//deployment properties
		public bool canReinforce;
		public bool canRedeploy;
		public bool canBeDefeated;
		public bool useDeductCost;
		public bool useCustomInstructions;
		public bool useThreatMultiplier;
		public bool useResetOnRedeployment;
		public Guid specificDeploymentPoint;
		public DeploymentSpot deploymentPoint;
		public EnemyGroupData enemyGroupData;

		public CustomEnemyDeployment()
		{

		}
	}
}
