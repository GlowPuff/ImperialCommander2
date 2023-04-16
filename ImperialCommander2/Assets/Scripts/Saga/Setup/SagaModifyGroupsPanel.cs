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

		Action callback;
		int dataMode, selectedExpansion, prevExp;
		bool updating;

		/// <summary>
		/// mode 0=Ignored, 1=villains
		/// </summary>
		public void Show( int mode, Action cb = null )
		{
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
			//core/other always true
			expansionToggles[0].interactable = true;
			expansionToggles[0].isOn = true;
			expansionToggles[7].interactable = true;
			expansionToggles[7].transform.GetChild( 1 ).GetComponent<Image>().color = Color.white;
			updating = false;

			UpdateExpansionCounts();
		}

		//change expansion
		public void OnChangeExpansion( int idx )
		{
			EventSystem.current.SetSelectedGameObject( null );
			//FindObjectOfType<Sound>().PlaySound( FX.Click );

			if ( prevExp == idx || updating )
				return;

			prevExp = idx;
			selectedExpansion = idx;
			nameText.text = "";

			string expansion = ((Expansion)idx).ToString();
			foreach ( Transform item in mugContainer )
			{
				Destroy( item.gameObject );
			}

			List<DeploymentCard> cards = new List<DeploymentCard>();
			//get the cards in selected expansion
			if ( dataMode == 0 )//ignored
			{
				updating = true;//avoid tripping toggle callback
				cards = DataStore.deploymentCards.Where( x => x.expansion == expansion ).ToList();

				//if showing OTHER, show only owned Figure Packs
				if ( expansion == "Other" )
					cards = cards.Where( x => DataStore.ownedFigurePacks.ContainsCard( x ) ).ToList();

				for ( int i = 0; i < cards.Count; i++ )
				{
					var mug = Instantiate( groupMugPrefab, mugContainer );
					mug.GetComponent<GroupMugshotToggle>().Init( cards[i], dataMode );
					if ( DataStore.sagaSessionData.MissionIgnored.Contains( cards[i] ) )
					{
						mug.GetComponent<GroupMugshotToggle>().isOn = true;
						mug.GetComponent<GroupMugshotToggle>().UpdateToggle();
					}
				}
				updating = false;
				UpdateExpansionCounts();
			}
			else//villains
			{
				updating = true;
				cards = DataStore.villainCards.Where( x => x.expansion == expansion ).ToList();
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
			}
		}

		public bool OnToggle( DeploymentCard card )
		{
			nameText.text = $"{card.name} [{card.id}]";

			if ( dataMode == 0 )
			{
				if ( !DataStore.sagaSessionData.MissionIgnored.Contains( card ) )
				{
					DataStore.sagaSessionData.MissionIgnored.Add( card );
					return true;
				}
			}
			else
			{
				if ( !DataStore.sagaSessionData.EarnedVillains.Contains( card ) )
				{
					DataStore.sagaSessionData.EarnedVillains.Add( card );
					return true;
				}
			}

			return false;
		}

		public void UpdateExpansionCounts()
		{
			for ( int i = 0; i < expansionToggles.Length; i++ )
			{
				if ( dataMode == 0 && !updating )
				{
					int count = DataStore.sagaSessionData.MissionIgnored.Where( x => x.expansion == ((Expansion)i).ToString() ).Count();

					//even though all 8 Other groups are ignored by default, only show a number up to the number owned to avoid confusion (ie: owning none of them would still show 8 ignored)
					if ( i == 7 )//Other
						count = Math.Min( count, DataStore.ownedFigurePacks.Count - (8 - count) );

					expansionToggles[i].transform.GetChild( 2 ).GetChild( 0 ).GetComponent<TextMeshProUGUI>().text = count.ToString();
				}
				else
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
