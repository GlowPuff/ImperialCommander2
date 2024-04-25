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
			//omit heroes already added to campaign
			var h = DataStore.heroCards.Where( x => !ch.Contains( x.id ) );

			//add global imports, omitting those already added to compaign
			h = h.Concat( DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Hero && !ch.Contains( x.deploymentCard.id ) ).Select( x => x.deploymentCard ) );

			foreach ( var item in h )
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

			var sc = FindObjectOfType<CampaignManager>().sagaCampaign.campaignAllies;

			//add global imports
			var allies = DataStore.allyCards.Concat( DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Ally ).Select( x => x.deploymentCard ) ).Where( x => !sc.Contains( x ) ).ToList();

			foreach ( var item in allies )
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

			//add global imports, omitting those already added to campaign
			var villains = DataStore.villainCards.Concat( DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Villain ).Select( x => x.deploymentCard ) ).Where( x => !sc.Contains( x ) ).ToList();

			//omit villains already added to campaign
			foreach ( var item in villains )
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

			var skills = SagaCampaign.campaignDataSkills.Concat( DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Hero ).SelectMany( x => x.heroSkills ) );

			foreach ( var item in skills.Where( x => x.owner == heroID ) )
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

		public void AddMission( CampaignType ctype, string campaignExpansionCode, MissionType missionType, Action<MissionCard> callback )
		{
			foreach ( Transform item in itemContainer )
				Destroy( item.gameObject );

			selectedExpansion = "Core";
			expansionCodes.Clear();
			//for Official campaigns (Story and Finale slots only), missions are restricted to those in the campaign's expansion
			if ( ctype == CampaignType.Official
				&& (missionType == MissionType.Story || missionType == MissionType.Finale) )
				selectedExpansion = campaignExpansionCode;

			expansionDropdown.ClearOptions();

			//the expansions dropdown menu is only visible for Custom campaigns, and Official campaigns where the type is NOT Side and NOT Finale
			//add translated expansion names for all owned expansions
			expansionDropdown.AddOptions(
				DataStore.translatedExpansionNames
				.Where( x => DataStore.ownedExpansions.Contains( x.Key.ToEnum( Expansion.Core ) ) )
				.Select( y => y.Value )
				.ToList() );

			//add owned expansions to codes
			expansionCodes = DataStore.ownedExpansions.Select( x => x.ToString() ).ToList();

			//add "Other" dropdown
			expansionDropdown.AddOptions( (new string[] { DataStore.translatedExpansionNames["Other"] }).ToList() );
			//add Other to codes
			expansionCodes.Add( "Other" );

			//side missions have access to custom missions, add "Custom" to dropdown
			if ( ctype == CampaignType.Custom || missionType == MissionType.Side || missionType == MissionType.Forced )
			{
				expansionDropdown.AddOptions( (new string[] { DataStore.uiLanguage.uiCampaign.customUC }).ToList() );
				//add Custom to codes
				expansionCodes.Add( "Custom" );
			}

			//for imported campaigns, add embedded Mission option
			if ( ctype == CampaignType.Imported )
			{
				string cname = FindObjectOfType<CampaignManager>().sagaCampaign.campaignName;
				expansionDropdown.AddOptions( (new string[] { cname }).ToList() );
				//add Custom to codes
				expansionCodes.Add( "Embedded" );
			}

			//add missions for the default selected expansion on initial display of the panel
			//for imported campaigns, restrict Story/Finale missions to the imported missions
			if ( ctype == CampaignType.Imported && (missionType == MissionType.Story || missionType == MissionType.Finale) )
			{
				var campaign = FindObjectOfType<CampaignManager>().sagaCampaign;
				var package = campaign.campaignPackage;
				foreach ( var item in package.campaignMissionItems )
				{
					var go = Instantiate( itemSkillSelectorPrefab, itemContainer );
					//store mission GUID into 'hero'
					//store imported campaign name into 'bonusText'
					//store package GUID into 'expansionText'
					var card = new MissionCard()
					{
						name = item.missionName,
						id = "Embedded",
						hero = item.missionGUID.ToString(),
						bonusText = campaign.campaignImportedName,
						expansionText = package.GUID.ToString()
					};
					go.GetComponent<ItemSkillSelectorPrefab>().InitEmbeddedMission( card );
				}
			}
			else
			{
				//default behavior in every other case, show missions from initially selected expansion in the dropdown (selectedExpansion)
				foreach ( var item in DataStore.missionCards[selectedExpansion] )
				{
					var go = Instantiate( itemSkillSelectorPrefab, itemContainer );
					go.GetComponent<ItemSkillSelectorPrefab>().Init( item );
				}
			}

			addMissionCallback = callback;
			Show();
			//make room for the expansion dropdown, which is only visible for Custom campaigns and official/imported campaigns where the slot type is NOT story and NOT finale
			if ( ctype == CampaignType.Custom || (missionType != MissionType.Story && missionType != MissionType.Finale) )
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

			//official missions
			if ( selectedExpansion != "Custom" && selectedExpansion != "Embedded" )
			{
				foreach ( var card in DataStore.missionCards[selectedExpansion] )
				{
					var go = Instantiate( itemSkillSelectorPrefab, itemContainer );
					go.GetComponent<ItemSkillSelectorPrefab>().Init( card );
				}
			}
			else if ( selectedExpansion == "Embedded" )//embedded missions
			{
				var campaign = FindObjectOfType<CampaignManager>().sagaCampaign;
				var package = campaign.campaignPackage;
				foreach ( var item in package.campaignMissionItems )
				{
					var go = Instantiate( itemSkillSelectorPrefab, itemContainer );
					//store mission GUID into 'hero'
					//store imported campaign name into 'bonusText'
					//store package GUID into 'expansionText'
					var card = new MissionCard()
					{
						name = item.missionName,
						id = "Embedded",
						hero = item.missionGUID.ToString(),
						bonusText = campaign.campaignImportedName,
						expansionText = package.GUID.ToString()
					};
					go.GetComponent<ItemSkillSelectorPrefab>().InitEmbeddedMission( card );
				}
			}
			else//custom missions
			{
				var missions = FileManager.GetCustomMissions().Where( x => x.missionGUID != null );
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

		/// <summary>
		/// Items
		/// </summary>
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
				items.Sort( ( i1, i2 ) =>
				{
					return string.Compare( i1.name, i2.name );
				} );

				foreach ( var item in items )
				{
					var go = Instantiate( itemSkillSelectorPrefab, itemContainer );
					go.GetComponent<ItemSkillSelectorPrefab>().Init( item, false );
				}
			}
			else//showing general items list
			{
				//don't show items already added to campaign
				var items = SagaCampaign.campaignDataItems.Where( x => x.tier == filterValue && !sc.Contains( x.id ) );
				items = items.OrderBy( x => x.name );
				foreach ( var item in items )
				{
					var go = Instantiate( itemSkillSelectorPrefab, itemContainer );
					go.GetComponent<ItemSkillSelectorPrefab>().Init( item, true );
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

			var items = SagaCampaign.campaignDataRewards.Where( x => !sc.Contains( x.id ) && x.type.ToString() == filter );
			items = items.OrderBy( x => x.name );
			//omit rewards already added to campaign
			foreach ( var item in items )
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