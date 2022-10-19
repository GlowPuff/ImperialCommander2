using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Saga
{
	public class CustomAddMissionBarPrefab : MonoBehaviour
	{
		public CanvasGroup buttonGroup;
		public TextMeshProUGUI storyText, sideText, interludeText, finaleText;

		bool disableMode = true;

		private void Awake()
		{
			if ( DataStore.uiLanguage == null )
				return;

			storyText.text = "+ " + DataStore.uiLanguage.uiCampaign.modeStoryUC;
			sideText.text = "+ " + DataStore.uiLanguage.uiCampaign.modeSideUC;
			interludeText.text = "+ " + DataStore.uiLanguage.uiCampaign.modeInterludeUC;
			finaleText.text = "+ " + DataStore.uiLanguage.uiCampaign.modeFinaleUC;
		}

		public void OnAddMission( int missionType )
		{
			FindObjectOfType<Sound>().PlaySound( FX.Click );
			EventSystem.current.SetSelectedGameObject( null );
			FindObjectOfType<CampaignManager>().OnAddCustomMission( (MissionType)missionType );
		}

		public void DeactivateModifyMode()
		{
			disableMode = true;
			buttonGroup.interactable = disableMode;
			FindObjectOfType<CampaignManager>().ToggleDisableUI( disableMode );
		}

		public void OnModifyMode()
		{
			disableMode = !disableMode;
			buttonGroup.interactable = disableMode;
			FindObjectOfType<CampaignManager>().ToggleDisableUI( disableMode );
		}
	}
}