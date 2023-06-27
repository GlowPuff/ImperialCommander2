using System;
using Newtonsoft.Json;

namespace Saga
{
	public class MapTile : IMapEntity
	{
		public Guid GUID { get; set; }
		public string name { get; set; }
		public EntityType entityType { get; set; }
		public Vector entityPosition { get; set; }
		public float entityRotation { get; set; }
		public bool hasProperties { get; }
		public bool hasColor { get; }
		public EntityProperties entityProperties { get; set; }
		public Guid mapSectionOwner { get; set; }
		//public bool isActive { get; set; } = false;//track visibility status in Saga

		//unity props
		[JsonIgnore]
		public TileRenderer tileRenderer { get; set; }

		//tile props
		public string textureName { get { return $"{expansion}_{tileID}{tileSide}"; } }
		public string tileID;
		public string tileSide;
		public Expansion expansion;
		[JsonIgnore]
		public int biomeWeight
		{
			get => tileRenderer.tileDescriptor.biomeWeight;
		}
		[JsonIgnore]
		public BiomeType biomeType
		{
			get
			{
				if ( tileSide == "A" )
					return tileRenderer.tileDescriptor.biomeTypeA;
				else
					return tileRenderer.tileDescriptor.biomeTypeB;
			}
		}
	}
}