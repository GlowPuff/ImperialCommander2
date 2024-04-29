using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class TranslatedEventActionConverter : JsonConverter
{
	public override bool CanWrite => false;
	public override bool CanRead => true;

	public override bool CanConvert( Type objectType )
	{
		return objectType == typeof( ITranslatedEventAction );
	}

	public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
	{
		throw new NotImplementedException();
	}

	public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
	{
		var jsonObject = JArray.Load( reader );
		var eventActionAction = default( ITranslatedEventAction );
		List<ITranslatedEventAction> eObserver = new List<ITranslatedEventAction>();

		foreach ( var item in jsonObject )
		{
			if ( !item.HasValues )
				continue;

			switch ( item["eventActionType"].Value<int>() )
			{
				case 6://D1
					eventActionAction = item.ToObject<TranslatedEnemyDeployment>();
					break;
				case 20://G9
					eventActionAction = item.ToObject<TranslatedInputPrompt>();
					break;
				case 16://G7
					eventActionAction = item.ToObject<TranslatedTextBox>();
					break;
				case 1://G2
					eventActionAction = item.ToObject<TranslatedChangeMissionInfo>();
					break;
				case 2://G3
					eventActionAction = item.ToObject<TranslatedChangeObjective>();
					break;
				case 5://G6
					eventActionAction = item.ToObject<TranslatedQuestionPrompt>();
					break;
				case 7://D2
					eventActionAction = item.ToObject<TranslatedAllyDeployment>();
					break;
				case 11://GM1
					eventActionAction = item.ToObject<TranslatedChangeGroupInstructions>();
					break;
				case 15://M2
					eventActionAction = item.ToObject<TranslatedModifyMapEntity>();
					break;
				case 17://GM4
					eventActionAction = item.ToObject<TranslatedChangeRepositionInstructions>();
					break;
				case 21://D6
					eventActionAction = item.ToObject<TranslatedCustomEnemyDeployment>();
					break;
			}
			eObserver.Add( eventActionAction );
		}
		return eObserver;
	}
}