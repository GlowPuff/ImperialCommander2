using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Saga
{
	public class MugshotToggle : MonoBehaviour
	{
		bool _isOn;
		CharacterType characterType;

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
		public void Init( DeploymentCard cd, int idx )
		{
			index = idx;
			characterType = cd.characterType;
			card = cd;
			mugImage.sprite = Resources.Load<Sprite>( cd.mugShotPath );

			isOn = false;

			UpdateToggle();
		}

		public void UpdateToggle()
		{
			EventSystem.current.SetSelectedGameObject( null );
			if ( isOn )//button is selected
			{
				bgImage.color = Color.green;
			}
			else
			{
				isOn = false;

				if ( card.isElite )
					bgImage.color = Color.red;
				else
					bgImage.color = Color.white;

				//handle banned allies
				if ( characterType == CharacterType.Ally )
				{
					bgImage.color = Color.white;

					if ( DataStore.sagaSessionData.BannedAllies.Contains( card.id ) )
					{
						bgImage.color = Color.red;
					}
				}
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
				if ( characterType == CharacterType.Hero )
					DataStore.sagaSessionData.MissionHeroes.Remove( card );
				else
					DataStore.sagaSessionData.selectedAlly = null;
			}

			if ( isOn && characterType == CharacterType.Ally )
				FindObjectOfType<SagaAddHeroPanel>().AllyToggle( index );

			UpdateToggle();
		}
	}
}
