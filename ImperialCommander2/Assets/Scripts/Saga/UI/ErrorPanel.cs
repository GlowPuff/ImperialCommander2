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

		Action closeCallback = null;

		/// <summary>
		/// centered message
		/// </summary>
		public void Show( string m, Action onCloseCallback = null )
		{
			exitText.text = DataStore.uiLanguage.uiSettings.quit;
			continueText.text = DataStore.uiLanguage.uiSetup.continueBtn;

			message.text = m;
			closeCallback = onCloseCallback;

			popupBase.Show();
		}

		/// <summary>
		/// Centered header, left-aligned message
		/// </summary>
		public void Show( string header, string m, Action onCloseCallback = null )
		{
			try
			{
				exitText.text = DataStore.uiLanguage.uiSettings.quit;
				continueText.text = DataStore.uiLanguage.uiSetup.continueBtn;
			}
			catch ( Exception )
			{
				exitText.text = "EXIT APP";
				continueText.text = "CONTINUE";
			}

			message.text = $"<color=yellow>{header}</color>\n\n<align=left>{m}</align>";
			closeCallback = onCloseCallback;

			popupBase.Show();
		}

		/// <summary>
		/// Centered header, left-aligned message and stack trace
		/// </summary>
		public void Show( string header, Exception e, Action onCloseCallback = null )
		{
			try
			{
				exitText.text = DataStore.uiLanguage.uiSettings.quit;
				continueText.text = DataStore.uiLanguage.uiSetup.continueBtn;
			}
			catch ( Exception )
			{
				exitText.text = "EXIT APP";
				continueText.text = "CONTINUE";
			}

			message.text = $"<color=yellow>{header}</color>\n\n<align=left><color=orange>{e.Message}</color>\n{e.StackTrace.Replace( " at ", "\nat " )}</align>";
			closeCallback = onCloseCallback;

			popupBase.Show();
		}

		public void ContinueApp()
		{
			Hide();
		}

		public void Hide()
		{
			popupBase.Close( closeCallback );
		}

		public void OnExitApp()
		{
			Application.Quit();
		}
	}
}
