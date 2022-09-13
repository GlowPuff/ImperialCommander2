using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MWheelHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler
{
	public int wheelValue;
	public int maxValue = 10;
	public int minValue = 0;
	public Text numberText;
	public TextMeshProUGUI numberTextTMP;

	public UnityEvent wheelValueChanged;

	//swiping
	public float distancePerTick = 15;//distance (pixels) have to swipe to register 1 tick of increment/decrement

	private float currentDistance = 0;

	bool isHovering = false;
	Sound sound;

	private void Start()
	{
		if ( numberText == null )
			numberText = GetComponent<Text>();
		if ( numberTextTMP == null )
			numberTextTMP = GetComponent<TextMeshProUGUI>();
		sound = FindObjectOfType<Sound>();
	}

	void Update()
	{
		if ( Input.mouseScrollDelta.magnitude > 0 && isHovering )
		{
			if ( Input.mouseScrollDelta.y == 1 )
			{
				wheelValue = Mathf.Min( maxValue, wheelValue + 1 );
				wheelValueChanged?.Invoke();
				sound.PlaySound( FX.Click );
			}
			else if ( Input.mouseScrollDelta.y == -1 )
			{
				wheelValue = Mathf.Max( minValue, wheelValue - 1 );
				wheelValueChanged?.Invoke();
				sound.PlaySound( FX.Click );
			}
		}

		if ( numberText != null )
			numberText.text = wheelValue.ToString();
		if ( numberTextTMP != null )
			numberTextTMP.text = wheelValue.ToString();
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
		if ( numberText != null )
			numberText.text = wheelValue.ToString();
		if ( numberTextTMP != null )
			numberTextTMP.text = wheelValue.ToString();
		wheelValueChanged?.Invoke();
	}

	public void OnDrag( PointerEventData eventData )
	{
		eventData.useDragThreshold = true;

		currentDistance += Math.Max( Math.Abs( eventData.delta.y ), Math.Abs( eventData.delta.x ) );
		if ( currentDistance >= distancePerTick )
		{
			currentDistance = 0;
			if ( eventData.delta.x > 0 || eventData.delta.y > 0 )
				wheelValue = Mathf.Min( maxValue, wheelValue + 1 );
			else
				wheelValue = Mathf.Max( minValue, wheelValue - 1 );

			wheelValueChanged?.Invoke();
		}
	}
}
