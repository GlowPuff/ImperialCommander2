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
		public Text exitText;

		/// <summary>
		/// centered message
		/// </summary>
		public void Show( string m )
		{
			message.text = m;

			popupBase.Show();
		}

		/// <summary>
		/// Centered header, left-aligned message
		/// </summary>
		public void Show( string header, string m )
		{
			message.text = $"<color=yellow>{header}</color>\n\n<align=left>{m}</align>";

			popupBase.Show();
		}

		/// <summary>
		/// Centered header, left-aligned message and stack trace
		/// </summary>
		public void Show( string header, Exception e )
		{
			message.text = $"<color=yellow>{header}</color>\n\n<align=left><color=orange>{e.Message}</color>\n{e.StackTrace.Replace( " at ", "\nat " )}</align>";

			popupBase.Show();
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
