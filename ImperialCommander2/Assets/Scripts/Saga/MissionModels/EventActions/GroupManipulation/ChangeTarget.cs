using System.Collections.Generic;

namespace Saga
{
	public class ChangeTarget : EventAction
	{
		public PriorityTargetType targetType;
		public GroupType groupType;
		public string otherTarget;
		public string specificAlly;
		public string specificHero;
		public int percentChance;
		public List<DeploymentCard> groupsToAdd = new List<DeploymentCard>();
		public GroupPriorityTraits groupPriorityTraits;

		public ChangeTarget()
		{

		}
	}
}
