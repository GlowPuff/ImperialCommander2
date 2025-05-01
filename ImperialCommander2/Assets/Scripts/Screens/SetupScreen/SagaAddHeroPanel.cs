using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Saga
{

	public class SagaAddHeroPanel : MonoBehaviour
	{
		public PopupBase popupBase;
		public GameObject heroMugPrefab;
		public Transform mugContainer;
		public TextMeshProUGUI nameText;
		public DynamicCardPrefab cardPrefab;
		public Toggle customToggle;
		public HelpPanel helpPanel;

		Action callback;
		int prevSelected = -1;
		CharacterType characterType;
		bool ignoreToggle;

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

			ignoreToggle = true;
			customToggle.isOn = false;
			ignoreToggle = false;
			ShowContent( false );
		}

		private void ShowContent( bool showCustom )
		{
			try
			{
				//populate mugshot toggles
				if ( characterType == CharacterType.Hero )
				{
					int i = 0;
					List<DeploymentCard> heroCards = new List<DeploymentCard>();

					if ( showCustom )
					{
						//add global imported characters - with null check
						if ( DataStore.globalImportedCharacters != null && DataStore.globalImportedCharacters.Any() )
						{
							var importedHeroes = DataStore.globalImportedCharacters
									.Where( x => x != null && x.deploymentCard != null && x.deploymentCard.characterType == CharacterType.Hero )
									.Select( x => x.deploymentCard )
									.ToList();

							if ( importedHeroes.Any() )
								heroCards.AddRange( importedHeroes );
						}

						//add embedded characters
						var setup = FindObjectOfType<SagaSetup>();
						if ( setup != null && setup.missionCustomHeroes != null && setup.missionCustomHeroes.Any() )
							heroCards.AddRange( setup.missionCustomHeroes );
					}

					//finally, add stock heroes
					if ( !showCustom && DataStore.heroCards != null && DataStore.heroCards.Any() )
						heroCards.AddRange( DataStore.heroCards );

					// Instantiate toggles for each hero
					foreach ( var item in heroCards )
					{
						if ( item == null )
							continue; // Skip null items

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
				else // allies
				{
					List<DeploymentCard> allyCards = new List<DeploymentCard>();

					if ( showCustom )
					{
						//add global imported characters
						if ( DataStore.globalImportedCharacters != null && DataStore.globalImportedCharacters.Any() )
						{
							var importedAllies = DataStore.globalImportedCharacters
									.Where( x => x != null && x.deploymentCard != null && x.deploymentCard.characterType == CharacterType.Ally )
									.Select( x => x.deploymentCard )
									.ToList();

							if ( importedAllies.Any() )
								allyCards.AddRange( importedAllies );
						}

						//add embedded characters
						var setup = FindObjectOfType<SagaSetup>();
						if ( setup != null && setup.missionCustomAllies != null && setup.missionCustomAllies.Any() )
							allyCards.AddRange( setup.missionCustomAllies );
					}

					//finally, add stock allies
					if ( !showCustom && DataStore.allyCards != null )
					{
						var nonEliteAllies = DataStore.allyCards.MinusElite();
						if ( nonEliteAllies != null && nonEliteAllies.Any() )
							allyCards.AddRange( nonEliteAllies );
					}

					//get the fixed ally in the selected mission, if any
					string fixedAlly = null;
					var setupObj = FindObjectOfType<SagaSetup>();
					if ( setupObj != null )
						fixedAlly = setupObj.fixedAlly;

					//remove fixed ally from the list
					if ( !string.IsNullOrEmpty( fixedAlly ) && allyCards.Count > 0 )
					{
						Debug.Log( $"Ally Panel::Omitting {allyCards.FirstOrDefault( x => x.id == fixedAlly )?.name} [{fixedAlly}] from the pool" );
						// Use Where instead of direct removal
						allyCards = allyCards.Where( x => x != null && x.id != fixedAlly ).ToList();
					}

					int i = 0;
					foreach ( var item in allyCards )
					{
						if ( item == null )
							continue; // Skip null items

						var mug = Instantiate( heroMugPrefab, mugContainer );
						mug.GetComponent<MugshotToggle>().Init( item, i++ );
						if ( DataStore.sagaSessionData.selectedAlly == item )
						{
							mug.GetComponent<MugshotToggle>().isOn = true;
							mug.GetComponent<MugshotToggle>().UpdateToggle();
						}
					}

					//show elite version of allies
					if ( !showCustom && DataStore.allyCards != null )
					{
						var eliteAllies = DataStore.allyCards.OnlyElite();
						if ( eliteAllies != null )
						{
							foreach ( var item in eliteAllies )
							{
								if ( item == null )
									continue; // Skip null items

								var mug = Instantiate( heroMugPrefab, mugContainer );
								mug.GetComponent<MugshotToggle>().Init( item, i++ );
								if ( DataStore.sagaSessionData.selectedAlly == item )
								{
									mug.GetComponent<MugshotToggle>().isOn = true;
									mug.GetComponent<MugshotToggle>().UpdateToggle();
								}
							}
						}
					}

					if ( allyCards.Count > 0 )
						cardPrefab.InitCard( allyCards[0], true );
				}
			}
			catch ( Exception ex )
			{
				Utils.LogError( $"Error in ShowContent: {ex.Message}\n{ex.StackTrace}" );
			}
		}

		/*private void ShowContent( bool showCustom )
		{
			//populate mugshot toggles
			if ( characterType == CharacterType.Hero )
			{
				int i = 0;
				List<DeploymentCard> heroCards = new List<DeploymentCard>();
				if ( showCustom )
				{
					//add global imported characters
					heroCards = DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Hero ).Select( x => x.deploymentCard ).ToList();

					//add embedded characters
					var setup = FindObjectOfType<SagaSetup>();
					heroCards = heroCards.Concat( setup.missionCustomHeroes ).ToList();
				}

				//finally, add stock heroes
				if ( !showCustom )
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
				List<DeploymentCard> allyCards = new List<DeploymentCard>();
				if ( showCustom )
				{
					//add global imported characters
					allyCards = DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Ally ).Select( x => x.deploymentCard ).ToList();

					//add embedded characters
					var setup = FindObjectOfType<SagaSetup>();
					allyCards = allyCards.Concat( setup.missionCustomAllies ).ToList();
				}

				//finally, add stock allies
				if ( !showCustom )
					allyCards = allyCards.Concat( DataStore.allyCards.MinusElite() ).ToList();

				//get the fixed ally in the selected mission, if any, so it can be omitted from the pool
				string fixedAlly = FindObjectOfType<SagaSetup>().fixedAlly;
				if ( !string.IsNullOrEmpty( fixedAlly ) && allyCards.Count > 0 )
				{
					Debug.Log( $"Ally Panel::Omitting {allyCards.FirstOrDefault( x => x.id == fixedAlly )?.name} [{fixedAlly}] from the pool" );
					//remove it from the list so it can't be chosen
					allyCards = allyCards.Where( x => x.id != fixedAlly ).ToList();
				}

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
				//show elite version of allies
				if ( !showCustom )
				{
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
				}

				if ( allyCards.Count > 0 )
					cardPrefab.InitCard( allyCards[0], true );
			}
		}*/

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

		public void OnCustomToggle( Toggle toggle )
		{
			if ( ignoreToggle )
				return;

			foreach ( Transform item in mugContainer )
			{
				Destroy( item.gameObject );
			}
			prevSelected = -1;
			ShowContent( toggle.isOn );
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

		public void OnHelpClick()
		{
			helpPanel.Show();
		}

		private void Update()
		{
			if ( Input.GetKeyDown( KeyCode.Space ) )
				OnClose();
		}
	}
}