using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Saga
{

	public class SagaAddHeroPanel : MonoBehaviour
	{
		public PopupBase popupBase;
		public GameObject heroMugPrefab;
		public Transform mugContainer;
		public TextMeshProUGUI nameText;
		public DynamicCardPrefab cardPrefab;

		Action callback;
		int prevSelected = -1;
		CharacterType characterType;

		/// <summary>
		/// mode 0=heroes, 1=allies
		/// </summary>
		public void Show( CharacterType cType, Action cb = null )
		{
			callback = cb;
			EventSystem.current.SetSelectedGameObject( null );
			popupBase.Show();

			nameText.text = "";
			characterType = cType;

			//populate mugshot toggles
			if ( characterType == CharacterType.Hero )
			{
				int i = 0;
				//add global imported characters
				List<DeploymentCard> heroCards = DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Hero ).Select( x => x.deploymentCard ).ToList();

				//add embedded characters
				var setup = FindObjectOfType<SagaSetup>();
				heroCards = heroCards.Concat( setup.missionCustomHeroes ).ToList();

				//finally, add stock heroes
				heroCards = heroCards.Concat( DataStore.heroCards ).ToList();

				foreach ( var item in heroCards )
				{
					var mug = Instantiate( heroMugPrefab, mugContainer );
					mug.GetComponent<MugshotToggle>().Init( item, i++ );
					if ( DataStore.sagaSessionData.MissionHeroes.Contains( item ) )
					{
						mug.GetComponent<MugshotToggle>().isOn = true;
						mug.GetComponent<MugshotToggle>().UpdateToggle();
					}
				}

				if ( heroCards.Count > 0 )
					cardPrefab.InitCard( heroCards[0], true );
			}
			else//allies
			{
				//add global imported characters
				List<DeploymentCard> allyCards = DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Ally ).Select( x => x.deploymentCard ).ToList();

				//add embedded characters
				var setup = FindObjectOfType<SagaSetup>();
				allyCards = allyCards.Concat( setup.missionCustomAllies ).ToList();

				//finally, add stock allies
				allyCards = allyCards.Concat( DataStore.allyCards.MinusElite() ).ToList();

				int i = 0;
				foreach ( var item in allyCards )
				{
					var mug = Instantiate( heroMugPrefab, mugContainer );
					mug.GetComponent<MugshotToggle>().Init( item, i++ );
					if ( DataStore.sagaSessionData.selectedAlly == item )
					{
						mug.GetComponent<MugshotToggle>().isOn = true;
						mug.GetComponent<MugshotToggle>().UpdateToggle();
					}
				}
				foreach ( var item in DataStore.allyCards.OnlyElite() )
				{
					var mug = Instantiate( heroMugPrefab, mugContainer );
					mug.GetComponent<MugshotToggle>().Init( item, i++ );
					if ( DataStore.sagaSessionData.selectedAlly == item )
					{
						mug.GetComponent<MugshotToggle>().isOn = true;
						mug.GetComponent<MugshotToggle>().UpdateToggle();
					}
				}

				if ( allyCards.Count > 0 )
					cardPrefab.InitCard( allyCards[0], true );
			}
		}

		public bool OnToggle( DeploymentCard card )
		{
			FindObjectOfType<Sound>().PlaySound( FX.Click );

			nameText.text = $"{card.name}";// [{card.id}]";

			if ( characterType == CharacterType.Hero )
			{
				if ( DataStore.sagaSessionData.MissionHeroes.Count < 4 && !DataStore.sagaSessionData.MissionHeroes.Contains( card ) )
				{
					DataStore.sagaSessionData.MissionHeroes.Add( card );
					cardPrefab.InitCard( card, true );
					return true;
				}
			}
			else//ally
			{
				if ( DataStore.sagaSessionData.selectedAlly != card )
				{
					DataStore.sagaSessionData.selectedAlly = card;
					cardPrefab.InitCard( card, true );
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