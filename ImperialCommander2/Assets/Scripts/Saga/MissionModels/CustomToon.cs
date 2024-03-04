using System;
using System.Collections.Generic;

namespace Saga
{
	public class CustomToon
	{
		//these properties don't change, even when copying from another Deployment Group
		public Guid customCharacterGUID;

		public string cardName;
		public string cardID;

		public string groupAttack;
		public string groupDefense;
		public GroupPriorityTraits groupPriorityTraits;

		public DeploymentCard deploymentCard;

		public Thumbnail thumbnail;
		public BonusEffect bonusEffect;
		public CardInstruction cardInstruction;

		public bool canRedeploy;
		public bool canReinforce;
		public bool canBeDefeated;
		public bool useThreatMultiplier;
		public Factions faction;
		public List<CampaignSkill> heroSkills;

		public CustomToon()
		{
		}
	}
}
