using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MWheelHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler
{
	public int wheelValue;
	public int maxValue = 10;
	public int minValue = 0;
	public Text numberText;

	//swiping
	public float distancePerTick = 15;//distance (pixels) have to swipe to register 1 tick of increment/decrement

	private float currentDistance = 0;

	bool isHovering = false;
	Sound sound;

	private void Start()
	{
		if ( numberText == null )
			numberText = GetComponent<Text>();
		sound = FindObjectOfType<Sound>();
	}

	void Update()
	{
		if ( Input.mouseScrollDelta.magnitude > 0 && isHovering )
		{
			if ( Input.mouseScrollDelta.y == 1 )
			{
				wheelValue = Mathf.Min( maxValue, wheelValue + 1 );
				sound.PlaySound( FX.Click );
			}
			else if ( Input.mouseScrollDelta.y == -1 )
			{
				wheelValue = Mathf.Max( minValue, wheelValue - 1 );
				sound.PlaySound( FX.Click );
			}
		}

		numberText.text = wheelValue.ToString();
	}

	public void OnPointerEnter( PointerEventData eventData )
	{
		isHovering = true;
	}

	public void OnPointerExit( PointerEventData eventData )
	{
		isHovering = false;
	}

	public void ResetWheeler( int value = 0 )
	{
		wheelValue = value;
		numberText.text = wheelValue.ToString();
	}

	public void OnDrag( PointerEventData eventData )
	{
		eventData.useDragThreshold = true;

		currentDistance += Math.Abs( eventData.delta.x );
		if ( currentDistance >= distancePerTick )
		{
			currentDistance = 0;
			if ( eventData.delta.x > 0 )
				wheelValue = Mathf.Min( maxValue, wheelValue + 1 );
			else
				wheelValue = Mathf.Max( minValue, wheelValue - 1 );
		}
	}
}
