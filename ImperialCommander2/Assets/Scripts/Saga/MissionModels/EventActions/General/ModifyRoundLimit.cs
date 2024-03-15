using System;

namespace Saga
{
	public class ModifyRoundLimit : EventAction
	{
		public int roundLimitModifier = 0;
		public Guid eventGUID = Guid.Empty;
		public bool disableRoundLimit = false;
		public int setLimitTo = 0;
		public bool setRoundLimit = false;

		public ModifyRoundLimit()
		{

		}
	}
}
