using UnityEngine;

public class WorldSpinner : MonoBehaviour
{
	public Transform world;
	public float lowx = -9f, highx = 15f, multiplierx = .25f;
	public float lowy = -14f, highy = 9f, multipliery = .5f;
	public float lowz = -11f, highz = 9f, multiplierz = 1f;
	public float scalar = 1f;

	void Update()
	{
		float xScalar = GlowEngine.SineAnimation(
			 lowx,
			 highx,
			multiplierx );

		float yScalar = GlowEngine.SineAnimation(
			 lowy,
			 highy,
			multipliery );

		float zScalar = GlowEngine.SineAnimation(
			 lowz,
			 highz,
			 multiplierz );

		world.Rotate( xScalar * Time.deltaTime * scalar, yScalar * Time.deltaTime * scalar, zScalar * Time.deltaTime * scalar );
	}
}
