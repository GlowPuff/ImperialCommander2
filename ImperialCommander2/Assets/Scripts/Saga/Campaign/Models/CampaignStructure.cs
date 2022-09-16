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

		//campaign UI - not in json
		public bool isItemChecked = false;
		public bool isForced = false;
	}
}