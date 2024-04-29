using System.Collections.Generic;

namespace Saga
{
	public class AddCampaignReward : EventAction
	{
		//store the ID
		public List<string> campaignItems { get; set; }
		public List<string> campaignRewards { get; set; }
		public List<string> earnedVillains { get; set; }
		public List<string> earnedAllies { get; set; }
		public int medpacsToModify { get; set; }
		public int threatToModify { get; set; }

		public AddCampaignReward()
		{

		}
	}
}
