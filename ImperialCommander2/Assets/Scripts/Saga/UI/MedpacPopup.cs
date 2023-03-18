using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Saga
{
	public class MedpacPopup : MonoBehaviour
	{
		public PopupBase popupBase;
		public Text continueBtn, medpacCounterText;
		public TextMeshProUGUI infoText;

		Action callback;

		public void Show( Action cb )
		{
			popupBase.Show();

			callback = cb;

			continueBtn.text = DataStore.uiLanguage.uiSetup.continueBtn.ToUpper();
			infoText.text = Utils.ReplaceGlyphs( DataStore.uiLanguage.sagaMainApp.medpacInfoUC );
			medpacCounterText.text = DataStore.sagaSessionData.gameVars.medPacCount.ToString();
		}

		public void onAdd()
		{
			EventSystem.current.SetSelectedGameObject( null );
			DataStore.sagaSessionData.gameVars.medPacCount++;
			medpacCounterText.text = DataStore.sagaSessionData.gameVars.medPacCount.ToString();
		}

		public void onRemove()
		{
			EventSystem.current.SetSelectedGameObject( null );
			DataStore.sagaSessionData.gameVars.medPacCount = Mathf.Max( 0, DataStore.sagaSessionData.gameVars.medPacCount - 1 );
			medpacCounterText.text = DataStore.sagaSessionData.gameVars.medPacCount.ToString();
		}

		public void onClose()
		{
			popupBase.Close();
			callback?.Invoke();
		}
	}
}