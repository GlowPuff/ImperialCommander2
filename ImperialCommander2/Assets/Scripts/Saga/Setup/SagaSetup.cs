using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Saga
{
	public class SagaSetup : MonoBehaviour
	{
		public Camera theCamera;
		//UI TRINKETS
		public GameObject descriptionTextBox;
		public Text difficultyText;
		public Transform heroContainer;
		public Button adaptiveButton, startMissionButton, viewMissionCardButton, campaignTilesButton;
		public GameObject miniMugPrefab;
		public Image allyImage;
		public MWheelHandler threatValue, addtlThreatValue;
		public TextMeshProUGUI versionText, additionalInfoText;
		//UI PANELS
		public SagaAddHeroPanel addHeroPanel;
		public SagaModifyGroupsPanel modifyGroupsPanel;
		public MissionCardZoom missionCardZoom;
		public ErrorPanel errorPanel;
		//OTHER
		public GameObject warpEffect, rightPanel;
		public Transform thrusterRoot, thrusterLeft, thrusterRight;
		public SagaSetupLanguageController languageController;
		public CanvasGroup faderCG;
		public ColorBlock redBlock;
		public ColorBlock greenBlock;
		public VolumeProfile volume;
		public MissionPicker missionPicker;
		public TextMeshProUGUI warpTitleText;
		public bool isDebugMode = false;
		public List<DeploymentCard> missionCustomAllies = new List<DeploymentCard>();
		public List<DeploymentCard> missionCustomHeroes = new List<DeploymentCard>();
		public List<DeploymentCard> missionCustomVillains = new List<DeploymentCard>();

		Sound sound;
		SagaSetupOptions setupOptions { get; set; }
		bool isFromCampaign = false;
		//these ignored groups are from mission/preset, and player can't toggle them
		List<DeploymentCard> disabledIgnoredGroups = new List<DeploymentCard>();

		void LogCallback( string condition, string stackTrace, LogType type )
		{
			//only capture errors, exceptions, asserts
			if ( type != LogType.Warning && type != LogType.Log )
				errorPanel.Show( $"An Error Occurred of Type <color=green>{type}</color>", $"<color=yellow>{condition}</color>\n\n{stackTrace.Replace( "(at", "\n\n(at" )}" );
		}

		private void OnDestroy()
		{
			Application.logMessageReceived -= LogCallback;
		}

		void Awake()
		{
			Debug.Log( "ENTERING SAGA SETUP" );
			//Exception handling for any Unity thrown exception, such as from asset management
			Application.logMessageReceived += LogCallback;

			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

			versionText.text = "App Mission Format: " + Utils.formatVersion;

			Screen.sleepTimeout = SleepTimeout.NeverSleep;

			float pixelHeightOfCurrentScreen = Screen.height;//.currentResolution.height;
			float pixelWidthOfCurrentScreen = Screen.width;//.currentResolution.width;
			float aspect = pixelWidthOfCurrentScreen / pixelHeightOfCurrentScreen;
			if ( aspect >= 2f )
			{
				thrusterLeft.position = new Vector3( -12f, thrusterLeft.position.y, thrusterLeft.position.z );
				thrusterRight.position = new Vector3( 12f, thrusterRight.position.y, thrusterRight.position.z );
			}
			if ( aspect < 1.7f )//less than 16:9, such as 16:10 and 4:3
			{
				warpTitleText.GetComponent<RectTransform>().SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, 175 );
			}

			//bootstrap the setup screen for debugging
			if ( isDebugMode && Application.isEditor )
				bootstrapDEBUG();

			//set translated UI
			try
			{
				languageController.SetTranslatedUI();
			}
			catch ( Exception e )
			{
				errorPanel.Show( "SetTranslatedUI()", e );
			}

			//apply settings
			if ( volume.TryGet<Bloom>( out var bloom ) )
				bloom.active = PlayerPrefs.GetInt( "bloom2" ) == 1;
			if ( volume.TryGet<Vignette>( out var vig ) )
				vig.active = PlayerPrefs.GetInt( "vignette" ) == 1;

			sound = FindObjectOfType<Sound>();
			//play menu ambient and music
			sound.PlayMusicAndMenuAmbient();

			setupOptions = new SagaSetupOptions();
			DataStore.StartNewSagaSession( setupOptions );
			//check if we're loading in from  the campaign manager
			//single shot mission, not from a campaign
			if ( RunningCampaign.sagaCampaignGUID != Guid.Empty )
				SetupCampaignMission();//setupOptions is set here

			ResetSetup( RunningCampaign.sagaCampaignGUID != Guid.Empty );
			UpdateHeroes();

			faderCG.alpha = 0;
			faderCG.DOFade( 1, .5f );
		}

		void bootstrapDEBUG()
		{
			FileManager.SetupDefaultFolders();
			RunningCampaign.Reset();
			DataStore.InitData();
		}

		/// <summary>
		/// set default mission options, add default ignored groups, add included global imports
		/// </summary>
		public void ResetSetup( bool isCampaign )
		{
			//difficulty
			difficultyText.text = DataStore.uiLanguage.uiSetup.normal;
			//adaptive
			adaptiveButton.colors = setupOptions.useAdaptiveDifficulty ? greenBlock : redBlock;
			//clear hero panel if not loading from campaign
			if ( heroContainer.childCount > 0 )
			{
				for ( int i = 1; i < heroContainer.childCount; i++ )
				{
					Destroy( heroContainer.GetChild( i ).gameObject );
				}
			}
			//threat value
			threatValue.ResetWheeler( setupOptions.threatLevel );
			//additional threat value
			addtlThreatValue.ResetWheeler( setupOptions.addtlThreat );

			//global imports not excluded from Expansions menu
			var included = DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Imperial && !DataStore.IgnoredPrefsImports.Contains( x.customCharacterGUID.ToString() ) );
			DataStore.sagaSessionData.globalImportedCharacters = included.Select( x => x.deploymentCard ).ToList();
			Debug.Log( $"{DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Imperial ).Count()} GLOBALLY IMPORTED IMPERIALS" );
			Debug.Log( $"{DataStore.IgnoredPrefsImports.Count} EXCLUDED IMPORTS (Expansion menu)" );
			Debug.Log( $"{DataStore.sagaSessionData.globalImportedCharacters.Count} ADDED TO SESSION IMPORTS" );

			languageController.UpdateVillainCount( DataStore.sagaSessionData.EarnedVillains.Count );

			if ( !isCampaign )
			{
				//clear ignored groups
				DataStore.sagaSessionData.MissionIgnored.Clear();
				//add default ignored
				//ignore "Other" expansion enemy groups by default, except owned packs
				DataStore.sagaSessionData.MissionIgnored.AddRange( DataStore.deploymentCards.Where( x => x.expansion == "Other" && !DataStore.ownedFigurePacks.ContainsCard( x ) ) );
				int impCount = DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Imperial ).Count() - DataStore.sagaSessionData.globalImportedCharacters.Count;
				languageController.UpdateIgnoredCount( DataStore.sagaSessionData.MissionIgnored.Count + impCount );
			}
		}

		public void OnCancel()
		{
			sound.FadeOutMusic();
			thrusterRoot.DOMoveZ( -30, .5f );
			if ( !isFromCampaign )
				faderCG.DOFade( 0, .5f ).OnComplete( () => SceneManager.LoadScene( "Title" ) );
			else
				faderCG.DOFade( 0, .5f ).OnComplete( () => SceneManager.LoadScene( "Campaign" ) );
		}

		/// <summary>
		/// setup the screen when loaded from the campaign manager
		/// </summary>
		void SetupCampaignMission()
		{
			isFromCampaign = true;
			var campaign = SagaCampaign.LoadCampaignState( RunningCampaign.sagaCampaignGUID );
			if ( campaign == null )
			{
				Debug.Log( "SetupCampaignMission()::Could not load the campaign" );
				errorPanel.Show( "SetupCampaignMission()", $"Campaign is null\n'{RunningCampaign.sagaCampaignGUID}'" );
				return;
			}

			campaignTilesButton.gameObject.SetActive( true );

			var structure = RunningCampaign.campaignStructure;
			//deactivate mission picker
			rightPanel.SetActive( false );
			//add heroes
			foreach ( var item in campaign.campaignHeroes )
			{
				var hero = DataStore.GetHero( item.heroID );
				if ( hero != null )
					DataStore.sagaSessionData.MissionHeroes.Add( hero );
			}
			//add villains
			foreach ( var item in campaign.campaignVillains )
			{
				DataStore.sagaSessionData.EarnedVillains.Add( item );
			}
			//clear ignored groups
			DataStore.sagaSessionData.MissionIgnored.Clear();
			//add default ignored
			//ignore "Other" expansion enemy groups by default, except owned packs
			var ignored = new HashSet<DeploymentCard>();
			DataStore.deploymentCards.Where( x => x.expansion == "Other" && !DataStore.ownedFigurePacks.ContainsCard( x ) ).ToList().ForEach( x => ignored.Add( x ) );
			if ( structure.missionSource == MissionSource.Embedded )
			{
				//get ignored groups from the embedded mission

				//embedded mission GUID is stored into 'hero'
				//imported campaign name is stored into 'bonusText'
				//package GUID is stored into 'expansionText'
				Mission m = FileManager.LoadEmbeddedMission( structure.packageGUID.ToString(), structure.projectItem.missionGUID, out DataStore.sagaSessionData.missionStringified );
				DataStore.mission = m;
				if ( m != null )
				{
					var ign = from c in DataStore.deploymentCards join i in m.missionProperties.bannedGroups on c.id equals i select c;
					ign.ToList().ForEach( x => ignored.Add( x ) );
					//add ignored from mission
					disabledIgnoredGroups.AddRange( ign );
				}
				else
					errorPanel.Show( "SetupCampaignMission()", $"Could not load embedded mission:\n{structure.projectItem.Title}::{structure.projectItem.missionGUID}" );
			}
			//add non-custom mission specific ignored groups, even if they are owned packs
			else if ( structure.missionSource == MissionSource.Official )
			{
				var presets = DataStore.missionPresets[structure.expansionCode.ToLower()];
				var mp = presets.Where( x => x.id.ToLower() == structure.missionID.ToLower() ).FirstOr( null );
				if ( mp != null )
				{
					var ign = from c in DataStore.deploymentCards join i in mp.ignoredGroups on c.id equals i select c;
					ign.ToList().ForEach( x => ignored.Add( x ) );
					//add ignored from preset
					disabledIgnoredGroups.AddRange( ign );
				}
			}
			else if ( structure.missionSource == MissionSource.Custom )
			{
				Mission m = FileManager.LoadMission( structure.projectItem.fullPathWithFilename );
				if ( m != null )
				{
					var ign = from c in DataStore.deploymentCards join i in m.missionProperties.bannedGroups on c.id equals i select c;
					ign.ToList().ForEach( x => ignored.Add( x ) );
					//add ignored from mission
					disabledIgnoredGroups.AddRange( ign );
				}
				else
					errorPanel.Show( "SetupCampaignMission()", $"Could not load custom mission:\n{structure.projectItem.fullPathWithFilename}" );
			}

			//finally, add the uniquely hashed set of ignored cards
			DataStore.sagaSessionData.MissionIgnored.AddRange( ignored );
			int impCount = DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Imperial ).Count() - DataStore.sagaSessionData.globalImportedCharacters.Count;
			languageController.UpdateIgnoredCount( DataStore.sagaSessionData.MissionIgnored.Count + impCount );

			setupOptions = new SagaSetupOptions()
			{
				threatLevel = structure.threatLevel,
				projectItem = structure.projectItem,
			};
			//set the mission
			missionPicker.selectedMission = setupOptions.projectItem;
			//set mission name
			missionPicker.missionNameText.text = structure.projectItem.Title;
			//set description
			missionPicker.missionDescriptionText.text = structure.projectItem.Description;
			//set additional text
			missionPicker.additionalInfoText.text = structure.projectItem.AdditionalInfo;
			//set threat
			threatValue.ResetWheeler( structure.threatLevel );
			//set text based on custom or built-in mission
			if ( structure.missionSource == MissionSource.Custom )
			{
				missionPicker.pickerMode = PickerMode.Custom;
				descriptionTextBox.gameObject.SetActive( true );
				viewMissionCardButton.gameObject.SetActive( false );
			}
			else if ( structure.missionSource == MissionSource.Official )
			{
				missionPicker.pickerMode = PickerMode.BuiltIn;
				descriptionTextBox.gameObject.SetActive( false );
				viewMissionCardButton.gameObject.SetActive( true );
			}
			else if ( structure.missionSource == MissionSource.Embedded )
			{
				missionPicker.pickerMode = PickerMode.Embedded;
				descriptionTextBox.gameObject.SetActive( false );
				viewMissionCardButton.gameObject.SetActive( false );
			}
		}

		public async void OnStartMission()
		{
			setupOptions.threatLevel = threatValue.wheelValue;
			setupOptions.addtlThreat = addtlThreatValue.wheelValue;
			setupOptions.projectItem = missionPicker.selectedMission;
			DataStore.sagaSessionData.setupOptions = setupOptions;

			missionPicker.isBusy = true;

			//load/validate the mission
			if ( missionPicker.pickerMode == PickerMode.Custom )
			{
				DataStore.mission = FileManager.LoadMission( setupOptions.projectItem.fullPathWithFilename, out DataStore.sagaSessionData.missionStringified );
				if ( DataStore.mission != null )
				{
					Warp();
				}
			}
			else if ( missionPicker.pickerMode == PickerMode.Embedded )
			{
				//mission is already loaded into DataStore.mission and stringified
				if ( DataStore.mission != null )
				{
					Warp();
				}
			}
			else if ( missionPicker.pickerMode == PickerMode.BuiltIn )
			{
				//if not English, try finding the translation
				if ( DataStore.languageCode == 0 )//En
					StartMission( setupOptions.projectItem.fullPathWithFilename );
				else
				{
					var list = await Addressables.LoadResourceLocationsAsync( $"{DataStore.languageCodeList[DataStore.languageCode].ToUpper()}-{setupOptions.projectItem.fullPathWithFilename}" ).Task;
					if ( list != null && list.Count > 0 )
					{
						Debug.Log( "OnStartMission::Found translation::" + $"{DataStore.languageCodeList[DataStore.languageCode].ToUpper()}-{setupOptions.projectItem.fullPathWithFilename}" );
						StartMission( $"{DataStore.languageCodeList[DataStore.languageCode].ToUpper()}-{setupOptions.projectItem.fullPathWithFilename}" );
					}
					else
					{
						Debug.Log( "OnStartMission::No translation found for "
							+ $"{DataStore.languageCodeList[DataStore.languageCode].ToUpper()}-{setupOptions.projectItem.fullPathWithFilename}" );
						StartMission( setupOptions.projectItem.fullPathWithFilename );
					}
				}
			}
		}

		/// <summary>
		/// start an official Mission
		/// </summary>
		void StartMission( string missionAddressableKey )
		{
			FileManager.LoadMissionFromAddressable( missionAddressableKey, ( m, s ) =>
			{
				DataStore.mission = m;
				if ( DataStore.mission != null )
				{
					DataStore.sagaSessionData.missionStringified = s;
					Warp();
				}
				else
					errorPanel.Show( "StartMission()", $"Could not load mission:\n'{missionAddressableKey}'" );
			} );
		}

		public void AddHero()
		{
			sound.PlaySound( FX.Click );
			EventSystem.current.SetSelectedGameObject( null );
			addHeroPanel.Show( CharacterType.Hero, () =>
			 {
				 UpdateHeroes();
			 } );
		}

		public void AddAlly()
		{
			sound.PlaySound( FX.Click );
			EventSystem.current.SetSelectedGameObject( null );
			if ( DataStore.sagaSessionData.selectedAlly == null )
			{
				addHeroPanel.Show( CharacterType.Ally, () =>
				{
					UpdateHeroes();
				} );
			}
			else
			{
				DataStore.sagaSessionData.selectedAlly = null;
				allyImage.gameObject.SetActive( false );
			}
		}

		public void OnIgnored()
		{
			sound.PlaySound( FX.Click );
			modifyGroupsPanel.Show( GroupSelectionMode.Ignored, disabledIgnoredGroups, () =>
			{
				int impCount = DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Imperial ).Count() - DataStore.sagaSessionData.globalImportedCharacters.Count;
				languageController.UpdateIgnoredCount( DataStore.sagaSessionData.MissionIgnored.Count + impCount );
			} );
		}

		public void OnVillains()
		{
			sound.PlaySound( FX.Click );
			modifyGroupsPanel.Show( GroupSelectionMode.Villains, null, () =>
			{
				languageController.UpdateVillainCount( DataStore.sagaSessionData.EarnedVillains.Count );
			} );
		}

		public void RemoveHero( DeploymentCard card )
		{
			DataStore.sagaSessionData.MissionHeroes.Remove( card );
			UpdateHeroes();
		}

		void UpdateHeroes()
		{
			for ( int i = 0; i < heroContainer.childCount; i++ )
			{
				Destroy( heroContainer.GetChild( i ).gameObject );
			}
			foreach ( var item in DataStore.sagaSessionData.MissionHeroes )
			{
				var mug = Instantiate( miniMugPrefab, heroContainer );
				mug.GetComponent<MiniMug>().Init( item );
			}
			if ( DataStore.sagaSessionData.MissionHeroes.Count > 0 )
				heroContainer.parent.GetChild( 2 ).gameObject.SetActive( false );
			else
				heroContainer.parent.GetChild( 2 ).gameObject.SetActive( true );

			//ally
			if ( DataStore.sagaSessionData.selectedAlly == null )
				allyImage.gameObject.SetActive( false );
			else
			{
				allyImage.gameObject.SetActive( true );
				allyImage.sprite = Resources.Load<Sprite>( DataStore.sagaSessionData.selectedAlly.mugShotPath );
			}
		}

		/// <summary>
		/// Official mission selected, pi is guaranteed not null
		/// </summary>
		public void OnOfficialMissionSelected( ProjectItem pi )
		{
			//clear ignored groups, banned allies, and custom heroes/allies
			DataStore.sagaSessionData.MissionIgnored.Clear();
			DataStore.sagaSessionData.BannedAllies.Clear();
			missionCustomAllies.Clear();
			missionCustomHeroes.Clear();
			missionCustomVillains.Clear();
			disabledIgnoredGroups.Clear();
			//remove any embedded villains from another Mission, otherwise they won't appear
			DataStore.sagaSessionData.EarnedVillains = DataStore.sagaSessionData.EarnedVillains.Where( x => !x.id.Contains( "TC" ) ).ToList();

			var expansion = pi.missionID.Split( ' ' )[0].ToLower();
			var id = pi.missionID.Split( ' ' )[1].ToLower();
			var presets = DataStore.missionPresets[expansion];
			var mp = presets.Where( x => x.id.ToLower() == $"{expansion}{id}" ).FirstOr( null );

			//ignore "Other" expansion enemy groups by default, except owned packs
			var ignored = new HashSet<DeploymentCard>( DataStore.deploymentCards.Where( x => x.expansion == "Other" && !DataStore.ownedFigurePacks.ContainsCard( x ) ) );

			if ( mp != null )
			{
				//get ignored from preset, even if they are owned packs
				var ign = from c in DataStore.deploymentCards join i in mp.ignoredGroups on c.id equals i select c;
				ign.ToList().ForEach( x => ignored.Add( x ) );
				//add ignored from preset
				disabledIgnoredGroups.AddRange( ign );

				threatValue.ResetWheeler( mp.defaultThreat );
			}
			else
				errorPanel.Show( "OnMissionSelected()", "MissionPreset is null" );

			//add the uniquely hashed set of ignored to the real list
			DataStore.sagaSessionData.MissionIgnored.AddRange( ignored );
			int impCount = DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Imperial ).Count() - DataStore.sagaSessionData.globalImportedCharacters.Count;
			languageController.UpdateIgnoredCount( DataStore.sagaSessionData.MissionIgnored.Count + impCount );

			//add banned allies and custom heroes/allies
			Mission m = FileManager.LoadMissionFromString( pi.stringifiedMission );
			if ( m != null )
			{
				//add custom allies/heroes/villains embedded in Mission
				missionCustomAllies = m.customCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Ally ).Select( x => x.deploymentCard ).ToList();
				missionCustomHeroes = m.customCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Hero ).Select( x => x.deploymentCard ).ToList();
				missionCustomVillains = m.customCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Villain ).Select( x => x.deploymentCard ).ToList();

				if ( m.missionProperties.useBannedAlly == YesNoAll.Yes )
					DataStore.sagaSessionData.BannedAllies.Add( m.missionProperties.bannedAlly );
				else if ( m.missionProperties.useBannedAlly == YesNoAll.Multi )
					DataStore.sagaSessionData.BannedAllies.AddRange( m.missionProperties.multipleBannedAllies );
				else if ( m.missionProperties.useBannedAlly == YesNoAll.All )
					DataStore.sagaSessionData.BannedAllies.AddRange( DataStore.allyCards.Select( x => x.id ) );
			}
			else
				errorPanel.Show( "OnMissionSelected()", "Mission is null" );
		}

		/// <summary>
		/// Custom mission selected, pi is guaranteed not null
		/// </summary>
		public void OnCustomMissionSelected( ProjectItem pi )
		{
			//clear ignored groups, banned allies, and custom heroes/allies
			DataStore.sagaSessionData.MissionIgnored.Clear();
			DataStore.sagaSessionData.BannedAllies.Clear();
			missionCustomAllies.Clear();
			missionCustomHeroes.Clear();
			missionCustomVillains.Clear();
			disabledIgnoredGroups.Clear();
			//remove any embedded villains from another Mission, otherwise they won't appear
			DataStore.sagaSessionData.EarnedVillains = DataStore.sagaSessionData.EarnedVillains.Where( x => !x.id.Contains( "TC" ) ).ToList();

			//set default values to UI - they don't exist in a custom mission
			threatValue.ResetWheeler( 3 );

			Mission m = FileManager.LoadMissionFromString( pi.stringifiedMission );
			if ( m != null )
			{
				//add custom allies/heroes/villains embedded in Mission
				missionCustomAllies = m.customCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Ally ).Select( x => x.deploymentCard ).ToList();
				missionCustomHeroes = m.customCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Hero ).Select( x => x.deploymentCard ).ToList();
				missionCustomVillains = m.customCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Villain ).Select( x => x.deploymentCard ).ToList();

				//ignore "Other" expansion enemy groups by default, except owned packs
				var ignored = new HashSet<DeploymentCard>( DataStore.deploymentCards.Where( x => x.expansion == "Other" && !DataStore.ownedFigurePacks.ContainsCard( x ) ) );
				//get ignored from mission, even if they are owned packs
				var ign = from c in DataStore.deploymentCards join i in m.missionProperties.bannedGroups on c.id equals i select c;
				ign.ToList().ForEach( x => ignored.Add( x ) );
				//add ignored from mission
				disabledIgnoredGroups.AddRange( ign );

				//add the uniquely hashed set of ignored to the real list
				DataStore.sagaSessionData.MissionIgnored.AddRange( ignored );
				int impCount = DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Imperial ).Count() - DataStore.sagaSessionData.globalImportedCharacters.Count;
				languageController.UpdateIgnoredCount( DataStore.sagaSessionData.MissionIgnored.Count + impCount );

				//add banned allies
				if ( m.missionProperties.useBannedAlly == YesNoAll.Yes )
					DataStore.sagaSessionData.BannedAllies.Add( m.missionProperties.bannedAlly );
				else if ( m.missionProperties.useBannedAlly == YesNoAll.Multi )
					DataStore.sagaSessionData.BannedAllies.AddRange( m.missionProperties.multipleBannedAllies );
				else if ( m.missionProperties.useBannedAlly == YesNoAll.All )
					DataStore.sagaSessionData.BannedAllies.AddRange( DataStore.allyCards.Select( x => x.id ) );
			}
			else
				errorPanel.Show( "OnMissionSelected()", $"Could not load mission:\n'{pi.fullPathWithFilename}'" );
		}

		public void OnModeChange( PickerMode pmode )
		{
			if ( pmode == PickerMode.Custom )
			{
				descriptionTextBox.gameObject.SetActive( true );
				viewMissionCardButton.gameObject.SetActive( false );
			}
			else
			{
				descriptionTextBox.gameObject.SetActive( false );
				viewMissionCardButton.gameObject.SetActive( true );
			}
		}

		public void OnDifficulty( Button thisButton )
		{
			sound.PlaySound( FX.Click );
			difficultyText.text = setupOptions.ToggleDifficulty();
		}

		public void OnAdaptiveDifficulty( Button thisButton )
		{
			sound.PlaySound( FX.Click );
			setupOptions.useAdaptiveDifficulty = !setupOptions.useAdaptiveDifficulty;
			thisButton.colors = setupOptions.useAdaptiveDifficulty ? greenBlock : redBlock;
		}

		public void OnViewMissionCard()
		{
			if ( missionPicker.pickerMode == PickerMode.BuiltIn )
			{
				sound.PlaySound( FX.Click );

				MissionCard mc;
				string mID = missionPicker.selectedMission.missionID.Replace( " ", "" );
				foreach ( var key in DataStore.missionCards.Keys )
				{
					mc = DataStore.missionCards[key].Where( x => x.id == mID ).FirstOr( null );
					if ( mc != null )
					{
						missionCardZoom.Show( mc );
						break;
					}
				}
			}
		}

		public void Warp()
		{
			sound.PlaySound( FX.Click );
			sound.FadeOutMusic();

			thrusterRoot.DOMoveZ( -30, 2 );
			warpTitleText.text = setupOptions.projectItem.Title;

			faderCG.DOFade( 0, .5f ).OnComplete( () =>
			{
				warpTitleText.transform.DOMove( warpTitleText.transform.position + warpTitleText.transform.up * 100f, 5 );
				warpTitleText.DOFade( 1, 2 );

				sound.PlaySound( 1 );
				sound.PlaySound( 2 );

				GlowTimer.SetTimer( 1.5f, () => warpEffect.SetActive( true ) );
				GlowTimer.SetTimer( 5, () =>
				{
					DOTween.To( () => theCamera.fieldOfView, x => theCamera.fieldOfView = x, 0, .25f )
					.OnComplete( () =>
					{
						//all effects/music finish, load the mission
						GlowTimer.SetTimer( 3, () =>
						{
							SceneManager.LoadScene( "Saga" );
						} );
					} );
				} );
			} );
		}

		private void Update()
		{
			if ( DataStore.sagaSessionData.MissionHeroes.Count > 0
				&& missionPicker.selectedMission != null
				&& !missionPicker.isBusy )
			{
				startMissionButton.interactable = true;
			}
			else
			{
				startMissionButton.interactable = false;
			}

			if ( missionPicker.selectedMission != null
				&& !missionPicker.isBusy )
			{
				viewMissionCardButton.interactable = true;
			}
			else
			{
				viewMissionCardButton.interactable = false;
			}
		}
	}
}
