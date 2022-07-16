using System.Collections.Generic;

namespace Saga
{
	public class RemoveGroup : EventAction
	{
		public List<DCPointer> groupsToRemove;
		public List<DCPointer> allyGroupsToRemove;

		public RemoveGroup()
		{

		}
	}
}
