using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ClickHandler : MonoBehaviour, IPointerClickHandler, IDragHandler
{
	public UnityEvent clickCallback;
	public UnityEvent doubleClickCallback;
	public UnityEvent rightClickCallback;
	public UnityEvent swipeCallback;

	public void OnDrag( PointerEventData eventData )
	{
		swipeCallback?.Invoke();
	}

	public void OnPointerClick( PointerEventData eventData )
	{
		if ( eventData.clickCount == 1 )
		{
			if ( eventData.button == PointerEventData.InputButton.Left )
				clickCallback?.Invoke();
			else if ( eventData.button == PointerEventData.InputButton.Right )
				rightClickCallback?.Invoke();
			else if ( eventData.pointerId == 1 )//touch
				clickCallback?.Invoke();
		}
		else if ( eventData.clickCount == 2 )
		{
			if ( eventData.button == PointerEventData.InputButton.Left )
				doubleClickCallback?.Invoke();
			else if ( eventData.pointerId == 1 )//touch
				doubleClickCallback?.Invoke();
		}
	}
}
