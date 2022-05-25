using System;

namespace Saga
{
	public class EnemyDeployment : EventAction
	{
		public string enemyName;
		public string deploymentGroup;//the ID
		public string repositionInstructions;
		public int threatCost;
		public string modification;
		public SourceType sourceType;
		public bool canReinforce;
		public bool canRedeploy;
		public bool canBeDefeated;
		public bool useThreat;
		public bool useCustomInstructions;
		public bool showMod;
		public Guid setTrigger;
		public Guid setEvent;
		public Guid specificDeploymentPoint;
		public DeploymentSpot deploymentPoint;//active or specific
		public EnemyGroupData enemyGroupData;

		public EnemyDeployment()
		{

		}
	}
}
