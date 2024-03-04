using System.Collections.Generic;

namespace Saga
{
	public class QuestionPrompt : EventAction
	{
		public string theText;
		public bool includeCancel;
		public List<ButtonAction> buttonList { get; set; } = new List<ButtonAction>();

		public QuestionPrompt()
		{

		}
	}
}
