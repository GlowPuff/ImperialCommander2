using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Saga
{
	public class ErrorPanel : MonoBehaviour
	{
		public TextMeshProUGUI message;
		public CanvasGroup cg;
		public PopupBase popupBase;
		public void Show( string m )
		{
			message.text = m;

			cg.DOFade( 1, .2f );
			popupBase.Show();
		}

		public void Hide()
		{
			cg.DOFade( 1, .2f );
			popupBase.Close();
		}
	}
}
