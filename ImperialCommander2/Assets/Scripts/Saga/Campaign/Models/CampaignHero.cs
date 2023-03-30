using System.Collections.Generic;

namespace Saga
{
	public class CampaignHero
	{
		public string heroID;
		public List<CampaignItem> campaignItems = new List<CampaignItem>();
		public List<CampaignSkill> campaignSkills = new List<CampaignSkill>();
		public int xpAmount;
	}
}
