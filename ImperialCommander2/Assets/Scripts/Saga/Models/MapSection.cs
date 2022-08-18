using System;
using System.Collections.Generic;

namespace Saga
{
	public class MapSection
	{
		public string name;
		public Guid GUID;
		public bool canRemove;
		public bool isActive;
		public bool invisibleUntilActivated;

		public List<Trigger> triggers;
		public List<MissionEvent> missionEvents;
		public List<MapTile> mapTiles;
	}
}
