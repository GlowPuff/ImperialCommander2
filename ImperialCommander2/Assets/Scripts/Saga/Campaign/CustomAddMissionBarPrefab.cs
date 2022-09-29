using UnityEngine;
using UnityEngine.EventSystems;

namespace Saga
{
	public class CustomAddMissionBarPrefab : MonoBehaviour
	{
		public CanvasGroup buttonGroup;

		bool disableMode = true;

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