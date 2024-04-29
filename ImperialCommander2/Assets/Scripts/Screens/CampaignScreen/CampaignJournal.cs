using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CampaignJournal : MonoBehaviour
{
	public PopupBase popupBase;
	public Text closeText;
	public TMP_InputField journalText;
	public TextMeshProUGUI placeholderText;
	public Scrollbar scrollbar;

	Action<string> cb;

	public void Show( string text, Action<string> callback )
	{
		EventSystem.current.SetSelectedGameObject( null );
		popupBase.Show();

		closeText.text = DataStore.uiLanguage.uiMainApp.continueBtn;
		placeholderText.text = DataStore.uiLanguage.sagaUISetup.campaignJournalUC;
		journalText.text = text ?? "";
		cb = callback;

		scrollbar.value = 1;//scroll to bottom
	}

	public void OnTop()
	{
		EventSystem.current.SetSelectedGameObject( null );
		scrollbar.value = 0;
	}

	public void OnBottom()
	{
		EventSystem.current.SetSelectedGameObject( null );
		scrollbar.value = 1;
	}

	public void Cancel()
	{
		EventSystem.current.SetSelectedGameObject( null );
		popupBase.Close();
		cb?.Invoke( journalText.text );
	}
}
