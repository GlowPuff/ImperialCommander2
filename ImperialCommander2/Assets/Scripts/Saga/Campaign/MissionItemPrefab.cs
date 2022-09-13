using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga
{
	public class MissionItemPrefab : MonoBehaviour
	{
		[HideInInspector]
		public CampaignStructure campaignStructure;

		public Text threatLevelText;
		public Toggle itemToggle;
		public TextMeshProUGUI missionType, missionName, itemText, agendaIcon;
		public GameObject removeForcedButton, agendaButton, dummyAgenda;
		public Image bgImage, nameButtonImage;

		public void Init( CampaignStructure cs )
		{
			//story
			bgImage.color = new Vector3( 0f, 0.6440244f, 1f ).ToColor();
			if ( cs.missionType == MissionType.Side )
				bgImage.color = new Vector3( .5f, 0.8362323f, 1f ).ToColor();
			if ( cs.missionType == MissionType.Forced )
			{
				nameButtonImage.enabled = false;
				bgImage.color = new Vector3( 1f, 0.1568628f, 0f ).ToColor();
			}
			if ( cs.missionType == MissionType.Finale )
			{
				nameButtonImage.enabled = false;
				bgImage.color = new Vector3( 1f, 0.7863293f, 0f ).ToColor();
			}
			if ( cs.missionType == MissionType.Introduction
				|| cs.missionType == MissionType.Interlude )
			{
				nameButtonImage.enabled = false;
				bgImage.color = new Vector3( 0f, 0.3215685f, .5f ).ToColor();
			}

			if ( !cs.isAgendaMission || cs.missionType == MissionType.Forced )
			{
				agendaButton.SetActive( false );
				dummyAgenda.SetActive( true );
			}

			campaignStructure = cs;

			missionType.text = cs.missionType.ToString();
			string mn = DataStore.missionCards[cs.expansionCode].Where( x => x.id == cs.missionID ).FirstOr( null )?.name;
			if ( !string.IsNullOrEmpty( mn ) )
				missionName.text = mn + "\n<color=orange>" + DataStore.translatedExpansionNames[cs.expansionCode] + "</color>";
			else
				missionName.text = "Select Mission";
			itemToggle.isOn = cs.isItemChecked;
			threatLevelText.text = cs.threatLevel.ToString();

			if ( cs.itemTier != null && cs.itemTier.Length > 0 )
			{
				var s = cs.itemTier.Select( x => "Tier " + x );
				itemText.text = s.Aggregate( ( acc, cur ) => acc + ", " + cur );
			}
			else
				itemText.transform.parent.gameObject.SetActive( false );

			if ( cs.missionType == MissionType.Introduction
				|| cs.missionType == MissionType.Forced
				|| cs.missionType == MissionType.Finale
				|| cs.missionType == MissionType.Interlude )
			{
				missionName.transform.parent.GetComponent<Button>().interactable = false;
			}

			if ( cs.isForced )
			{
				missionType.gameObject.SetActive( false );
				removeForcedButton.SetActive( true );
			}
		}

		public void RemoveMission()
		{
			FindObjectOfType<Sound>().PlaySound( FX.Click );
			FindObjectOfType<CampaignManager>().RemoveForcedMission( campaignStructure.missionID );
		}

		public void OnAgendaClicked()
		{
			FindObjectOfType<Sound>().PlaySound( FX.Click );

			if ( agendaIcon.text == "U" || agendaIcon.text == "V" )
			{
				agendaIcon.text = "c";
				agendaIcon.color = new Vector3( 1f, 0.7863293f, 0f ).ToColor();
				return;
			}

			string msg = "";
			if ( GlowEngine.RandomBool() )//rebel
			{
				msg = "The Empire uses its resources elsewhere. Nothing happens.";
				agendaIcon.text = "V";
				agendaIcon.color = new Vector3( 0f, 0.6440244f, 1f ).ToColor();
			}
			else//imperial
			{
				msg = "The Empire sees you as a threat worth stopping. Draw a card from the Imperial Mission Deck and put it into play. If it is a forced mission, immediately resolve that mission. If it is a side mission, it becomes an active side mission.";
				agendaIcon.text = "U";
				agendaIcon.color = new Vector3( 1f, 0.1568628f, 0f ).ToColor();
			}
			GlowEngine.FindUnityObject<CampaignMessagePopup>().Show( "agenda mission", msg );
		}

		public void OnMissionNameClick()
		{
			FindObjectOfType<Sound>().PlaySound( FX.Click );

			FindObjectOfType<CampaignManager>().OnMissionNameClick( campaignStructure.missionType, ( card ) =>
			{
				campaignStructure.missionID = card.id;
				campaignStructure.expansionCode = card.expansion.ToString();
				missionName.text = DataStore.missionCards[campaignStructure.expansionCode].Where( x => x.id == campaignStructure.missionID ).FirstOr( null )?.name + "\n<color=orange>" + DataStore.translatedExpansionNames[campaignStructure.expansionCode] + "</color>";
			} );
		}

		public void OnItemToggle( Toggle t )
		{
			FindObjectOfType<Sound>().PlaySound( FX.Click );
			campaignStructure.isItemChecked = t.isOn;
		}
	}
}