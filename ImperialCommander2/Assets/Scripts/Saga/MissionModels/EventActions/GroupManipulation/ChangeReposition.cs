using System.Collections.Generic;

namespace Saga
{
	public class ChangeReposition : EventAction
	{
		public string theText;
		public bool useSpecific;
		public List<DCPointer> repoGroups = new List<DCPointer>();

		public ChangeReposition()
		{

		}
	}
}
