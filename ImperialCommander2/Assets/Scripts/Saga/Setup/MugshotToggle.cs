using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Saga
{
	public class MugshotToggle : MonoBehaviour
	{
		bool _isOn;
		int dataMode;

		[HideInInspector]
		public int index;

		public Image mugImage, bgImage;

		[HideInInspector]
		public bool isOn
		{
			get { return _isOn; }
			set { _isOn = value; }
		}
		DeploymentCard card;

		/// <summary>
		/// 0=hero, 1=ally
		/// </summary>
		public void Init( int mode, DeploymentCard cd, int idx )
		{
			index = idx;
			dataMode = mode;
			card = cd;
			if ( mode == 0 )
				mugImage.sprite = Resources.Load<Sprite>( $"Cards/Heroes/{cd.id}" );
			else
				mugImage.sprite = Resources.Load<Sprite>( $"Cards/Allies/{cd.id.Replace( "A", "M" )}" );

			isOn = false;
		}

		public void UpdateToggle()
		{
			EventSystem.current.SetSelectedGameObject( null );
			if ( isOn )
				bgImage.color = Color.green;
			else
			{
				bgImage.color = Color.white;
				isOn = false;
			}
		}

		public void OnToggle()
		{
			EventSystem.current.SetSelectedGameObject( null );
			isOn = !isOn;
			if ( isOn && !FindObjectOfType<SagaAddHeroPanel>().OnToggle( card ) )
				isOn = false;
			if ( !isOn )
			{
				if ( dataMode == 0 )
					DataStore.sagaSessionData.MissionHeroes.Remove( card );
				else
					DataStore.sagaSessionData.selectedAlly = null;
			}

			if ( isOn && dataMode == 1 )
				FindObjectOfType<SagaAddHeroPanel>().AllyToggle( index );

			UpdateToggle();
		}
	}
}
