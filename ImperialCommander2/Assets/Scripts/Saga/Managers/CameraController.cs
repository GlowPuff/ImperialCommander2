using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Saga
{
	public class CameraController : MonoBehaviour
	{
		public Camera cam, cam2D;
		public Transform camRotator;
		public float rotationSensitivity = 25f;

		public float resetWheelValue = -1;
		public float wheelValue;
		public int maxValue = 10;
		public int minValue = 0;
		public float interval = 1;

		//private Sound sound;
		private bool acceptNavivation = true;
		private Vector3 dragOrigin, rotOrigin, camLocalOrigin, camNormal, touchStart;
		private float rotStart;
		private bool mButtonDown = false;

		bool oneClick = false;
		bool isTouching = false;
		float doubleClickTimer;
		float delay = .35f;
		float prevDistance = 0;
		float curDistance = 0;

		private void Start()
		{
			//sound = FindObjectOfType<Sound>();
			camLocalOrigin = cam.transform.localPosition;
			camNormal = cam.transform.forward.normalized;
		}

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
					Vector3 nv = camLocalOrigin + camNormal * wheelValue;
					nv.x = 0;
					cam.transform.localPosition = nv;
				}
				else if ( curDistance < prevDistance )
				{
					//zooming in
					wheelValue -= interval / 3f;
					wheelValue = Mathf.Clamp( wheelValue, minValue, maxValue );
					Vector3 nv = camLocalOrigin + camNormal * wheelValue;
					nv.x = 0;
					cam.transform.localPosition = nv;
				}

				prevDistance = curDistance;
			}
		}

		void updateTranslation( int pointerID )
		{
			//make sure not clicking UI
			//get mouse world coords on first click
			if ( (Input.GetMouseButtonDown( 0 ) && !EventSystem.current.IsPointerOverGameObject( pointerID )) )
			{
				dragOrigin = GetMousePosition();
				mButtonDown = true;
			}
			//get distance between current and saved position while held down
			if ( Input.GetMouseButton( 0 ) && mButtonDown )
			{
				Vector3 difference = dragOrigin - GetMousePosition();
				//move camera by that distance
				transform.position += difference;
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
					Vector3 nv = camLocalOrigin + camNormal * wheelValue;
					nv.x = 0;
					cam.transform.localPosition = nv;
					//cam.fieldOfView = wheelValue;
					//sound.PlaySound( FX.Click );
				}
				else if ( Input.mouseScrollDelta.y == -1 )//down/zoom out
				{
					wheelValue -= interval;
					wheelValue = Mathf.Clamp( wheelValue, minValue, maxValue );
					Vector3 nv = camLocalOrigin + camNormal * wheelValue;
					nv.x = 0;
					cam.transform.localPosition = nv;
					//cam.fieldOfView = wheelValue;
					//sound.PlaySound( FX.Click );
				}
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
					Vector3 camP = GetMousePosition();
					//vector towards where clicked
					Vector3 dir = Vector3.Normalize( cam.transform.position - camP );
					Vector3 target = camP + dir * 3f;
					MoveTo( target, 1, 0, false );
				}
			}
			if ( oneClick )
			{
				if ( (Time.time - doubleClickTimer) > delay )
					oneClick = false;
			}
		}

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
				//DOTween.To( () => cam.fieldOfView, x => cam.fieldOfView = x, maxValue, speed ).SetEase( Ease.InOutCubic );
				//camRotator.DORotateQuaternion( Quaternion.Euler( 0, 0, 0 ), speed ).SetEase( Ease.InOutCubic );
			}
		}

		public void MoveToImmediate( Vector3 p, float offset = 0, bool reset = false, Action callback = null )
		{
			transform.position = new Vector3( p.x, 0, p.z - offset );
			if ( reset )
			{
				wheelValue = resetWheelValue;
				cam.transform.localPosition = camLocalOrigin + camNormal * wheelValue;
				//cam.fieldOfView = wheelValue;
				camRotator.rotation = Quaternion.Euler( 0, 0, 0 );
			}
			callback?.Invoke();
		}

		Vector3 GetMousePosition()
		{
			Plane plane = new Plane( Vector3.up, 0 );
			float distance;
			Ray ray = cam.ScreenPointToRay( Input.mousePosition );
			if ( plane.Raycast( ray, out distance ) )
			{
				return ray.GetPoint( distance );
			}
			else
				return Vector3.zero;
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
	}
}
