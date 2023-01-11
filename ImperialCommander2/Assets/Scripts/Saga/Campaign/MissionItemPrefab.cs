using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Saga
{
	public class MissionItemPrefab : MonoBehaviour
	{
		[HideInInspector]
		public CampaignStructure campaignStructure;

		public Text threatLevelText;
		public Toggle itemToggle;
		public TextMeshProUGUI missionType, missionName, itemText, agendaIcon, modMissionNameText, modifyText, removeText;
		public GameObject removeForcedButton, agendaButton, dummyAgenda, dummyItemToggle, modifyMissionPanel;
		public Image bgImage, nameButtonImage, chevronImage;
		public CanvasGroup buttonGroup;
		public Button playMissionButton;

		public void Init( CampaignStructure cs )
		{
			campaignStructure = cs;

			modifyText.text = DataStore.uiLanguage.uiCampaign.modifyUC;
			removeText.text = DataStore.uiLanguage.uiCampaign.removeUC;

			///set colors
			//story
			bgImage.color = new Vector3( 0f, 0.6440244f, 1f ).ToColor();

			if ( cs.missionType == MissionType.Side )
				bgImage.color = new Vector3( .5f, 0.8362323f, 1f ).ToColor();
			if ( cs.missionType == MissionType.Forced )
			{
				dummyItemToggle.SetActive( true );
				bgImage.color = new Vector3( 1f, 0.1568628f, 0f ).ToColor();
			}
			if ( cs.missionType == MissionType.Finale )
			{
				itemToggle.gameObject.SetActive( false );
				dummyItemToggle.SetActive( true );
				bgImage.color = new Vector3( 1f, 0.7863293f, 0f ).ToColor();
			}
			if ( cs.missionType == MissionType.Introduction
				|| cs.missionType == MissionType.Interlude )
			{
				bgImage.color = new Vector3( 0f, 0.3215685f, .5f ).ToColor();
			}

			if ( !cs.isAgendaMission || cs.missionType == MissionType.Forced )
			{
				agendaButton.SetActive( false );
				dummyAgenda.SetActive( true );
			}

			if ( cs.isAgendaMission )
			{
				if ( cs.agendaType == AgendaType.Rebel )
				{
					agendaIcon.text = "V";
					agendaIcon.color = new Vector3( 0f, 0.6440244f, 1f ).ToColor();
				}
				else if ( cs.agendaType == AgendaType.Imperial )
				{
					agendaIcon.text = "U";
					agendaIcon.color = new Vector3( 1f, 0.1568628f, 0f ).ToColor();
				}
			}

			//set translated expansion name IF it's not a custom mission
			if ( !string.IsNullOrEmpty( cs.missionID ) && cs.missionID != "Custom" )
			{
				var mcard = DataStore.GetMissionCard( cs.missionID );
				string mn = mcard?.name;
				if ( mcard != null && !string.IsNullOrEmpty( mn ) )
					missionName.text = mn + "\n<color=orange>" + DataStore.translatedExpansionNames[mcard.expansion.ToString()] + "</color>";
				else
					missionName.text = DataStore.uiLanguage.uiCampaign.selectMissionUC;
			}
			else if ( !string.IsNullOrEmpty( cs.missionID ) && cs.missionID == "Custom" )
			{
				missionName.text = cs.projectItem.Title + $"\n<color=orange>{DataStore.uiLanguage.uiCampaign.customUC}: " + DataStore.translatedExpansionNames[cs.expansionCode] + "</color>";
			}
			else
				missionName.text = DataStore.uiLanguage.uiCampaign.selectMissionUC;

			missionType.text = DataStore.uiLanguage.uiCampaign.missionTypeStrings[(int)cs.missionType];
			modMissionNameText.text = missionName.text.Replace( "orange", "black" );
			itemToggle.isOn = cs.isItemChecked;
			threatLevelText.text = cs.threatLevel.ToString();

			if ( cs.itemTier != null && cs.itemTier.Length > 0 )
			{
				var s = cs.itemTier.Select( x => $"{DataStore.uiLanguage.uiCampaign.tierUC} " + x );
				itemText.text = s.Aggregate( ( acc, cur ) => acc + ", " + cur );
			}
			else
				itemText.transform.parent.gameObject.SetActive( false );

			//make the mission selectable or not (selectable by default)
			if ( !cs.canModify )
			{
				missionName.transform.parent.GetComponent<Button>().interactable = false;
				nameButtonImage.enabled = false;
			}

			if ( cs.isForced )
			{
				missionType.gameObject.SetActive( false );
				removeForcedButton.SetActive( true );
			}

			//if it's not a custom campaign, fill in the project item properties
			if ( !cs.isCustom
				&& !string.IsNullOrEmpty( cs.missionID )
				&& cs.missionID != "Custom" )
			{
				var card = DataStore.GetMissionCard( cs.missionID );
				if ( card != null )
				{
					campaignStructure.projectItem = new ProjectItem()
					{
						missionID = card.id,
						Description = card.descriptionText,
						AdditionalInfo = card.bonusText,
						fullPathWithFilename = $"{card.id.ToUpper()}",
						Title = DataStore.missionCards[campaignStructure.expansionCode].Where( x => x.id == campaignStructure.missionID ).FirstOr( new MissionCard() { name = card.name } )?.name
					};
				}
			}
		}

		/// <summary>
		/// remove forced/custom mission
		/// </summary>
		public void RemoveMission()
		{
			FindObjectOfType<Sound>().PlaySound( FX.Click );
			FindObjectOfType<CampaignManager>().RemoveMission( campaignStructure.GUID );
		}

		public void ModifyMission()
		{
			FindObjectOfType<Sound>().PlaySound( FX.Click );
			FindObjectOfType<CampaignManager>().OnModifyCustomMission( this );
		}

		public void OnModify( CampaignModify modifier )
		{
			campaignStructure.isAgendaMission = modifier.agendaToggle;
			campaignStructure.threatLevel = modifier.threatValue;
			campaignStructure.itemTier = modifier.itemTierArray;

			agendaButton.SetActive( campaignStructure.isAgendaMission );
			dummyAgenda.SetActive( !campaignStructure.isAgendaMission );

			agendaButton.SetActive( modifier.agendaToggle );
			threatLevelText.text = modifier.threatValue.ToString();
			var s = modifier.itemTierArray.Select( x => $"{DataStore.uiLanguage.uiCampaign.tierUC} " + x );
			itemText.text = s.Aggregate( ( acc, cur ) => acc + ", " + cur );
		}

		public void OnAgendaClicked()
		{
			EventSystem.current.SetSelectedGameObject( null );
			FindObjectOfType<Sound>().PlaySound( FX.Click );

			if ( agendaIcon.text == "U" || agendaIcon.text == "V" )
			{
				agendaIcon.text = "c";
				agendaIcon.color = new Vector3( 1f, 0.7863293f, 0f ).ToColor();
				campaignStructure.agendaType = AgendaType.NotSet;
				return;
			}

			string msg = "";
			if ( GlowEngine.RandomBool() )//rebel
			{
				msg = DataStore.uiLanguage.uiCampaign.agendaRebelUC;
				agendaIcon.text = "V";
				agendaIcon.color = new Vector3( 0f, 0.6440244f, 1f ).ToColor();
				campaignStructure.agendaType = AgendaType.Rebel;
			}
			else//imperial
			{
				msg = DataStore.uiLanguage.uiCampaign.agendaImperialUC;
				agendaIcon.text = "U";
				agendaIcon.color = new Vector3( 1f, 0.1568628f, 0f ).ToColor();
				campaignStructure.agendaType = AgendaType.Imperial;
			}
			GlowEngine.FindUnityObject<CampaignMessagePopup>().Show( DataStore.uiLanguage.uiCampaign.agendaMission, msg );
		}

		public void OnMissionNameClick()
		{
			FindObjectOfType<Sound>().PlaySound( FX.Click );
			EventSystem.current.SetSelectedGameObject( null );

			FindObjectOfType<CampaignManager>().OnMissionNameClick( campaignStructure.missionType, ( card ) =>
			{
				campaignStructure.missionID = card.id;
				campaignStructure.expansionCode = card.expansion.ToString();

				if ( card.id != "Custom" )
				{
					missionName.text = DataStore.missionCards[campaignStructure.expansionCode].Where( x => x.id == campaignStructure.missionID ).FirstOr( new MissionCard() { name = card.name } )?.name + "\n<color=orange>" + DataStore.translatedExpansionNames[campaignStructure.expansionCode] + "</color>";

					campaignStructure.projectItem.missionID = card.id;
					campaignStructure.projectItem.Description = card.descriptionText;
					campaignStructure.projectItem.AdditionalInfo = card.bonusText;
					campaignStructure.projectItem.fullPathWithFilename = $"{card.id.ToUpper()}";
					campaignStructure.projectItem.Title = DataStore.missionCards[campaignStructure.expansionCode].Where( x => x.id == campaignStructure.missionID ).FirstOr( new MissionCard() { name = card.name } )?.name;
				}
				else
				{
					missionName.text = card.name + $"\n<color=orange>{DataStore.uiLanguage.uiCampaign.customUC}: " + DataStore.translatedExpansionNames[campaignStructure.expansionCode] + "</color>";

					//full path to custom mission is stored in 'hero' only for the purpose of getting it into the campaign structure
					//additional info is stored in 'bonusText'
					campaignStructure.projectItem.fullPathWithFilename = card.hero;
					campaignStructure.projectItem.missionID = card.id;
					campaignStructure.projectItem.Description = card.descriptionText;
					campaignStructure.projectItem.AdditionalInfo = card.bonusText;
					campaignStructure.projectItem.Title = card.name;
				}
				modMissionNameText.text = missionName.text.Replace( "orange", "black" );
			} );
		}

		public void OnItemToggle( Toggle t )
		{
			FindObjectOfType<Sound>().PlaySound( FX.Click );
			campaignStructure.isItemChecked = t.isOn;
		}

		public void ToggleModifyMode( bool enable )
		{
			if ( campaignStructure.missionType != MissionType.Introduction )
				modifyMissionPanel.SetActive( enable );
			else
				buttonGroup.interactable = !enable;
		}

		public void OnStartMission()
		{
			EventSystem.current.SetSelectedGameObject( null );
			FindObjectOfType<Sound>().PlaySound( FX.Click );

			FindObjectOfType<CampaignManager>().StartMission( campaignStructure );
		}

		private void Update()
		{
			playMissionButton.interactable =
				FindObjectOfType<CampaignManager>().sagaCampaign.campaignHeroes.Count > 0
				&& !string.IsNullOrEmpty( campaignStructure.missionID );
			if ( playMissionButton.interactable )
				chevronImage.color = new Vector3( 0, 1, 0 ).ToColor();
			else
				chevronImage.color = new Vector4( 0, .5f, 0, .75f );
		}
	}
}