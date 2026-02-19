using System;
using DG.Tweening;
using Saga;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
	public CanvasGroup cg;
	public Image fader, regularEnemyButton1, regularEnemyButton2, eliteEnemySingleButton, eliteEnemyButton1, eliteEnemyButton2, villainButton, cancelFader;
	public Toggle musicToggle, soundToggle, ambientToggle, closeWindowToggle, zoomToggle, skipWarpIntroToggle, roundLimitToggleOn, roundLimitToggleOff, roundLimitToggleDangerous, bloomToggle, vignetteToggle, viewToggle;
	public Sound sound;
	public GameObject returnButton, eliteSelectionSingle, eliteSelectionDual;
	public VolumeProfile volume;
	public SettingsPanelLanguageController languageController;
	public GameObject audioPanel, gfxPanel, uiPanel, colorPanel, mapperPanel;
	public Toggle audioToggle;
	public MWheelHandler musicWheelHandler, ambientWheelHandler, soundWheelHandler;
	public BiomeType biomeType;
	public HelpPanel audioHelpPanel;
	public HelpPanel uiHelpPanel;
	public HelpPanel graphicsHelpPanel;
	public HelpPanel colorHelpPanel;
	public HelpPanel mapHelpPanel;
	public Text keyValue, mapActivateImperials, mapToggleCamView, mapToggleMapVisibility, mapNavForward, mapNavBack, mapNavLeft, mapNavRight, mapNavCW, mapNavCCW;
	public ScrollRect mapperScrollRect;

	Action<SettingsCommand> quitAction;
	Action callbackAction;
	bool toggleBusy;
	bool awaitInput = false;
	string oldValue, inputCommand;

	public void Show( Action<SettingsCommand> onQuit, BiomeType btype = BiomeType.Menu, Action callback = null )
	{
		quitAction = onQuit;
		biomeType = btype;
		callbackAction = callback;
		awaitInput = false;
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

		//set toggles
		toggleBusy = true;//to avoid OnToggle()

		musicToggle.isOn = PlayerPrefs.GetInt( "music" ) == 1;
		soundToggle.isOn = PlayerPrefs.GetInt( "sound" ) == 1;
		ambientToggle.isOn = PlayerPrefs.GetInt( "ambient" ) == 1;
		closeWindowToggle.isOn = PlayerPrefs.GetInt( "closeWindowToggle" ) == 1;
		zoomToggle.isOn = PlayerPrefs.GetInt( "zoombuttons" ) == 1;
		skipWarpIntroToggle.isOn = PlayerPrefs.GetInt( "skipWarpIntro" ) == 1;
		roundLimitToggleOn.isOn = PlayerPrefs.GetInt( "roundLimitToggle" ) == 1;//0=off, 1=on, 2=dangerous
		roundLimitToggleOff.isOn = PlayerPrefs.GetInt( "roundLimitToggle" ) == 0;//0=off, 1=on, 2=dangerous
		roundLimitToggleDangerous.isOn = PlayerPrefs.GetInt( "roundLimitToggle" ) == 2;//0=off, 1=on, 2=dangerous
		bloomToggle.isOn = PlayerPrefs.GetInt( "bloom2" ) == 1;
		vignetteToggle.isOn = PlayerPrefs.GetInt( "vignette" ) == 1;
		viewToggle.isOn = PlayerPrefs.GetInt( "viewToggle" ) == 1;

		toggleBusy = false;

		regularEnemyButton1.color = DataStore.pipColors[PlayerPrefs.GetInt( "defaultRegularEnemyColor1" )].ToColor();
		regularEnemyButton2.color = DataStore.pipColors[PlayerPrefs.GetInt( "defaultRegularEnemyColor2" )].ToColor();
		eliteEnemySingleButton.color = DataStore.pipColors[PlayerPrefs.GetInt( "defaultEliteEnemyColor1" )].ToColor();
		eliteEnemyButton1.color = DataStore.pipColors[PlayerPrefs.GetInt( "defaultEliteEnemyColor1" )].ToColor();
		eliteEnemyButton2.color = DataStore.pipColors[PlayerPrefs.GetInt( "defaultEliteEnemyColor2" )].ToColor();
		villainButton.color = DataStore.pipColors[PlayerPrefs.GetInt( "defaultVillainColor" )].ToColor();

		UpdateDefaultKeyMaps();

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
		PlayerPrefs.SetInt( "ambient", ambientToggle.isOn ? 1 : 0 );
		PlayerPrefs.SetInt( "musicVolume", musicWheelHandler.wheelValue );
		PlayerPrefs.SetInt( "ambientVolume", ambientWheelHandler.wheelValue );
		PlayerPrefs.SetInt( "soundVolume", soundWheelHandler.wheelValue );
		PlayerPrefs.SetInt( "closeWindowToggle", closeWindowToggle.isOn ? 1 : 0 );
		PlayerPrefs.SetInt( "zoombuttons", zoomToggle.isOn ? 1 : 0 );
		PlayerPrefs.SetInt( "skipWarpIntro", skipWarpIntroToggle.isOn ? 1 : 0 );
		PlayerPrefs.SetInt( "bloom2", bloomToggle.isOn ? 1 : 0 );
		PlayerPrefs.SetInt( "vignette", vignetteToggle.isOn ? 1 : 0 );
		PlayerPrefs.SetInt( "defaultRegularEnemyColor1", ColorToIndex( regularEnemyButton1.color ) );
		PlayerPrefs.SetInt( "defaultRegularEnemyColor2", ColorToIndex( regularEnemyButton2.color ) );
		if ( PlayerPrefs.GetString( "Lothal" ) == "true" )
		{
			PlayerPrefs.SetInt( "defaultEliteEnemyColor1", ColorToIndex( eliteEnemyButton1.color ) );
		}
		else
		{
			PlayerPrefs.SetInt( "defaultEliteEnemyColor1", ColorToIndex( eliteEnemySingleButton.color ) );
		}
		PlayerPrefs.SetInt( "defaultEliteEnemyColor2", ColorToIndex( eliteEnemyButton2.color ) );
		PlayerPrefs.SetInt( "defaultVillainColor", ColorToIndex( villainButton.color ) );

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
		else if ( mapperPanel.activeInHierarchy )
			mapHelpPanel.Show();
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

	public void OnToggle( Toggle t )
	{
		//don't process toggle just because we manually set it on/off in Show()
		if ( toggleBusy )
			return;

		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Click );
		//audio
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
		else if ( t.name.ToLower() == "ambient toggle" )
		{
			PlayerPrefs.SetInt( "ambient", t.isOn ? 1 : 0 );
			if ( t.isOn )
				sound.StartAmbientSound( biomeType );
			else
				sound.StopAmbientSound();
		}
		//UI
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
		else if ( t.name.ToLower() == "" )
		{
			PlayerPrefs.SetInt( "skipWarpIntro", t.isOn ? 1 : 0 );
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
		//GRAPHICS
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
		else if ( t.name.ToLower() == "view toggle" )
		{
			PlayerPrefs.SetInt( "viewToggle", t.isOn ? 1 : 0 );
			var c = FindObjectOfType<CameraController>();
			if ( c != null )
			{
				c.CameraViewToggle( t.isOn ? CameraView.TopDown : CameraView.Normal );
			}
		}
	}

	public void OnAudioTab()
	{
		audioPanel.SetActive( true );
		gfxPanel.SetActive( false );
		uiPanel.SetActive( false );
		colorPanel.SetActive( false );
		mapperPanel.SetActive( false );
	}

	public void OnUITab()
	{
		audioPanel.SetActive( false );
		gfxPanel.SetActive( false );
		uiPanel.SetActive( true );
		colorPanel.SetActive( false );
		mapperPanel.SetActive( false );
	}

	public void OnGraphicsTab()
	{
		audioPanel.SetActive( false );
		gfxPanel.SetActive( true );
		uiPanel.SetActive( false );
		colorPanel.SetActive( false );
		mapperPanel.SetActive( false );
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
		mapperPanel.SetActive( false );
	}

	public void OnMapperTab()
	{
		audioPanel.SetActive( false );
		gfxPanel.SetActive( false );
		uiPanel.SetActive( false );
		colorPanel.SetActive( false );
		mapperPanel.SetActive( true );
		mapperScrollRect.verticalNormalizedPosition = 1;
	}

	public void ToggleColor( Image i )
	{
		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Click );

		int colorIndex = ColorToIndex( i.color );
		//red black purple blue green gray
		colorIndex = colorIndex == 6 ? 0 : colorIndex + 1;
		i.color = DataStore.pipColors[colorIndex].ToColor();
	}

	public void OnEditKey( Button inputMapperCommand )
	{
		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Click );
		awaitInput = true;
		//get the Text object (mapper value) higher up in the hierarchy of this button
		keyValue = inputMapperCommand.transform.parent.parent.GetChild( 1 ).GetComponent<Text>();
		oldValue = keyValue.text;
		cancelFader.gameObject.SetActive( true );
		cancelFader.color = new Color( 0, 0, 0, 0 );
		cancelFader.DOFade( .95f, .5f );
		inputCommand = inputMapperCommand.name;
	}

	public void OnResetKeyMaps()
	{
		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Click );
		DataStore.SetDefaultKeyMaps();
		PlayerPrefs.Save();

		UpdateDefaultKeyMaps();
	}

	public void UpdateDefaultKeyMaps()
	{
		mapActivateImperials.text = PlayerPrefs.GetString( "mapActivateImperials" );
		mapToggleCamView.text = PlayerPrefs.GetString( "mapToggleCamView" );
		mapToggleMapVisibility.text = PlayerPrefs.GetString( "mapToggleMapVisibility" );
		mapNavForward.text = PlayerPrefs.GetString( "mapNavForward" );
		mapNavBack.text = PlayerPrefs.GetString( "mapNavBack" );
		mapNavLeft.text = PlayerPrefs.GetString( "mapNavLeft" );
		mapNavRight.text = PlayerPrefs.GetString( "mapNavRight" );
		mapNavCW.text = PlayerPrefs.GetString( "mapNavCW" );
		mapNavCCW.text = PlayerPrefs.GetString( "mapNavCCW" );
	}

	public void OnCancelInput()
	{
		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Click );
		awaitInput = false;
		keyValue.text = oldValue;
		cancelFader.gameObject.SetActive( false );
	}

	private void Update()
	{
		if ( !awaitInput )
			return;

		if ( Input.GetKeyDown( KeyCode.Escape ) )
		{
			awaitInput = false;
			cancelFader.gameObject.SetActive( false );
			return;
		}

		//determine which key was just pressed on this frame and store it in a variable, but not mouse or joystick buttons
		if ( Input.anyKeyDown )
		{
			foreach ( KeyCode kcode in Enum.GetValues( typeof( KeyCode ) ) )
			{
				if ( Input.GetKeyDown( kcode ) && !kcode.ToString().Contains( "Mouse" ) )
				{
					KeyCode pressedKey = kcode;
					Debug.Log( "Key pressed: " + pressedKey );
					keyValue.text = pressedKey.ToString();
					cancelFader.gameObject.SetActive( false );
					PlayerPrefs.SetString( inputCommand, pressedKey.ToString() );
					PlayerPrefs.Save();
					Debug.Log( "SET: " + inputCommand );
					break;
				}
			}
		}
	}
}
