using System;
using System.Collections.Generic;

namespace Saga
{
	public class MissionProperties
	{
		public string missionName, missionID, fixedAlly, bannedAlly, missionInfo, specificAlly, specificHero, priorityOther, missionDescription, campaignName, startingObjective, additionalMissionInfo;
		public bool optionalDeployment, factionImperial, factionMercenary;
		public YesNoAll useFixedAlly, useBannedAlly, banAllAllies;
		public CustomInstructionType customInstructionType;
		//public ThreatModifierType initialThreatType;
		public PriorityTargetType priorityTargetType;
		public Guid startingEvent;
		//public int initialThreatModifier, initialThreatMultiplier;
		public MissionType missionType;
		public ChangeReposition changeRepositionOverride;
		public List<MissionSubType> missionSubType;
		public List<string> bannedGroups = new List<string>();
	}
}
