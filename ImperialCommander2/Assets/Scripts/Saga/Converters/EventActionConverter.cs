using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Saga
{
	public class EventActionConverter : JsonConverter
	{
		public override bool CanWrite => false;
		public override bool CanRead => true;

		public override bool CanConvert( Type objectType )
		{
			return objectType == typeof( EventAction );
		}

		public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
		{
			throw new NotImplementedException();
		}

		public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
		{
			var jsonObject = JArray.Load( reader );
			var eventActionAction = default( IEventAction );
			List<IEventAction> eObserver = new List<IEventAction>();

			foreach ( var item in jsonObject )
			{
				if ( !item.HasValues )
					continue;

				switch ( item["eventActionType"].Value<int>() )
				{
					case 0:
						eventActionAction = item.ToObject<MissionManagement>();
						break;
					case 1:
						eventActionAction = item.ToObject<ChangeMissionInfo>();
						break;
					case 2:
						eventActionAction = item.ToObject<ChangeObjective>();
						break;
					case 3:
						eventActionAction = item.ToObject<ModifyVariable>();
						break;
					case 4:
						eventActionAction = item.ToObject<ModifyThreat>();
						break;
					case 5:
						eventActionAction = item.ToObject<QuestionPrompt>();
						break;
					case 6:
						eventActionAction = item.ToObject<EnemyDeployment>();
						break;
					case 7:
						eventActionAction = item.ToObject<AllyDeployment>();
						break;
					case 8:
						eventActionAction = item.ToObject<OptionalDeployment>();
						break;
					case 9:
						eventActionAction = item.ToObject<RandomDeployment>();
						break;
					case 10:
						eventActionAction = item.ToObject<AddGroupDeployment>();
						break;
					case 11:
						eventActionAction = item.ToObject<ChangeInstructions>();
						break;
					case 12:
						eventActionAction = item.ToObject<ChangeTarget>();
						break;
					case 13:
						eventActionAction = item.ToObject<ChangeGroupStatus>();
						break;
					case 14:
						eventActionAction = item.ToObject<MapManagement>();
						break;
					case 15:
						eventActionAction = item.ToObject<ModifyMapEntity>();
						break;
					case 16:
						eventActionAction = item.ToObject<ShowTextBox>();
						break;
					case 17:
						eventActionAction = item.ToObject<ChangeReposition>();
						break;
					case 18:
						eventActionAction = item.ToObject<ResetGroup>();
						break;
					case 19:
						eventActionAction = item.ToObject<ActivateEventGroup>();
						break;
					case 20:
						eventActionAction = item.ToObject<InputPrompt>();
						break;
					case 21:
						eventActionAction = item.ToObject<CustomEnemyDeployment>();
						break;
					case 22:
						eventActionAction = item.ToObject<RemoveGroup>();
						break;
					case 23:
						eventActionAction = item.ToObject<QueryGroup>();
						break;
					case 24:
						eventActionAction = item.ToObject<CampaignModifyXP>();
						break;
					case 25:
						eventActionAction = item.ToObject<CampaignModifyCredits>();
						break;
					case 26:
						eventActionAction = item.ToObject<CampaignModifyFameAwards>();
						break;
					case 27:
						eventActionAction = item.ToObject<CampaignSetNextMission>();
						break;
					case 28:
						eventActionAction = item.ToObject<ModifyRoundLimit>();
						break;
					case 29:
						eventActionAction = item.ToObject<SetCountdown>();
						break;
				}
				eObserver.Add( eventActionAction );
			}

			return eObserver;
		}
	}
}
