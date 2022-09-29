using System;

namespace Saga
{
	public class CampaignStructure
	{
		public MissionType missionType;
		public string missionID;
		public int threatLevel;
		public string[] itemTier;
		public string expansionCode;
		public bool isAgendaMission;

		//set in campaign UI - not loaded from campaign structure JSONs
		public bool isItemChecked = false;
		public bool isForced = false;
		public AgendaType agendaType = AgendaType.NotSet;
		public bool isCustom = false;//is part of a custom campaign
		public ProjectItem projectItem;
		public Guid GUID;

		public CampaignStructure()
		{
			GUID = Guid.NewGuid();
			projectItem = new ProjectItem();
		}
	}
}