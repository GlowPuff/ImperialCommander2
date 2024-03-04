namespace Saga
{
	public class CampaignSetNextMission : EventAction
	{
		public string customMissionID { get; set; } = "";
		public string missionID { get; set; } = "Core 1";

		public CampaignSetNextMission()
		{

		}
	}
}
