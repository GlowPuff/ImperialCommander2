using System.Collections.Generic;

public class EventList
{
	public List<CardEvent> events;
}

public class CardEvent
{
	public string eventName, eventID, eventFlavor, eventRule;
	public List<string> content;
}