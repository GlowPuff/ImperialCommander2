namespace Saga
{
	public class ModifyThreat : EventAction
	{
		public ThreatAction threatAction;
		public int fixedValue;
		public int threatValue;
		public ThreatModifierType threatModifierType;

		public ModifyThreat()
		{

		}
	}
}
