using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SettingsScreen : MonoBehaviour
{
	public CanvasGroup cg;
	public Image fader;
	public Toggle musicToggle, soundToggle, bloomToggle, vignetteToggle, ambientToggle, closeWindowToggle, zoomToggle, viewToggle;
	public Sound sound;
	public GameObject returnButton;
	public VolumeProfile volume;
	public SettingsLanguageController languageController;
	public GameObject audioPanel, gfxPanel, uiPanel;
	public Toggle audioToggle;

	Action<SettingsCommand> quitAction;

	public void Show( Action<SettingsCommand> onQuit )
	{
		quitAction = onQuit;
		//remove return to title button only if we're already on the title screen
		returnButton.SetActive( FindObjectOfType<TitleController>() == null );

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
		ambientToggle.isOn = PlayerPrefs.GetInt( "ambient" ) == 1;
		closeWindowToggle.isOn = PlayerPrefs.GetInt( "closeWindowToggle" ) == 1;
		zoomToggle.isOn = PlayerPrefs.GetInt( "zoombuttons" ) == 1;
		viewToggle.isOn = PlayerPrefs.GetInt( "viewToggle" ) == 1;

		//set the translated UI strings
		languageController.SetTranslatedUI();

		audioToggle.isOn = true;
		audioPanel.SetActive( true );
		gfxPanel.SetActive( false );
		uiPanel.SetActive( false );
	}

	public void OnOK()
	{
		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Click );
		PlayerPrefs.SetInt( "music", musicToggle.isOn ? 1 : 0 );
		PlayerPrefs.SetInt( "sound", soundToggle.isOn ? 1 : 0 );
		PlayerPrefs.SetInt( "bloom", bloomToggle.isOn ? 1 : 0 );
		PlayerPrefs.SetInt( "vignette", vignetteToggle.isOn ? 1 : 0 );
		PlayerPrefs.SetInt( "ambient", ambientToggle.isOn ? 1 : 0 );
		PlayerPrefs.SetInt( "closeWindowToggle", closeWindowToggle.isOn ? 1 : 0 );
		PlayerPrefs.SetInt( "zoombuttons", zoomToggle.isOn ? 1 : 0 );
		PlayerPrefs.SetInt( "viewToggle", viewToggle.isOn ? 1 : 0 );

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
				sound.FadeOutMusic();
		}
		else if ( t.name.ToLower() == "sound toggle" )
		{
			PlayerPrefs.SetInt( "sound", t.isOn ? 1 : 0 );
		}
		else if ( t.name.ToLower() == "bloom toggle" )
		{
			if ( volume.TryGet<Bloom>( out var bloom ) )
				bloom.active = t.isOn;
		}
		else if ( t.name.ToLower() == "vignette toggle" )
		{
			if ( volume.TryGet<Vignette>( out var vig ) )
				vig.active = t.isOn;
		}
		else if ( t.name.ToLower() == "ambient toggle" )
		{
			if ( t.isOn )
				sound.StartAmbientSound();
			else
				sound.StopAmbientSound();
		}
		else if ( t.name.ToLower() == "close window toggle" )
		{
			PlayerPrefs.SetInt( "closeWindowToggle", t.isOn ? 1 : 0 );
		}
		else if ( t.name.ToLower() == "zoom toggle" )
		{
			PlayerPrefs.SetInt( "zoombuttons", t.isOn ? 1 : 0 );
			var c = FindObjectOfType<Saga.SagaController>();
			if ( c != null )
				c.OnZoomBarToggle( t.isOn );
		}
		else if ( t.name.ToLower() == "view toggle" )
		{
			PlayerPrefs.SetInt( "viewToggle", t.isOn ? 1 : 0 );
			var c = FindObjectOfType<Saga.CameraController>();
			if ( c != null )
			{
				c.CameraViewToggle( t.isOn ? Saga.CameraView.TopDown : Saga.CameraView.Normal );
			}
		}
	}

	public void OnQuit()
	{
		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Click );
		quitAction?.Invoke( SettingsCommand.Quit );
	}

	public void OnReturnTitles()
	{
		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Click );
		fader.DOFade( 0, .5f ).OnComplete( () =>
		{
			gameObject.SetActive( false );
			quitAction?.Invoke( SettingsCommand.ReturnTitles );
		} );
		cg.DOFade( 0, .2f );
		transform.GetChild( 0 ).DOScale( .85f, .5f ).SetEase( Ease.OutExpo );
	}

	public void OnAudioTab()
	{
		audioPanel.SetActive( true );
		gfxPanel.SetActive( false );
		uiPanel.SetActive( false );
	}

	public void OnGraphicsTab()
	{
		audioPanel.SetActive( false );
		gfxPanel.SetActive( true );
		uiPanel.SetActive( false );
	}

	public void OnUITab()
	{
		audioPanel.SetActive( false );
		gfxPanel.SetActive( false );
		uiPanel.SetActive( true );
	}
}
