using System.Linq;
using DG.Tweening;
using Saga;
using UnityEngine;

public class Sound : MonoBehaviour
{
	public AudioSource source;
	public AudioSource musicSource;
	public AudioSource ambientSource;
	public AudioClip[] clips;
	public AudioClip[] defeatedClips;
	public float maxMusicVolume = .5f;
	public string AmbientSound { get => ambientSource.clip.name; }

	public bool soundEnabled { get { return PlayerPrefs.GetInt( "sound" ) == 1; } }
	public bool musicEnabled { get { return PlayerPrefs.GetInt( "music" ) == 1; } }

	public void SetAudioVolumes()
	{
		musicSource.volume = PlayerPrefs.GetInt( "musicVolume" ) / 10f;
		ambientSource.volume = PlayerPrefs.GetInt( "ambientVolume" ) / 10f;
		source.volume = PlayerPrefs.GetInt( "soundVolume" ) / 10f;
	}

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
			source.volume = PlayerPrefs.GetInt( "soundVolume" ) / 10f;
			source.PlayOneShot( clip );
		}
	}

	/// <summary>
	/// call on screen Start() to check and play/not play music
	/// </summary>
	//public void CheckAudio()
	//{
	//	musicSource.volume = PlayerPrefs.GetInt( "musicVolume" ) / 10f;
	//	ambientSource.volume = PlayerPrefs.GetInt( "ambientVolume" ) / 10f;

	//	if ( PlayerPrefs.GetInt( "music" ) == 0 )
	//		musicSource.Stop();
	//	if ( PlayerPrefs.GetInt( "ambient" ) == 0 )
	//		ambientSource.Stop();
	//}

	/// <summary>
	/// Call on startup of all screens (Start) to set volumes and start the music and menu ambient sound
	/// </summary>
	public void PlayMusicAndMenuAmbient()
	{
		SetAudioVolumes();

		if ( PlayerPrefs.GetInt( "music" ) == 1 )
			PlayMusic( PlayerPrefs.GetInt( "musicVolume" ) );
		//only start the menu ambient sound if we're NOT in a Mission
		if ( PlayerPrefs.GetInt( "ambient" ) == 1 && !FindObjectOfType<SagaController>() )
			StartAmbientSound( BiomeType.Menu );
	}

	/// <summary>
	/// volume = 1-10
	/// </summary>
	public void PlayMusic( int volume )
	{
		if ( !musicSource.isPlaying )
		{
			musicSource.volume = volume / 10f;
			musicSource.Play();
		}
	}

	public void StopMusic()
	{
		musicSource.Stop();
	}

	/// <summary>
	/// starts a sound IF it's not playing, using the PlayerPrefs ambient volume
	/// </summary>
	public void StartAmbientSound( BiomeType bt = BiomeType.Menu )
	{
		if ( bt != BiomeType.None
			&& !ambientSource.isPlaying
			&& PlayerPrefs.GetInt( "ambient" ) == 1 )
		{
			Debug.Log( $"StartAmbientSound()::playing [{bt}]" );
			ambientSource.clip = Resources.Load<AudioClip>( $"Sounds/ambient/{bt}" );
			if ( ambientSource.clip != null )
			{
				ambientSource.clip.name = bt.ToString();
				ambientSource.volume = PlayerPrefs.GetInt( "ambientVolume" ) / 10f;
				ambientSource.Play();
			}
		}
	}

	public void StopAmbientSound()
	{
		ambientSource.Stop();
	}

	/// <summary>
	/// Changes the ambient sound, using the PlayerPrefs ambient volume
	/// </summary>
	public void ChangeAmbient( BiomeType biomeType )
	{
		//only play sound if it's not aleady playing
		if ( biomeType != BiomeType.None
			&& PlayerPrefs.GetInt( "ambient" ) == 1
			&& ambientSource.clip?.name != biomeType.ToString() )
		{
			Debug.Log( $"ChangeAmbient()::changing ambient sound to [{biomeType}]" );
			ambientSource.DOFade( 0, 1 ).OnComplete( () =>
			{
				ambientSource.Stop();
				ambientSource.clip = Resources.Load<AudioClip>( $"Sounds/ambient/{biomeType}" );
				if ( ambientSource.clip != null )
				{
					ambientSource.clip.name = biomeType.ToString();
					ambientSource.DOFade( PlayerPrefs.GetInt( "ambientVolume" ) / 10f, 1 );
					ambientSource.Play();
				}
			} );
		}
		else if ( biomeType == BiomeType.None )
		{
			//if it's None, keep playing current sound
			//StopAmbientSound();
			//ambientSource.clip.name = "none";
		}
		else
			Debug.Log( $"ChangeAmbient()::ambient sound already [{biomeType}] or ambient is OFF" );
	}

	public void FadeOutMusic()
	{
		musicSource.DOFade( 0, 1 ).OnComplete( () => StopMusic() );
	}

	/// <summary>
	/// value = 1-10
	/// </summary>
	public void SetMusicVolume( int value )
	{
		musicSource.volume = value / 10f;
	}

	/// <summary>
	/// value = 1-10
	/// </summary>
	public void SetAmbientVolume( int value )
	{
		ambientSource.volume = value / 10f;
	}

	/// <summary>
	/// value = 1-10
	/// </summary>
	public void SetSoundVolume( int value )
	{
		source.volume = value / 10f;
	}
}
