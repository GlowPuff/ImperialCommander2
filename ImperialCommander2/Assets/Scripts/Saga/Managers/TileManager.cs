using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Saga
{
	public class TileManager : MonoBehaviour
	{
		public GameObject tilePrefab;
		List<MapSection> mapSections;
		List<TileDescriptor> tileDescriptors;

		public bool tilesLoaded
		{
			get
			{
				bool loaded = true;
				foreach ( var tr in tileRenderers )
				{
					if ( !tr.isLoaded )
						loaded = false;
				}
				return loaded;
			}
		}

		public List<TileRenderer> tileRenderers
		{
			get
			{
				var tiles = new List<TileRenderer>();
				foreach ( Transform child in transform )
					tiles.Add( child.GetComponent<TileRenderer>() );
				return tiles;
			}
		}

		/// <summary>
		/// Create the tiles in the whole mission, doesn't show them
		/// </summary>
		public void InstantiateTiles( List<MapSection> sections )
		{
			mapSections = sections;
			tileDescriptors = TileDescriptor.LoadData();

			foreach ( var s in mapSections )
			{
				foreach ( var mt in s.mapTiles )
				{
					var t = Instantiate( tilePrefab, transform );
					TileRenderer tileRenderer = t.GetComponent<TileRenderer>();
					t.GetComponent<TileRenderer>().LoadTile( mt, tileDescriptors.Where( x => x.expansion == mt.expansion.ToString() && x.id.ToString() == mt.tileID ).First() );
					mt.tileRenderer = tileRenderer;
				}
			}
		}

		/// <summary>
		/// example: Core1B
		/// </summary>
		public void CamToTile( Guid tileID, bool immediate = false, Action callback = null )
		{
			foreach ( var tr in tileRenderers )
			{
				if ( tr.mapTile.GUID == tileID )//tr.mapTile.textureName == tileID )
				{
					if ( immediate )
						FindObjectOfType<CameraController>().MoveToImmediate( tr.transform.position, 5, true, callback );
					else
					{
						if ( tr.isLoaded )
							FindObjectOfType<CameraController>().MoveTo( tr.transform.position, 2, 5, true, callback );
					}
				}
			}
		}

		public void CamToSection( Guid guid )
		{
			int idx = mapSections.IndexOf( mapSections.Where( x => x.GUID == guid ).First() );
			CamToSection( idx );
		}

		public void CamToSection( int index, bool immediate = false, Action callback = null )
		{
			Vector3 sum = Vector3.zero;
			var s = mapSections[index];
			var tiles = tileRenderers.Where( x => x.mapTile.mapSectionOwner == s.GUID ).ToList();
			if ( tiles.Count > 0 )
			{
				for ( int i = 0; i < tiles.Count(); i++ )
					sum += tiles[i].transform.position;
				//average the positions
				sum /= tiles.Count;
				if ( immediate )
					FindObjectOfType<CameraController>().MoveToImmediate( sum, 5, true, callback );
				else
					FindObjectOfType<CameraController>().MoveTo( sum, 2, 5, true, callback );
			}
			else
				callback?.Invoke();
		}

		/// <summary>
		/// Marks section as active, shows section, tiles and entities within (does NOT toggle IsActive for entities)
		/// </summary>
		public void ActivateMapSection( int index )
		{
			mapSections[index].isActive = true;
			foreach ( var tr in tileRenderers )
			{
				if ( tr.mapTile.mapSectionOwner == mapSections[index].GUID && tr.mapTile.entityProperties.isActive )
					tr.ShowTile();
			}

			//show ACTIVE entities in this section
			GlowTimer.SetTimer( 1f, () => FindObjectOfType<MapEntityManager>().ToggleSectionEntitiesVisibility( mapSections[index].GUID, true ) );
		}

		/// <summary>
		/// Hides a section and all entities within (does NOT toggle IsActive)
		/// </summary>
		public void DeactivateMapSection( int index )
		{
			foreach ( var tr in tileRenderers )
			{
				if ( tr.mapTile.mapSectionOwner == mapSections[index].GUID )
					tr.HideTile();
			}
			//hide entities in this section
			FindObjectOfType<MapEntityManager>().ToggleSectionEntitiesVisibility( mapSections[index].GUID, false );
			mapSections[index].isActive = false;
		}

		/// <summary>
		/// Marks session as active, shows section, tiles and entities within (does NOT toggle IsActive for entities),returns all active tiles and entities
		/// </summary>
		public Tuple<List<string>, List<string>> ActivateMapSection( Guid guid )
		{
			List<string> tiles = new List<string>();
			List<string> entities = new List<string>();

			MapSection ms = mapSections.Where( x => x.GUID == guid ).FirstOr( null );
			if ( ms != null )
			{
				ActivateMapSection( mapSections.IndexOf( ms ) );
				tiles.AddRange( ms.mapTiles.Where( x => x.entityProperties.isActive ).Select( x => $"{x.expansion} {x.tileID}{x.tileSide}" ) );
				entities = FindObjectOfType<MapEntityManager>().GetActiveEntities( ms.GUID );
			}

			return new Tuple<List<string>, List<string>>( tiles, entities );
		}

		/// <summary>
		/// Hides a section and all entities within (does NOT toggle IsActive)
		/// </summary>
		public List<string> DeactivateMapSection( Guid guid )
		{
			List<string> tiles = new List<string>();
			MapSection ms = mapSections.Where( x => x.GUID == guid ).FirstOr( null );
			if ( ms != null )
			{
				DeactivateMapSection( mapSections.IndexOf( ms ) );
				tiles.AddRange( ms.mapTiles.Select( x => $"{x.expansion} {x.tileID}{x.tileSide}" ) );
			}
			return tiles;
		}

		/// <summary>
		/// Shows all map sections that aren't marked to start hidden, returns all active tiles and entities
		/// </summary>
		public Tuple<List<string>, List<string>> ActivateAllVisibleSections()
		{
			List<string> tiles = new List<string>();
			List<string> entities = new List<string>();

			for ( int i = 0; i < mapSections.Count; i++ )
			{
				if ( !mapSections[i].invisibleUntilActivated )
				{
					entities = entities.Concat( FindObjectOfType<MapEntityManager>().GetActiveEntities( mapSections[i].GUID ) ).ToList();
					ActivateMapSection( i );
					tiles.AddRange( mapSections[i].mapTiles.Where( x => x.entityProperties.isActive ).Select( x => $"{x.expansion} {x.tileID}{x.tileSide}" ) );
				}
			}

			return new Tuple<List<string>, List<string>>( tiles, entities );
		}

		public string ActivateTile( Guid guid )
		{
			foreach ( var tr in tileRenderers )
			{
				if ( tr.mapTile.GUID == guid )
				{
					tr.ModifyVisibility( true );
					return $"{tr.mapTile.expansion} {tr.mapTile.tileID}{tr.mapTile.tileSide}";
				}
			}
			return "";
		}

		public string DeactivateTile( Guid guid )
		{
			foreach ( var tr in tileRenderers )
			{
				if ( tr.mapTile.GUID == guid )
				{
					tr.ModifyVisibility( false );
					return $"{tr.mapTile.expansion} {tr.mapTile.tileID}{tr.mapTile.tileSide}";
				}
			}
			return "";
		}

		public bool IsMapSectionActive( Guid guid )
		{
			return mapSections.Any( x => x.GUID == guid && x.isActive );
		}

		//public void ShowSectionTiles( Guid guid )
		//{
		//	foreach ( var tr in tileRenderers )
		//	{
		//		if ( tr.mapTile.mapSectionOwner == guid )
		//			tr.ShowTile();
		//	}
		//}
	}
}
