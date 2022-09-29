using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Saga
{
	public class AddItemHeroAllyVillainPopup : MonoBehaviour
	{
		public PopupBase popupBase;
		public GameObject itemSkillPrefab, toonPrefab, itemSkillSelectorPrefab;
		public Transform itemContainer;
		public ScrollRect scrollRect;
		public TMP_Dropdown expansionDropdown;
		public RectTransform scrollRectTransform;
		public Text cancelText;

		Action<DeploymentCard> addHeroCallback, addAllyCallback, addVillainCallback;
		Action<CampaignItem> addItemCallback;
		Action<CampaignSkill> addSkillCallback;
		Action<MissionCard> addMissionCallback;

		string selectedExpansion = "";
		List<string> expansionCodes = new List<string>();
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

		public void AddHero( Action<DeploymentCard> callback )
		{
			foreach ( Transform item in itemContainer )
				Destroy( item.gameObject );

			foreach ( var item in DataStore.heroCards )
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

			foreach ( var item in DataStore.villainCards )
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

		public void AddItem( Action<CampaignItem> callback )
		{
			foreach ( Transform item in itemContainer )
				Destroy( item.gameObject );

			foreach ( var item in SagaCampaign.campaignDataItems )
			{
				var go = Instantiate( itemSkillSelectorPrefab, itemContainer );
				go.GetComponent<ItemSkillSelectorPrefab>().Init( item );
			}

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
			expansionDropdown.AddOptions(
				DataStore.translatedExpansionNames
				.Where( x => DataStore.ownedExpansions.Contains( x.Key.ToEnum( Expansion.Core ) ) )
				.Select( y => y.Value )
				.ToList() );

			expansionCodes = DataStore.ownedExpansions.Select( x => x.ToString() ).ToList();

			if ( expansionCode == "Custom" )
			{
				expansionDropdown.AddOptions( (new string[] { "Custom Mission" }).ToList() );
				expansionCodes.Add( "Custom" );
			}

			foreach ( var item in DataStore.missionCards[selectedExpansion] )
			{
				var go = Instantiate( itemSkillSelectorPrefab, itemContainer );
				go.GetComponent<ItemSkillSelectorPrefab>().Init( item );
			}

			addMissionCallback = callback;
			Show();
			if ( expansionCode == "Custom" || (missionType != MissionType.Story && missionType != MissionType.Finale) )
			{
				scrollRectTransform.offsetMax = new Vector2( scrollRectTransform.offsetMax.x, -155 );
				expansionDropdown.gameObject.SetActive( true );
			}
		}

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
					go.GetComponent<ItemSkillSelectorPrefab>().Init( card );
				}
			}
		}

		public void OnClose()
		{
			popupBase.Close();
		}

		private void Update()
		{
			if ( Input.GetKeyDown( KeyCode.Space ) )
				OnClose();
		}
	}
}