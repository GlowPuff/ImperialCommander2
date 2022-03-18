using DG.Tweening;
using System.Linq;
using UnityEngine;

public class Sound : MonoBehaviour
{
	public AudioSource source;
	public AudioSource musicSource;
	public AudioSource ambientSource;
	public AudioClip[] clips;
	public float maxMusicVolume = .5f;

	public void PlaySound( FX sound )
	{
		if ( PlayerPrefs.GetInt( "sound" ) == 1 )
			source.PlayOneShot( clips[(int)sound] );
	}

	public void playDeploymentSound( string id )
	{
		var depsnd = DataStore.deploymentSounds.Where( x => x.idMatch.Contains( id ) ).FirstOr( null );
		if ( depsnd != null && PlayerPrefs.GetInt( "sound" ) == 1 )
		{
			var snd = depsnd.sounds[Random.Range( 0, depsnd.sounds.Length )];
			var clip = Resources.Load<AudioClip>( "sounds/" + snd );
			source.PlayOneShot( clip );
		}
	}

	/// <summary>
	/// call on screen Start() to check and play/not play music
	/// </summary>
	public void CheckAudio()
	{
		musicSource.volume = maxMusicVolume;
		if ( PlayerPrefs.GetInt( "music" ) == 0 )
			musicSource.Stop();
		if ( PlayerPrefs.GetInt( "sound" ) == 0 )
			ambientSource.Stop();
	}

	public void PlayMusic()
	{
		musicSource.Play();
	}

	public void StopMusic()
	{
		musicSource.Stop();
	}

	public void StartAmbientSound()
	{
		ambientSource.Play();
	}

	public void StopAmbientSound()
	{
		ambientSource.Stop();
	}

	public void FadeOutMusic()
	{
		musicSource.DOFade( 0, 1 );
	}
}
