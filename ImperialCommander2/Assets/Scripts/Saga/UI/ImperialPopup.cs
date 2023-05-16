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
		public PopupBase popupBase;
		public Button optionalDeploymentButton;

		public void Show()
		{
			titleText.text = DataStore.uiLanguage.sagaMainApp.imperialMenu;
			threat.text = DataStore.uiLanguage.uiMainApp.debugThreatUC;
			threatValue.text = DataStore.sagaSessionData.gameVars.currentThreat.ToString();
			depMod.text = DataStore.uiLanguage.uiMainApp.debugDepModUC;
			depmodValue.text = DataStore.sagaSessionData.gameVars.deploymentModifier.ToString();
			closeButtonText.text = DataStore.uiLanguage.uiSetup.continueBtn;
			threatText.text = DataStore.uiLanguage.uiMainApp.modThreatHeading;

			optionalDeploymentButton.interactable = DataStore.gameType == GameType.Saga;

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

		public void OptionalDeployment()
		{
			if ( DataStore.gameType != GameType.Saga )
				return;

			Close();
			Debug.Log( "ImperialPopup::OptionalDeployment()::PROCESSING OptionalDeployment" );
			DeploymentGroupOverride ovrd = new DeploymentGroupOverride( "" );
			ovrd.deploymentPoint = DeploymentSpot.Active;
			ovrd.specificDeploymentPoint = System.Guid.Empty;
			ovrd.useThreat = true;
			ovrd.threatCost = 0;
			FindObjectOfType<SagaController>().deploymentPopup.Show( DeployMode.Landing, true, true, null, ovrd );
		}

		private void Update()
		{
			if ( Input.GetKeyDown( KeyCode.Space ) )
				Close();
		}
	}
}
