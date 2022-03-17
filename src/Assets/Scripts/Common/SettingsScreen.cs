using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SettingsScreen : MonoBehaviour
{
	public CanvasGroup cg;
	public Image fader;
	public Toggle musicToggle, soundToggle, bloomToggle, vignetteToggle;
	public Sound sound;
	public GameObject returnButton;
	public VolumeProfile volume;
	public SettingsLanguageController languageController;

	Action<SettingsCommand> closeAction;

	public void Show( Action<SettingsCommand> a, bool fromTitle = false )
	{
		closeAction = a;
		//remove return to title button
		returnButton.SetActive( !fromTitle );

		gameObject.SetActive( true );
		fader.color = new Color( 0, 0, 0, 0 );
		fader.DOFade( .95f, .5f );
		cg.DOFade( 1, .5f );
		transform.GetChild( 0 ).localScale = new Vector3( .85f, .85f, .85f );
		transform.GetChild( 0 ).DOScale( 1, .5f ).SetEase( Ease.OutExpo );

		musicToggle.isOn = PlayerPrefs.GetInt( "music" ) == 1;
		soundToggle.isOn = PlayerPrefs.GetInt( "sound" ) == 1;
		bloomToggle.isOn = PlayerPrefs.GetInt( "bloom" ) == 1;
		vignetteToggle.isOn = PlayerPrefs.GetInt( "vignette" ) == 1;

		//set the translated UI strings
		languageController.SetTranslatedUI();
	}

	public void OnOK()
	{
		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Click );
		PlayerPrefs.SetInt( "music", musicToggle.isOn ? 1 : 0 );
		PlayerPrefs.SetInt( "sound", soundToggle.isOn ? 1 : 0 );
		PlayerPrefs.SetInt( "bloom", bloomToggle.isOn ? 1 : 0 );
		PlayerPrefs.SetInt( "vignette", vignetteToggle.isOn ? 1 : 0 );
		PlayerPrefs.Save();

		FindObjectOfType<Sound>().PlaySound( FX.Click );

		fader.DOFade( 0, .5f ).OnComplete( () =>
		{
			gameObject.SetActive( false );
		} );
		cg.DOFade( 0, .2f );
		transform.GetChild( 0 ).DOScale( .85f, .5f ).SetEase( Ease.OutExpo );
	}

	public void OnToggle( Toggle t )
	{
		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Click );
		if ( t.name.ToLower() == "music toggle" )
		{
			if ( t.isOn )
				sound.PlayMusic();
			else
				sound.StopMusic();
		}
		else if ( t.name.ToLower() == "sound toggle" )
		{
			if ( t.isOn )
				sound.StartAmbientSound();
			else
				sound.StopAmbientSound();
			PlayerPrefs.SetInt( "sound", soundToggle.isOn ? 1 : 0 );
		}
		else if ( t.name.ToLower() == "bloom toggle" )
		{
			if ( volume.TryGet<Bloom>( out var bloom ) )
				bloom.active = t.isOn;
		}
		else if ( t.name.ToLower() == "vignette toggle" )
			if ( volume.TryGet<Vignette>( out var vig ) )
				vig.active = t.isOn;
	}

	public void OnQuit()
	{
		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Click );
		closeAction?.Invoke( SettingsCommand.Quit );
	}

	public void OnReturnTitles()
	{
		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Click );
		fader.DOFade( 0, .5f ).OnComplete( () =>
		{
			gameObject.SetActive( false );
			closeAction?.Invoke( SettingsCommand.ReturnTitles );
		} );
		cg.DOFade( 0, .2f );
		transform.GetChild( 0 ).DOScale( .85f, .5f ).SetEase( Ease.OutExpo );
	}
}
