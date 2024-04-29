using System;

namespace Saga
{
	public class Trigger
	{
		public string name;
		public Guid GUID;
		public bool isGlobal;
		public bool useReset;
		public int initialValue;
		public int maxValue;
		public Guid eventGUID;
	}

	public class TriggerState //: ICloneable
	{
		public int currentValue;
		public Trigger trigger;

		public TriggerState( Trigger t )
		{
			trigger = t;
		}

		public void ResetValue()
		{
			currentValue = 0;
		}

		//public object Clone()
		//{
		//	TriggerState ts = new TriggerState( trigger );
		//	ts.currentValue = currentValue;
		//	return ts;
		//}
	}

	public class TriggeredBy
	{
		public string triggerName;
		//guid of the trigger to listen to
		public Guid triggerGUID;
		//fire the event when trigger is this value
		public int triggerValue;
	}

	public class TriggerModifier
	{
		public string triggerName;
		public Guid triggerGUID;
		public int setValue, modifyValue;
	}
}