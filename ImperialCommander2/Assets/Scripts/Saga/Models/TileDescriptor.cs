using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Saga
{
	public class TileDescriptor
	{
		public string expansion { get; set; }
		public int id { get; set; }
		public int width { get; set; }
		public int height { get; set; }
		public string biomeA { get; set; }
		public string biomeB { get; set; }
		[JsonIgnore]
		public BiomeType biomeTypeA
		{
			get
			{
				Enum.TryParse( biomeA, true, out BiomeType res );
				return res;
			}
		}
		[JsonIgnore]
		public BiomeType biomeTypeB
		{
			get
			{
				Enum.TryParse( biomeB, true, out BiomeType res );
				return res;
			}
		}
		[JsonIgnore]
		public int biomeWeight => width * height;

		public static List<TileDescriptor> LoadData()
		{
			string assetName = "dimensions.json";
			try
			{
				TextAsset json = Resources.Load<TextAsset>( "dimensions" );
				return JsonConvert.DeserializeObject<List<TileDescriptor>>( json.text );
			}
			catch ( JsonReaderException e )
			{
				Utils.LogError( $"TileDescriptor::LoadData() ERROR:\r\nError parsing {assetName}\n{e.Message}" );
				return null;
			}
		}
	}
}
