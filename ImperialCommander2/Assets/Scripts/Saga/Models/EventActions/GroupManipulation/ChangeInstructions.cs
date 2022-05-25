using System.Collections.Generic;

namespace Saga
{
	public class ChangeInstructions : EventAction
	{
		public string theText;
		public CustomInstructionType instructionType;
		public List<DCPointer> groupsToAdd = new List<DCPointer>();

		public ChangeInstructions()
		{

		}
	}
}
