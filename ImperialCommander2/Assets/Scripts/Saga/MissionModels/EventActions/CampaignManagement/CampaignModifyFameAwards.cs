namespace Saga
{
	public class CampaignModifyFameAwards : EventAction
	{
		public int fameToAdd { get; set; } = 0;
		public int awardsToAdd { get; set; } = 0;

		public CampaignModifyFameAwards()
		{

		}
	}
}
