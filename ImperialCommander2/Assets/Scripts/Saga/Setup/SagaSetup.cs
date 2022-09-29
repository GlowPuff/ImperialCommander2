using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Saga
{
	public class SagaSetup : MonoBehaviour
	{
		public Camera theCamera;
		//UI TRINKETS
		public GameObject descriptionTextBox;
		public Text difficultyText, initialText, additionalText;
		public Transform heroContainer;
		public Button adaptiveButton, startMissionButton, viewMissionCardButton;
		public GameObject miniMugPrefab;
		public Image allyImage, allyBG;
		public MWheelHandler threatValue, addtlThreatValue;
		public TextMeshProUGUI versionText, additionalInfoText;
		//UI PANELS
		public SagaAddHeroPanel addHeroPanel;
		public SagaModifyGroupsPanel modifyGroupsPanel;
		public MissionCardZoom missionCardZoom;
		//OTHER
		public GameObject warpEffect, rightPanel;
		public Transform thrusterRoot, thrusterLeft, thrusterRight;
		public SagaSetupLanguageController languageController;
		public CanvasGroup faderCG;
		public ColorBlock redBlock;
		public ColorBlock greenBlock;
		public VolumeProfile volume;
		public MissionPicker missionPicker;

		Sound sound;
		SagaSetupOptions setupOptions { get; set; }
		bool isFromCampaign = false;

		void Awake()
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

			versionText.text = "App Mission Format: " + Utils.formatVersion;

			Screen.sleepTimeout = SleepTimeout.NeverSleep;

			int pixelHeightOfCurrentScreen = Screen.height;//.currentResolution.height;
			int pixelWidthOfCurrentScreen = Screen.width;//.currentResolution.width;
			float aspect = pixelWidthOfCurrentScreen / pixelHeightOfCurrentScreen;
			if ( aspect >= 2f )
			{
				thrusterLeft.position = new Vector3( -12f, thrusterLeft.position.y, thrusterLeft.position.z );
				thrusterRight.position = new Vector3( 12f, thrusterRight.position.y, thrusterRight.position.z );
			}

			//bootstrap the setup screen for debugging
			//bootstrapDEBUG();

			//set translated UI
			languageController.SetTranslatedUI();

			//apply settings
			if ( volume.TryGet<Bloom>( out var bloom ) )
				bloom.active = PlayerPrefs.GetInt( "bloom" ) == 1;
			if ( volume.TryGet<Vignette>( out var vig ) )
				vig.active = PlayerPrefs.GetInt( "vignette" ) == 1;

			sound = FindObjectOfType<Sound>();
			sound.CheckAudio();

			setupOptions = new SagaSetupOptions();
			DataStore.StartNewSagaSession( setupOptions );
			//check if we're loading in from  the campaign manager
			//single shot mission, not from a campaign
			if ( RunningCampaign.campaignStructure == null )
			{
				threatValue.ResetWheeler( 0 );
			}
			else//from campaign manager
			{
				//setupOptions is set here
				SetupCampaignMission();
			}

			ResetSetup();
			UpdateHeroes();
			addtlThreatValue.ResetWheeler();

			faderCG.alpha = 0;
			faderCG.DOFade( 1, .5f );
		}

		void bootstrapDEBUG()
		{
			DataStore.InitData();
		}

		/// <summary>
		/// set default mission options, add default ignored groups
		/// </summary>
		public void ResetSetup()
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
			initialText.text = setupOptions.threatLevel.ToString();
			//additional threat value
			additionalText.text = setupOptions.addtlThreat.ToString();
			//clear ignored groups
			DataStore.sagaSessionData.MissionIgnored.Clear();
			//add default ignored
			//ignore "Other" expansion enemy groups by default
			DataStore.sagaSessionData.MissionIgnored.AddRange( DataStore.deploymentCards.Where( x => x.expansion == "Other" ) );
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
				return;
			}

			var structure = RunningCampaign.campaignStructure;
			//deactivate mission picker
			rightPanel.SetActive( false );
			//add heroes
			foreach ( var item in campaign.campaignHeroes )
			{
				var hero = DataStore.GetHero( item.heroID );
				DataStore.sagaSessionData.MissionHeroes.Add( hero );
			}

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
			if ( structure.missionID == "Custom" )
			{
				missionPicker.pickerMode = PickerMode.Custom;
				descriptionTextBox.gameObject.SetActive( true );
				viewMissionCardButton.gameObject.SetActive( false );
			}
			else
			{
				missionPicker.pickerMode = PickerMode.BuiltIn;
				descriptionTextBox.gameObject.SetActive( false );
				viewMissionCardButton.gameObject.SetActive( true );
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
					Warp();
			}
			else
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

		void StartMission( string missionAddressableKey )
		{
			AsyncOperationHandle<TextAsset> loadHandle = Addressables.LoadAssetAsync<TextAsset>( missionAddressableKey );
			loadHandle.Completed += ( x ) =>
			{
				if ( x.Status == AsyncOperationStatus.Succeeded )
				{
					DataStore.sagaSessionData.missionStringified = x.Result.text;
					DataStore.mission = FileManager.LoadMissionFromString( x.Result.text );
					if ( DataStore.mission != null )
						Warp();
				}
				Addressables.Release( loadHandle );
			};
		}

		public void AddHero()
		{
			sound.PlaySound( FX.Click );
			EventSystem.current.SetSelectedGameObject( null );
			addHeroPanel.Show( 0, () =>
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
				addHeroPanel.Show( 1, () =>
				{
					UpdateHeroes();
				} );
			}
			else
			{
				DataStore.sagaSessionData.selectedAlly = null;
				allyImage.gameObject.SetActive( false );
				allyBG.gameObject.SetActive( false );
			}
		}

		public void OnIgnored()
		{
			sound.PlaySound( FX.Click );
			modifyGroupsPanel.Show( 0 );
		}

		public void OnVillains()
		{
			sound.PlaySound( FX.Click );
			modifyGroupsPanel.Show( 1 );
		}

		public void RemoveHero( DeploymentCard card )
		{
			DataStore.sagaSessionData.MissionHeroes.Remove( card );
			UpdateHeroes();
		}

		void UpdateHeroes()
		{
			for ( int i = 1; i < heroContainer.childCount; i++ )
			{
				Destroy( heroContainer.GetChild( i ).gameObject );
			}
			foreach ( var item in DataStore.sagaSessionData.MissionHeroes )
			{
				var mug = Instantiate( miniMugPrefab, heroContainer );
				mug.transform.GetChild( 0 ).GetComponent<Image>().sprite = Resources.Load<Sprite>( $"Cards/Heroes/{item.id}" );
				mug.GetComponent<MiniMug>().card = item;
			}
			if ( DataStore.sagaSessionData.MissionHeroes.Count > 0 )
				heroContainer.parent.GetChild( 0 ).gameObject.SetActive( false );
			else
				heroContainer.parent.GetChild( 0 ).gameObject.SetActive( true );

			//ally
			allyBG.gameObject.SetActive( false );
			if ( DataStore.sagaSessionData.selectedAlly == null )
				allyImage.gameObject.SetActive( false );
			else
			{
				allyImage.gameObject.SetActive( true );
				allyImage.sprite = Resources.Load<Sprite>( $"Cards/Allies/{DataStore.sagaSessionData.selectedAlly.id.Replace( "A", "M" )}" );
				if ( DataStore.sagaSessionData.selectedAlly.isElite )
					allyBG.gameObject.SetActive( true );
			}
		}

		/// <summary>
		/// Official mission selected
		/// </summary>
		public void OnMissionSelected( MissionPreset mp )
		{
			//clear ignored groups
			DataStore.sagaSessionData.MissionIgnored.Clear();
			//add default ignored
			//ignore "Other" expansion enemy groups by default
			DataStore.sagaSessionData.MissionIgnored.AddRange( DataStore.deploymentCards.Where( x => x.expansion == "Other" ) );

			if ( mp != null )
			{
				//get ignored from preset
				var ign = from c in DataStore.deploymentCards join i in mp.ignoredGroups on c.id equals i select c;
				DataStore.sagaSessionData.MissionIgnored.AddRange( ign );

				threatValue.ResetWheeler( mp.defaultThreat );
				initialText.text = mp.defaultThreat.ToString();
			}
		}

		/// <summary>
		/// Custom mission selected
		/// </summary>
		public void OnMissionSelected( ProjectItem pi )
		{
			threatValue.ResetWheeler( 3 );
			initialText.text = "3";

			Mission m = FileManager.LoadMission( pi.fullPathWithFilename );
			if ( m != null )
			{
				//clear ignored groups
				DataStore.sagaSessionData.MissionIgnored.Clear();
				//add default ignored
				//ignore "Other" expansion enemy groups by default
				DataStore.sagaSessionData.MissionIgnored.AddRange( DataStore.deploymentCards.Where( x => x.expansion == "Other" ) );
				//get ignored from mission
				var ign = from c in DataStore.deploymentCards join i in m.missionProperties.bannedGroups on c.id equals i select c;
				DataStore.sagaSessionData.MissionIgnored.AddRange( ign );
			}
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

			faderCG.DOFade( 0, .5f ).OnComplete( () =>
			{
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
