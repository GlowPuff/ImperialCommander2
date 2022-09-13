using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CampaignMessagePopup : MonoBehaviour
{
	public PopupBase popupBase;
	public Text titleText, closeText;
	public TextMeshProUGUI bodyText;

	public void Show( string title, string message )
	{
		EventSystem.current.SetSelectedGameObject( null );
		popupBase.Show();

		closeText.text = DataStore.uiLanguage.uiMainApp.close;
		titleText.text = title;
		bodyText.text = message;
	}

	public void OnClose()
	{
		popupBase.Close();
	}

	private void Update()
	{
		if ( Input.GetKeyDown( KeyCode.Space ) )
			OnClose();
	}
}
