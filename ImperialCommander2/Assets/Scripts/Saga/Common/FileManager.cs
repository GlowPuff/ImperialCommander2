using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Saga
{
	public class FileManager
	{
		public static string baseDocumentFolder, campaignPath, customMissionPath, importedCharactersPath, classicSessionPath, sagaSessionPath, campaignSessionPath, classicDefaultsPath, customCampaignPath;

		/// <summary>
		/// Creates default folders and sets up folder path properties
		/// </summary>
		public static bool SetupDefaultFolders()
		{
			Debug.Log( "SETTING UP DEFAULT FOLDERS" );

			campaignPath = Path.Combine( Application.persistentDataPath, "SagaCampaigns" );
			classicSessionPath = Path.Combine( Application.persistentDataPath, "Session" );
			sagaSessionPath = Path.Combine( Application.persistentDataPath, "SagaSession" );
			campaignSessionPath = Path.Combine( Application.persistentDataPath, "CampaignSession" );
			classicDefaultsPath = Path.Combine( Application.persistentDataPath, "Defaults" );

			//OS-specific paths that are NOT 'persistentDataPath', except on Android
#if UNITY_ANDROID
			baseDocumentFolder = Application.persistentDataPath;
			customMissionPath = Path.Combine( Application.persistentDataPath, "CustomMissions" );
			importedCharactersPath = Path.Combine( Application.persistentDataPath, "ImportedCharacters" );
			customCampaignPath = Path.Combine( Application.persistentDataPath, "CustomCampaigns" );
#else
			baseDocumentFolder = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), "ImperialCommander" );
			customMissionPath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), "ImperialCommander", "CustomMissions" );
			importedCharactersPath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), "ImperialCommander", "ImportedCharacters" );
			customCampaignPath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), "ImperialCommander", "CustomCampaigns" );
