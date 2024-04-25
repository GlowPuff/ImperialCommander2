using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga
{
	public class ItemSkillSelectorPrefab : MonoBehaviour
	{
		public TextMeshProUGUI nameText, typeText, costText;
		public GameObject cointObject, plusObject;
		public Button addButton;
		public GameObject storyIcon, sideIcon;

		CampaignItem campaignItem;
		CampaignSkill campaignSkill;
		MissionCard missionCard;
		CampaignReward campaignReward;
		int itemType;//0=item, 1=skill, 2=mission, 3=reward
		bool usingCoins = false;

		//Init() called from AddCampaignItemPopup window to add specified item types

		public void Init( CampaignItem item, bool showCoinIcon )
		{
			itemType = 0;
			campaignItem = item;
			nameText.text = $"{item.name} / <color=orange>{DataStore.uiLanguage.uiCampaign.tierUC} {item.tier}</color>";
			typeText.text = item.type;
			costText.text = $"{DataStore.uiLanguage.uiCampaign.costUC}: {item.cost}";
			plusObject.SetActive( !showCoinIcon );
			cointObject.SetActive( showCoinIcon );
			usingCoins = showCoinIcon;
			if ( usingCoins && item.cost > RunningCampaign.sagaCampaign.credits )
				addButton.interactable = false;
		}

		public void Init( CampaignSkill item )
		{
			itemType = 1;
			campaignSkill = item;
			typeText.text = "A";
			nameText.text = $"{item.name}";
			costText.text = $"{DataStore.uiLanguage.uiCampaign.costUC}: {item.cost}";
		}

		public void Init( MissionCard card, string filename = "" )
		{
			//store path into 'hero'
			//store additional info into 'bonusText'
			itemType = 2;
			missionCard = card;
			nameText.text = missionCard.name;
			typeText.text = "U";
			typeText.color = new Vector3( 1f, 0.1568628f, 0f ).ToColor();
			costText.text = filename;
			foreach ( var item in card.missionType )
			{
				if ( item == global::MissionType.Story )
				{
					typeText.gameObject.SetActive( false );
					storyIcon.SetActive( true );
					sideIcon.SetActive( false );
				}
				else if ( item == global::MissionType.Side )
				{
					typeText.gameObject.SetActive( false );
					storyIcon.SetActive( false );
					sideIcon.SetActive( true );
				}
			}
		}

		public void InitEmbeddedMission( MissionCard card )
		{
			//store mission GUID into 'hero'
			//store imported campaign name into 'bonusText'
			itemType = 2;
			missionCard = card;
			nameText.text = missionCard.name;
			typeText.text = "U";
			typeText.color = new Vector3( 1f, 0.1568628f, 0f ).ToColor();
			costText.text = $"{DataStore.uiLanguage.uiCampaign.campaignUC}: {card.bonusText}";
		}

		public void Init( CampaignReward item )
		{
			itemType = 3;
			campaignReward = item;
			if ( !string.IsNullOrEmpty( item.extra ) )
				nameText.text = $"{item.name} ({item.extra})";
			else
				nameText.text = $"{item.name}";
			typeText.text = "V";
			costText.text = "";
		}

		public void OnAdd()
		{
			if ( itemType == 0 )
			{
				FindObjectOfType<AddCampaignItemPopup>().OnAddItem( campaignItem );
				if ( usingCoins )
				{
					FindObjectOfType<CampaignManager>().sound.PlaySound( 1 );
					FindObjectOfType<CampaignManager>().UpdateCreditsUI( RunningCampaign.sagaCampaign.credits - campaignItem.cost );
				}
			}
			else if ( itemType == 1 )
				FindObjectOfType<AddCampaignItemPopup>().OnAddSkill( campaignSkill );
			else if ( itemType == 2 )
				FindObjectOfType<AddCampaignItemPopup>().OnAddMission( missionCard );
			else if ( itemType == 3 )
				FindObjectOfType<AddCampaignItemPopup>().OnAddReward( campaignReward );
		}
	}
}
