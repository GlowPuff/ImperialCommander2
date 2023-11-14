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

			//sort tiles by number
			tiles = tiles.OrderBy( x => int.Parse( x.Split( ' ' )[1] ) ).ToArray();
			var groupedTiles = tiles.GroupBy( x => x );

			foreach ( var item in groupedTiles )
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
				if ( item.Count() > 1 )
				{
					nt.text = $"{item.Key} x {item.Count()}";
				}
				else
				{
					nt.text = item.Key;
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
