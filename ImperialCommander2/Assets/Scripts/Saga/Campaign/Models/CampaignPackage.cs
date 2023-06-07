using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Saga
{
	public class CampaignPackage
	{
		public Guid GUID;
		public string campaignName;
		public string campaignInstructions;
		public List<CampaignMissionItem> campaignMissionItems { get; set; } = new List<CampaignMissionItem>();
		public List<CampaignStructure> campaignStructure { get; set; } = new List<CampaignStructure>();

		[JsonIgnore]
		public byte[] iconBytesBuffer = new byte[0];

		public CampaignPackage() { }
	}

	public class CampaignMissionItem
	{
		public Guid GUID;//GUID of this object, not the mission
		public Guid missionGUID;
		public string missionName;

		//store the actual mission for packing as an individual file later, but don't serialize it here
		[JsonIgnore]
		public Mission mission { get; set; }
	}
}
