using System.Collections.Generic;

namespace Saga
{
	public class QuestionPrompt : EventAction
	{
		public string theText;
		public List<ButtonAction> buttonList { get; set; } = new List<ButtonAction>();

		public QuestionPrompt()
		{

		}
	}
}
