using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Saga
{
	//story missions are restricted to the selected expansion
	//side missions can be freely selected

	public class SagaCampaign
	{
		//campaign state
		public int formatVersion;
		//campaignExpansionCode is the CODE, ie: Core
		public string campaignName, campaignExpansionCode, campaignImportedName;
		public string campaignJournal = "";
		//XP is no longer used, XP is stored per Hero in CampaignHero
		public int XP, credits, fame, awards;
		public Guid GUID = Guid.Empty;
		public List<CampaignHero> campaignHeroes = new List<CampaignHero>();
		public List<CampaignStructure> campaignStructure = new List<CampaignStructure>();
		public List<DeploymentCard> campaignVillains = new List<DeploymentCard>();
		public List<DeploymentCard> campaignAllies = new List<DeploymentCard>();
		//store the ID
		public List<string> campaignItems = new List<string>();
		public List<string> campaignRewards = new List<string>();
		public List<string> campaignIgnored = new List<string>();
		//public bool isImported, isCustom;
		public CampaignType campaignType;
		public CampaignPackage campaignPackage;

		//data sets
		[JsonIgnore]
		public static List<CampaignItem> campaignDataItems;
		[JsonIgnore]
		public static List<CampaignSkill> campaignDataSkills;
		[JsonIgnore]
		public static List<CampaignStructure> campaignDataStructures;
		[JsonIgnore]
		public static List<CampaignReward> campaignDataRewards;

		/// <summary>
		/// use CreateNewCampaign() to create new campaigns
		/// </summary>
		public SagaCampaign()
		{
		}

		public static SagaCampaign CreateNewCampaign( string cname, string expansionCode )
		{
			SagaCampaign c = new SagaCampaign();
			c.formatVersion = 2;
			c.GUID = Guid.NewGuid();
			c.campaignName = cname;
			c.campaignExpansionCode = expansionCode;
			c.campaignType = expansionCode == "Custom" ? CampaignType.Custom : CampaignType.Official;

			c.LoadCampaignData();
			//now campaign structures are set for this campaign's expansion
			c.campaignStructure = campaignDataStructures;

			return c;
		}

		public static SagaCampaign CreateNewImportedCampaign( string cname, CampaignPackage package )
		{
			SagaCampaign c = new SagaCampaign();
			c.formatVersion = 2;
			c.GUID = Guid.NewGuid();
			c.campaignName = cname;
			c.campaignExpansionCode = "Imported";
			c.campaignType = CampaignType.Imported;
			c.campaignPackage = package;
			c.campaignImportedName = package.campaignName;
			c.campaignStructure = package.campaignStructure;

			c.LoadCampaignData();
			return c;
		}

		/// <summary>
		/// For existing campaigns that are loaded, fix the expansion code for "Other" missions which have their "expansionCode" property incorrectly set to the Campaign's expansion code
		/// </summary>
		public void FixExpansionCodes()
		{
			//Doesn't seem to be necessary anymore?
			//official campaigns containing "Other" missions retain their "Other" expansionCode just fine

			//for ( int i = 0; i < campaignStructures.Count; i++ )
			//{
			//	campaignStructure[i].expansionCode = campaignStructures[i].expansionCode;
			//}
		}

		/// <summary>
		/// set the translated card data for lists
		/// onlyOfficial = true when loading state to translate text
		/// custom and imported mission data don't get translated, so skip them on loading state
		/// </summary>
		public void LoadCampaignData( bool onlyOfficial = false )
		{
			try
			{
				//items
				campaignDataItems = LoadAsset<List<CampaignItem>>( $"Languages/{DataStore.Language}/CampaignData/items" );

				//skills
				campaignDataSkills = LoadAsset<List<CampaignSkill>>( $"Languages/{DataStore.languageCodeList[DataStore.languageCode]}/CampaignData/skills" );

				//rewards
				campaignDataRewards = LoadAsset<List<CampaignReward>>( $"Languages/{DataStore.Language}/CampaignData/rewards" );

				//mission structure
				if ( campaignType == CampaignType.Official )//official campaign
				{
					campaignDataStructures = LoadAsset<List<CampaignStructure>>( $"CampaignData/{campaignExpansionCode}" );
					//if the mission ID is already set, make it NOT selectable in the future
					campaignDataStructures = campaignDataStructures.Select( x =>
					{
						x.missionSource = MissionSource.None;
						if ( !string.IsNullOrEmpty( x.missionID ) )
						{
							x.missionSource = MissionSource.Official;
							x.canModify = false;
						}
						return x;
					} ).ToList();
				}
				else if ( !onlyOfficial && campaignType == CampaignType.Custom )
				{
					campaignDataStructures = new List<CampaignStructure>
					{
						new CampaignStructure()
						{
							missionType = MissionType.Introduction,
							missionID = "",
							threatLevel = 1,
							expansionCode = "",
							isAgendaMission = false,
							missionSource= MissionSource.None,
							itemTier = new string[] { "1" }
						}
					};
				}
				else if ( !onlyOfficial && campaignType == CampaignType.Imported )//custom campaign package
				{
					campaignDataStructures = campaignStructure;
					campaignDataStructures = campaignDataStructures.Select( x =>
					{
						Guid guid = Guid.Parse( x.missionID );
						x.canModify = guid == Guid.Empty;
						x.packageGUID = campaignPackage.GUID;
						return x;
					} ).ToList();
				}
			}
			catch ( JsonReaderException e )
			{
				Debug.Log( $"LoadCampaignData() ERROR:\r\nError parsing Campaign data" );
				Debug.Log( e.Message );
				throw new Exception();
			}
		}

		//public static MissionPreset GetMissionPreset( Expansion expansion, string expCode )
		//{
		//	var presets = LoadAsset<List<MissionPreset>>( $"MissionPresets/{expansion}" );
		//	return presets.Where( x => x.id == expCode.ToLower() ).FirstOr( null );
		//}

		/// <summary>
		/// load the actual Campaign state
		/// </summary>
		public static SagaCampaign LoadCampaignState( Guid guid )
		{
			string json = "";
			try
			{
				string path = Path.Combine( FileManager.campaignPath, $"{guid}.json" );
				using ( StreamReader sr = new StreamReader( path ) )
				{
					json = sr.ReadToEnd();
				}
				var state = JsonConvert.DeserializeObject<SagaCampaign>( json );
				//skip loading custom/import structure template, only translate official Mission data
				state.LoadCampaignData( true );

				Debug.Log( "***CAMPAIGN LOADED***" );
				return state;
			}
			catch ( Exception e )
			{
				Utils.LogError( "LoadCampaignState()::" + e.Message );
				return null;
			}
		}

		public void SaveCampaignState( List<CampaignStructure> structures = null )
		{
			Debug.Log( "SaveCampaignState()::SAVING CAMPAIGN" );

			if ( structures != null )
				campaignStructure = structures;

			try
			{
				if ( GUID == Guid.Empty )
					throw new Exception( "Campaign GUID is Empty" );

				string output = JsonConvert.SerializeObject( this, Formatting.Indented );
				string outpath = Path.Combine( FileManager.campaignPath, $"{GUID}.json" );
				using ( var stream = File.CreateText( outpath ) )
				{
					stream.Write( output );
				}

				Debug.Log( "***CAMPAIGN SAVED***" );
			}
			catch ( Exception e )
			{
				Utils.LogError( "SaveCampaignState()::" + e.Message );
			}
		}

		public static T LoadAsset<T>( string assetName )
		{
			try
			{
				string json = Resources.Load<TextAsset>( assetName ).text;
				return JsonConvert.DeserializeObject<T>( json );
			}
			catch ( JsonReaderException e )
			{
				Debug.Log( $"LoadCampaignData() ERROR:\r\nError parsing Campaign data [{assetName}]" );
				Debug.Log( e.Message );
				throw e;
			}
		}

		public CampaignItem GetItemFromID( string id ) => campaignDataItems.FirstOrDefault( x => x.id == id );
		public CampaignSkill GetSkillFromID( string id ) => campaignDataSkills.FirstOrDefault( x => x.id == id );
		public CampaignReward GetRewardFromID( string id ) => campaignDataRewards.FirstOrDefault( x => x.id == id );

		public string GetCampaignInfo()
		{
			if ( campaignType == CampaignType.Imported )//custom campaign package
			{
				//check if there's a translation first
				string instructions = FileManager.LoadEmbeddedCampaignInstructions( campaignPackage.GUID );
				if ( !string.IsNullOrEmpty( instructions ) )
					return instructions;
				else
					return campaignPackage.campaignInstructions;
			}
			else if ( campaignType == CampaignType.Custom )
			{
				return "";
			}
			else
			{
				string text = Resources.Load<TextAsset>( $"Languages/{DataStore.Language}/CampaignData/CampaignInfo/{campaignExpansionCode}Info" ).text;
				return text != null ? text : string.Empty;
			}
		}

		public void SetNextStoryMission( string customMissionID, MissionSource source )
		{
			Debug.Log( $"SetNextStoryMission()::{customMissionID}" );

			//'source' is determined by whether the "next mission" is Custom or Official

			try
			{
				if ( GUID == Guid.Empty )
					throw new Exception( "Campaign GUID is Empty" );
				if ( string.IsNullOrEmpty( customMissionID ) )
					throw new Exception( "Custom Mission ID is Empty" );

				//get the index of the current mission in the structure list
				int idx = campaignStructure.FindIndex( x => x.missionID.ToLower() == DataStore.sagaSessionData.setupOptions.projectItem.missionID.ToLower() );
				if ( idx != -1 )
				{
					//find the next Story/Finale mission AFTER the current Mission in the structure list
					for ( int index = idx + 1; index < campaignStructure.Count; index++ )
					{
						if ( campaignStructure[index].missionType == MissionType.Story
							|| campaignStructure[index].missionType == MissionType.Finale )
						{
							if ( source == MissionSource.Embedded )
							{
								var m = campaignPackage.campaignMissionItems.Where( x => x.customMissionIdentifier.ToLower() == customMissionID.ToLower() ).FirstOr( null );
								if ( m != null )
								{
									Debug.Log( $"campaignStructure with Index={index} changed" );
									campaignStructure[index].missionSource = MissionSource.Embedded;
									campaignStructure[index].missionID = m.missionGUID.ToString();
									campaignStructure[index].projectItem.Title = m.missionName;
									campaignStructure[index].projectItem.missionGUID = m.missionGUID.ToString();

									var currentCampaign = FileManager.importedCampaigns.FirstOrDefault( x => x.campaignName == RunningCampaign.sagaCampaign.campaignImportedName );
									var selectedMission = currentCampaign.campaignMissionItems.FirstOrDefault( x => x.missionGUID == m.missionGUID );
									campaignStructure[index].projectItem.Description = selectedMission.mission.missionProperties.missionDescription;
									campaignStructure[index].projectItem.AdditionalInfo = selectedMission.mission.missionProperties.additionalMissionInfo;

									break;
								}
								else
									Debug.Log( $"WARNING::SetNextStoryMission()::Couldn't find [Embedded] Mission with customMissionIdentifier={customMissionID}" );
							}
							else//official Mission
							{
								var m = DataStore.GetMissionCard( customMissionID );
								if ( m != null )
								{
									Debug.Log( $"campaignStructure with Index={index} changed" );
									campaignStructure[index].missionSource = MissionSource.Official;
									campaignStructure[index].expansionCode = Utils.ParseExpansionName( customMissionID );
									campaignStructure[index].missionID = customMissionID;
									break;
								}
								else
									Debug.Log( $"WARNING::SetNextStoryMission()::Couldn't find [Official] Mission with customMissionIdentifier={customMissionID}" );
							}
						}
						else
							Debug.Log( $"WARNING::SetNextStoryMission()::Couldn't find a STORY Mission that follows this Mission in the Campaign structure: {DataStore.sagaSessionData.setupOptions.projectItem.missionID}" );
					}
				}
				else
				{
					Debug.Log( "WARNING::SetNextStoryMission()::Couldn't find this Mission in the Campaign Mission structure" );
				}
			}
			catch ( Exception e )
			{
				Utils.LogWarning( $"SetNextStoryMission()::ERROR\n{e.Message}" );
			}
		}

		public void ModifyNextMissionThreatLevel( int level )
		{
			Debug.Log( $"ModifyNextMissionThreatLevel()::Modify Threat Level by: {level}" );

			try
			{
				//get the index of the current mission in the structure list
				int idx = campaignStructure.FindIndex( x => x.missionID.ToLower() == DataStore.sagaSessionData.setupOptions.projectItem.missionID.ToLower() );
				if ( idx != -1 )
				{
					//find the next mission (any) AFTER the current Mission in the structure list
					if ( idx + 1 < campaignStructure.Count )
					{
						campaignStructure[idx + 1].threatLevel = Math.Max( 0, campaignStructure[idx + 1].threatLevel + level );
						campaignStructure[idx + 1].threatModifiedByMission = true;
						Debug.Log( $"ModifyNextMissionThreatLevel()::New Threat Level for [Mission index {idx + 1}] Set to [{campaignStructure[idx + 1].threatLevel}]" );
					}
					else
						Debug.Log( "ModifyNextMissionThreatLevel()::There are no Missions after this one in the Campaign Mission structure" );
				}
				else
				{
					Debug.Log( "WARNING::SetNextStoryMission()::Couldn't find this Mission in the Campaign Mission structure" );
				}
			}
			catch ( Exception e )
			{
				Utils.LogWarning( $"ModifyNextMissionThreatLevel()::ERROR\n{e.Message}" );
			}
		}
	}
}