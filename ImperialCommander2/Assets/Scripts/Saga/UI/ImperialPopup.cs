using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga
{
	public class ImperialPopup : MonoBehaviour
	{
		public GameObject handItemPrefab;
		public Text titleText, closeButtonText, threatText;
		public TextMeshProUGUI threat, depMod, threatValue, depmodValue;
		public Transform container;
		//public CanvasGroup cg;
		public PopupBase popupBase;

		public void Show()
		{
			titleText.text = "imperial menu";//DataStore.uiLanguage.uiMainApp.debugDepHandUC.ToLowerInvariant();
			threat.text = DataStore.uiLanguage.uiMainApp.debugThreatUC;
			threatValue.text = DataStore.sagaSessionData.gameVars.currentThreat.ToString();
			depMod.text = DataStore.uiLanguage.uiMainApp.debugDepModUC;
			depmodValue.text = DataStore.sagaSessionData.gameVars.deploymentModifier.ToString();
			closeButtonText.text = DataStore.uiLanguage.uiSetup.continueBtn;
			threatText.text = DataStore.uiLanguage.uiMainApp.modThreatHeading;

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

	}
}
