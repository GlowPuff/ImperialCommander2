using System;

namespace Saga
{
	public class SetCountdown : EventAction
	{
		public string countdownTimerName;
		public int countdownTimer { get; set; } = 0;
		public Guid eventGUID { get; set; } = Guid.Empty;
		public Guid triggerGUID { get; set; } = Guid.Empty;
		public bool showPlayerCountdown { get; set; } = false;//whether to show a number in IC2 so players know how many rounds remain

		//This property is only used by IC2, and is not part of the normal model ICE uses
		public int endRound = 0;

		public SetCountdown()
		{

		}
	}
}