using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Saga
{

	public class SagaAddHeroPanel : MonoBehaviour
	{
		public PopupBase popupBase;
		public GameObject heroMugPrefab;
		public Transform mugContainer;

		Action callback;
		int dataMode, prevSelected = -1;

		/// <summary>
		/// mode 0=heroes, 1=allies
		/// </summary>
		public void Show( int mode, Action cb = null )
		{
			callback = cb;
			dataMode = mode;
			EventSystem.current.SetSelectedGameObject( null );
			popupBase.Show();

			//populate mugshot toggles
			if ( dataMode == 0 )
			{
				int i = 0;
				foreach ( var item in DataStore.heroCards )
				{
					var mug = Instantiate( heroMugPrefab, mugContainer );
					mug.GetComponent<MugshotToggle>().Init( dataMode, item, i++ );
					if ( DataStore.sagaSessionData.MissionHeroes.Contains( item ) )
					{
						mug.GetComponent<MugshotToggle>().isOn = true;
						mug.GetComponent<MugshotToggle>().UpdateToggle();
					}
				}
			}
			else//allies
			{
				int i = 0;
				foreach ( var item in DataStore.allyCards.MinusElite() )
				{
					var mug = Instantiate( heroMugPrefab, mugContainer );
					mug.GetComponent<MugshotToggle>().Init( dataMode, item, i++ );
					if ( DataStore.sagaSessionData.selectedAlly == item )
					{
						mug.GetComponent<MugshotToggle>().isOn = true;
						mug.GetComponent<MugshotToggle>().UpdateToggle();
					}
				}
			}
		}

		public bool OnToggle( DeploymentCard card )
		{
			FindObjectOfType<Sound>().PlaySound( FX.Click );
			if ( dataMode == 0 )
			{
				if ( DataStore.sagaSessionData.MissionHeroes.Count < 4 && !DataStore.sagaSessionData.MissionHeroes.Contains( card ) )
				{
					DataStore.sagaSessionData.MissionHeroes.Add( card );
					return true;
				}
			}
			else
			{
				if ( DataStore.sagaSessionData.selectedAlly != card )
				{
					DataStore.sagaSessionData.selectedAlly = card;
					return true;
				}
			}

			return false;
		}

		public void AllyToggle( int idx )
		{
			if ( prevSelected != -1 && prevSelected != idx )
			{
				mugContainer.GetChild( prevSelected ).GetComponent<MugshotToggle>().isOn = false;
				mugContainer.GetChild( prevSelected ).GetComponent<MugshotToggle>().UpdateToggle();
			}

			prevSelected = idx;
		}

		public void OnClose()
		{
			FindObjectOfType<Sound>().PlaySound( FX.Click );
			callback?.Invoke();
			popupBase.Close( () =>
			{
				foreach ( Transform item in mugContainer )
				{
					Destroy( item.gameObject );
				}
			} );
		}

		private void Update()
		{
			if ( Input.GetKeyDown( KeyCode.Space ) )
				OnClose();
		}
	}
}