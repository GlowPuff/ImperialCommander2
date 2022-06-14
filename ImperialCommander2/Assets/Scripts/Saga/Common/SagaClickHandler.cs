using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Saga
{

	public class SagaClickHandler : MonoBehaviour, IPointerClickHandler/*, IBeginDragHandler, IDragHandler, IEndDragHandler*/
	{
		public UnityEvent clickCallback;
		public UnityEvent doubleClickCallback;
		public UnityEvent rightClickCallback;
		public UnityEvent swipeCallback;

		//private Vector2 deltaValue = Vector2.zero;
		float lastTimeClick;

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

		//public void OnBeginDrag( PointerEventData data )
		//{
		//	deltaValue = Vector2.zero;
		//}

		//public void OnEndDrag( PointerEventData data )
		//{
		//	deltaValue = Vector2.zero;
		//}

		//public void OnDrag( PointerEventData data )
		//{
		//	deltaValue += data.delta;
		//	if ( data.dragging && deltaValue.magnitude > .3f && inputTimer == 0 )
		//	{
		//		inputTimer = 2;
		//		doubleClickCallback?.Invoke();
		//	}
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
			}
			//handle double click
			else if ( eventData.clickCount == 2
				&& eventData.button == PointerEventData.InputButton.Left )
			{
				doubleClickCallback?.Invoke();
			}

			//handle double tap
			float currentTimeClick = eventData.clickTime;
			if ( Mathf.Abs( currentTimeClick - lastTimeClick ) < 0.75f )
			{
				doubleClickCallback?.Invoke();
			}

			lastTimeClick = currentTimeClick;
		}

		private void Update()
		{
			//handle double tap
			//foreach ( var touch in Input.touches )
			//{
			//	if ( touch.tapCount == 2 )
			//	{
			//		doubleClickCallback?.Invoke();
			//	}
			//}
		}
	}
}