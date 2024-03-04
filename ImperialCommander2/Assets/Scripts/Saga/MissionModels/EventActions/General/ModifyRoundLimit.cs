namespace Saga
{
	public class ModifyRoundLimit : EventAction
	{
		public int roundLimitModifier { get; set; } = 0;

		public ModifyRoundLimit()
		{

		}
	}
}
