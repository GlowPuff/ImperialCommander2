using System;
using System.Collections.Generic;

namespace Saga
{
	public class InputPrompt : EventAction
	{
		public string failText;
		public string theText;
		public Guid failTriggerGUID;
		public Guid failEventGUID;
		public List<InputRange> inputList = new List<InputRange>();

		public InputPrompt()
		{

		}
	}
}
