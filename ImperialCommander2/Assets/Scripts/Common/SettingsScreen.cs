using System;
using DG.Tweening;
using Saga;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SettingsScreen : MonoBehaviour
{
	public CanvasGroup cg;
	public Image fader, regularEnemyButton1, regularEnemyButton2, eliteEnemySingleButton, eliteEnemyButton1, eliteEnemyButton2, villainButton;
	public Toggle musicToggle, soundToggle, bloomToggle, vignetteToggle, ambientToggle, closeWindowToggle, zoomToggle, viewToggle, roundLimitToggleOn, roundLimitToggleOff, roundLimitToggleDangerous, skipWarpIntroToggle;
	public Sound sound;
	public GameObject returnButton;
	public VolumeProfile volume;
	public SettingsLanguageController languageController;
	public GameObject audioPanel, gfxPanel, uiPanel, colorPanel;
	public Toggle audioToggle;
	public MWheelHandler musicWheelHandler, ambientWheelHandler, soundWheelHandler;
	public BiomeType biomeType;
	public HelpPanel audioHelpPanel;
	public HelpPanel uiHelpPanel;
	public HelpPanel graphicsHelpPanel;
	public HelpPanel colorHelpPanel;
	public GameObject eliteSelectionSingle, eliteSelectionDual;

	Action<SettingsCommand> quitAction;
	Action callbackAction;
	bool toggleBusy;

	public void Show( Action<SettingsCommand> onQuit, BiomeType btype = BiomeType.Menu, Action callback = null )
	{
		quitAction = onQuit;
		biomeType = btype;
		callbackAction = callback;
		//remove return to title button only if we're already on the title screen
		returnButton.SetActive( FindObjectOfType<TitleController>() == null );

		gameObject.SetActive( true );
		fader.color = new Color( 0, 0, 0, 0 );
		fader.DOFade( .95f, .5f );
		cg.DOFade( 1, .5f );
		transform.GetChild( 0 ).localScale = new Vector3( .85f, .85f, .85f );
		transform.GetChild( 0 ).DOScale( 1, .5f ).SetEase( Ease.OutExpo );

		musicWheelHandler.ResetWheeler( PlayerPrefs.GetInt( "musicVolume" ) );
		ambientWheelHandler.ResetWheeler( PlayerPrefs.GetInt( "ambientVolume" ) );
		soundWheelHandler.ResetWheeler( PlayerPrefs.GetInt( "soundVolume" ) );

		musicWheelHandler.wheelValueChangedCallback = () =>
		{
			sound.SetMusicVolume( musicWheelHandler.wheelValue );
		};
		ambientWheelHandler.wheelValueChangedCallback = () =>
		{
			sound.SetAmbientVolume( ambientWheelHandler.wheelValue );
		};
		soundWheelHandler.wheelValueChangedCallback = () =>
		{
			sound.SetSoundVolume( soundWheelHandler.wheelValue );
		};

		toggleBusy = true;//to avoid OnToggle()
		musicToggle.isOn = PlayerPrefs.GetInt( "music" ) == 1;
		soundToggle.isOn = PlayerPrefs.GetInt( "sound" ) == 1;
		bloomToggle.isOn = PlayerPrefs.GetInt( "bloom2" ) == 1;
		vignetteToggle.isOn = PlayerPrefs.GetInt( "vignette" ) == 1;
		ambientToggle.isOn = PlayerPrefs.GetInt( "ambient" ) == 1;
		closeWindowToggle.isOn = PlayerPrefs.GetInt( "closeWindowToggle" ) == 1;
		zoomToggle.isOn = PlayerPrefs.GetInt( "zoombuttons" ) == 1;
		viewToggle.isOn = PlayerPrefs.GetInt( "viewToggle" ) == 1;
		roundLimitToggleOn.isOn = PlayerPrefs.GetInt( "roundLimitToggle" ) == 1;//0=off, 1=on, 2=dangerous
		roundLimitToggleOff.isOn = PlayerPrefs.GetInt( "roundLimitToggle" ) == 0;//0=off, 1=on, 2=dangerous
		roundLimitToggleDangerous.isOn = PlayerPrefs.GetInt( "roundLimitToggle" ) == 2;//0=off, 1=on, 2=dangerous
		skipWarpIntroToggle.isOn = PlayerPrefs.GetInt( "skipWarpIntro" ) == 1;
		toggleBusy = false;

		regularEnemyButton1.color = DataStore.pipColors[PlayerPrefs.GetInt( "defaultRegularEnemyColor1" )].ToColor();
		regularEnemyButton2.color = DataStore.pipColors[PlayerPrefs.GetInt( "defaultRegularEnemyColor2" )].ToColor();
		eliteEnemySingleButton.color = DataStore.pipColors[PlayerPrefs.GetInt( "defaultEliteEnemyColor1" )].ToColor();
		eliteEnemyButton1.color = DataStore.pipColors[PlayerPrefs.GetInt( "defaultEliteEnemyColor1" )].ToColor();
		eliteEnemyButton2.color = DataStore.pipColors[PlayerPrefs.GetInt( "defaultEliteEnemyColor2" )].ToColor();
		villainButton.color = DataStore.pipColors[PlayerPrefs.GetInt( "defaultVillainColor" )].ToColor();

		//set the translated UI strings
		languageController.SetTranslatedUI();

		audioToggle.isOn = true;
		audioPanel.SetActive( true );
		gfxPanel.SetActive( false );
		uiPanel.SetActive( false );
		colorPanel.SetActive( false );
	}

	public void OnOK()
	{
		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Click );
		PlayerPrefs.SetInt( "music", musicToggle.isOn ? 1 : 0 );
		PlayerPrefs.SetInt( "sound", soundToggle.isOn ? 1 : 0 );
		PlayerPrefs.SetInt( "bloom2", bloomToggle.isOn ? 1 : 0 );
		PlayerPrefs.SetInt( "vignette", vignetteToggle.isOn ? 1 : 0 );
		PlayerPrefs.SetInt( "ambient", ambientToggle.isOn ? 1 : 0 );
		PlayerPrefs.SetInt( "closeWindowToggle", closeWindowToggle.isOn ? 1 : 0 );
		PlayerPrefs.SetInt( "zoombuttons", zoomToggle.isOn ? 1 : 0 );
		PlayerPrefs.SetInt( "musicVolume", musicWheelHandler.wheelValue );
		PlayerPrefs.SetInt( "ambientVolume", ambientWheelHandler.wheelValue );
		PlayerPrefs.SetInt( "soundVolume", soundWheelHandler.wheelValue );
		PlayerPrefs.SetInt( "skipWarpIntro", skipWarpIntroToggle.isOn ? 1 : 0 );
		PlayerPrefs.SetInt( "defaultRegularEnemyColor1", ColorToIndex(regularEnemyButton1.color) );
		PlayerPrefs.SetInt( "defaultRegularEnemyColor2", ColorToIndex(regularEnemyButton2.color) );
		if (PlayerPrefs.GetString("Lothal") == "true")
		{
			PlayerPrefs.SetInt( "defaultEliteEnemyColor1", ColorToIndex(eliteEnemyButton1.color) );
		}
		else
		{
			PlayerPrefs.SetInt( "defaultEliteEnemyColor1", ColorToIndex(eliteEnemySingleButton.color) );
		}
		PlayerPrefs.SetInt( "defaultEliteEnemyColor2", ColorToIndex(eliteEnemyButton2.color) );
		PlayerPrefs.SetInt( "defaultVillainColor", ColorToIndex(villainButton.color) );

		PlayerPrefs.Save();

		FindObjectOfType<Sound>().PlaySound( FX.Click );

		fader.DOFade( 0, .5f ).OnComplete( () =>
		{
			gameObject.SetActive( false );
			callbackAction?.Invoke();
		} );
		cg.DOFade( 0, .2f );
		transform.GetChild( 0 ).DOScale( .85f, .5f ).SetEase( Ease.OutExpo );
	}

	public void OnToggle( Toggle t )
	{
		//don't process toggle just because we manually set it on/off in Show()
		if ( toggleBusy )
			return;

		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Click );
		if ( t.name.ToLower() == "music toggle" )
		{
			if ( t.isOn )
				sound.PlayMusic( musicWheelHandler.wheelValue );
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
			PlayerPrefs.SetInt( "ambient", t.isOn ? 1 : 0 );
			if ( t.isOn )
				sound.StartAmbientSound( biomeType );
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
			var c = FindObjectOfType<SagaController>();
			if ( c != null )
				c.OnZoomBarToggle( t.isOn );
		}
		else if ( t.name.ToLower() == "view toggle" )
		{
			PlayerPrefs.SetInt( "viewToggle", t.isOn ? 1 : 0 );
			var c = FindObjectOfType<CameraController>();
			if ( c != null )
			{
				c.CameraViewToggle( t.isOn ? CameraView.TopDown : CameraView.Normal );
			}
		}
		else if ( t.name.ToLower() == "round limit on" )
		{
			PlayerPrefs.SetInt( "roundLimitToggle", 1 );
		}
		else if ( t.name.ToLower() == "round limit off" )
		{
			PlayerPrefs.SetInt( "roundLimitToggle", 0 );
		}
		else if ( t.name.ToLower() == "round limit dangerous" )
		{
			PlayerPrefs.SetInt( "roundLimitToggle", 2 );
		}
		else if ( t.name.ToLower() == "" )
		{
			PlayerPrefs.SetInt( "skipWarpIntro", t.isOn ? 1 : 0 );
		}
	}

	public void OnQuit()
	{
		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Click );
		callbackAction?.Invoke();
		quitAction?.Invoke( SettingsCommand.Quit );
	}

	public void OnReturnTitles()
	{
		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Click );
		fader.DOFade( 0, .5f ).OnComplete( () =>
		{
			gameObject.SetActive( false );
			callbackAction?.Invoke();
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
		colorPanel.SetActive( false );
	}

	public void OnGraphicsTab()
	{
		audioPanel.SetActive( false );
		gfxPanel.SetActive( true );
		uiPanel.SetActive( false );
		colorPanel.SetActive( false );
	}

	public void OnUITab()
	{
		audioPanel.SetActive( false );
		gfxPanel.SetActive( false );
		uiPanel.SetActive( true );
		colorPanel.SetActive( false );
	}

	public void OnColorTab()
	{
		audioPanel.SetActive( false );
		gfxPanel.SetActive( false );
		uiPanel.SetActive( false );
		if ( PlayerPrefs.GetString( "Lothal" ) == "true" )
		{
			eliteSelectionSingle.SetActive( false );
			eliteSelectionDual.SetActive( true );
		}
		else
		{
			eliteSelectionSingle.SetActive( true );
			eliteSelectionDual.SetActive( false );
		}
		colorPanel.SetActive( true );
	}

	public void ToggleColor(Image i)
	{
		EventSystem.current.SetSelectedGameObject(null);
		sound.PlaySound(FX.Click);

		int colorIndex = ColorToIndex(i.color);
		//red black purple blue green gray
		colorIndex = colorIndex == 6 ? 0 : colorIndex + 1;
		i.color = DataStore.pipColors[colorIndex].ToColor();
	}

	public void OnHelpClick()
	{
		if ( audioPanel.activeInHierarchy )
			audioHelpPanel.Show();
		else if ( gfxPanel.activeInHierarchy )
			graphicsHelpPanel.Show();
		else if ( uiPanel.activeInHierarchy )
			uiHelpPanel.Show();
		else if ( colorPanel.activeInHierarchy )
			colorHelpPanel.Show();
	}

	private int ColorToIndex( Color c )
	{
		if ( c.Equals( DataStore.pipColors[0].ToColor() ) )
			return 0;
		if ( c.Equals( DataStore.pipColors[1].ToColor() ) )
			return 1; 
		if ( c.Equals( DataStore.pipColors[2].ToColor() ) )
			return 2;
		if ( c.Equals( DataStore.pipColors[3].ToColor() ) )
			return 3;
		if ( c.Equals( DataStore.pipColors[4].ToColor() ) )
			return 4;
		if ( c.Equals( DataStore.pipColors[5].ToColor() ) )
			return 5;
		if ( c.Equals( DataStore.pipColors[6].ToColor() ) )
			return 6;

		//Default returns grey
		return 0;
	}
}
