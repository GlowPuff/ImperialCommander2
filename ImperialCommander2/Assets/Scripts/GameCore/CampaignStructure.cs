using System;
using Newtonsoft.Json;

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

		//if this isn't empty, it's used instead of the missionID
		//ONLY used with custom missions inside a custom campaign
		//assigned by the 'set next mission' event action and SagaCampaign.SetNextStoryMission()
		public string customMissionID;

		//below properties are set in campaign UI - not loaded from campaign structure JSONs
		public bool isItemChecked = false;
		public bool isForced = false;
		public AgendaType agendaType = AgendaType.NotSet;
		public MissionSource missionSource;
		public ProjectItem projectItem;
		public Guid structureGUID;
		public Guid packageGUID;
		public bool canModify = true;

		public CampaignStructure()
		{
			structureGUID = Guid.NewGuid();
			customMissionID = "";
			projectItem = new ProjectItem();
		}

		public CampaignStructure UniqueCopy()
		{
			string json = JsonConvert.SerializeObject( this );
			return JsonConvert.DeserializeObject<CampaignStructure>( json );
		}
	}
}