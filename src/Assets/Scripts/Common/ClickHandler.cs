using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ClickHandler : MonoBehaviour, IPointerClickHandler, IDragHandler
{
	public UnityEvent clickCallback;
	public UnityEvent rightClickCallback;
	public UnityEvent swipeCallback;

	public void OnDrag( PointerEventData eventData )
	{
		swipeCallback.Invoke();
	}

	public void OnPointerClick( PointerEventData eventData )
	{
		if ( eventData.button == PointerEventData.InputButton.Left )
			clickCallback.Invoke();
		else if ( eventData.button == PointerEventData.InputButton.Right )
			rightClickCallback.Invoke();
		else if ( eventData.pointerId == 1 )//touch
			clickCallback.Invoke();
	}
}
