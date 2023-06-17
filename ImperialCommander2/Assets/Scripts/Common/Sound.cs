using System.Linq;
using DG.Tweening;
using UnityEngine;

public class Sound : MonoBehaviour
{
	public AudioSource source;
	public AudioSource musicSource;
	public AudioSource ambientSource;
	public AudioClip[] clips;
	public AudioClip[] defeatedClips;
	public float maxMusicVolume = .5f;

	public bool soundEnabled { get { return PlayerPrefs.GetInt( "sound" ) == 1; } }
	public bool musicEnabled { get { return PlayerPrefs.GetInt( "music" ) == 1; } }

	public void PlaySound( FX sound )
	{
		if ( PlayerPrefs.GetInt( "sound" ) == 1 )
			source.PlayOneShot( clips[(int)sound] );
	}

	public void PlaySound( int clipIndex )
	{
		//if ( PlayerPrefs.GetInt( "sound" ) == 1 )
		if ( clipIndex <= clips.Length - 1 )
			source.PlayOneShot( clips[clipIndex] );
	}

	public void PlayDefeatedSound()
	{
		if ( PlayerPrefs.GetInt( "sound" ) == 1 && defeatedClips.Length > 0 )
			source.PlayOneShot( defeatedClips[Random.Range( 0, defeatedClips.Length )] );
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
		if ( PlayerPrefs.GetInt( "ambient" ) == 0 )
			ambientSource.Stop();
	}

	public void PlayMusic()
	{
		musicSource.volume = .5f;
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
		musicSource.DOFade( 0, 1 ).OnComplete( () => StopMusic() );
	}
}
