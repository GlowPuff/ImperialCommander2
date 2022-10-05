using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using DG.Tweening;
using Newtonsoft.Json;
using Saga;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleController : MonoBehaviour
{
	public Fader fader;
	public Animator animator;
	public Sound soundController;
	public NewGameScreen newGameScreen;
	public TitleText titleText;
	public GameObject donateButton, docsButton, versionButton, tutorialGoButton, sagaClassicLayoutContainer, campaignContainer;
	public VolumeProfile volume;
	public Button continueButton, campaignContinueButton;
	public Transform busyIconTF;
	public TextMeshProUGUI versionText;
	public MissionTextBox versionPopup;
	public TMP_Dropdown languageDropdown, tutorialDropdown;
	public TextMeshProUGUI donateText, docsText, panelDescriptionText, campaignPanelDescriptionText;
	public Toggle sagaToggle, classicToggle, campaignToggle;
	public TutorialPanel tutorialPanel;
	public NewCampaignPanel newCampaignPanel;
	public ContinueCampaignPanel continueCampaignPanel;

	//UI objects using language translations
	public Text uiMenuHeader, uiNewGameBtn, uiContinueBtn, uiCampaignNewBtn, uiCampaignLoadBtn, uiCampaignContinueBtn, uiExpansionsBtn, uiOptionsBtn, bespinExp, hothExp, jabbaExp, empireExp, lothalExp, twinExp;

	private int m_OpenParameterId;
	private int expID;
	private NetworkStatus networkStatus;
	private GitHubResponse gitHubResponse = null;
	private bool skipDropdown = true;

	void Start()
	{
		System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
		System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
		System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

#if !UNITY_ANDROID
		Application.runInBackground = true;
#endif
		Screen.fullScreen = true;
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		fader.UnFade( 2 );
		if ( !PlayerPrefs.HasKey( "music" ) )
			PlayerPrefs.SetInt( "music", 1 );
		if ( !PlayerPrefs.HasKey( "sound" ) )
			PlayerPrefs.SetInt( "sound", 1 );
		if ( !PlayerPrefs.HasKey( "bloom" ) )
			PlayerPrefs.SetInt( "bloom", 1 );
		if ( !PlayerPrefs.HasKey( "vignette" ) )
			PlayerPrefs.SetInt( "vignette", 1 );
		if ( !PlayerPrefs.HasKey( "language" ) )
			PlayerPrefs.SetInt( "language", 0 );
		if ( !PlayerPrefs.HasKey( "ambient" ) )
			PlayerPrefs.SetInt( "ambient", 1 );
		//save defaults
		PlayerPrefs.Save();

		RunningCampaign.Reset();
		//create all card lists, load app settings, mission presets and translations
		DataStore.InitData();

		//set translated UI
		SetTranslatedUI();

		languageDropdown.value = PlayerPrefs.GetInt( "language" );
		skipDropdown = false;

		if ( volume.TryGet<Bloom>( out var bloom ) )
			bloom.active = PlayerPrefs.GetInt( "bloom" ) == 1;
		if ( volume.TryGet<Vignette>( out var vig ) )
			vig.active = PlayerPrefs.GetInt( "vignette" ) == 1;

		//check if saved state is valid
		continueButton.interactable = IsSagaSessionValid( "SagaSession" );
		campaignContinueButton.interactable = IsSagaSessionValid( "CampaignSession" );

		FindObjectOfType<Sound>().CheckAudio();

		networkStatus = NetworkStatus.Busy;
		versionText.text = DataStore.appVersion;

		if ( NetworkInterface.GetIsNetworkAvailable() )
			StartCoroutine( StartVersionCheck() );
		else
		{
			networkStatus = NetworkStatus.Error;
			busyIconTF.GetComponent<Image>().color = new Color( 1, 0, 0 );
		}

		//check if we should load right into Saga for mission testing
		if ( BootStrapTestMission() )
		{
			SceneManager.LoadScene( "Saga" );
		}
	}

#if UNITY_EDITOR
	//private void DEBUG()
	//{
	//	var p = Saga.FileManager.GetProjects();
	//	foreach ( var project in p )
	//		Debug.Log( project.fileName );
	//	var m = Saga.FileManager.LoadMission( "test.json" );
	//	var dp = m.mapEntities[2] as Saga.DeploymentPoint;
	//	Debug.Log( dp.deploymentColor );
	//}
#endif

	/// <summary>
	/// Check for command line to load right into a Saga mission for quick testing of missions
	/// </summary>
	private bool BootStrapTestMission()
	{
		//first array element is appName.exe
		string[] args = Environment.GetCommandLineArgs();
		//args = new string[2] { "foo.exe", "atest.json" };//"CORE1-A New Threat.json" };//DEBUG TESTING
		if ( args.Length == 2 )
		{
			string path = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), "ImperialCommander", args[1] );
			var setupOptions = new SagaSetupOptions()
			{
				difficulty = Difficulty.Medium,
				threatLevel = 3,
				projectItem = new ProjectItem() { fullPathWithFilename = path, fileName = args[1] },
			};

			Debug.Log( "***BootStrapTestMission***" );
			DataStore.StartNewSagaSession( setupOptions );
			//add some heroes to test with
			DataStore.sagaSessionData.MissionHeroes.Add( DataStore.heroCards[0] );
			DataStore.sagaSessionData.MissionHeroes.Add( DataStore.heroCards[1] );
			DataStore.sagaSessionData.selectedAlly = DataStore.allyCards[0];
			//try to load the mission
			DataStore.mission = FileManager.LoadMission( setupOptions.projectItem.fullPathWithFilename );
			if ( DataStore.mission != null )
				return true;
		}
		return false;
	}

	private void OnEnable()
	{
		m_OpenParameterId = Animator.StringToHash( "flip in" );
		expID = Animator.StringToHash( "exp flip in" );
		animator.SetBool( m_OpenParameterId, true );

		titleText.Show();
	}

	public void FlipIn( Animator anim )
	{
		anim.SetBool( m_OpenParameterId, true );
	}

	public void ReturnTo()
	{
		EventSystem.current.SetSelectedGameObject( null );
		FlipIn( animator );
		titleText.Show();
		titleText.FlipIn();
		donateButton.SetActive( true );
		docsButton.SetActive( true );
		versionButton.SetActive( true );
		tutorialGoButton.SetActive( true );
		languageDropdown.gameObject.SetActive( true );
		tutorialDropdown.gameObject.SetActive( true );
	}

	public void OnNewGame()
	{
		EventSystem.current.SetSelectedGameObject( null );
		soundController.PlaySound( FX.Click );
		animator.SetBool( m_OpenParameterId, false );
		animator.SetBool( expID, false );

		titleText.FlipOut();

		donateButton.SetActive( false );
		docsButton.SetActive( false );
		versionButton.SetActive( false );
		tutorialGoButton.SetActive( false );
		languageDropdown.gameObject.SetActive( false );
		tutorialDropdown.gameObject.SetActive( false );

		if ( DataStore.gameType == GameType.Classic )
		{
			DataStore.StartNewSession();
			newGameScreen.ActivateScreen();
		}
		else
		{
			soundController.FadeOutMusic();
			FadeOut( 1 );

			float foo = 1;
			DOTween.To( () => foo, x => foo = x, 0, 1 ).OnComplete( () =>
			 SceneManager.LoadScene( "SagaSetup" ) );
		}
	}

	public void OnContinueSession()
	{
		EventSystem.current.SetSelectedGameObject( null );
		soundController.PlaySound( FX.Click );

		if ( DataStore.gameType == GameType.Classic )
		{
			SessionData session = LoadSession();
			if ( session != null )
			{
				DataStore.sessionData = session;

				animator.SetBool( m_OpenParameterId, false );
				animator.SetBool( expID, false );
				titleText.FlipOut();
				donateButton.SetActive( false );
				docsButton.SetActive( false );
				versionButton.SetActive( false );
				tutorialGoButton.SetActive( false );
				languageDropdown.gameObject.SetActive( false );
				tutorialDropdown.gameObject.SetActive( false );
				soundController.FadeOutMusic();
				FadeOut( 1 );

				float foo = 1;
				DOTween.To( () => foo, x => foo = x, 0, 1 ).OnComplete( () =>
				 SceneManager.LoadScene( "Main" ) );
			}
			else
				continueButton.interactable = false;
		}
		else
		{
			SagaSession session = LoadSagaSession( "SagaSession" );
			if ( session != null )
			{
				DataStore.sagaSessionData = session;
				DataStore.sagaSessionData.gameVars.isNewGame = false;

				animator.SetBool( m_OpenParameterId, false );
				animator.SetBool( expID, false );
				titleText.FlipOut();
				donateButton.SetActive( false );
				docsButton.SetActive( false );
				versionButton.SetActive( false );
				tutorialGoButton.SetActive( false );
				languageDropdown.gameObject.SetActive( false );
				tutorialDropdown.gameObject.SetActive( false );
				soundController.FadeOutMusic();
				FadeOut( 1 );

				float foo = 1;
				DOTween.To( () => foo, x => foo = x, 0, 1 ).OnComplete( () =>
				 SceneManager.LoadScene( "Warp" ) );
			}
			else
				continueButton.interactable = false;
		}
	}

	public void StartTutorial()
	{
		animator.SetBool( m_OpenParameterId, false );
		animator.SetBool( expID, false );
		titleText.FlipOut();
		donateButton.SetActive( false );
		docsButton.SetActive( false );
		versionButton.SetActive( false );
		tutorialGoButton.SetActive( false );
		languageDropdown.gameObject.SetActive( false );
		tutorialDropdown.gameObject.SetActive( false );
		soundController.FadeOutMusic();
		FadeOut( 1 );

		float foo = 1;
		DOTween.To( () => foo, x => foo = x, 0, 1 ).OnComplete( () =>
		 SceneManager.LoadScene( "Warp" ) );
	}

	public void OnExpansions()
	{
		EventSystem.current.SetSelectedGameObject( null );
		soundController.PlaySound( FX.Click );
		if ( animator.GetBool( expID ) == true )
			animator.SetBool( expID, false );
		else
		{
			animator.SetBool( expID, true );
			FindObjectOfType<ExpansionsPanel>().ActivatePanel();
		}
	}

	public void OnOptions()
	{
		EventSystem.current.SetSelectedGameObject( null );
		soundController.PlaySound( FX.Click );
		GlowEngine.FindUnityObject<SettingsScreen>().Show( OnSettingsClose, true );
	}

	void OnSettingsClose( SettingsCommand s )
	{
		Application.Quit();
	}

	public void OnCloseExpansions()
	{
		EventSystem.current.SetSelectedGameObject( null );
		soundController.PlaySound( FX.Click );
		animator.SetBool( expID, false );
	}

	public void ToggleExpansion( Toggle t )
	{
		EventSystem.current.SetSelectedGameObject( null );
		soundController.PlaySound( FX.Click );
		if ( t.isOn )
			DataStore.AddExpansion( t.name );
		else
			DataStore.RemoveExpansions( t.name );
	}

	public void FadeOut( float time )
	{
		fader.FadeToBlack( time );
	}

	public void OnDonate()
	{
		Application.OpenURL( "https://paypal.me/glowpuff" );
	}

	public void OnWikiDocs()
	{
		Application.OpenURL( "https://github.com/GlowPuff/ImperialCommander2/wiki" );
	}

	public void OnVersionPopup()
	{
		if ( gitHubResponse != null )
			versionPopup.Show( gitHubResponse.body );
	}

	public void OnDownloadLatest()
	{
		Application.OpenURL( "https://github.com/GlowPuff/ImperialCommander2/releases" );
	}

	public void OnLanguageChange()
	{
		//setting the dropdown value fires this event - it does not need to fire upon app load
		//skip it on app load, otherwise it fires twice on startup
		//make sure NOT to skip it if language actually changes
		if ( skipDropdown )
		{
			skipDropdown = false;
			return;
		}

		DataStore.languageCode = languageDropdown.value;
		PlayerPrefs.SetInt( "language", DataStore.languageCode );
		//reload translated data
		DataStore.LoadTranslatedData();

		//update the translated title menu UI
		SetTranslatedUI();
	}

	public void OnTutorialGo()
	{
		tutorialPanel.Show( tutorialDropdown.value );
	}

	private void SetTranslatedUI()
	{
		UITitle ui = DataStore.uiLanguage.uiTitle;
		uiMenuHeader.text = ui.menuHeading;

		uiNewGameBtn.text = ui.newGameBtn;
		uiCampaignNewBtn.text = ui.newGameBtn;

		uiContinueBtn.text = ui.continueBtn;
		uiCampaignContinueBtn.text = ui.continueBtn;

		uiCampaignLoadBtn.text = "load";//needs translation

		uiExpansionsBtn.text = ui.campaignsBtn;
		uiOptionsBtn.text = ui.optionsBtn;
		donateText.text = ui.supportUC;
		docsText.text = ui.docsUC;

		//expansion text
		UIExpansions uie = DataStore.uiLanguage.uiExpansions;
		bespinExp.text = uie.bespin;
		hothExp.text = uie.hoth;
		jabbaExp.text = uie.jabba;
		empireExp.text = uie.empire;
		lothalExp.text = uie.lothal;
		twinExp.text = uie.twin;
	}

	private bool IsSessionValid()
	{
		string basePath = Path.Combine( Application.persistentDataPath, "Session", "sessiondata.json" );

		if ( !File.Exists( basePath ) )
			return false;

		string json = "";
		try
		{
			using ( StreamReader sr = new StreamReader( basePath ) )
			{
				json = sr.ReadToEnd();
			}
			SessionData session = JsonConvert.DeserializeObject<SessionData>( json );

			return session.stateManagementVersion == 3;
		}
		catch ( Exception e )
		{
			Debug.Log( "***ERROR*** IsSessionValid:: " + e.Message );
			DataStore.LogError( "IsSessionValid() TRACE:\r\n" + e.Message );
			return false;
		}
	}

	private bool IsSagaSessionValid( string sessionFolder )
	{
		string basePath = Path.Combine( Application.persistentDataPath, sessionFolder, "sessiondata.json" );

		if ( !File.Exists( basePath ) )
			return false;

		string json = "";
		try
		{
			using ( StreamReader sr = new StreamReader( basePath ) )
			{
				json = sr.ReadToEnd();
			}
			SagaSession session = JsonConvert.DeserializeObject<SagaSession>( json );

			return session.stateManagementVersion == 1;
		}
		catch ( Exception e )
		{
			Debug.Log( "***ERROR*** IsSagaSessionValid:: " + e.Message );
			DataStore.LogError( "IsSagaSessionValid() TRACE:\r\n" + e.Message );
			return false;
		}
	}

	private SessionData LoadSession()
	{
		string basePath = Path.Combine( Application.persistentDataPath, "Session", "sessiondata.json" );

		string json = "";

		try
		{
			using ( StreamReader sr = new StreamReader( basePath ) )
			{
				json = sr.ReadToEnd();
			}
			SessionData session = JsonConvert.DeserializeObject<SessionData>( json );

			return session;
		}
		catch ( Exception e )
		{
			Debug.Log( "***ERROR*** LoadSession:: " + e.Message );
			DataStore.LogError( "TRACE:\r\n" + e.Message );
			return null;
		}
	}

	private SagaSession LoadSagaSession( string sessionFolder )
	{
		string basePath = Path.Combine( Application.persistentDataPath, sessionFolder, "sessiondata.json" );

		string json = "";

		try
		{
			using ( StreamReader sr = new StreamReader( basePath ) )
			{
				json = sr.ReadToEnd();
			}
			SagaSession session = JsonConvert.DeserializeObject<SagaSession>( json );

			return session;
		}
		catch ( Exception e )
		{
			Debug.Log( "***ERROR*** LoadSagaSession:: " + e.Message );
			DataStore.LogError( "LoadSagaSession() TRACE:\r\n" + e.Message );
			return null;
		}
	}

	private void Update()
	{
		if ( networkStatus == NetworkStatus.Busy )
			busyIconTF.Rotate( new Vector3( 0, 0, Time.deltaTime * 175f ) );
		else
			busyIconTF.localRotation = Quaternion.Euler( 0, 0, 0 );

		//pulse scale if network error or wrong version
		if ( networkStatus == NetworkStatus.Error || networkStatus == NetworkStatus.WrongVersion )
			busyIconTF.localScale = GlowEngine.SineAnimation( .9f, 1.1f, 15 ).ToVector3();
	}

	private IEnumerator CheckVersion()
	{
		// /repos/{owner}/{repo}/releases
		var web = UnityWebRequest.Get( "https://api.github.com/repos/GlowPuff/ImperialCommander2/releases/latest" );
		yield return web.SendWebRequest();
		if ( web.result == UnityWebRequest.Result.ConnectionError )
		{
			Debug.Log( "network error" );
			networkStatus = NetworkStatus.Error;
			busyIconTF.GetComponent<Image>().color = new Color( 1, 0, 0 );
			gitHubResponse = null;
		}
		else
		{
			//parse JSON response
			gitHubResponse = JsonConvert.DeserializeObject<GitHubResponse>( web.downloadHandler.text );

			if ( gitHubResponse.tag_name == DataStore.appVersion )
			{
				networkStatus = NetworkStatus.UpToDate;
				busyIconTF.GetComponent<Image>().color = new Color( 0, 1, 0 );
			}
			else
			{
				networkStatus = NetworkStatus.WrongVersion;
				busyIconTF.GetComponent<Image>().color = new Color( 1, 0.5586207f, 0 );
			}
		}

		yield return null;
	}

	private IEnumerator StartVersionCheck()
	{
		//first check if internet is available
		var ping = new System.Net.NetworkInformation.Ping();
		var reply = ping.Send( new IPAddress( new byte[] { 8, 8, 8, 8 } ), 5000 );
		if ( reply.Status == IPStatus.Success )
		{
			//internet available, check for latest version
			StartCoroutine( CheckVersion() );
		}
		else
		{
			networkStatus = NetworkStatus.Error;
			busyIconTF.GetComponent<Image>().color = new Color( 1, 0, 0 );
			gitHubResponse = null;
		}

		yield return null;
	}

	public void OnNewCampaign()
	{
		EventSystem.current.SetSelectedGameObject( null );
		soundController.PlaySound( FX.Click );
		animator.SetBool( m_OpenParameterId, false );
		animator.SetBool( expID, false );

		newCampaignPanel.Show( () =>
		{
			animator.SetBool( m_OpenParameterId, true );
		} );
	}

	public void OnLoadCampaign()
	{
		EventSystem.current.SetSelectedGameObject( null );
		soundController.PlaySound( FX.Click );
		animator.SetBool( m_OpenParameterId, false );
		animator.SetBool( expID, false );

		continueCampaignPanel.Show( () =>
		{
			animator.SetBool( m_OpenParameterId, true );
			//if last campaign used was just deleted, disable continue button
			campaignContinueButton.interactable = IsSagaSessionValid( "CampaignSession" );
		} );
	}

	public void OnContinueCampaign()
	{
		EventSystem.current.SetSelectedGameObject( null );
		soundController.PlaySound( FX.Click );

		SagaSession session = LoadSagaSession( "CampaignSession" );
		if ( session != null )
		{
			var cs = SagaCampaign.LoadCampaignState( session.campaignGUID );

			RunningCampaign.sagaCampaignGUID = session.campaignGUID;
			RunningCampaign.expansionCode = cs.campaignExpansionCode;

			DataStore.sagaSessionData.gameVars.isNewGame = false;

			animator.SetBool( m_OpenParameterId, false );
			animator.SetBool( expID, false );
			titleText.FlipOut();
			donateButton.SetActive( false );
			docsButton.SetActive( false );
			versionButton.SetActive( false );
			tutorialGoButton.SetActive( false );
			languageDropdown.gameObject.SetActive( false );
			tutorialDropdown.gameObject.SetActive( false );
			soundController.FadeOutMusic();
			FadeOut( 1 );

			float foo = 1;
			DOTween.To( () => foo, x => foo = x, 0, 1 ).OnComplete( () =>
			 SceneManager.LoadScene( "Warp" ) );
		}
		else
			campaignContinueButton.interactable = false;
	}

	public void DeleteCampaignState( Guid guid )
	{
		//if a campaign is deleted, check if any saved state belongs to that campaign, and remove that if it is
		SagaSession session = LoadSagaSession( "CampaignSession" );
		if ( session != null && session.campaignGUID == guid )
		{
			campaignContinueButton.interactable = false;
			RunningCampaign.sagaCampaignGUID = session.campaignGUID;
			session.RemoveState();
			RunningCampaign.Reset();
		}
	}

	public void NavToCampaignScreen( SagaCampaign campaign )
	{
		if ( campaign == null )
			return;

		RunningCampaign.sagaCampaignGUID = campaign.GUID;
		RunningCampaign.expansionCode = campaign.campaignExpansionCode;

		soundController.FadeOutMusic();
		FadeOut( 1 );

		float foo = 1;
		DOTween.To( () => foo, x => foo = x, 0, 1 ).OnComplete( () =>
		 SceneManager.LoadScene( "Campaign" ) );
	}

	public void OnToggleMode( Toggle toggle )
	{
		if ( sagaToggle.isOn )
		{
			DataStore.gameType = GameType.Saga;
			sagaClassicLayoutContainer.SetActive( true );
			campaignContainer.SetActive( false );
			//check if saved state is valid
			continueButton.interactable = IsSagaSessionValid( "SagaSession" );
			panelDescriptionText.text = "Play a single, narratively driven mission with a 3D map presentation.";
		}
		else if ( classicToggle.isOn )
		{
			DataStore.gameType = GameType.Classic;
			sagaClassicLayoutContainer.SetActive( true );
			campaignContainer.SetActive( false );
			//check if saved state is valid
			continueButton.interactable = IsSessionValid();
			panelDescriptionText.text = "Play a single mission using your mission guide book and no 3D map.";
		}
		else if ( campaignToggle.isOn )
		{
			DataStore.gameType = GameType.Saga;
			continueButton.interactable = false;
			sagaClassicLayoutContainer.SetActive( false );
			campaignContainer.SetActive( true );
			campaignContinueButton.interactable = IsSagaSessionValid( "CampaignSession" );
			campaignPanelDescriptionText.text = "Embark on a new or existing Campaign through the galaxy.";
		}
	}
}