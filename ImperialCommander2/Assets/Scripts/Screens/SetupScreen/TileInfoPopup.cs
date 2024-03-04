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
			EventSystem.current.SetSelectedGameObject( null );
			popupBase.Show();

			foreach ( Transform item in container )
			{
				Destroy( item.gameObject );
			}

			//sort and group tiles by number, i.e. "Core 2A", "Core 11A", "Empire 2B"
			var orderedAndGrouped = tiles
					.OrderBy(str => str.Split(' ')[0])  // Order alphabetically
					.ThenBy(str => int.Parse(str.Split(' ')[1].TrimEnd('A', 'B'))) // Then order by entire numerical values
					.ThenBy(str => str.EndsWith("A") ? 0 : 1) // Finally, order by A/B values
					.GroupBy(str => str) // Group the strings
					.Select(group => new
					{
						Tile = group.Key,
						Count = group.Count()
					});

			foreach ( var item in orderedAndGrouped)
			{
				GameObject go = new GameObject( "content item" );
				go.layer = 5;
				go.transform.SetParent( container );
				go.transform.localPosition = Vector2.zero;
				go.transform.localScale = Vector3.one;
				go.transform.localEulerAngles = Vector3.zero;

				TextMeshProUGUI nt = go.AddComponent<TextMeshProUGUI>();
				nt.color = Color.white;
				nt.fontSize = 25;
				nt.alignment = TextAlignmentOptions.Center;
				nt.horizontalAlignment = HorizontalAlignmentOptions.Left;

				// Display a count when more than one tile is needed
				if (item.Count > 1 )
				{
					nt.text = $"{item.Tile} x {item.Count}";
				}
				else
				{
					nt.text = item.Tile;
				}
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
