using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Saga
{
	public class SagaModifyGroupsPanel : MonoBehaviour
	{
		public PopupBase popupBase;
		public GameObject groupMugPrefab;
		public Toggle[] expansionToggles;
		public Transform mugContainer;
		public TextMeshProUGUI nameText;
		public DynamicCardPrefab cardPrefab;

		Action callback;
		int dataMode, selectedExpansion, prevExp;
		bool updating;
		List<DeploymentCard> disabledGroups = new List<DeploymentCard>();

		/// <summary>
		/// mode 0=Ignored, 1=villains
		/// </summary>
		public void Show( int mode, List<DeploymentCard> disabledG = null, Action cb = null )
		{
			disabledGroups = disabledG ?? new List<DeploymentCard>();
			callback = cb;
			dataMode = mode;
			EventSystem.current.SetSelectedGameObject( null );
			popupBase.Show();

			prevExp = -1;
			updating = false;
			ResetExpansionUI();

			OnChangeExpansion( 0 );
		}

		void ResetExpansionUI()
		{
			updating = true;
			for ( int i = 0; i < expansionToggles.Length; i++ )
			{
				expansionToggles[i].interactable = false;
				expansionToggles[i].transform.GetChild( 1 ).GetComponent<Image>().color = new Color( 1, 1, 1, .2f );
			}
			//only enable buttons for owned expansions
			for ( int i = 0; i < DataStore.ownedExpansions.Count; i++ )
			{
				expansionToggles[(int)DataStore.ownedExpansions[i]].interactable = true;
				expansionToggles[(int)DataStore.ownedExpansions[i]].transform.GetChild( 1 ).GetComponent<Image>().color = Color.white;
			}
			//core/other/imports always true
			expansionToggles[0].interactable = true;
			expansionToggles[0].isOn = true;
			expansionToggles[7].interactable = true;
			expansionToggles[7].transform.GetChild( 1 ).GetComponent<Image>().color = Color.white;
			expansionToggles[8].interactable = true;
			expansionToggles[8].transform.GetChild( 1 ).GetComponent<Image>().color = Color.white;
			updating = false;

			UpdateExpansionCounts();
		}

		//change expansion
		public void OnChangeExpansion( int idx )
		{
			EventSystem.current.SetSelectedGameObject( null );

			if ( prevExp == idx || updating )
				return;

			prevExp = idx;
			selectedExpansion = idx;
			nameText.text = "";

			foreach ( Transform item in mugContainer )
			{
				Destroy( item.gameObject );
			}

			if ( dataMode == 0 )//ignored
			{
				if ( idx == 8 )//imports tab
					UpdateIgnoredImported();
				else//everything else
					UpdateIgnored( idx );
			}
			else//villains
			{
				UpdateVillains( idx );
			}
		}

		void UpdateIgnored( int idx )
		{
			updating = true;//avoid tripping toggle callback

			string expansion = ((Expansion)idx).ToString();
			List<DeploymentCard> cards = DataStore.deploymentCards.Where( x => x.expansion == expansion ).ToList();

			//if showing the OTHER tab, show only owned Figure Packs
			if ( expansion == "Other" )
				cards = cards.Where( x => DataStore.ownedFigurePacks.ContainsCard( x ) ).ToList();

			for ( int i = 0; i < cards.Count; i++ )
			{
				var mug = Instantiate( groupMugPrefab, mugContainer );
				mug.GetComponent<GroupMugshotToggle>().Init( cards[i], dataMode );

				//if the card is in the mission ignored list toggle it ON (ignored)
				if ( DataStore.sagaSessionData.MissionIgnored.Contains( cards[i] ) )
				{
					mug.GetComponent<GroupMugshotToggle>().isOn = true;
					mug.GetComponent<GroupMugshotToggle>().UpdateToggle();
				}
				//disable the toggle if it's on the mission/preset ignore list
				if ( disabledGroups.ContainsCard( cards[i] ) )
					mug.GetComponent<GroupMugshotToggle>().DisableMug();
			}

			updating = false;
			UpdateExpansionCounts();
			if ( cards.Count > 0 )
				cardPrefab.InitCard( cards[0] );
		}

		void UpdateIgnoredImported()
		{
			updating = true;//avoid tripping toggle callback

			//add imported Imperial cards
			List<DeploymentCard> cards = DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Imperial ).Select( x => x.deploymentCard ).ToList();

			for ( int i = 0; i < cards.Count; i++ )
			{
				var mug = Instantiate( groupMugPrefab, mugContainer );
				mug.GetComponent<GroupMugshotToggle>().Init( cards[i], dataMode );

				//set the ignore toggle (ON) based on priority (low first)
				bool ignore = false;
				//if it's excluded from Expansions, DO ignore it
				ignore = DataStore.IgnoredPrefsImports.Contains( cards[i].customCharacterGUID.ToString() );
				//if it's excluded from Expansions but INCLUDED in the session imports, do NOT ignore it
				if ( ignore && DataStore.sagaSessionData.globalImportedCharacters.ContainsCard( cards[i] ) )
					ignore = false;
				//if it's NOT excluded from Expansions but INCLUDED in the session imports, do NOT ignore
				else if ( !ignore && DataStore.sagaSessionData.globalImportedCharacters.ContainsCard( cards[i] ) )
					ignore = false;
				//if it's excluded from Expansions AND NOT in the session imports, DO ignore it
				else if ( ignore && !DataStore.sagaSessionData.globalImportedCharacters.ContainsCard( cards[i] ) )
					ignore = true;
				//if it's NOT excluded from Expansions AND NOT in the session imports, DO ignore it
				else if ( !ignore && !DataStore.sagaSessionData.globalImportedCharacters.ContainsCard( cards[i] ) )
					ignore = true;

				if ( ignore )
				{
					mug.GetComponent<GroupMugshotToggle>().isOn = true;
					mug.GetComponent<GroupMugshotToggle>().UpdateToggle();
				}
				//disable the toggle if it's on the mission/preset ignore list
				if ( disabledGroups.ContainsCard( cards[i] ) )
					mug.GetComponent<GroupMugshotToggle>().DisableMug();
			}

			updating = false;
			UpdateExpansionCounts();
			if ( cards.Count > 0 )
				cardPrefab.InitCard( cards[0] );
		}

		void UpdateVillains( int idx )
		{
			updating = true;//avoid tripping toggle callback

			string expansion = ((Expansion)idx).ToString();//tab 8 (imported) will == "8"
			List<DeploymentCard> cards = DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Villain && x.deploymentCard.expansion == expansion ).Select( x => x.deploymentCard ).ToList();

			//add embedded characters
			var setup = FindObjectOfType<SagaSetup>();
			cards = cards.Concat( setup.missionCustomVillains ).Where( x => x.expansion == expansion ).ToList();

			//finally, add stock villains
			cards = cards.Concat( DataStore.villainCards.Where( x => x.expansion == expansion ) ).ToList();

			for ( int i = 0; i < cards.Count; i++ )
			{
				var mug = Instantiate( groupMugPrefab, mugContainer );
				mug.GetComponent<GroupMugshotToggle>().Init( cards[i], dataMode );
				if ( DataStore.sagaSessionData.EarnedVillains.Contains( cards[i] ) )
				{
					mug.GetComponent<GroupMugshotToggle>().isOn = true;
					mug.GetComponent<GroupMugshotToggle>().UpdateToggle();
				}
			}

			updating = false;
			if ( cards.Count > 0 )
				cardPrefab.InitCard( cards[0] );
		}

		/// <summary>
		/// Fires when the toggle is ON
		/// </summary>
		public bool OnToggle( DeploymentCard card )
		{
			nameText.text = $"{card.name}";// [{card.id}]";

			if ( dataMode == 0 )
			{
				var cardlist = card.IsImported ? DataStore.sagaSessionData.globalImportedCharacters : DataStore.sagaSessionData.MissionIgnored;

				cardPrefab.InitCard( card );

				if ( card.IsImported )
				{
					if ( cardlist.Contains( card ) )
					{
						//globalImportedCharacters is an INCLUSIVE list, so REMOVE it when toggled ON
						Debug.Log( $"{card.name} REMOVED FROM SESSION IMPORTS" );
						cardlist.Remove( card );
						return true;
					}
				}
				else
				{
					if ( !cardlist.Contains( card ) )
					{
						cardlist.Add( card );
						return true;
					}
				}
			}
			else
			{
				if ( !DataStore.sagaSessionData.EarnedVillains.Contains( card ) )
				{
					DataStore.sagaSessionData.EarnedVillains.Add( card );
					cardPrefab.InitCard( card );
					return true;
				}
				else
					return false;
			}

			return false;
		}

		public void UpdateExpansionCounts()
		{
			for ( int i = 0; i < expansionToggles.Length; i++ )
			{
				if ( dataMode == 0 && !updating )//ignored mode
				{
					int count = 0;

					//even though all 8 Other groups are ignored by default, only show a number up to the number owned to avoid confusion (ie: owning none of them would still show 8 ignored)
					if ( i == 7 )//Other
					{
						count = DataStore.sagaSessionData.MissionIgnored.Where( x => x.expansion == ((Expansion)i).ToString() ).Count();
						count = Math.Min( count, DataStore.ownedFigurePacks.Count - (8 - count) );
					}
					else if ( i == 8 )//imported
						count = DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Imperial ).Count() - DataStore.sagaSessionData.globalImportedCharacters.Count;
					else//everything else
						count = DataStore.sagaSessionData.MissionIgnored.Where( x => x.expansion == ((Expansion)i).ToString() ).Count();

					expansionToggles[i].transform.GetChild( 2 ).GetChild( 0 ).GetComponent<TextMeshProUGUI>().text = count.ToString();
				}
				else//villain mode
				{
					var list = DataStore.sagaSessionData.EarnedVillains.Where( x => x.expansion == ((Expansion)i).ToString() ).ToList();
					expansionToggles[i].transform.GetChild( 2 ).GetChild( 0 ).GetComponent<TextMeshProUGUI>().text = list.Count.ToString();
				}
			}
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
