using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainGameController : MonoBehaviour
{
	public Image faderOverlay;
	public DeploymentGroupManager dgManager;
	public Image[] heroImages;
	public MWheelHandler threatWheelHandler;
	public Text roundText;
	public MissionTextBox missionTextBox;
	public GenericChooser genericChooser;
	public EventPopup eventPopup;
	public EnemyActivationPopup enemyActivationPopup;
	public DeploymentPopup deploymentPopup;
	public RandomDeployPopup randomDeployPopup;
	public Button activateImperialButton, endTurnButton, fameButton;
	public VolumeProfile volume;
	public FamePopup famePopup;
	public MainLanguageController languageController;
	public Toggle pauseThreatToggle, pauseDeploymentToggle;

	Sound sound;

	private void Start()
	{
		System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
		System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
		System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		//DEBUG BOOTSTRAP A MISSION
		//debugBootstrap();

		//apply settings
		sound = FindObjectOfType<Sound>();
		sound.CheckAudio();
		if ( volume.TryGet<Bloom>( out var bloom ) )
			bloom.active = PlayerPrefs.GetInt( "bloom" ) == 1;
		if ( volume.TryGet<Vignette>( out var vig ) )
			vig.active = PlayerPrefs.GetInt( "vignette" ) == 1;

		//set translated UI
		languageController.SetTranslatedUI();

		//see if it's a new game or restoring state
		if ( DataStore.sessionData.gameVars.isNewGame )
		{
			Debug.Log( "STARTING NEW GAME" );
			DataStore.sessionData.InitGameVars();
			ResetUI();
			StartNewGame();
		}
		else
		{
			Debug.Log( "CONTINUING GAME" );
			ResetUI();
			ContinueGame();
		}
	}

	private void debugBootstrap()
	{
		DataStore.InitData();
		DataStore.StartNewSession();
		DataStore.sessionData.selectedMissionName = "";
		DataStore.sessionData.selectedMissionExpansion = Expansion.Core;
		DataStore.sessionData.selectedMissionID = "core3";
		//optional dep
		//DataStore.sessionData.optionalDeployment = YesNo.Yes;
		//difficulty
		DataStore.sessionData.difficulty = Difficulty.Medium;
		//bootstrap threat
		DataStore.sessionData.threatLevel = 6;
		//bootstrap a hero
		DataStore.sessionData.MissionHeroes.Add( DataStore.heroCards.cards[0] );
		DataStore.sessionData.MissionHeroes.Add( DataStore.heroCards.cards[1] );

		//bootstrap some starting enemy groups
		DataStore.sessionData.MissionStarting.Add( DataStore.deploymentCards.cards.Where( x => x.id == "DG015" ).FirstOrDefault() );
		DataStore.sessionData.MissionStarting.Add( DataStore.deploymentCards.cards.Where( x => x.id == "DG002" ).FirstOrDefault() );

		//bootstrap reserved
		//DataStore.sessionData.MissionReserved.Add( DataStore.deploymentCards.cards.Where( x => x.id == "DG003" ).FirstOrDefault() );
		//DataStore.sessionData.MissionReserved.Add( DataStore.deploymentCards.cards.Where( x => x.id == "DG006" ).FirstOrDefault() );

		//bootstrap an ally
		DataStore.sessionData.selectedAlly = DataStore.allyCards.cards.Where( x => x.id == "A005" ).FirstOrDefault();

		//bootstrap factions
		//DataStore.sessionData.includeImperials = false;

		//bootstrap earned villains
		DataStore.sessionData.EarnedVillains.Add( DataStore.villainCards.cards.Where( x => x.id == "DG072" ).FirstOrDefault() );//darth vader
	}

	/// <summary>
	/// build card decks, activate optional deployment
	/// </summary>
	void StartNewGame()
	{
		fameButton.interactable = DataStore.sessionData.useAdaptiveDifficulty;
		//create deployment hand and manual deploy list
		DataStore.CreateDeploymentHand();
		//foreach ( var d in dh )
		//	Debug.Log( "DH: " + d.name );
		DataStore.CreateManualDeployment();
		//deploy heroes
		for ( int i = 0; i < DataStore.sessionData.MissionHeroes.Count; i++ )
			dgManager.DeployHeroAlly( DataStore.sessionData.MissionHeroes[i] );
		if ( DataStore.sessionData.MissionHeroes.Count == 3 )
		{
			Debug.Log( "Creating dummy hero" );
			dgManager.DeployHeroAlly( new CardDescriptor() { isDummy = true } );
		}
		//deploy ally
		if ( DataStore.sessionData.selectedAlly != null )
			dgManager.DeployHeroAlly( DataStore.sessionData.selectedAlly );
		//lay out starting groups
		dgManager.DeployStartingGroups();
		//perform option deployment if it's toggled
		if ( DataStore.sessionData.optionalDeployment == YesNo.Yes )
			GlowTimer.SetTimer( 1, () => deploymentPopup.Show( DeployMode.Landing, false, true ) );
	}

	void ContinueGame()
	{
		if ( DataStore.LoadState() )
		{
			//restore deployed enemies and heroes/allies
			dgManager.RestoreState();

			//update UI with loaded state
			//round #
			roundText.text = DataStore.uiLanguage.uiMainApp.roundHeading + "\r\n" + DataStore.sessionData.gameVars.round;
			//toggle pause threat/deployment buttons
			if ( DataStore.sessionData.gameVars.pauseThreatIncrease )
				pauseThreatToggle.isOn = true;
			if ( DataStore.sessionData.gameVars.pauseDeployment )
				pauseDeploymentToggle.isOn = true;

			fameButton.interactable = DataStore.sessionData.useAdaptiveDifficulty;

			GlowEngine.FindObjectsOfTypeSingle<QuickMessage>().Show( DataStore.uiLanguage.uiMainApp.restoredMsgUC );
		}
		else
		{
			GlowEngine.FindObjectsOfTypeSingle<QuickMessage>().Show( DataStore.uiLanguage.uiMainApp.restoreErrorMsgUC );
		}
	}

	void ResetUI()
	{
		faderOverlay.gameObject.SetActive( true );
		faderOverlay.DOFade( 0, 1 ).OnComplete( () => faderOverlay.gameObject.SetActive( false ) );
		roundText.text = DataStore.uiLanguage.uiMainApp.roundHeading + "\r\n1";

	}

	public void OnPauseThreat( Toggle t )
	{
		sound.PlaySound( FX.Click );
		DataStore.sessionData.gameVars.pauseThreatIncrease = t.isOn;
		string s = t.isOn ? DataStore.uiLanguage.uiMainApp.pauseThreatMsgUC : DataStore.uiLanguage.uiMainApp.UnPauseThreatMsgUC;
		GlowEngine.FindObjectsOfTypeSingle<QuickMessage>().Show( s );
	}

	public void OnPauseDeploy( Toggle t )
	{
		sound.PlaySound( FX.Click );
		DataStore.sessionData.gameVars.pauseDeployment = t.isOn;
		string s = t.isOn ? DataStore.uiLanguage.uiMainApp.pauseDepMsgUC : DataStore.uiLanguage.uiMainApp.unPauseDepMsgUC;
		GlowEngine.FindObjectsOfTypeSingle<QuickMessage>().Show( s );
	}

	public void OnActivateImperial()
	{
		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Click );
		int[] rnd;
		CardDescriptor toActivate = null;
		//find a non-exhausted group and activate it, bias to priority 1
		var groups = dgManager.GetNonExhaustedGroups();
		if ( groups.Count > 0 )
		{
			var p1 = groups.Where( x => x.priority == 1 ).ToList();
			var others = groups.Where( x => x.priority != 1 ).ToList();
			var all = p1.Concat( others ).ToList();
			//70% chance to priority 1 groups
			if ( p1.Count > 0 && GlowEngine.RandomBool( 70 ) )
			{
				rnd = GlowEngine.GenerateRandomNumbers( p1.Count );
				toActivate = p1[rnd[0]];
			}
			else
			{
				rnd = GlowEngine.GenerateRandomNumbers( all.Count );
				toActivate = all[rnd[0]];
			}

			ActivateEnemy( toActivate );
		}
	}

	public void OnMissionRules()
	{
		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Click );
		var txt = Resources.Load<TextAsset>( $"Languages/{DataStore.languageCodeList[DataStore.languageCode]}/MissionText/{DataStore.sessionData.selectedMissionID}rules" );
		if ( txt != null )
		{
			if ( GlowEngine.FindObjectsOfTypeSingle<EnemyActivationPopup>().gameObject.activeInHierarchy )
				GlowEngine.FindObjectsOfTypeSingle<EnemyActivationPopup>().OnReturn( false );
			missionTextBox.Show( txt.text, OnReturn );
		}
		else
			GlowEngine.FindObjectsOfTypeSingle<QuickMessage>().Show( "Could not find Mission Rules: " + DataStore.sessionData.selectedMissionID );
	}

	public void OnMissionInfo()
	{
		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Click );
		var txt = Resources.Load<TextAsset>( $"Languages/{DataStore.languageCodeList[DataStore.languageCode]}/MissionText/{DataStore.sessionData.selectedMissionID}info" );
		if ( txt != null )
		{
			if ( GlowEngine.FindObjectsOfTypeSingle<EnemyActivationPopup>().gameObject.activeInHierarchy )
				GlowEngine.FindObjectsOfTypeSingle<EnemyActivationPopup>().OnReturn( false );
			missionTextBox.Show( txt.text, OnReturn );
		}
		else
			GlowEngine.FindObjectsOfTypeSingle<QuickMessage>().Show( "Could not find Mission Info: " + DataStore.sessionData.selectedMissionID );
	}

	private void OnReturn()
	{
		if ( GlowEngine.FindObjectsOfTypeSingle<EnemyActivationPopup>().gameObject.activeInHierarchy )
			GlowEngine.FindObjectsOfTypeSingle<EnemyActivationPopup>().OnReturn();
	}

	public void OnOptionalDeploy()
	{
		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Click );
		deploymentPopup.Show( DeployMode.Landing, false, true );
	}

	public void OnEndRound()
	{
		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Vader );
		//pause dep ON, pause threat OFF = activate with CALM
		//pause dep ON, pause threat ON = just ready all groups
		//pause dep OFF, pause threat ON = normal rold 2D6 but no threat gain

		if ( DataStore.sessionData.gameVars.pauseDeployment && !DataStore.sessionData.gameVars.pauseThreatIncrease )
		{
			//session saved after deployment finishes
			deploymentPopup.Show( DeployMode.Calm, false, false, DoEvent );
		}
		else if ( DataStore.sessionData.gameVars.pauseDeployment && DataStore.sessionData.gameVars.pauseThreatIncrease )
		{
			dgManager.ReadyAllGroups();
			DoEvent();
			DataStore.sessionData.SaveSession( "Session" );//in case no event activates
		}
		else if ( !DataStore.sessionData.gameVars.pauseDeployment && DataStore.sessionData.gameVars.pauseThreatIncrease )
		{
			DoDeployment( true );//session saved after deployment finishes
		}
		else//normal deployment
			DoDeployment( false );//session saved after deployment finishes

		DataStore.sessionData.gameVars.round++;
		roundText.text = DataStore.uiLanguage.uiMainApp.roundHeading + "\r\n" + DataStore.sessionData.gameVars.round.ToString();
		dgManager.ReadyAllGroups();

		//debug stuff
		//deploymentPopup.Show( DeployMode.Onslaught );

		//eventPopup.Show( DataStore.cardEvents.Where( x => x.eventID == "E3" ).First() );

		//int rnd = Random.Range( 10, 69 );
		//var dude = DataStore.GetEnemy( "DG0" + rnd );
		//if ( dude != null )
		//	enemyActivationPopup.Show( dude );
		//else
		//	Debug.Log( "NOT FOUND: " + rnd );

		//enemyActivationPopup.Show( DataStore.GetEnemy( "DG055" ) );
	}

	void DoDeployment( bool skipThreatIncrease )
	{
		EventSystem.current.SetSelectedGameObject( null );
		int[] rnd = GlowEngine.GenerateRandomNumbers( 6 );
		int roll1 = rnd[0] + 1;

		rnd = GlowEngine.GenerateRandomNumbers( 6 );
		int roll2 = rnd[0] + 1;

		Debug.Log( "ROLLED: " + (roll1 + roll2).ToString() );
		Debug.Log( "DEP MODIFIER: " + DataStore.sessionData.gameVars.deploymentModifier );

		int total = roll1 + roll2 + DataStore.sessionData.gameVars.deploymentModifier;
		Debug.Log( "TOTAL ROLLED VALUE: " + total );

		if ( total <= 4 )
			deploymentPopup.Show( DeployMode.Calm, skipThreatIncrease, false );
		else if ( total > 4 && total <= 7 )
			deploymentPopup.Show( DeployMode.Reinforcements, skipThreatIncrease, false, DoEvent );
		else if ( total > 7 && total <= 10 )
			deploymentPopup.Show( DeployMode.Landing, skipThreatIncrease, false, DoEvent );
		else if ( total > 10 )
			deploymentPopup.Show( DeployMode.Onslaught, skipThreatIncrease, false, DoEvent );
	}

	void DoEvent()
	{
		EventSystem.current.SetSelectedGameObject( null );
		//1 in 4 chance to do an event
		int[] rnd = GlowEngine.GenerateRandomNumbers( 4 );
		int roll1 = rnd[0] + 1;

		if ( roll1 == 1 && DataStore.sessionData.gameVars.eventsTriggered < 3 )
		{
			DataStore.sessionData.gameVars.eventsTriggered++;
			rnd = GlowEngine.GenerateRandomNumbers( DataStore.cardEvents.Count );
			//get a random event
			var ev = DataStore.cardEvents[rnd[0]];
			//remove it from the list of events so it won't activate again
			DataStore.cardEvents.Remove( ev );
			//activate it
			eventPopup.Show( ev, () => DataStore.sessionData.SaveSession( "Session" ) );
		}
	}

	public void OnApplyThreatModifier()
	{
		sound.PlaySound( FX.CopyThat );
		DataStore.sessionData.ModifyThreat( threatWheelHandler.wheelValue, true );
		threatWheelHandler.ResetWheeler();
	}

	public void OnSettings()
	{
		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Click );
		GlowEngine.FindObjectsOfTypeSingle<SettingsScreen>().Show( OnSettingsClose );
	}

	void OnSettingsClose( SettingsCommand c )
	{
		//save the state on exit
		DataStore.sessionData.SaveSession( "Session" );

		if ( c == SettingsCommand.ReturnTitles )
		{
			sound.FadeOutMusic();
			faderOverlay.gameObject.SetActive( true );
			faderOverlay.DOFade( 1, 1 ).OnComplete( () =>
			{
				SceneManager.LoadScene( "Title" );
			} );
		}
		else
			Application.Quit();
	}

	public void OnReserved()
	{
		EventSystem.current.SetSelectedGameObject( null );
		//minus deployed, minus dep hand
		sound.PlaySound( FX.Click );
		genericChooser.Show( ChooserMode.DeploymentGroups, DataStore.sessionData
			.MissionReserved
			.MinusDeployed()
			.MinusInDeploymentHand(), AddGroup );
	}

	public void OnAlly()
	{
		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Click );
		//filter out DEPLOYED allies
		var allies = DataStore.allyCards.cards.Where( x => !DataStore.deployedHeroes.Contains( x ) ).ToList();
		genericChooser.Show( ChooserMode.Ally, allies, AddAlly );
	}

	public void OnEnemy()
	{
		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Click );
		//try from manual list, skip dep hand, skip deployed
		if ( DataStore.manualDeploymentList
			.MinusInDeploymentHand()
			.MinusDeployed()
			.Count > 0 )
		{
			genericChooser.Show( ChooserMode.DeploymentGroups, DataStore.manualDeploymentList
				.MinusInDeploymentHand()
				.MinusDeployed(), AddGroup );
		}//try dep hand
		else if ( DataStore.deploymentHand.Count > 0 )
		{
			genericChooser.Show( ChooserMode.DeploymentGroups, DataStore.deploymentHand, AddGroup );
		}
		else//no groups, just show custom
			genericChooser.Show( ChooserMode.DeploymentGroups, new List<CardDescriptor>(), AddGroup );
	}

	public void OnRandom()
	{
		EventSystem.current.SetSelectedGameObject( null );
		sound.PlaySound( FX.Click );
		randomDeployPopup.Show();
	}

	public void AddGroup( CardDescriptor cd )
	{
		if ( cd != null )
			dgManager.DeployGroup( cd, true );
	}

	public void AddAlly( CardDescriptor cd )
	{
		if ( cd != null )
			dgManager.DeployHeroAlly( cd );
	}

	public void ActivateEnemy( CardDescriptor cd )
	{
		if ( cd == null )
			return;

		dgManager.ExhaustGroup( cd.id );
		enemyActivationPopup.Show( cd );
	}

	public void OnShowDebug()
	{
		EventSystem.current.SetSelectedGameObject( null );
		GlowEngine.FindObjectsOfTypeSingle<DebugPopup>().Show();
	}

	public void OnShowFamePopup()
	{
		EventSystem.current.SetSelectedGameObject( null );
		famePopup.Show();
	}

	private void Update()
	{
		bool allActivated = false;
		foreach ( Transform enemy in dgManager.gridContainer )
		{
			DGPrefab dg = enemy.GetComponent<DGPrefab>();
			if ( !dg.IsExhausted )
				allActivated = true;
		}
		if ( allActivated )
		{
			activateImperialButton.interactable = true;
			float s = GlowEngine.SineAnimation( .95f, 1, 10f );
			activateImperialButton.transform.localScale = new Vector3( s, s );
		}
		else
		{
			activateImperialButton.interactable = false;
			activateImperialButton.transform.localScale = Vector3.one;
		}

		endTurnButton.interactable = !activateImperialButton.interactable;

		if ( Input.GetKeyDown( KeyCode.Escape ) )
		{
			if ( FindObjectOfType<DebugPopup>() != null )
				FindObjectOfType<DebugPopup>().OnClose();
			else
			{
				EventSystem.current.SetSelectedGameObject( null );
				GlowEngine.FindObjectsOfTypeSingle<DebugPopup>().Show();
			}
		}
	}
}
