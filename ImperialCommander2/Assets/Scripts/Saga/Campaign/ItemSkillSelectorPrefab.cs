using TMPro;
using UnityEngine;

namespace Saga
{
	public class ItemSkillSelectorPrefab : MonoBehaviour
	{
		public TextMeshProUGUI nameText, typeText, costText;

		CampaignItem campaignItem;
		CampaignSkill campaignSkill;
		MissionCard missionCard;
		int itemType;//0=item, 1=skill, 2=mission

		public void Init( CampaignItem item )
		{
			itemType = 0;
			campaignItem = item;
			nameText.text = $"{item.name} / <color=orange>Tier {item.tier}</color>";
			typeText.text = item.type;
			costText.text = $"Cost: {item.cost}";
		}

		public void Init( CampaignSkill item )
		{
			itemType = 1;
			campaignSkill = item;
			typeText.text = "A";
			nameText.text = $"{item.name}";
			costText.text = $"Cost: {item.cost}";
		}

		public void Init( MissionCard card )
		{
			itemType = 2;
			missionCard = card;
			nameText.text = missionCard.name;
			typeText.text = "U";
			typeText.color = new Vector3( 1f, 0.1568628f, 0f ).ToColor();
			costText.text = "";
		}

		public void OnAdd()
		{
			if ( itemType == 0 )
				FindObjectOfType<AddItemHeroAllyVillainPopup>().OnAddItem( campaignItem );
			else if ( itemType == 1 )
				FindObjectOfType<AddItemHeroAllyVillainPopup>().OnAddSkill( campaignSkill );
			else if ( itemType == 2 )
				FindObjectOfType<AddItemHeroAllyVillainPopup>().OnAddMission( missionCard );
		}
	}
}
