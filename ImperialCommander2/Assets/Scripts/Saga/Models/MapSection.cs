using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Saga
{
	public class MapSection
	{
		public string name;
		public Guid GUID;
		public bool canRemove;
		public bool isActive;
		public bool invisibleUntilActivated;

		public List<Trigger> triggers;
		public List<MissionEvent> missionEvents;
		public List<MapTile> mapTiles;

		/// <summary>
		/// Calculates highest weighted biome in this section's tiles
		/// </summary>
		public BiomeType GetBiomeType()
		{
			//iterate tiles in this section, collect biomes used and their weights, return highest weighted biome
			BiomeType btype = BiomeType.None;
			int weight = 0;
			Dictionary<BiomeType, int> biomes = new Dictionary<BiomeType, int>();
			foreach ( var t in mapTiles )
			{
				var b = t.GetBiomeType();
				var w = t.GetBiomeWeight();
				if ( b != BiomeType.None )
				{
					if ( !biomes.ContainsKey( b ) )
						biomes.Add( b, w );
					else
					{
						biomes[b] = biomes[b] + w;
					}
				}
			}
			//determine biome with the most weight
			biomes.ToList().ForEach( t =>
			{
				if ( t.Value > weight )
				{
					btype = t.Key;
					weight = t.Value;
				}
				Debug.Log( $"MAPTILE BIOMES: {t.Key}::{t.Value}" );
			} );
			return btype;
		}
	}
}
