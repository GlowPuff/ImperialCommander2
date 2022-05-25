using System.Collections.Generic;

namespace Saga
{
	public class ModifyVariable : EventAction
	{
		public List<TriggerModifier> triggerList { get; set; } = new List<TriggerModifier>();

		public ModifyVariable()
		{

		}
	}
}
