using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WarpManager : MonoBehaviour
{
	public Camera theCamera;
	public GameObject warpEffect;

	Sound sound;

	void Awake()
	{
		sound = FindObjectOfType<Sound>();
		sound.CheckAudio();

		Warp();
	}

	public void Warp()
	{
		float timer = 3;

		sound.PlaySound( 1 );
		sound.PlaySound( 2 );
		GlowTimer.SetTimer( 1.5f, () => warpEffect.SetActive( true ) );
		GlowTimer.SetTimer( 5, () =>
		{
			DOTween.To( () => theCamera.fieldOfView, x => theCamera.fieldOfView = x, 0, .25f )
			.OnComplete( () =>
			{
				//all effects/music finish, load the mission
				GlowTimer.SetTimer( timer, () =>
				{
					SceneManager.LoadScene( "Saga" );
				} );
			} );
		} );
	}
}
