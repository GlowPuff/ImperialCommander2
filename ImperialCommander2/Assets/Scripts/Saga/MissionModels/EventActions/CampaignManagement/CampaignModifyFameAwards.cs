namespace Saga
{
	public class CampaignModifyFameAwards : EventAction
	{
		public int fameToAdd { get; set; } = 0;
		public int awardsToAdd { get; set; } = 0;
		public string customIdentifier { get; set; } = "";

		public CampaignModifyFameAwards()
		{

		}
	}
}
