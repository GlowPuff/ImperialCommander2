using System;

namespace Saga
{
	public static class RunningCampaign
	{
		private static CampaignModifiers _campaignModifiers;

		public static Guid sagaCampaignGUID = Guid.Empty;
		public static string expansionCode;
		public static CampaignStructure campaignStructure;
		public static CampaignModifiers campaignModifiers
		{
			get
			{
				if ( _campaignModifiers == null )
				{
					_campaignModifiers = new CampaignModifiers();
				}
				return _campaignModifiers;
			}
		}

		public static void Reset()
		{
			sagaCampaignGUID = Guid.Empty;
			expansionCode = "";
			campaignStructure = null;
			_campaignModifiers = new CampaignModifiers();
		}

		public static void ResetCampaignModifiers()
		{
			_campaignModifiers = new CampaignModifiers();
		}
	}

	public class CampaignModifiers
	{
		public int modifyXP, modifyCredits, modifyFame, modifyAwards;
		public string missionID, customMissionID;
	}
}
