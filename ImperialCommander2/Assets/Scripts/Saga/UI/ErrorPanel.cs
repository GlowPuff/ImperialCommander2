using TMPro;
using UnityEngine;

namespace Saga
{
	public class ErrorPanel : MonoBehaviour
	{
		public TextMeshProUGUI message;
		public PopupBase popupBase;
		public void Show( string m )
		{
			message.text = m;

			popupBase.Show();
		}

		public void Hide()
		{
			popupBase.Close();
		}
	}
}
