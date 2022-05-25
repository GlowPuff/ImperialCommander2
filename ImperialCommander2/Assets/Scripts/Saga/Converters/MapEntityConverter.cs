using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Saga
{
	public class MapEntityConverter : JsonConverter
	{
		public override bool CanWrite => false;
		public override bool CanRead => true;
		public override bool CanConvert( Type objectType ) => objectType == typeof( IMapEntity );
		public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer ) => throw new NotImplementedException();
		public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
		{
			var jsonObject = JArray.Load( reader );
			var entity = default( IMapEntity );
			List<IMapEntity> eObserver = new List<IMapEntity>();

			foreach ( var item in jsonObject )
			{
				switch ( item["entityType"].Value<int>() )
				{
					case 0://tile
						entity = item.ToObject<MapTile>();
						break;
					case 1://console
						entity = item.ToObject<Terminal>();
						break;
					case 2://Crate
						entity = item.ToObject<Crate>();
						break;
					case 3://DeploymentPoint
						entity = item.ToObject<DeploymentPoint>();
						break;
					case 4://Token
						entity = item.ToObject<Token>();
						break;
					case 5://Highlight
						entity = item.ToObject<SpaceHighlight>();
						break;
					case 6://Door
						entity = item.ToObject<Door>();
						break;
				}
				eObserver.Add( entity );
			}

			return eObserver;
		}
	}
}
