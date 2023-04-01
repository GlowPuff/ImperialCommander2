using System;

namespace Saga
{
	public class CustomToon
	{
		//these properties don't change, even when copying from another Deployment Group
		public Guid customCharacterGUID { get; set; }

		public string cardName;
		public string cardID;

		public string groupAttack;
		public string groupDefense;
		public string outlineColor;
		public GroupPriorityTraits groupPriorityTraits;

		public DeploymentCard deploymentCard;

		public CharacterType characterType;
		public Thumbnail thumbnail;
		public string bonuses;
		public CardInstruction cardInstruction;

		public bool canRedeploy;
		public bool canReinforce;
		public bool canBeDefeated;
		public bool useThreatMultiplier;
		public Factions faction;

		public CustomToon()
		{
		}
	}
}
