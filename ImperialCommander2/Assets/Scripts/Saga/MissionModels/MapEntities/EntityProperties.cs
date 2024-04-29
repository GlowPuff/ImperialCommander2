using System.Collections.Generic;

namespace Saga
{
	public class EntityProperties
	{
		public string name { get; set; }
		public bool isActive { get; set; }
		public string theText { get; set; }
		public string entityColor;
		public List<ButtonAction> buttonActions { get; set; } = new List<ButtonAction>();

		public EntityProperties()
		{
			isActive = true;
		}
	}
}