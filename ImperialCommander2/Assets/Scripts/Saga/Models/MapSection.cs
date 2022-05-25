using System;
using System.Collections.ObjectModel;

namespace Saga
{
	public class MapSection
	{
		public string name;
		public Guid GUID;
		public bool canRemove;
		public bool isActive;
		public bool invisibleUntilActivated;

		public ObservableCollection<Trigger> triggers;
		public ObservableCollection<MissionEvent> missionEvents;
		public ObservableCollection<MapTile> mapTiles;
	}
}
