using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmDeletePopup : MonoBehaviour
{
	public PopupBase popupBase;
	public Text startText, cancelText;
	public TextMeshProUGUI nameText;

	Action callback;

	public void Show( string name, Action onDelete )
	{
		nameText.text = name;
		startText.text = DataStore.uiLanguage.uiTitle.delete;
		cancelText.text = DataStore.uiLanguage.uiSetup.cancel;
		callback = onDelete;

		popupBase.Show();
	}

	public void OnDelete()
	{
		Close();
		callback?.Invoke();
	}

	public void Close()
	{
		popupBase.Close();
	}
}
