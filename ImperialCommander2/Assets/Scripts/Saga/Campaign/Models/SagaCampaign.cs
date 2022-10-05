using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Saga
{
	public static class RunningCampaign
	{
		public static Guid sagaCampaignGUID = Guid.Empty;
		public static string expansionCode;
		public static CampaignStructure campaignStructure;

		public static void Reset()
		{
			sagaCampaignGUID = Guid.Empty;
			expansionCode = "";
			campaignStructure = null;
		}
	}

	//story missions are restricted to the selected expansion
	//side missions are free

	public class SagaCampaign
	{
		//campaign state
		public string campaignName, campaignExpansionCode;//campaignExpansion is the CODE, ie: Core
		public int XP, credits, fame, awards;
		public Guid GUID;
		public List<CampaignHero> campaignHeroes = new List<CampaignHero>();
		public List<CampaignStructure> campaignStructure = new List<CampaignStructure>();
		//store the ID
		public List<string> campaignVillains = new List<string>();
		public List<string> campaignAllies = new List<string>();
		public List<string> campaignItems = new List<string>();

		//data sets
		[JsonIgnore]
		public static List<CampaignItem> campaignDataItems;
		[JsonIgnore]
		public static List<CampaignSkill> campaignDataSkills;
		[JsonIgnore]
		public static List<DeploymentCard> allyDataCards;
		[JsonIgnore]
		public static List<DeploymentCard> villainDataCards;
		[JsonIgnore]
		public static List<CampaignStructure> campaignStructures;

		/// <summary>
		/// use CreateNewCampaign() to create new campaigns
		/// </summary>
		public SagaCampaign()
		{
		}

		public static SagaCampaign CreateNewCampaign( string cname, string expansionCode )
		{
			SagaCampaign c = new SagaCampaign();
			c.GUID = Guid.NewGuid();
			c.campaignName = cname;
			c.campaignExpansionCode = expansionCode;

			c.LoadCampaignData();
			//now campaign structures are set for this campaign's expansion
			c.campaignStructure = campaignStructures;

			return c;
		}

		/// <summary>
		/// just the card data for lists
		/// </summary>
		public void LoadCampaignData()
		{
			try
			{
				//items
				campaignDataItems = LoadAsset<List<CampaignItem>>( "CardData/items" );

				//skills
				campaignDataSkills = LoadAsset<List<CampaignSkill>>( "CardData/skills" );

				//allies
				allyDataCards = LoadAsset<List<DeploymentCard>>( "CardData/allies" );

				//villains
				villainDataCards = LoadAsset<List<DeploymentCard>>( "CardData/villains" );

				//mission structure
				if ( campaignExpansionCode != "Custom" )
				{
					campaignStructures = LoadAsset<List<CampaignStructure>>( $"CampaignData/{campaignExpansionCode}" );
				}
				else
				{
					campaignStructures = new List<CampaignStructure>();
					campaignStructures.Add( new CampaignStructure()
					{
						missionType = MissionType.Introduction,
						missionID = "",
						threatLevel = 1,
						expansionCode = "",
						isAgendaMission = false,
						isCustom = true,
						itemTier = new string[] { "1" }
					} );
				}

			}
			catch ( JsonReaderException e )
			{
				Debug.Log( $"LoadCampaignData() ERROR:\r\nError parsing Campaign data" );
				Debug.Log( e.Message );
				throw new Exception();
			}
		}

		public static MissionPreset GetMissionPreset( Expansion expansion, string expCode )
		{
			var presets = LoadAsset<List<MissionPreset>>( $"MissionPresets/{expansion}" );
			return presets.Where( x => x.id == expCode.ToLower() ).FirstOr( null );
		}

		/// <summary>
		/// load the actual Campaign state
		/// </summary>
		public static SagaCampaign LoadCampaignState( Guid guid )
		{
			string basePath = Path.Combine( Application.persistentDataPath, "SagaCampaigns" );

			string json = "";
			try
			{
				string path = Path.Combine( basePath, $"{guid}.json" );
				using ( StreamReader sr = new StreamReader( path ) )
				{
					json = sr.ReadToEnd();
				}
				var state = JsonConvert.DeserializeObject<SagaCampaign>( json );
				state.LoadCampaignData();

				Debug.Log( "***CAMPAIGN LOADED***" );
				return state;
			}
			catch ( Exception e )
			{
				Debug.Log( "***ERROR*** LoadCampaignState():: " + e.Message );
				DataStore.LogError( "LoadCampaignState() TRACE:\r\n" + e.Message );
				return null;
			}
		}

		public void SaveCampaignState( List<CampaignStructure> structures = null )
		{
			Debug.Log( "SaveCampaignState()::SAVING CAMPAIGN" );

			string basePath = Path.Combine( Application.persistentDataPath, "SagaCampaigns" );

			if ( structures != null )
				campaignStructure = structures;

			try
			{
				if ( !Directory.Exists( basePath ) )
					Directory.CreateDirectory( basePath );

				string output = JsonConvert.SerializeObject( this, Formatting.Indented );
				string outpath = Path.Combine( basePath, $"{GUID}.json" );
				using ( var stream = File.CreateText( outpath ) )
				{
					stream.Write( output );
				}

				Debug.Log( "***CAMPAIGN SAVED***" );
			}
			catch ( Exception e )
			{
				Debug.Log( "***ERROR*** SaveCampaignState():: " + e.Message );
				DataStore.LogError( "SaveCampaignState() TRACE:\r\n" + e.Message );
			}
		}

		static T LoadAsset<T>( string assetName )
		{
			try
			{
				TextAsset json = Resources.Load<TextAsset>( assetName );
				return JsonConvert.DeserializeObject<T>( json.text );
			}
			catch ( JsonReaderException e )
			{
				Debug.Log( $"LoadCampaignData() ERROR:\r\nError parsing Campaign data" );
				Debug.Log( e.Message );
				throw e;
			}
		}

		public CampaignItem GetItemFromID( string id ) => campaignDataItems.First( x => x.id == id );
		public CampaignSkill GetSkillFromID( string id ) => campaignDataSkills.First( x => x.id == id );

		public string GetCampaignInfo()
		{
			TextAsset text = Resources.Load<TextAsset>( $"CampaignData/{campaignExpansionCode}Info" );
			return text?.text;
		}
	}
}
