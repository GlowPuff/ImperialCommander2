using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CampaignMessagePopup : MonoBehaviour
{
	public PopupBase popupBase;
	public Text titleText, closeText;
	public TextMeshProUGUI bodyText;

	public void Show( string title, string message, int height = 500 )
	{
		EventSystem.current.SetSelectedGameObject( null );
		popupBase.Show();

		closeText.text = DataStore.uiLanguage.uiMainApp.continueBtn;
		titleText.text = title.ToUpper();
		bodyText.text = message;

		var rt = GetComponent<RectTransform>();
		Vector2 size = bodyText.GetPreferredValues( message, 733, 0 );
		var windowH = Mathf.Clamp( size.y + 250, 275, 700 );
		rt.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, windowH );
		//rt.sizeDelta = new Vector2( rt.sizeDelta.x, height );
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
