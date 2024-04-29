namespace Saga
{
	public class CampaignModifyCredits : EventAction
	{
		public int creditsToModify { get; set; } = 0;
		public bool multiplyByHeroCount { get; set; } = false;

		public CampaignModifyCredits()
		{

		}
	}
}
