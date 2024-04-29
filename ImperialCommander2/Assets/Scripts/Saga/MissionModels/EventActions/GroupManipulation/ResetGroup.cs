using System.Collections.Generic;

namespace Saga
{
	public class ResetGroup : EventAction
	{
		public List<DCPointer> groupsToAdd;

		public bool resetAll;

		public ResetGroup()
		{

		}
	}
}
