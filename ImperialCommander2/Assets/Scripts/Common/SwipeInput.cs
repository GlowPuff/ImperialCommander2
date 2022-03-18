using System;
using UnityEngine;
using UnityEngine.Events;

public class SwipeInput : MonoBehaviour
{

	public float swipeThreshold = 20f;
	public float timeThreshold = 0.5f;
	public float distancePerTick = 15;//distance (pixels) have to swipe to register 1 tick of increment/decrement

	public UnityEvent onSwipeLeft;
	public UnityEvent onSwipeRight;
	//public UnityEvent onSwipeUp;
	//public UnityEvent onSwipeDown;

	private Vector2 _fingerDown;
	private DateTime _fingerDownTime;
	private Vector2 _fingerUp;
	private DateTime _fingerUpTime;
	private bool isSwiping = false;
	private float currentDistance = 0;

	private void Update()
	{
		if ( Input.GetMouseButtonDown( 0 ) )
		{
			_fingerDown = Input.mousePosition;
			_fingerUp = Input.mousePosition;
			_fingerDownTime = DateTime.Now;
			currentDistance = 0;
			isSwiping = true;
		}

		if ( Input.GetMouseButtonUp( 0 ) )
		{
			_fingerDown = Input.mousePosition;
			_fingerUpTime = DateTime.Now;
			isSwiping = false;
			//CheckSwipe();
		}

		foreach ( var touch in Input.touches )
		{
			if ( touch.phase == TouchPhase.Began )
			{
				_fingerDown = touch.position;
				_fingerUp = touch.position;
				_fingerDownTime = DateTime.Now;
				currentDistance = 0;
				isSwiping = true;
			}

			if ( touch.phase == TouchPhase.Ended )
			{
				_fingerDown = touch.position;
				_fingerUpTime = DateTime.Now;
				isSwiping = false;
				//CheckSwipe();
			}
		}

		if ( isSwiping )
		{
			Vector2 currentPosition = _fingerDown;
			bool moved = false;

			if ( Input.GetMouseButton( 0 ) )
			{
				currentPosition = Input.mousePosition;
				if ( currentPosition != _fingerDown )
				{
					moved = true;
				}
			}
			else
			{
				foreach ( var touch in Input.touches )
				{
					if ( touch.phase == TouchPhase.Moved )
					{
						currentPosition = touch.position;
						moved = true;
					}
				}
			}

			if ( moved )
			{
				//print( "moved" );
				var dirVector = _fingerUp - _fingerDown;
				var direction = GlowEngine.VectorToAngle( dirVector );
				if ( direction < 0 )
				{
					currentDistance += Vector2.Distance( _fingerDown, currentPosition );
				}
				else
				{
					currentDistance -= Vector2.Distance( _fingerDown, currentPosition );
				}

				//print( "DISTANCE: " + currentDistance );

				if ( Math.Abs( currentDistance ) >= distancePerTick )
				{
					currentDistance = 0;
					if ( direction < 0 )
					{
						print( "tick right" );
						_fingerDown = currentPosition;
						onSwipeRight?.Invoke();
					}
					else
					{
						print( "tick left" );
						_fingerDown = currentPosition;
						onSwipeLeft?.Invoke();
					}
				}
			}
		}
	}

	private void CheckSwipe()
	{
		var duration = (float)_fingerUpTime.Subtract( _fingerDownTime ).TotalSeconds;
		var dirVector = _fingerUp - _fingerDown;

		if ( duration > timeThreshold )
			return;
		if ( dirVector.magnitude < swipeThreshold )
			return;

		var direction = GlowEngine.VectorToAngle( dirVector );

		//print( direction );

		//if ( direction >= 45 && direction < 135 )
		//	onSwipeUp.Invoke();
		if ( direction < 0 )//( direction >= 135 && direction < 225 )
		{
			//print( "right" );
			onSwipeRight.Invoke();
		}
		//else if ( direction >= 225 && direction < 315 )
		//	onSwipeDown.Invoke();
		else //if ( direction >= 315 && direction < 360 || direction >= 0 && direction < 45 )
		{
			//print( "left" );
			onSwipeLeft.Invoke();
		}
	}
}