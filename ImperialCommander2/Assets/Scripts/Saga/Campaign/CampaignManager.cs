using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Saga
{
	public class CampaignManager : MonoBehaviour
	{
		[HideInInspector]
		public SagaCampaign sagaCampaign;

		public Fader fader;
		public VolumeProfile volume;
		public GameObject leftPanel, rightPanel, switchButton;
		//prefabs
		public GameObject forceMissionItemPrefab, listItemPrefab, missionItemPrefab, customAddMissionBarPrefab;
		//UI
		public CampaignJournal campaignJournal;
		public AddCampaignItemPopup addCampaignItemPopup;
		public ModifyCustomPropsPopup modifyCustomPropsPopup;
		public TMP_InputField campaignNameInputField;
		public Transform villainContainer, allyContainer, itemContainer, structureContainer, rewardContainer;
		public CampaignHeroPrefab[] heroPrefabs;
		public TextMeshProUGUI xpText, creditsText, fameText, awardsText;
		public MWheelHandler xpWheel, creditsWheel, fameWheel, awardsWheel;
		public Text campaignExpansion;
		public CanvasGroup topButtonsGroup, rightPanelGroup;
		//translatable UI
		public CampaignLanguageController languageController;

		Sound sound;
		int view = 1;//0=left, 1=right

		void Start()
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

			Screen.sleepTimeout = SleepTimeout.NeverSleep;

			float pixelHeightOfCurrentScreen = Screen.height;//.currentResolution.height;
			float pixelWidthOfCurrentScreen = Screen.width;//.currentResolution.width;
			float aspect = pixelWidthOfCurrentScreen / pixelHeightOfCurrentScreen;
			if ( aspect < 1.7f )//less than 16:9, such as 16:10 and 4:3
			{
				//switch to single view
				switchButton.SetActive( true );
				leftPanel.SetActive( false );
			}

			//re-initialize all card data, otherwise deployed cards and other items carry over when coming back from a campaign game
			DataStore.InitData();

			//BOOTSTRAP CAMPAIGN
			bootstrapCampaign( true );//true = production build

			fader.UnFade( 2 );

			//apply settings
			sound = FindObjectOfType<Sound>();
			sound.CheckAudio();

			//set translated UI
			languageController.SetTranslatedUI();

			if ( volume.TryGet<Bloom>( out var bloom ) )
				bloom.active = PlayerPrefs.GetInt( "bloom" ) == 1;
			if ( volume.TryGet<Vignette>( out var vig ) )
				vig.active = PlayerPrefs.GetInt( "vignette" ) == 1;

			//popuplate UI with loaded campaign data
			InitUI();
		}

		void bootstrapCampaign( bool isProduction )
		{
			Debug.Log( $"***BOOTSTRAP (Campaign Manager) PRODUCTION={isProduction}***" );
			if ( !isProduction )
				DataStore.InitData();

			//campaign is already setup from Title screen
			if ( RunningCampaign.sagaCampaignGUID != null && RunningCampaign.sagaCampaignGUID != Guid.Empty )
			{
				sagaCampaign = SagaCampaign.LoadCampaignState( RunningCampaign.sagaCampaignGUID );
			}
			else//error or debugging, setup new test campaign
			{
				RunningCampaign.expansionCode = "Core";
				sagaCampaign = SagaCampaign.CreateNewCampaign( "Error/Debug - Not Found", RunningCampaign.expansionCode );
			}
		}

		/// <summary>
		/// fill in name and item/villain/ally lists from campaign data
		/// </summary>
		private void InitUI()
		{
			campaignNameInputField.text = sagaCampaign.campaignName;
			//use translated expansion name
			if ( sagaCampaign.campaignExpansionCode != "Custom" )
				campaignExpansion.text = DataStore.translatedExpansionNames[sagaCampaign.campaignExpansionCode];
			else
				campaignExpansion.text = DataStore.uiLanguage.uiCampaign.customCampaign;

			creditsWheel.ResetWheeler( sagaCampaign.credits );
			xpWheel.ResetWheeler( sagaCampaign.XP );
			fameWheel.ResetWheeler( sagaCampaign.fame );
			awardsWheel.ResetWheeler( sagaCampaign.awards );

			//items
			foreach ( var item in sagaCampaign.campaignItems )
				AddItemToUI( sagaCampaign.GetItemFromID( item ) );
			//allies
			foreach ( var id in sagaCampaign.campaignAllies )
				AddAllyToUI( DataStore.GetAlly( id ) );
			foreach ( var id in sagaCampaign.campaignVillains )
				AddVillainToUI( DataStore.GetEnemy( id ) );
			//heroes
			int c = sagaCampaign.campaignHeroes.Count;
			for ( int i = 0; i < c; i++ )
				heroPrefabs[i].AddHeroToUI( sagaCampaign.campaignHeroes[i] );
			//rewards
			foreach ( var item in sagaCampaign.campaignRewards )
				AddRewardToUI( sagaCampaign.GetRewardFromID( item ) );
			//campaign structure
			foreach ( Transform item in structureContainer )
				Destroy( item.gameObject );
			//if it's a custom campaign, add the custom add mission bar
			if ( sagaCampaign.campaignExpansionCode == "Custom" )
			{
				var cgo = Instantiate( customAddMissionBarPrefab, structureContainer );
			}
			//add campaign structure
			foreach ( var item in sagaCampaign.campaignStructure )
			{
				var go = Instantiate( missionItemPrefab, structureContainer );
				go.GetComponent<MissionItemPrefab>().Init( item );
			}
			//add forced mission bar
			if ( sagaCampaign.campaignExpansionCode != "Custom" )
			{
				//add 1 force mission item
				var fgo = Instantiate( forceMissionItemPrefab, structureContainer );
				fgo.GetComponent<ForceMissionItemPrefab>().Init( OnAddForcedMissionClick );
			}
		}

		#region UI callbacks from "AddCampaignItemPopup" window
		void AddAllyToUI( DeploymentCard a )
		{
			var go = Instantiate( listItemPrefab, allyContainer );
			go.GetComponent<CampaignListItemPrefab>().InitAlly( a.name, ( n ) =>
			{
				sagaCampaign.campaignAllies.Remove( a.id );
				Destroy( go );
			} );
		}

		void AddVillainToUI( DeploymentCard v )
		{
			var go = Instantiate( listItemPrefab, villainContainer );
			go.GetComponent<CampaignListItemPrefab>().InitVillain( v.name, ( n ) =>
			{
				sagaCampaign.campaignVillains.Remove( v.id );
				Destroy( go );
			} );
		}

		void AddItemToUI( CampaignItem item )
		{
			var go = Instantiate( listItemPrefab, itemContainer );
			go.GetComponent<CampaignListItemPrefab>().InitGeneralItem( item.name, ( n ) =>
			{
				sagaCampaign.campaignItems.Remove( item.id );
				Destroy( go );
			} );
		}

		void AddRewardToUI( CampaignReward item )
		{
			var go = Instantiate( listItemPrefab, rewardContainer );
			go.GetComponent<CampaignListItemPrefab>().InitGeneralItem( item.name, ( n ) =>
			{
				sagaCampaign.campaignRewards.Remove( item.id );
				Destroy( go );
			} );
		}

		public void OnXPChanged()
		{
			sagaCampaign.XP = xpWheel.wheelValue;
		}

		public void OnCreditsChanged()
		{
			sagaCampaign.credits = creditsWheel.wheelValue;
		}

		public void OnFameChanged()
		{
			sagaCampaign.fame = fameWheel.wheelValue;
		}

		public void OnAwardsChanged()
		{
			sagaCampaign.awards = awardsWheel.wheelValue;
		}
		#endregion

		#region ADD BUTTONS
		public void OnAddAlly()
		{
			addCampaignItemPopup.AddAlly( ( a ) =>
			{
				if ( !sagaCampaign.campaignAllies.Contains( a.id ) )
				{
					sagaCampaign.campaignAllies.Add( a.id );
					AddAllyToUI( a );
				}
			} );
		}

		public void OnAddVillain()
		{
			addCampaignItemPopup.AddVillain( ( v ) =>
			{
				if ( !sagaCampaign.campaignVillains.Contains( v.id ) )
				{
					sagaCampaign.campaignVillains.Add( v.id );
					AddVillainToUI( v );
				}
			} );
		}

		public void OnAddItem()
		{
			addCampaignItemPopup.AddItem( ( item ) =>
			{
				if ( !sagaCampaign.campaignItems.Contains( item.id ) )
				{
					sagaCampaign.campaignItems.Add( item.id );
					AddItemToUI( item );
				}
			}, false );
		}

		public void OnAddReward()
		{
			addCampaignItemPopup.AddReward( ( item ) =>
			{
				if ( !sagaCampaign.campaignRewards.Contains( item.id ) )
				{
					sagaCampaign.campaignRewards.Add( item.id );
					AddRewardToUI( item );
				}
			} );
		}
		#endregion

		public void AddHeroToCampaign( CampaignHero hero )
		{
			sagaCampaign.campaignHeroes.Add( hero );
		}

		public void RemoveHeroFromCampaign( CampaignHero hero )
		{
			sagaCampaign.campaignHeroes.Remove( hero );
		}

		public void OnAddForcedMissionClick()
		{
			addCampaignItemPopup.AddMission( sagaCampaign.campaignExpansionCode, MissionType.Forced, AddForcedMission );
		}

		public void OnMissionNameClick( MissionType missionType, Action<MissionCard> callback )
		{
			addCampaignItemPopup.AddMission( sagaCampaign.campaignExpansionCode, missionType, callback );
		}

		public void OnAddCustomMission( MissionType missionType )
		{
			modifyCustomPropsPopup.Show( new CampaignModify()
			{
				missionType = missionType,
				threatValue = 1,
				itemTierArray = new string[] { "1" },
				missionID = "",
				expansionCode = ""
			}, AddCustomMission );
		}

		public void OnModifyCustomMission( MissionItemPrefab missionItem )
		{
			CampaignModify modifier = new CampaignModify()
			{
				missionType = missionItem.campaignStructure.missionType,
				threatValue = missionItem.campaignStructure.threatLevel,
				itemTierArray = missionItem.campaignStructure.itemTier,
				agendaToggle = missionItem.campaignStructure.isAgendaMission,
				missionID = missionItem.campaignStructure.missionID,
				expansionCode = missionItem.campaignStructure.expansionCode
			};

			modifyCustomPropsPopup.Show( modifier, ( mod ) =>
			{
				missionItem.OnModify( mod );
				//go back to normal campaign list mode
				FindObjectOfType<CustomAddMissionBarPrefab>().DeactivateModifyMode();
			} );
		}

		void AddCustomMission( CampaignModify modifier )
		{
			var cs = new CampaignStructure()
			{
				missionType = modifier.missionType,
				missionID = modifier.missionID,
				threatLevel = modifier.threatValue,
				itemTier = modifier.itemTierArray,
				expansionCode = modifier.expansionCode,
				isAgendaMission = modifier.agendaToggle,
				isCustom = true
			};
			//add the newly added mission
			var go = Instantiate( missionItemPrefab, structureContainer );
			go.GetComponent<MissionItemPrefab>().Init( cs );
		}

		void AddForcedMission( MissionCard card )
		{
			var cs = new CampaignStructure()
			{
				missionType = MissionType.Forced,
				missionID = card.id,
				threatLevel = SagaCampaign.GetMissionPreset( card.expansion, card.id ).defaultThreat,
				isForced = true,
				expansionCode = card.expansion.ToString(),
				projectItem = new ProjectItem()
				{
					missionID = card.id,
					Description = card.descriptionText,
					AdditionalInfo = card.bonusText,
					fullPathWithFilename = $"{card.id.ToUpper()}-{card.name}",
					Title = DataStore.missionCards[card.expansion.ToString()].Where( x => x.id == card.id ).FirstOr( new MissionCard() { name = card.name } )?.name
				}
			};
			//remove the "add forced mission" prefab
			foreach ( Transform item in structureContainer )
			{
				if ( item.GetComponent<ForceMissionItemPrefab>() != null )
					Destroy( item.gameObject );
			}
			//add the newly added mission
			var go = Instantiate( missionItemPrefab, structureContainer );
			go.GetComponent<MissionItemPrefab>().Init( cs );
			//add the "forced mission" prefab back to bottom of list
			var fgo = Instantiate( forceMissionItemPrefab, structureContainer );
			fgo.GetComponent<ForceMissionItemPrefab>().Init( OnAddForcedMissionClick );
		}

		public void RemoveMission( Guid GUID )
		{
			foreach ( Transform item in structureContainer )
			{
				var pf = item.GetComponent<MissionItemPrefab>();
				if ( pf != null )
				{
					if ( pf.campaignStructure.isCustom )
						FindObjectOfType<CustomAddMissionBarPrefab>().DeactivateModifyMode();
					if ( pf.campaignStructure.GUID == GUID )
						Destroy( item.gameObject );
				}
			}
		}

		public void ToggleDisableUI( bool disable )
		{
			topButtonsGroup.interactable = rightPanelGroup.interactable = disable;
			foreach ( Transform item in structureContainer )
			{
				var prefab = item.GetComponent<MissionItemPrefab>();
				if ( prefab != null )
				{
					prefab.ToggleModifyMode( !disable );
				}
			}
		}

		public void OnSaveCampaign()
		{
			var list = new List<CampaignStructure>();
			foreach ( Transform item in structureContainer )
			{
				var cs = item.GetComponent<MissionItemPrefab>();
				if ( cs != null )
					list.Add( cs.campaignStructure );
			}

			sagaCampaign.SaveCampaignState( list );
		}

		public void OnInfoClicked()
		{
			GlowEngine.FindUnityObject<CampaignMessagePopup>().Show( DataStore.uiLanguage.uiCampaign.campaignSetup, Utils.ReplaceGlyphs( sagaCampaign.GetCampaignInfo() ), 700 );
		}

		public void OnJournalClicked()
		{
			campaignJournal.Show( sagaCampaign.campaignJournal, ( text ) => sagaCampaign.campaignJournal = text );
		}

		public void OnSwitchViewClicked()
		{
			view = view == 0 ? 1 : 0;
			leftPanel.SetActive( view == 0 );
			rightPanel.SetActive( view == 1 );
		}

		public void OnExitCampaignScreen()
		{
			//auto-save
			OnSaveCampaign();

			RunningCampaign.Reset();

			fader.Fade();
			float foo = 1;
			DOTween.To( () => foo, x => foo = x, 0, .5f ).OnComplete( () =>
			 SceneManager.LoadScene( "Title" ) );
		}

		public void OnEndEditCampaignName()
		{
			sagaCampaign.campaignName = campaignNameInputField.text;
		}

		public void StartMission( CampaignStructure campaignStructure )
		{
			//auto-save
			OnSaveCampaign();

			RunningCampaign.campaignStructure = campaignStructure;
			RunningCampaign.sagaCampaignGUID = sagaCampaign.GUID;

			fader.Fade();
			float foo = 1;
			DOTween.To( () => foo, x => foo = x, 0, .5f ).OnComplete( () =>
			 SceneManager.LoadScene( "SagaSetup" ) );
		}
	}
}