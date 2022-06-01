using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Saga
{
	public class MissionEvent
	{
		public Guid GUID { get; set; }
		public bool isGlobal;
		public string name;
		public string eventText;
		public string allyDefeated;
		public string heroWounded;
		public string heroWithdraws;
		public string activationOf;
		public int startOfRound;
		public int endOfRound;
		public bool useStartOfRound;
		public bool useEndOfRound;
		public bool useStartOfEachRound;
		public bool useEndOfEachRound;
		public bool useAllGroupsDefeated;
		public bool useAllHeroesWounded;
		public bool useAllyDefeated;
		public bool useHeroWounded;
		public bool useHeroWithdraws;
		public bool useAnyHeroWounded;
		public bool useActivation;
		public bool behaviorAll;
		public bool isRepeatable;
		public bool isEndOfCurrentRound;

		//upkeep
		public bool hasActivatedThisRound { get; set; } = false;
		public bool usesEnd
		{
			get { return useEndOfEachRound || isEndOfCurrentRound || useEndOfRound || useStartOfEachRound || useStartOfRound; }
		}

		public List<TriggeredBy> additionalTriggers { get; set; }

		[JsonConverter( typeof( EventActionConverter ) )]
		public List<IEventAction> eventActions { get; set; }
	}
}