using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Saga
{
	public class AddCampaignItemPopup : MonoBehaviour
	{
		public PopupBase popupBase;
		public GameObject itemSkillPrefab, toonPrefab, itemSkillSelectorPrefab, tierFilterContainer, rewardFilterContainer;
		public Transform itemContainer;
		public ScrollRect scrollRect;
		public TMP_Dropdown expansionDropdown, rewardFilterDropdown;
		public RectTransform scrollRectTransform;
		public Text cancelText;
		public Toggle tier1Toggle, tier2Toggle, tier3Toggle;

		Action<DeploymentCard> addHeroCallback, addAllyCallback, addVillainCallback;
		Action<CampaignItem> addItemCallback;
		Action<CampaignSkill> addSkillCallback;
		Action<MissionCard> addMissionCallback;
		Action<CampaignReward> addRewardCallback;

		string selectedExpansion = "";
		List<string> expansionCodes = new List<string>();
		bool blockToggle = true;
		bool useGeneralItemList = false;
		//scrollview top=60

		void Show()
		{
			EventSystem.current.SetSelectedGameObject( null );
			popupBase.Show();

			cancelText.text = DataStore.uiLanguage.uiSetup.cancel;

			scrollRect.normalizedPosition = new Vector2( 0, 200 );
			scrollRectTransform.offsetMax = new Vector2( scrollRectTransform.offsetMax.x, -60 );
			expansionDropdown.gameObject.SetActive( false );
		}

		#region CampaignManager callback delegates
		public void AddHero( Action<DeploymentCard> callback )
		{
			foreach ( Transform item in itemContainer )
				Destroy( item.gameObject );

			var sc = FindObjectOfType<CampaignManager>().sagaCampaign;
			var ch = new HashSet<string>( sc.campaignHeroes.Select( x => x.heroID ) );
			var h = DataStore.heroCards.Where( x => !ch.Contains( x.id ) );

			foreach ( var item in h )//DataStore.heroCards )
			{
				var go = Instantiate( toonPrefab, itemContainer );
				go.GetComponent<ToonSelectorPrefab>().InitHero( item );
			}

			addHeroCallback = callback;
			Show();
		}

		public void AddAlly( Action<DeploymentCard> callback )
		{
			foreach ( Transform item in itemContainer )
				Destroy( item.gameObject );

			foreach ( var item in DataStore.allyCards )
			{
				var go = Instantiate( toonPrefab, itemContainer );
				go.GetComponent<ToonSelectorPrefab>().InitAlly( item );
			}

			addAllyCallback = callback;
			Show();
		}

		public void AddVillain( Action<DeploymentCard> callback )
		{
			foreach ( Transform item in itemContainer )
				Destroy( item.gameObject );

			var sc = FindObjectOfType<CampaignManager>().sagaCampaign.campaignVillains;

			//omit villains already added to campaign
			foreach ( var item in DataStore.villainCards.Where( x => !sc.Contains( x.id ) ) )
			{
				var go = Instantiate( toonPrefab, itemContainer );
				go.GetComponent<ToonSelectorPrefab>().InitVillain( item );
			}

			addVillainCallback = callback;
			Show();
		}

		public void AddSkill( string heroID, Action<CampaignSkill> callback )
		{
			foreach ( Transform item in itemContainer )
				Destroy( item.gameObject );

			var sagaCampaign = FindObjectOfType<CampaignManager>().sagaCampaign;

			foreach ( var item in SagaCampaign.campaignDataSkills.Where( x => x.owner == heroID ) )
			{
				var go = Instantiate( itemSkillSelectorPrefab, itemContainer );
				go.GetComponent<ItemSkillSelectorPrefab>().Init( item );
			}

			addSkillCallback = callback;
			Show();
		}

		public void AddItem( Action<CampaignItem> callback, bool useGeneralItemList )
		{
			this.useGeneralItemList = useGeneralItemList;

			if ( !useGeneralItemList )
			{
				blockToggle = true;
				tier2Toggle.isOn = false;
				tier3Toggle.isOn = false;
				tier1Toggle.isOn = true;
				tierFilterContainer.SetActive( true );
			}
			blockToggle = false;

			OnTierFilter( 1 );

			addItemCallback = callback;
			Show();
		}

		public void AddMission( string expansionCode, MissionType missionType, Action<MissionCard> callback )
		{
			foreach ( Transform item in itemContainer )
				Destroy( item.gameObject );

			selectedExpansion = "Core";
			expansionCodes.Clear();
			//restricted to current expansion missions (story and finale)
			if ( expansionCode != "Custom"
				&& (missionType == MissionType.Story || missionType == MissionType.Finale) )
				selectedExpansion = expansionCode;
			expansionDropdown.ClearOptions();
			//add translated expansion name
			expansionDropdown.AddOptions(
				DataStore.translatedExpansionNames
				.Where( x => DataStore.ownedExpansions.Contains( x.Key.ToEnum( Expansion.Core ) ) )
				.Select( y => y.Value )
				.ToList() );

			expansionCodes = DataStore.ownedExpansions.Select( x => x.ToString() ).ToList();

			//add "Other" dropdown
			expansionDropdown.AddOptions( (new string[] { DataStore.translatedExpansionNames["Other"] }).ToList() );
			expansionCodes.Add( DataStore.uiLanguage.uiCampaign.otherUC );

			//side missions have access to custom missions, add "Custom" to dropdown
			if ( expansionCode == "Custom" || missionType == MissionType.Side || missionType == MissionType.Forced )
			{
				expansionDropdown.AddOptions( (new string[] { DataStore.uiLanguage.uiCampaign.customUC }).ToList() );
				expansionCodes.Add( "Custom" );
			}

			foreach ( var item in DataStore.missionCards[selectedExpansion] )
			{
				var go = Instantiate( itemSkillSelectorPrefab, itemContainer );
				go.GetComponent<ItemSkillSelectorPrefab>().Init( item );
			}

			addMissionCallback = callback;
			Show();
			//make room for the expansion dropdown
			if ( expansionCode == "Custom" || (missionType != MissionType.Story && missionType != MissionType.Finale) )
			{
				scrollRectTransform.offsetMax = new Vector2( scrollRectTransform.offsetMax.x, -155 );
				expansionDropdown.gameObject.SetActive( true );
			}
		}

		public void AddReward( Action<CampaignReward> callback )
		{
			foreach ( Transform item in itemContainer )
				Destroy( item.gameObject );

			rewardFilterContainer.SetActive( true );
			rewardFilterDropdown.options.Clear();
			rewardFilterDropdown.AddOptions( new string[] {
				DataStore.uiLanguage.uiCampaign.campaignUC,
				DataStore.uiLanguage.uiCampaign.generalUC,
				DataStore.uiLanguage.uiCampaign.heroUC,
				DataStore.uiLanguage.uiCampaign.personalUC
			}.ToList() );
			rewardFilterDropdown.value = 0;
			OnRewardFilter();

			addRewardCallback = callback;
			Show();
		}
		#endregion

		public void OnAddHero( DeploymentCard card )
		{
			addHeroCallback?.Invoke( card );
			OnClose();
		}

		public void OnAddAlly( DeploymentCard card )
		{
			addAllyCallback?.Invoke( card );
			OnClose();
		}

		public void OnAddVillain( DeploymentCard card )
		{
			addVillainCallback?.Invoke( card );
			OnClose();
		}

		public void OnAddItem( CampaignItem item )
		{
			addItemCallback?.Invoke( item );
			OnClose();
		}

		public void OnAddSkill( CampaignSkill skill )
		{
			addSkillCallback?.Invoke( skill );
			OnClose();
		}

		public void OnAddMission( MissionCard card )
		{
			addMissionCallback?.Invoke( card );
			OnClose();
		}

		public void OnAddReward( CampaignReward card )
		{
			addRewardCallback?.Invoke( card );
			OnClose();
		}

		public void OnRemove()
		{

		}

		public void OnExpansionChanged()
		{
			//get string from selection dropdown value
			selectedExpansion = expansionCodes[expansionDropdown.value];

			foreach ( Transform item in itemContainer )
				Destroy( item.gameObject );

			if ( selectedExpansion != "Custom" )
			{
				foreach ( var card in DataStore.missionCards[selectedExpansion] )
				{
					var go = Instantiate( itemSkillSelectorPrefab, itemContainer );
					go.GetComponent<ItemSkillSelectorPrefab>().Init( card );
				}
			}
			else
			{
				var missions = FileManager.GetCustomMissions();
				//sort alphabetically
				missions = missions.OrderBy( x => x.Title ).ToList();

				foreach ( var item in missions )
				{
					var go = Instantiate( itemSkillSelectorPrefab, itemContainer );
					//store path into 'hero'
					//store additional info into 'bonusText'
					var card = new MissionCard()
					{
						name = item.Title,
						id = "Custom",
						hero = item.fullPathWithFilename,
						bonusText = item.AdditionalInfo,
						descriptionText = item.Description,
					};
					go.GetComponent<ItemSkillSelectorPrefab>().Init( card, item.fileName );
				}
			}
		}

		public void OnTierFilter( int filterValue )
		{
			if ( blockToggle )
				return;

			foreach ( Transform item in itemContainer )
				Destroy( item.gameObject );

			//showing items heroes can use
			var sc = FindObjectOfType<CampaignManager>().sagaCampaign.campaignItems;
			if ( useGeneralItemList )
			{
				var items = SagaCampaign.campaignDataItems.Where( x => sc.Contains( x.id ) ).ToList();
				foreach ( var item in items )
				{
					var go = Instantiate( itemSkillSelectorPrefab, itemContainer );
					go.GetComponent<ItemSkillSelectorPrefab>().Init( item );
				}
			}
			else//showing general items list
			{
				//don't show items already added to campaign
				foreach ( var item in SagaCampaign.campaignDataItems.Where( x => x.tier == filterValue && !sc.Contains( x.id ) ) )
				{
					var go = Instantiate( itemSkillSelectorPrefab, itemContainer );
					go.GetComponent<ItemSkillSelectorPrefab>().Init( item );
				}
			}

			//scroll to top
			scrollRect.verticalNormalizedPosition = 3000;
		}

		public void OnRewardFilter()
		{
			string filter = "Campaign";
			if ( rewardFilterDropdown.value == 0 )
				filter = "Campaign";
			else if ( rewardFilterDropdown.value == 1 )
				filter = "General";
			else if ( rewardFilterDropdown.value == 2 )
				filter = "HeroNumber";
			else if ( rewardFilterDropdown.value == 3 )
				filter = "Personal";

			foreach ( Transform item in itemContainer )
				Destroy( item.gameObject );

			var sc = FindObjectOfType<CampaignManager>().sagaCampaign.campaignRewards;
			//omit rewards already added to campaign
			foreach ( var item in SagaCampaign.campaignDataRewards.Where( x => !sc.Contains( x.id ) && x.type.ToString() == filter ) )
			{
				var go = Instantiate( itemSkillSelectorPrefab, itemContainer );
				go.GetComponent<ItemSkillSelectorPrefab>().Init( item );
			}

			//scroll to top
			scrollRect.verticalNormalizedPosition = 3000;
		}

		public void OnClose()
		{
			tierFilterContainer.SetActive( false );
			rewardFilterContainer.SetActive( false );
			popupBase.Close();
		}

		private void Update()
		{
			if ( Input.GetKeyDown( KeyCode.Space ) )
				OnClose();
		}
	}
}