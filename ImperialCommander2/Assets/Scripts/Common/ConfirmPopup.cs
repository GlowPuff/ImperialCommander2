using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Saga
{

	public class ConfirmPopup : MonoBehaviour
	{
		public CanvasGroup cg;
		public Button woundButton;

		Action defeatCallback, woundCallback, exhaustCallback;
		float sx, os;
		SagaHGPrefab hgFab;
		SagaDGPrefab dgFab;

		/// <summary>
		/// only enemy groups call this
		/// </summary>
		public void ShowLeft( Transform tf, SagaDGPrefab pfab, Action dCB = null, Action eCB = null )
		{
			dgFab = pfab;
			dgFab.isConfirming = true;
			hgFab = null;
			defeatCallback = dCB;
			exhaustCallback = eCB;
			float offset = -30;
			os = offset * Screen.width / 1920f;

			cg.alpha = 0;
			float scalar = 175 * Screen.width / 1920f;
			sx = Screen.width - scalar;
			//sx = tf.position.x - 8;
			transform.position = new Vector3( sx, tf.position.y, 0 );//transform.position.z
			gameObject.SetActive( true );
			transform.DOMoveX( sx + os, .25f );
			cg.DOFade( 1, .2f );
		}

		/// <summary>
		/// only Heroes call this
		/// </summary>
		public void ShowRight( Transform tf, SagaHGPrefab pfab, string hname, bool isHero, bool isWounded, Action dCB = null, Action wCB = null )
		{
			dgFab = null;
			hgFab = pfab;
			hgFab.isConfirming = true;
			defeatCallback = dCB;
			woundCallback = wCB;
			exhaustCallback = null;
			float offset = 30;
			os = offset * Screen.width / 1920f;

			cg.alpha = 0;
			float scalar = 180 * Screen.width / 1920f;
			sx = scalar;
			transform.position = new Vector3( sx, tf.position.y, 0 );
			gameObject.SetActive( true );
			transform.DOMoveX( sx + os, .25f );
			cg.DOFade( 1, .2f );

			//woundButton.gameObject.SetActive( isHero );
			//woundButton.interactable = !isWounded;
		}

		public void Hide( Action cb = null )
		{
			cg.DOFade( 0, .2f );
			transform.DOMoveX( sx, .25f ).OnComplete( () =>
			{
				if ( hgFab != null )
					hgFab.isConfirming = false;
				if ( dgFab != null )
					dgFab.isConfirming = false;
				gameObject.SetActive( false );
				cb?.Invoke();
			} );
		}

		public void OnWound()
		{
			woundCallback?.Invoke();
		}

		public void OnDefeat()
		{
			defeatCallback?.Invoke();
		}

		public void OnExhaust()
		{
			exhaustCallback?.Invoke();
		}
	}
}