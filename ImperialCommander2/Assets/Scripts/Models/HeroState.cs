using UnityEngine;

public class HeroState
{
	public bool[] hasActivated;
	public HeroHealth heroHealth;
	public bool isHealthy;

	public void Init( int playerCount )
	{
		Debug.Log( $"Init Hero State with {playerCount} heroes" );
		heroHealth = HeroHealth.Healthy;
		isHealthy = true;
		hasActivated = new bool[2] { false, false };
	}
}
