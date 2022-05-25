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

		public static List<TileDescriptor> LoadData()
		{
			string assetName = "dimensions.json";
			try
			{
				TextAsset json = Resources.Load<TextAsset>( "dimensions" );
				return JsonConvert.DeserializeObject<List<TileDescriptor>>( json.text );
			}
			catch ( JsonReaderException )
			{
				//Utils.Log( $"TileDescriptor::LoadData() ERROR:\r\nError parsing {assetName}" );
				//Utils.Log( e.Message );
				throw new Exception( $"TileDescriptor::LoadData() ERROR:\r\nError parsing {assetName}" );
			}
		}
	}
}
