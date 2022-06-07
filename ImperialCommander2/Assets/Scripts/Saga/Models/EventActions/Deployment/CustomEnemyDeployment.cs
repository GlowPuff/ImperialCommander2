using System;

namespace Saga
{
	public class CustomEnemyDeployment : EventAction
	{
		public MarkerType customType;
		public string thumbnailGroupImperial;
		public string thumbnailGroupRebel;
		public string repositionInstructions;
		public string groupAttack;
		public string groupDefense;
		public string surges;
		public string bonuses;
		public string keywords;
		public string abilities;
		//public string modification;
		public int groupCost;
		public int groupRedeployCost;
		public int groupSize;
		public int groupHealth;
		public int groupSpeed;
		//public SourceType sourceType;
		public bool canReinforce;
		public bool canRedeploy;
		public bool canBeDefeated;
		public bool useDeductCost;
		public bool useCustomInstructions;
		public bool useThreatMultiplier;
		//public bool useGenericMugshot;
		//public bool showMod;
		public Guid specificDeploymentPoint;
		public DeploymentSpot deploymentPoint;
		public EnemyGroupData enemyGroupData;

		public CustomEnemyDeployment()
		{

		}
	}
}
