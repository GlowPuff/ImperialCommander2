using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MWheelHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IPointerClickHandler
{
	public bool disableMouseWheel = false;
	public int wheelValue;
	public int maxValue = 10;
	public int minValue = 0;
	public int stepValue = 1;
	public Text numberText;
	public TextMeshProUGUI numberTextTMP;
	public ValueAdjuster valueAdjuster;
	public UnityEvent wheelValueChanged;
	public Action wheelValueChangedCallback;

	//swiping
	public float distancePerTick = 15;//distance (pixels) have to swipe to register 1 tick of increment/decrement

	//private float currentDistance = 0;

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
		if ( !disableMouseWheel && Input.mouseScrollDelta.magnitude > 0 && isHovering )
		{
			if ( Input.mouseScrollDelta.y == 1 )
			{
				OnAdd();
			}
			else if ( Input.mouseScrollDelta.y == -1 )
			{
				OnSubtract();
			}
		}
	}

	public void OnPointerEnter( PointerEventData eventData )
	{
		isHovering = true;
	}

	public void OnPointerExit( PointerEventData eventData )
	{
		isHovering = false;
	}

	public void OnPointerClick( PointerEventData eventData )
	{
		if ( eventData.button == PointerEventData.InputButton.Left )
		{
			if ( valueAdjuster != null )
				valueAdjuster.Show( wheelValue, this );
		}
	}

	public void OnAdd()
	{
		wheelValue = Mathf.Min( maxValue, wheelValue + stepValue );
		wheelValueChanged?.Invoke();
		wheelValueChangedCallback?.Invoke();
		sound?.PlaySound( FX.Click );
		UpdateTargetValue();
	}

	public void OnSubtract()
	{
		wheelValue = Mathf.Max( minValue, wheelValue - stepValue );
		wheelValueChanged?.Invoke();
		wheelValueChangedCallback?.Invoke();
		sound?.PlaySound( FX.Click );
		UpdateTargetValue();
	}

	public void ResetWheeler( int value = 0 )
	{
		wheelValue = value;
		UpdateTargetValue();
		wheelValueChanged?.Invoke();
	}

	/// <summary>
	/// deprecated
	/// </summary>
	public void OnDrag( PointerEventData eventData )
	{
		//eventData.useDragThreshold = true;

		//currentDistance += Math.Max( Math.Abs( eventData.delta.y ), Math.Abs( eventData.delta.x ) );
		//if ( currentDistance >= distancePerTick )
		//{
		//	currentDistance = 0;
		//	if ( eventData.delta.x > 0 || eventData.delta.y > 0 )
		//		wheelValue = Mathf.Min( maxValue, wheelValue + stepValue );
		//	else
		//		wheelValue = Mathf.Max( minValue, wheelValue - stepValue );

		//	wheelValueChanged?.Invoke();
		//	UpdateTargetValue();
		//}
	}

	void UpdateTargetValue()
	{
		valueAdjuster?.SetValue( wheelValue );
		if ( numberText != null )
			numberText.text = wheelValue.ToString();
		if ( numberTextTMP != null )
			numberTextTMP.text = wheelValue.ToString();
	}
}
