using System;

namespace Saga
{
	public static class RunningCampaign
	{
		public static SagaCampaign sagaCampaign;
		public static Guid sagaCampaignGUID = Guid.Empty;
		public static string expansionCode;
		public static CampaignStructure campaignStructure;

		public static void Reset()
		{
			sagaCampaignGUID = Guid.Empty;
			expansionCode = "";
			campaignStructure = null;
			sagaCampaign = null;
		}
	}
}
