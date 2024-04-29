using System;
using System.Collections.Generic;

namespace Saga
{
	public class MissionProperties
	{
		public string missionName, missionID, fixedAlly, bannedAlly, missionInfo, specificAlly, specificHero, priorityOther, missionDescription, campaignName, startingObjective, additionalMissionInfo;
		public bool optionalDeployment, factionImperial, factionMercenary;
		public YesNoAll useFixedAlly, useBannedAlly;
		public CustomInstructionType customInstructionType;
		public PriorityTargetType priorityTargetType;
		public Guid startingEvent;
		public MissionType missionType;
		public ChangeReposition changeRepositionOverride;
		public List<MissionSubType> missionSubType;
		public List<string> bannedGroups = new List<string>();
		public List<string> multipleBannedAllies = new List<string>();

		//Mission format 22
		public string customMissionIdentifier = Guid.Empty.ToString();
		public int roundLimit = -1;
		public Guid roundLimitEvent = Guid.Empty;
		public bool useAlternateEventSystem = false;
	}
}
