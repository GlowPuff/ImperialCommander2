using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Saga
{

	public class SagaClickHandler : MonoBehaviour, IPointerClickHandler//, IDragHandler
	{
		public UnityEvent clickCallback;
		public UnityEvent doubleClickCallback;
		public UnityEvent rightClickCallback;
		public UnityEvent swipeCallback;

		//public void OnDrag( PointerEventData eventData )
		//{
		//enable scroll view to handle dragging
		//	ScrollRect scrollRect = transform.parent.parent.parent.parent.GetComponent<ScrollRect>();
		//	eventData.pointerDrag = scrollRect.gameObject;
		//	EventSystem.current.SetSelectedGameObject( scrollRect.gameObject );
		//	scrollRect.OnInitializePotentialDrag( eventData );
		//	scrollRect.OnBeginDrag( eventData );

		//	swipeCallback.Invoke();
		//}

		public void OnPointerClick( PointerEventData eventData )
		{
			//handle single click mouse and touch
			if ( eventData.clickCount == 1 )
			{
				if ( eventData.button == PointerEventData.InputButton.Left )
					clickCallback?.Invoke();
				else if ( eventData.button == PointerEventData.InputButton.Right )
					rightClickCallback?.Invoke();
				//else if ( eventData.pointerId == 1 )//touch
				//	clickCallback.Invoke();
			}
			//handle double click mouse and touch
			else if ( eventData.clickCount == 2 )
			{
				if ( eventData.button == PointerEventData.InputButton.Left )
					doubleClickCallback?.Invoke();
				else if ( eventData.pointerId == 1 )//touch
					doubleClickCallback?.Invoke();
			}
		}

		private void Update()
		{
			//handle single touch (left click)
			if ( Input.touchCount == 1 )
			{
				var touch = Input.GetTouch( 0 );
				if ( touch.phase == TouchPhase.Began )
					clickCallback?.Invoke();
			}
			//handle 2-finger touch input (right click)
			else if ( Input.touchCount == 2 )
			{
				var touch = Input.GetTouch( 0 );
				var touch2 = Input.GetTouch( 1 );
				if ( touch.phase == TouchPhase.Began && touch2.phase == TouchPhase.Began )
					rightClickCallback?.Invoke();
			}
		}
	}
}