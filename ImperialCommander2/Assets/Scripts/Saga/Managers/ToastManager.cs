using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ToastManager : MonoBehaviour
{
	public float visibleTime = 3f;
	public Transform toastTransform;
	public TextMeshProUGUI messageText;
	public CanvasGroup cg;

	bool isActivated = false;
	List<string> messageList = new List<string>();

	/// <summary>
	/// Shows a 3-second toast notification, accumulating messages if additional messages are shown while the toast is already active
	/// </summary>
	public void ShowToast( string message )
	{
		if ( !isActivated )
		{
			isActivated = true;

			messageList.Add( message );

			toastTransform.gameObject.SetActive( true );
			cg.alpha = 0;

			cg.DOFade( 1, .5f );
			GlowTimer.SetTimer( visibleTime, () =>
			{
				cg.DOFade( 0, .25f ).OnComplete( () =>
						{
							toastTransform.gameObject.SetActive( false );
							messageList.Clear();
							isActivated = false;
						}
					);
			} );
		}
		else
			messageList.Add( message );

		messageText.text = messageList.Aggregate( ( acc, cur ) => acc + ", " + cur );
	}

}
