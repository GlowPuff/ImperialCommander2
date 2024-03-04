using System;

namespace Saga
{
	public class SetCountdown : EventAction
	{
		public int countdownTimer { get; set; } = 0;
		public Guid eventGUID { get; set; } = Guid.Empty;
		public Guid triggerGUID { get; set; } = Guid.Empty;
		public bool showPlayerCountdown { get; set; } = false;//whether to show a number in IC2 so players know how many rounds remain

		public SetCountdown()
		{

		}
	}
}