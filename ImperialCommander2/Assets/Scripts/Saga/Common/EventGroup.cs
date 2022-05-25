using System;
using System.Collections.Generic;

namespace Saga
{
	public class EventGroup
	{
		public string name;
		public Guid GUID;
		public bool repeateable, isUnique;
		public Guid triggerGUID;
		public List<Guid> missionEvents = new List<Guid>();
	}
}
