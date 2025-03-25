using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Saga
{
	public class CameraController : MonoBehaviour
	{
		public Camera cam, cam2D, topDownCamera;
		public Transform camRotator, camBirdLocalPosition;
		public float rotationSensitivity = 25f;

		public float resetWheelValue = -1;
		public float wheelValue;
		public int maxValue = 10;
		public int minValue = 0;
		public float interval = 1;

		//private Sound sound;
		private bool acceptNavivation = true;
		private Vector3 dragOrigin, rotOrigin, camLocalOrigin, camNormal, touchStart, topDownCamLocalOrigin;
		private float rotStart;
		private bool mButtonDown = false;

		bool oneClick = false;
		bool isTouching = false;
		float doubleClickTimer;
		float delay = .35f;
		float prevDistance = 0;
		float curDistance = 0;
		CameraView viewMode = CameraView.Normal;

		public Camera ActiveCamera
		{
			get { return viewMode == CameraView.Normal ? cam : topDownCamera; }
		}

		private void Start()
		{
			//sound = FindObjectOfType<Sound>();
			camLocalOrigin = cam.transform.localPosition;
			camNormal = cam.transform.forward.normalized;
			topDownCamLocalOrigin = topDownCamera.transform.localPosition;
		}

		//private void Update()
		//{
		//	int pointerID = -1;//mouse
		//	if ( Input.touchCount > 0 && Input.GetTouch( 0 ).phase == TouchPhase.Began )
		//	{
		//		pointerID = Input.GetTouch( 0 ).fingerId;
		//		touchStart = cam2D.ScreenToViewportPoint( Input.GetTouch( 0 ).position );
		//		rotStart = camRotator.rotation.eulerAngles.y;
		//	}

		//	isTouching = Input.touchCount == 0 ? false : true;
		//	if ( !isTouching )
		//	{
		//		prevDistance = curDistance = 0;
		//	}

		//	if ( Input.touchCount == 2 )
		//		HandleTouchGestures(); // New combined handler for both zoom and rotation

		//	if ( acceptNavivation )
		//	{
		//		if ( Input.touchCount < 2 )
		//			updateTranslation( pointerID );

		//		if ( !isTouching )
		//		{
		//			updateRotation();
		//			updateZoom();
		//			updateReset();
		//		}

		//		updateDoubleClick( pointerID );
		//	}
		//}

		private void Update()
		{
			int pointerID = -1;//mouse
			if ( Input.touchCount > 0 && Input.GetTouch( 0 ).phase == TouchPhase.Began )
			{
				pointerID = Input.GetTouch( 0 ).fingerId;
				touchStart = cam2D.ScreenToViewportPoint( Input.GetTouch( 0 ).position );
				rotStart = camRotator.rotation.eulerAngles.y;
			}

			isTouching = Input.touchCount == 0 ? false : true;
			if ( !isTouching )
			{
				prevDistance = curDistance = 0;
			}

			if ( Input.touchCount == 2 )
				HandleTouchZoom();
			else if ( Input.touchCount == 3 )
				HandleTouchRotate();

			if ( acceptNavivation )
			{
				if ( Input.touchCount < 2 )
					updateTranslation( pointerID );

				if ( !isTouching )
				{
					updateRotation();
					updateZoom();
					updateReset();
				}

				updateDoubleClick( pointerID );
			}
		}

		private Vector2 prevPos1, prevPos2; // Add these as class fields
		private bool wasZoomingLastFrame = false;

		//void HandleTouchGestures()
		//{
		//	if ( FindObjectOfType<SagaEventManager>().UIShowing
		//			|| EventSystem.current.IsPointerOverGameObject( -1 ) )
		//		return;

		//	Touch touch1 = Input.GetTouch( 0 );
		//	Touch touch2 = Input.GetTouch( 1 );

		//	// Store current positions
		//	Vector2 currentPos1 = touch1.position;
		//	Vector2 currentPos2 = touch2.position;

		//	if ( touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began )
		//	{
		//		prevPos1 = currentPos1;
		//		prevPos2 = currentPos2;
		//		prevDistance = Vector2.Distance( currentPos1, currentPos2 );
		//		wasZoomingLastFrame = false;
		//		return;
		//	}

		//	if ( touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved )
		//	{
		//		// Calculate the current distance between fingers
		//		curDistance = Vector2.Distance( currentPos1, currentPos2 );

		//		// Calculate the difference in distances
		//		float deltaDistance = curDistance - prevDistance;

		//		// If the distance change is significant, handle as zoom
		//		if ( Mathf.Abs( deltaDistance ) > 5f )
		//		{
		//			// Zooming
		//			if ( curDistance > prevDistance )
		//			{
		//				wheelValue += interval / 3f;
		//			}
		//			else
		//			{
		//				wheelValue -= interval / 3f;
		//			}

		//			wheelValue = Mathf.Clamp( wheelValue, minValue, maxValue );

		//			// Apply zoom
		//			Vector3 nv = camLocalOrigin + camNormal * wheelValue;
		//			nv.x = 0;
		//			cam.transform.localPosition = nv;
		//			nv = topDownCamLocalOrigin + Vector3.down * wheelValue;
		//			nv.x = 0;
		//			topDownCamera.transform.localPosition = nv;

		//			wasZoomingLastFrame = true;
		//		}
		//		// If not zooming significantly, handle rotation
		//		else if ( !wasZoomingLastFrame )
		//		{
		//			// Calculate rotation angle
		//			float prevAngle = Mathf.Atan2( prevPos2.y - prevPos1.y, prevPos2.x - prevPos1.x ) * Mathf.Rad2Deg;
		//			float currentAngle = Mathf.Atan2( currentPos2.y - currentPos1.y, currentPos2.x - currentPos1.x ) * Mathf.Rad2Deg;
		//			float rotationAngle = currentAngle - prevAngle;

		//			// Apply rotation
		//			camRotator.rotation *= Quaternion.Euler( 0, -rotationAngle * (rotationSensitivity * 0.05f), 0 );
		//		}

		//		// Store positions for next frame
		//		prevPos1 = currentPos1;
		//		prevPos2 = currentPos2;
		//		prevDistance = curDistance;
		//	}
		//}

		//void HandleTouchRotate()
		//{
		//	if ( FindObjectOfType<SagaEventManager>().UIShowing
		//			|| EventSystem.current.IsPointerOverGameObject( -1 ) )
		//		return;

		//	// Get both touch positions
		//	Touch touch1 = Input.GetTouch( 0 );
		//	Touch touch2 = Input.GetTouch( 1 );

		//	if ( touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved )
		//	{
		//		// Get the previous frame's touch positions
		//		Vector2 prevPos1 = touch1.position - touch1.deltaPosition;
		//		Vector2 prevPos2 = touch2.position - touch2.deltaPosition;

		//		// Get current frame's touch positions
		//		Vector2 currentPos1 = touch1.position;
		//		Vector2 currentPos2 = touch2.position;

		//		// Calculate the angles between the two lines
		//		float prevAngle = Mathf.Atan2( prevPos2.y - prevPos1.y, prevPos2.x - prevPos1.x ) * Mathf.Rad2Deg;
		//		float currentAngle = Mathf.Atan2( currentPos2.y - currentPos1.y, currentPos2.x - currentPos1.x ) * Mathf.Rad2Deg;

		//		// Calculate rotation angle
		//		float rotationAngle = currentAngle - prevAngle;

		//		// Apply rotation with sensitivity
		//		camRotator.rotation *= Quaternion.Euler( 0, -rotationAngle * (rotationSensitivity * 0.05f), 0 );
		//	}
		//}

		void HandleTouchRotate()
		{
			if ( FindObjectOfType<SagaEventManager>().UIShowing
				|| EventSystem.current.IsPointerOverGameObject( -1 ) )
				return;

			if ( Input.GetTouch( 0 ).phase == TouchPhase.Moved )
			{
				Vector3 curPosition = cam2D.ScreenToViewportPoint( Input.GetTouch( 0 ).position );
				//rotate
				float diff = touchStart.x - curPosition.x;
				Vector2 delta = Input.GetTouch( 0 ).deltaPosition;
				//only allow one movement per touchdown
				camRotator.rotation = Quaternion.Euler( 0, rotStart + diff * -rotationSensitivity * 1.5f, 0 );
			}
		}

		void HandleTouchZoom()
		{
			if ( FindObjectOfType<SagaEventManager>().UIShowing
				|| EventSystem.current.IsPointerOverGameObject( -1 ) )
				return;

			if ( Input.GetTouch( 0 ).phase == TouchPhase.Moved )
			{
				Vector2 curPosition = Input.GetTouch( 0 ).position;
				Vector2 curPosition2 = Input.GetTouch( 1 ).position;

				curDistance = Vector2.Distance( curPosition, curPosition2 );
				if ( curDistance > prevDistance )
				{
					//zooming out
					wheelValue += interval / 3f;
					wheelValue = Mathf.Clamp( wheelValue, minValue, maxValue );
				}
				else if ( curDistance < prevDistance )
				{
					//zooming in
					wheelValue -= interval / 3f;
					wheelValue = Mathf.Clamp( wheelValue, minValue, maxValue );
				}

				Vector3 nv = camLocalOrigin + camNormal * wheelValue;
				nv.x = 0;
				cam.transform.localPosition = nv;
				nv = topDownCamLocalOrigin + Vector3.down * wheelValue;
				nv.x = 0;
				topDownCamera.transform.localPosition = nv;

				prevDistance = curDistance;
			}
		}

		void updateTranslation( int pointerID )
		{
			//make sure not clicking UI
			//get mouse world coords on first click
			if ( (Input.GetMouseButtonDown( 0 ) && !EventSystem.current.IsPointerOverGameObject( pointerID )) )
			{
				//dragOrigin = GetMousePosition();
				if ( GetMousePosition( out Vector3 dOrigin ) )
				{
					dragOrigin = dOrigin;
					mButtonDown = true;
				}
			}
			//get distance between current and saved position while held down
			if ( Input.GetMouseButton( 0 ) && mButtonDown )
			{
				//Vector3 difference = dragOrigin - GetMousePosition();
				//move camera by that distance
				if ( GetMousePosition( out Vector3 p ) )
				{
					Vector3 difference = dragOrigin - p;
					transform.position += difference;
				}
				else
					mButtonDown = false;
			}
			else
				mButtonDown = false;
		}

		void updateRotation()
		{
			if ( FindObjectOfType<SagaEventManager>().UIShowing
				|| EventSystem.current.IsPointerOverGameObject( -1 ) )
				return;

			if ( Input.GetMouseButtonDown( 1 ) )
			{
				rotOrigin = cam2D.ScreenToViewportPoint( Input.mousePosition );
				rotStart = camRotator.rotation.eulerAngles.y;
			}
			if ( Input.GetMouseButton( 1 ) )
			{
				float diff = rotOrigin.x - cam2D.ScreenToViewportPoint( Input.mousePosition ).x;
				camRotator.rotation = Quaternion.Euler( 0, rotStart + diff * -rotationSensitivity, 0 );
			}
		}

		void updateZoom()
		{
			if ( FindObjectOfType<SagaEventManager>().UIShowing
				|| EventSystem.current.IsPointerOverGameObject( -1 ) )
				return;
			/*
			 * Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            float zoomDistance = zoomSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
            camera.transform.Translate(ray.direction * zoomDistance, Space.World);
			*/
			//0 to 1.5
			if ( Input.mouseScrollDelta.magnitude > 0 )
			{
				if ( Input.mouseScrollDelta.y == 1 )//up/zoom in
				{
					wheelValue += interval;
					wheelValue = Mathf.Clamp( wheelValue, minValue, maxValue );
				}
				else if ( Input.mouseScrollDelta.y == -1 )//down/zoom out
				{
					wheelValue -= interval;
					wheelValue = Mathf.Clamp( wheelValue, minValue, maxValue );
				}

				Vector3 nv = camLocalOrigin + camNormal * wheelValue;
				nv.x = 0;
				cam.transform.localPosition = nv;
				nv = topDownCamLocalOrigin + Vector3.down * wheelValue;
				nv.x = 0;
				topDownCamera.transform.localPosition = nv;
			}
		}

		void updateReset()
		{
			if ( Input.GetMouseButtonDown( 2 ) )
			{
				wheelValue = resetWheelValue;
				//cam.transform.localPosition = camLocalOrigin + camNormal * wheelValue;
				//camRotator.rotation = Quaternion.Euler( 0, 0, 0 );
				Vector3 nv = camLocalOrigin + camNormal * wheelValue;
				nv.x = 0;
				cam.transform.DOLocalMove( nv, 1 ).SetEase( Ease.InOutCubic );
				nv = topDownCamLocalOrigin + Vector3.down * wheelValue;
				nv.x = 0;
				topDownCamera.transform.DOLocalMove( nv, 1 ).SetEase( Ease.InOutCubic );
				camRotator.DORotateQuaternion( Quaternion.Euler( 0, 0, 0 ), 1 ).SetEase( Ease.InOutCubic );
			}
		}

		/// <summary>
		/// moves camera to d-clicked position
		/// </summary>
		void updateDoubleClick( int pointerID )
		{
			if ( Input.GetMouseButtonDown( 0 ) && !EventSystem.current.IsPointerOverGameObject( pointerID ) )
			{
				if ( !oneClick )//first click, no previous clicks
				{
					oneClick = true;
					doubleClickTimer = Time.time;//save the current time
				}
				else
				{
					oneClick = false;//found a double click, now reset
					if ( GetMousePosition( out Vector3 camP ) )
					{
						//Vector3 camP = GetMousePosition();
						//vector towards where clicked
						if ( viewMode == CameraView.Normal )
						{
							Vector3 dir = Vector3.Normalize( cam.transform.position - camP );
							Vector3 target = camP + dir * 3f;
							MoveTo( target, 1, 0, false );
						}
						else
						{
							Vector3 dir = Vector3.Normalize( topDownCamera.transform.position - camP );
							Vector3 target = camP + dir * 3f;
							MoveTo( target, 1, 0, false );
						}
					}
				}
			}
			if ( oneClick )
			{
				if ( (Time.time - doubleClickTimer) > delay )
					oneClick = false;
			}
		}

		/// <summary>
		/// offset is no longer used
		/// </summary>
		public void MoveTo( Vector3 p, float speed = 1, float offset = 0, bool reset = false, Action callback = null )
		{
			acceptNavivation = false;
			p = new Vector3( p.x, 0, p.z/* - offset*/ );
			transform.DOKill( true );
			transform.DOMove( p, speed ).OnComplete( () =>
			{
				acceptNavivation = true;
				callback?.Invoke();
			} ).SetEase( Ease.InOutCubic );

			if ( reset )
			{
				wheelValue = resetWheelValue;
				Vector3 nv = camLocalOrigin + camNormal * wheelValue;
				nv.x = 0;
				cam.transform.DOLocalMove( nv, speed ).SetEase( Ease.InOutCubic );
				nv = topDownCamLocalOrigin + Vector3.down * wheelValue;
				nv.x = 0;
				topDownCamera.transform.DOLocalMove( nv, speed ).SetEase( Ease.InOutCubic );
			}
		}

		public void MoveToImmediate( Vector3 p, float offset = 0, bool reset = false, Action callback = null )
		{
			transform.position = new Vector3( p.x, 0, p.z - offset );
			if ( reset )
			{
				wheelValue = resetWheelValue;
				cam.transform.localPosition = camLocalOrigin + camNormal * wheelValue;
				topDownCamera.transform.localPosition = topDownCamLocalOrigin + Vector3.down * wheelValue;
				camRotator.rotation = Quaternion.Euler( 0, 0, 0 );
			}
			callback?.Invoke();
		}

		bool GetMousePosition( out Vector3 position )
		{
			position = Vector3.zero;

			try
			{
				Plane plane = new Plane( Vector3.up, 0 );
				float distance;
				var mousePos = Input.mousePosition;

				//if mouse is outside of screen, return false and avoid out of frustum errors with ScreenPointToRay
				if ( mousePos.x < 0 || mousePos.x >= Screen.width
					|| mousePos.y < 0 || mousePos.y >= Screen.height )
					return false;

				Ray ray = cam.ScreenPointToRay( mousePos );
				if ( plane.Raycast( ray, out distance ) )
				{
					position = ray.GetPoint( distance );
					return true;
				}
				else
				{
					return true;
				}
			}
			catch ( Exception e )
			{
				Utils.LogWarning( e.Message );
				return false;
			}
		}

		public void ToggleNavigation( bool canNav )
		{
			//Debug.Log( $"ToggleNavigation()::{canNav}" );
			acceptNavivation = canNav;
		}

		public void MoveToEntity( Guid guid, Action cb = null )
		{
			if ( guid != Guid.Empty )
			{
				IMapEntity e = FindObjectOfType<SagaController>().mapEntityManager.GetEntity( guid );
				if ( e != null )
					MoveTo( e.entityPosition.ToUnityV3(), 1, 5, true, cb );
			}
			else
				cb?.Invoke();
		}

		public void OnZoomInButton()
		{
			wheelValue += interval;
			wheelValue = Mathf.Clamp( wheelValue, minValue, maxValue );
			Vector3 nv = camLocalOrigin + camNormal * wheelValue;
			nv.x = 0;
			cam.transform.localPosition = nv;
			nv = topDownCamLocalOrigin + Vector3.down * wheelValue;
			nv.x = 0;
			topDownCamera.transform.localPosition = nv;
		}

		public void OnZoomOutButton()
		{
			wheelValue -= interval;
			wheelValue = Mathf.Clamp( wheelValue, minValue, maxValue );
			Vector3 nv = camLocalOrigin + camNormal * wheelValue;
			nv.x = 0;
			cam.transform.localPosition = nv;
			nv = topDownCamLocalOrigin + Vector3.down * wheelValue;
			nv.x = 0;
			topDownCamera.transform.localPosition = nv;
		}

		public void CameraViewToggle( CameraView mode )
		{
			viewMode = mode;

			if ( mode == CameraView.Normal )
			{
				cam.gameObject.SetActive( true );
				topDownCamera.gameObject.SetActive( false );
				minValue = -25;
				//reset the zoom if the wheelValue is now out of range for the current mode
				OnZoomInButton();
				OnZoomOutButton();
			}
			else
			{
				cam.gameObject.SetActive( false );
				topDownCamera.gameObject.SetActive( true );
				camRotator.rotation = Quaternion.Euler( 0, 0, 0 );
				minValue = -35;
			}
		}
	}
}
