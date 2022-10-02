public class HeroState
{
	public bool[] hasActivated;
	public bool isHealthy
	{
		get
		{
			return !isWounded && !isDefeated;
		}
	}
	public bool isWounded;
	public bool isDefeated;//a defeated hero is also marked as wounded

	public void Init()
	{
		//Debug.Log( $"Init Hero State with {playerCount} heroes" );
		//heroHealth = HeroHealth.Healthy;
		isWounded = false;
		isDefeated = false;
		hasActivated = new bool[2] { false, false };
	}
}
