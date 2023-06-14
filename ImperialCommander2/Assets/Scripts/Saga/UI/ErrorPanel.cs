using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga
{
	public class ErrorPanel : MonoBehaviour
	{
		public TextMeshProUGUI message;
		public PopupBase popupBase;
		public Text exitText, continueText;

		/// <summary>
		/// centered message
		/// </summary>
		public void Show( string m )
		{
			exitText.text = DataStore.uiLanguage.uiSettings.quit;
			continueText.text = DataStore.uiLanguage.uiSetup.continueBtn;

			message.text = m;

			popupBase.Show();
		}

		/// <summary>
		/// Centered header, left-aligned message
		/// </summary>
		public void Show( string header, string m )
		{
			exitText.text = DataStore.uiLanguage.uiSettings.quit;
			continueText.text = DataStore.uiLanguage.uiSetup.continueBtn;

			message.text = $"<color=yellow>{header}</color>\n\n<align=left>{m}</align>";

			popupBase.Show();
		}

		/// <summary>
		/// Centered header, left-aligned message and stack trace
		/// </summary>
		public void Show( string header, Exception e )
		{
			exitText.text = DataStore.uiLanguage.uiSettings.quit;
			continueText.text = DataStore.uiLanguage.uiSetup.continueBtn;

			message.text = $"<color=yellow>{header}</color>\n\n<align=left><color=orange>{e.Message}</color>\n{e.StackTrace.Replace( " at ", "\nat " )}</align>";

			popupBase.Show();
		}

		public void ContinueApp()
		{
			Hide();
		}

		public void Hide()
		{
			popupBase.Close();
		}

		public void OnExitApp()
		{
			Application.Quit();
		}
	}
}
