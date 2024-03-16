using System;
using System.Collections.Generic;
using Newtonsoft.Json.Utilities;
using Saga;
using UnityEngine;

/// <summary>
/// allows forcing the compiler to generate code for a type so they aren't stripped by IL2CPP (Android)
/// </summary>
public class AotTypeEnforcer : MonoBehaviour
{
	public void Awake()
	{
		//some of these may not actually be used in JSON deserialization, but to be safe I list them all

		//MissionProperties
		AotHelper.EnsureList<MissionSubType>();
		AotHelper.EnsureList<string>();

		//Mission model
		AotHelper.EnsureList<MapSection>();
		AotHelper.EnsureList<Trigger>();
		AotHelper.EnsureList<MissionEvent>();
		AotHelper.EnsureList<IMapEntity>();
		AotHelper.EnsureList<EnemyGroupData>();
		AotHelper.EnsureList<EventGroup>();
		AotHelper.EnsureList<EntityGroup>();
		AotHelper.EnsureList<CustomToon>();

		//MapSection
		AotHelper.EnsureList<MapTile>();

		//Other stuff
		AotHelper.EnsureList<DCPointer>();
		AotHelper.EnsureList<TriggeredBy>();
		AotHelper.EnsureList<IEventAction>();
		AotHelper.EnsureList<ButtonAction>();
		AotHelper.EnsureList<DPData>();
		AotHelper.EnsureList<Guid>();
		AotHelper.EnsureList<InstructionOption>();
		AotHelper.EnsureList<CampaignSkill>();

		//SagaSession
		AotHelper.EnsureList<DeploymentCard>();
		AotHelper.EnsureList<LogItem>();

		//SagaGameVars
		AotHelper.EnsureList<DeploymentGroupOverride>();
		AotHelper.EnsureDictionary<string, SetCountdown>();

		//Dictionaries
		AotHelper.EnsureDictionary<Guid, int>();
		AotHelper.EnsureDictionary<string, List<MissionCard>>();
		AotHelper.EnsureDictionary<string, string>();
		AotHelper.EnsureDictionary<string, List<MissionPreset>>();

		//Campaign Package
		AotHelper.EnsureList<CampaignMissionItem>();
		AotHelper.EnsureList<CampaignStructure>();
		AotHelper.EnsureList<CampaignTranslationItem>();

	}
}