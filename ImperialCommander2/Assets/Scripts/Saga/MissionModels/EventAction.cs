using System;

namespace Saga
{
	public abstract class EventAction : IEventAction
	{
		public Guid GUID { get; set; }
		public EventActionType eventActionType { get; set; }
		public string displayName { get; set; }

		public EventAction()
		{

		}
	}
}
