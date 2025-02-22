using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Saga
{
	public class TileInfoPopup : MonoBehaviour
	{
		public PopupBase popupBase;
		public Transform container;

		public void Show( string[] tiles )
		{
			var tileExpansionTranslatedNames = new Dictionary<string, string>()
				{
					{ Expansion.Core.ToString(), $"<color=#7FD3FF>{DataStore.uiLanguage.sagaMainApp.mmCoreTileNameUC}</color>" },
					{ Expansion.Twin.ToString(), $"<color=#7FD3FF>{DataStore.uiLanguage.sagaMainApp.mmTwinTileNameUC}</color>" },
					{ Expansion.Hoth.ToString(), $"<color=#7FD3FF>{DataStore.uiLanguage.sagaMainApp.mmHothTileNameUC}</color>" },
					{ Expansion.Bespin.ToString(), $"<color=#7FD3FF>{DataStore.uiLanguage.sagaMainApp.mmBespinTileNameUC}</color>" },
					{ Expansion.Jabba.ToString(), $"<color=#7FD3FF>{DataStore.uiLanguage.sagaMainApp.mmJabbaTileNameUC}</color>" },
					{ Expansion.Empire.ToString(), $"<color=#7FD3FF>{DataStore.uiLanguage.sagaMainApp.mmEmpireTileNameUC}</color>" },
					{ Expansion.Lothal.ToString(), $"<color=#7FD3FF>{DataStore.uiLanguage.sagaMainApp.mmLothalTileNameUC}</color>" },
				};

			EventSystem.current.SetSelectedGameObject( null );
			popupBase.Show();

			foreach ( Transform item in container )
			{
				Destroy( item.gameObject );
			}

			//sort and group tiles by number, i.e. "Core 2A", "Core 11A", "Empire 2B"
			var orderedAndGrouped = tiles
					.OrderBy( str => str.Split( ' ' )[0] )  // Order alphabetically
					.ThenBy( str => int.Parse( str.Split( ' ' )[1].TrimEnd( 'A', 'B' ) ) ) // Then order by entire numerical values
					.ThenBy( str => str.EndsWith( "A" ) ? 0 : 1 ) // Finally, order by A/B values
					.GroupBy( str => str ) // Group the strings
					.Select( group => new
					{
						Tile = group.Key,
						Count = group.Count()
					} );

			foreach ( var item in orderedAndGrouped )
			{
				GameObject go = new GameObject( "content item" );
				go.layer = 5;
				go.transform.SetParent( container );
				go.transform.localPosition = Vector2.zero;
				go.transform.localScale = Vector3.one;
				go.transform.localEulerAngles = Vector3.zero;

				TextMeshProUGUI nt = go.AddComponent<TextMeshProUGUI>();
				nt.color = Color.white;
				nt.fontSize = 40;
				nt.alignment = TextAlignmentOptions.Center;
				nt.horizontalAlignment = HorizontalAlignmentOptions.Left;

				// Replacing tile expansion name with translated name
				string expansionName = item.Tile.Split( ' ' )[0];
				string itemTileTranslated = tileExpansionTranslatedNames.ContainsKey( expansionName ) ? item.Tile.Replace( expansionName, tileExpansionTranslatedNames[expansionName] ) : item.Tile;

				// Display a count when more than one tile is needed
				if ( item.Count > 1 )
				{
					itemTileTranslated = $"{Utils.ReplaceGlyphs( itemTileTranslated )} <color=orange>x {item.Count}</color>";
				}
				else
				{
					itemTileTranslated = Utils.ReplaceGlyphs( itemTileTranslated );
				}

				if (Utils.tileShapes.ContainsKey(item.Tile))
					itemTileTranslated = $"{itemTileTranslated} <font=\"TilesIcons SDF\">{Utils.tileShapes[item.Tile]}</font>";

				nt.text = itemTileTranslated;
			}
		}

		public void OnClose()
		{
			popupBase.Close();
		}

		private void Update()
		{
			if ( Input.GetKeyDown( KeyCode.Space ) )
				OnClose();
		}
	}
}
