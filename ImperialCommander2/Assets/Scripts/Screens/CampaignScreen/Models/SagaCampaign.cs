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
		public int XP, credits, fame, awards;
		public Guid GUID;
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
			//Official campaigns containing "Other" missions retain their "Other" expansionCode just fine

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
				else if ( !onlyOfficial && campaignType == CampaignType.Imported )
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

		public CampaignItem GetItemFromID( string id ) => campaignDataItems.First( x => x.id == id );
		public CampaignSkill GetSkillFromID( string id ) => campaignDataSkills.First( x => x.id == id );
		public CampaignReward GetRewardFromID( string id ) => campaignDataRewards.First( x => x.id == id );

		public string GetCampaignInfo()
		{
			if ( campaignType == CampaignType.Imported )
			{
				return campaignPackage.campaignInstructions;
			}
			else
			{
				string text = Resources.Load<TextAsset>( $"Languages/{DataStore.Language}/CampaignData/CampaignInfo/{campaignExpansionCode}Info" ).text;
				return text != null ? text : string.Empty;
			}
		}
	}
}
