using System;
using System.Collections.Generic;

namespace Saga
{
	public class EntityGroup
	{
		public string name;
		public Guid GUID, triggerGUID;
		public bool repeateable;
		public List<Guid> missionEntities = new List<Guid>();
	}
}
