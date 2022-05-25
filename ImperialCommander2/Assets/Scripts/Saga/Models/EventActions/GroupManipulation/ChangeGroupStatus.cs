using System.Collections.Generic;

namespace Saga
{
	public class ChangeGroupStatus : EventAction
	{
		public List<DCPointer> readyGroups { get; set; } = new List<DCPointer>();
		public List<DCPointer> exhaustGroups { get; set; } = new List<DCPointer>();

		public ChangeGroupStatus()
		{

		}
	}
}
