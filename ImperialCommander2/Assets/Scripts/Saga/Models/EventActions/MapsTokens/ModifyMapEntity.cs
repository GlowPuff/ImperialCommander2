using System.Collections.Generic;

namespace Saga
{
	public class ModifyMapEntity : EventAction
	{
		public List<EntityModifier> entitiesToModify = new List<EntityModifier>();

		public ModifyMapEntity()
		{

		}
	}
}