#endif

			try
			{
#if !UNITY_ANDROID
				CreateFolder( baseDocumentFolder );
#endif
				CreateFolder( campaignPath );
				CreateFolder( classicSessionPath );
				CreateFolder( sagaSessionPath );
				CreateFolder( campaignSessionPath );
				CreateFolder( classicDefaultsPath );
				CreateFolder( customMissionPath );
				CreateFolder( importedCharactersPath );
				CreateFolder( customCampaignPath );

				return true;
			}
			catch ( Exception e )
			{
				Utils.LogError( "CreateFolders()::Could not create default folders.\n" + e.Message );
				return false;
			}
		}

		/// <summary>
		/// If the folder doesn't exist, create it
		/// </summary>
		public static bool CreateFolder( string path )
		{
			try
			{
				if ( !Directory.Exists( path ) )
					Directory.CreateDirectory( path );
				return true;
			}
			catch ( Exception e )
			{
				Utils.LogError( $"CreateFolder()::Failed to create path:\n{path}\n{e.Message}" );
				return false;
			}
		}

		public static Mission LoadMissionFromString( string json )
		{
			//make sure it's a mission, simple check for a property in the text
			if ( !json.Contains( "missionGUID" ) )
				return null;

			try
			{
				var m = JsonConvert.DeserializeObject<Mission>( json );
				//Debug.Log( "LoadMissionFromString: " + m.missionProperties.missionID );
				return m;
			}
			catch ( Exception e )
			{
				Utils.LogError( "LoadMissionFromString()::Could not load the Mission. Exception: " + e.Message );
				return null;
			}
		}

		/// <summary>
		/// Loads a mission from its FULL PATH .json
		/// </summary>
		public static Mission LoadMission( string filename, out string missionStringified )
		{
			string json = "";

			try
			{
				using ( StreamReader sr = new StreamReader( filename ) )
				{
					json = sr.ReadToEnd();
				}

				missionStringified = json;
				var m = JsonConvert.DeserializeObject<Mission>( json );
				//overwrite fileName so it's up-to-date
				FileInfo fi = new FileInfo( filename );
				m.fileName = fi.Name;
				Debug.Log( "Loaded Mission: " + m.fileName );
				return m;
			}
			catch ( Exception e )
			{
				Utils.LogError( "LoadMission()::Could not load the Mission. Exception: " + e.Message );
				missionStringified = null;
				return null;
			}
		}

		/// <summary>
		/// Loads a mission from its FULL PATH .json, ignoring the stringified version
		/// </summary>
		public static Mission LoadMission( string filename )
		{
			return LoadMission( filename, out string foo );
		}

		public static void LoadMissionFromAddressable( string missionAddressableKey, Action<Mission, string> callback )
		{
			Mission mission = null;
			string s = "";

			AsyncOperationHandle<TextAsset> loadHandle = Addressables.LoadAssetAsync<TextAsset>( missionAddressableKey );
			loadHandle.Completed += ( x ) =>
			{
				if ( x.Status == AsyncOperationStatus.Succeeded )
				{
					s = x.Result.text;
					mission = FileManager.LoadMissionFromString( x.Result.text );
				}
				Addressables.Release( loadHandle );

				callback( mission, s );
			};
		}

		/// <summary>
		/// Only collect campaigns with formatVersion == Utils.expectedCampaignFormatVersion
		/// </summary>
		public static List<SagaCampaign> GetCampaigns()
		{
			List<SagaCampaign> clist = new List<SagaCampaign>();

			try
			{
				string json = "";
				List<string> filenames = Directory.GetFiles( campaignPath ).Where( x =>
				{
					FileInfo fi = new FileInfo( x );
					return fi.Extension == ".json";
				} ).ToList();

				foreach ( var item in filenames )
				{
					string path = Path.Combine( campaignPath, item );
					using ( StreamReader sr = new StreamReader( path ) )
					{
						json = sr.ReadToEnd();
					}
					var campaign = JsonConvert.DeserializeObject<SagaCampaign>( json );
					if ( campaign.formatVersion == Utils.expectedCampaignFormatVersion )
						clist.Add( campaign );
				}

				return clist;
			}
			catch ( Exception e )
			{
				Utils.LogError( "GetCampaigns()::" + e.Message );
				return clist;
			}
		}

		public static void DeleteCampaign( Guid guid )
		{
			try
			{
				DirectoryInfo di = new DirectoryInfo( campaignPath );

				List<string> filenames = di.GetFiles().Where( file => file.Extension == ".json" ).Select( x => x.Name ).ToList();
				if ( filenames.Contains( $"{guid}.json" ) )
				{
					var file = filenames.Where( x => x.Contains( $"{guid}.json" ) ).First();
					File.Delete( Path.Combine( campaignPath, file ) );
				}
			}
			catch ( Exception e )
			{
				Utils.LogError( "DeleteCampaign()::" + e.Message );
			}
		}

		public static List<ProjectItem> GetCustomMissions()
		{
			List<ProjectItem> missions = new List<ProjectItem>();

			try
			{
				RecursiveFolderSearch( customMissionPath, missions );

				return missions;
			}
			catch ( Exception e )
			{
				Utils.LogError( "GetCustomMissions()::" + e.Message );
				return missions;
			}
		}

		/// <summary>
		/// Custom mission recursive folder search
		/// </summary>
		static void RecursiveFolderSearch( string basePath, List<ProjectItem> missions )
		{
			var di = new DirectoryInfo( basePath );
			var folders = di.GetDirectories();

			//add missions in current folder (basePath)
			ProjectItem[] projectItems = GetProjects( basePath ).ToArray();
			missions.AddRange( projectItems );

			foreach ( var folder in folders )
			{
				//iterate each folder within this one
				RecursiveFolderSearch( folder.FullName, missions );
			}
		}

		/// <summary>
		/// Custom mission ProjectItem converter 
		/// </summary>
		public static IEnumerable<ProjectItem> GetProjects( string pathToUse )
		{
			List<ProjectItem> items = new List<ProjectItem>();

			try
			{
				DirectoryInfo di = new DirectoryInfo( pathToUse );
				FileInfo[] files = di.GetFiles().Where( file => file.Extension == ".json" ).ToArray();
				//find mission files
				foreach ( FileInfo fi in files )
				{
					string text = File.ReadAllText( fi.FullName );
					var pi = CreateProjectItem( text, fi.Name, fi.FullName );
					if ( pi != null )
						items.Add( pi );
				}
				items.Sort();
				return items;
			}
			catch ( Exception )
			{
				return items;
			}
		}

		public static ProjectItem CreateProjectItem( string stringifiedMission, string fileName = "", string fullName = "" )
		{
			ProjectItem projectItem = new ProjectItem();

			var mission = LoadMissionFromString( stringifiedMission );
			if ( mission != null )
			{
				projectItem.fullPathWithFilename = fullName;
				projectItem.fileName = fileName;
				projectItem.Title = mission.missionProperties.missionName;
				projectItem.Date = mission.saveDate;
				projectItem.fileVersion = mission.fileVersion;
				projectItem.timeTicks = mission.timeTicks;
				projectItem.Description = mission.missionProperties.missionDescription;
				projectItem.missionID = mission.missionProperties.missionID;
				projectItem.missionGUID = mission.missionGUID.ToString();
				projectItem.AdditionalInfo = mission.missionProperties.additionalMissionInfo;
				projectItem.stringifiedMission = stringifiedMission;
			}
			else
				return null;
			//else
			//	UnityEngine.Object.FindObjectOfType<SagaSetup>()?.errorPanel?.Show( "FileManager::CreateProjectItem()", "Mission is null" );

			return projectItem;
		}

		/// <summary>
		/// Imports global custom characters
		/// </summary>
		public static List<CustomToon> LoadGlobalImportedCharacters()
		{
			string json = "";
			List<CustomToon> importedToons = new List<CustomToon>();

			try
			{
				var filenames = Directory.GetFiles( importedCharactersPath );
				foreach ( string filename in filenames )
				{
					using ( StreamReader sr = new StreamReader( filename ) )
					{
						json = sr.ReadToEnd();
					}
					var toon = JsonConvert.DeserializeObject<CustomToon>( json );
					//recreate the GUID in case multiple physical file copies of this toon are added
					toon.customCharacterGUID = Guid.NewGuid();
					toon.deploymentCard.customCharacterGUID = toon.customCharacterGUID;
					//rename the ID for global imports so they don't conflic with Mission-embedded custom characters using TC# for the ID
					toon.cardID = toon.customCharacterGUID.ToString();
					toon.deploymentCard.id = toon.cardID;
					//also rename the ID for their instructions and bonus effects
					toon.bonusEffect.bonusID = toon.cardID;
					toon.cardInstruction.instID = toon.cardID;
					//force expansion to "Other"
					toon.deploymentCard.expansion = "Other";
					importedToons.Add( toon );
					//activation instructions
					DataStore.importedActivationInstructions.Add( toon.cardInstruction );
					//bonus effects
					DataStore.importedBonusEffects.Add( toon.bonusEffect );
				}

				Debug.Log( $"LoadGlobalImportedCharacters()::FOUND {importedToons.Count} CHARACTERS" );
				if ( importedToons.Count > 0 )
				{
					Debug.Log( $"HEROES: {importedToons.Where( x => x.deploymentCard.characterType == CharacterType.Hero ).Count()}" );
					Debug.Log( $"ALLIES: {importedToons.Where( x => x.deploymentCard.characterType == CharacterType.Ally ).Count()}" );
					Debug.Log( $"REBELS: {importedToons.Where( x => x.deploymentCard.characterType == CharacterType.Rebel ).Count()}" );
					Debug.Log( $"IMPERIALS: {importedToons.Where( x => x.deploymentCard.characterType == CharacterType.Imperial ).Count()}" );
					Debug.Log( $"VILLAINS: {importedToons.Where( x => x.deploymentCard.characterType == CharacterType.Villain ).Count()}" );
				}
				return importedToons;
			}
			catch ( Exception e )
			{
				Utils.LogError( "LoadGlobalImportedCharacters()::Could not load Imported Characters. Exception: " + e.Message );
				return importedToons;
			}
		}

		/// <param name="skipMissions">Skip loading Missions</param>
		/// <returns></returns>
		public static CampaignPackage LoadCampaignPackage( string fullFilename, bool skipMissions = false )
		{
			CampaignPackage package = null;

			try
			{
				List<Mission> missionList = new List<Mission>();
				//create the zip file
				using ( FileStream zipPath = new FileStream( fullFilename, FileMode.Open ) )
				{
					//open the archive
					using ( ZipArchive archive = new ZipArchive( zipPath, ZipArchiveMode.Read ) )
					{
						foreach ( var entry in archive.Entries )
						{
							//deserialize the CampaignPackage
							if ( entry.Name == "campaign_package.json" )
							{
								//open the package meta file
								using ( TextReader tr = new StreamReader( entry.Open() ) )
								{
									package = JsonConvert.DeserializeObject<CampaignPackage>( tr.ReadToEnd() );
								}
							}
							else if ( !skipMissions )//deserialize the individual missions
							{
								using ( TextReader tr = new StreamReader( entry.Open() ) )
								{
									missionList.Add( JsonConvert.DeserializeObject<Mission>( tr.ReadToEnd() ) );
								}
							}
						}

						//now add all the missions to the CampaignPackage
						if ( !skipMissions )
						{
							foreach ( var item in package.campaignMissionItems )
							{
								var m = missionList.Where( x => x.missionGUID == item.missionGUID ).FirstOr( null );
								if ( m != null )
									item.mission = m;
								else
									throw new Exception( $"Missing Mission in the zip archive:\n{item.missionName}\n{item.missionGUID}" );
							}
						}
					}
				}

				return package;
			}
			catch ( Exception ee )
			{
				Utils.LogError( $"LoadCampaignPackage()::Error loading the Campaign Package.\n{ee.Message}" );
				return null;
			}
		}

		public static List<CampaignPackage> GetCampaignPackageList( bool skipMissions )
		{
			List<CampaignPackage> importedCampaigns = new List<CampaignPackage>();

			try
			{
				var filenames = Directory.GetFiles( customCampaignPath );
				foreach ( string filename in filenames )
				{
					var cc = LoadCampaignPackage( filename, skipMissions );
					importedCampaigns.Add( cc );
				}
				Debug.Log( $"GetCampaignPackageList()::FOUND {importedCampaigns.Count} CUSTOM CAMPAIGNS" );
				return importedCampaigns;
			}
			catch ( Exception e )
			{
				Utils.LogError( "GetCampaignPackageList()::Could not create Custom Campaign List. Exception: " + e.Message );
				return importedCampaigns;
			}
		}

		public static Mission LoadEmbeddedMission( string campaignGUID, string missionGUID, out string missionStringified )
		{
			missionStringified = null;

			if ( string.IsNullOrEmpty( campaignGUID ) || string.IsNullOrEmpty( missionGUID ) )
				return null;
			//do a full load of the package, including the missions
			var packages = GetCampaignPackageList( false );
			var p = packages.Where( x => x.GUID.ToString() == campaignGUID ).FirstOr( null );
			if ( p != null )
			{
				missionStringified = JsonConvert.SerializeObject( p );
				return p.campaignMissionItems.Where( x => x.mission.missionGUID.ToString() == missionGUID ).FirstOr( null )?.mission;
			}

			return null;
		}
	}
}