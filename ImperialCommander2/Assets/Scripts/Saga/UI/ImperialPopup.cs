using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga
{
	public class ImperialPopup : MonoBehaviour
	{
		public GameObject handItemPrefab, handBlocker;
		public Text titleText, closeButtonText, threatText;
		public TextMeshProUGUI threat, depMod, threatValue, depmodValue;
		public Toggle threatToggle, deployToggle;
		public Transform container;
		//public CanvasGroup cg;
		public PopupBase popupBase;

		public void Show()
		{
			titleText.text = DataStore.uiLanguage.sagaMainApp.imperialMenu.ToUpper();
			threat.text = DataStore.uiLanguage.uiMainApp.debugThreatUC;
			threatValue.text = DataStore.sagaSessionData.gameVars.currentThreat.ToString();
			depMod.text = DataStore.uiLanguage.uiMainApp.debugDepModUC;
			depmodValue.text = DataStore.sagaSessionData.gameVars.deploymentModifier.ToString();
			closeButtonText.text = DataStore.uiLanguage.uiSetup.continueBtn.ToUpper();
			threatText.text = DataStore.uiLanguage.uiMainApp.modThreatHeading.ToUpper();

			//set toggle values
			handBlocker.SetActive( true );
			//toggle pause threat/deployment buttons
			threatToggle.gameObject.SetActive( false );
			deployToggle.gameObject.SetActive( false );
			threatToggle.isOn = DataStore.sagaSessionData.gameVars.pauseThreatIncrease;
			deployToggle.isOn = DataStore.sagaSessionData.gameVars.pauseDeployment;
			threatToggle.gameObject.SetActive( true );
			deployToggle.gameObject.SetActive( true );
			popupBase.Show( () =>
			{
				foreach ( var item in DataStore.deploymentHand )
				{
					var obj = Instantiate( handItemPrefab, container );
					obj.GetComponent<ImperialHandItem>().Init( item );
				}
			} );
		}

		public void Close()
		{
			popupBase.Close( () =>
			{
				foreach ( Transform item in container )
				{
					Destroy( item.gameObject );
				}
			} );
		}

		public void AddThreat()
		{
			DataStore.sagaSessionData.ModifyThreat( 1, true );
			threatValue.text = DataStore.sagaSessionData.gameVars.currentThreat.ToString();
		}

		public void RemoveThreat()
		{
			DataStore.sagaSessionData.ModifyThreat( -1, true );
			threatValue.text = DataStore.sagaSessionData.gameVars.currentThreat.ToString();
		}

		public void ToggleHandVisibility()
		{
			handBlocker.SetActive( !handBlocker.activeSelf );
		}

		private void Update()
		{
			if ( Input.GetKeyDown( KeyCode.Space ) )
				Close();
		}
	}
}
