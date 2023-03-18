using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Saga
{
	public class FileManager
	{
		public static void CreateFolders()
		{
			//campaign folder
			string basePath = Path.Combine( Application.persistentDataPath, "SagaCampaigns" );
			if ( !Directory.Exists( basePath ) )
				Directory.CreateDirectory( basePath );

			//custom mission folder
#if UNITY_ANDROID
			basePath = Path.Combine( Application.persistentDataPath, "CustomMissions" );
#else
			basePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), "ImperialCommander" );
#endif
			if ( !Directory.Exists( basePath ) )
				Directory.CreateDirectory( basePath );
		}

		public static Mission LoadMissionFromString( string json )
		{
			//make sure it's a mission, simple check for a property in the text
			if ( !json.Contains( "missionGUID" ) )
				return null;

			try
			{
				var m = JsonConvert.DeserializeObject<Mission>( json );
				Debug.Log( "LoadMissionFromString: " + m.missionProperties.missionID );
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

		public static List<SagaCampaign> GetCampaigns()
		{
			List<SagaCampaign> clist = new List<SagaCampaign>();
			string basePath = Path.Combine( Application.persistentDataPath, "SagaCampaigns" );

			if ( !Directory.Exists( basePath ) )
			{
				Directory.CreateDirectory( basePath );
			}

			try
			{
				string json = "";
				List<string> filenames = Directory.GetFiles( basePath ).Where( x =>
				{
					FileInfo fi = new FileInfo( x );
					return fi.Extension == ".json";
				} ).ToList();

				foreach ( var item in filenames )
				{
					string path = Path.Combine( basePath, item );
					using ( StreamReader sr = new StreamReader( path ) )
					{
						json = sr.ReadToEnd();
					}
					var campaign = JsonConvert.DeserializeObject<SagaCampaign>( json );
					clist.Add( campaign );
				}

				return clist;
			}
			catch ( Exception e )
			{
				Debug.Log( "***ERROR*** GetCampaigns():: " + e.Message );
				DataStore.LogError( "GetCampaigns() TRACE:\r\n" + e.Message );
				return clist;
			}
		}

		public static void DeleteCampaign( Guid guid )
		{
			string basePath = Path.Combine( Application.persistentDataPath, "SagaCampaigns" );

			try
			{
				DirectoryInfo di = new DirectoryInfo( basePath );

				List<string> filenames = di.GetFiles().Where( file => file.Extension == ".json" ).Select( x => x.Name ).ToList();
				if ( filenames.Contains( $"{guid.ToString()}.json" ) )
				{
					var file = filenames.Where( x => x.Contains( $"{guid.ToString()}.json" ) ).First();
					File.Delete( Path.Combine( basePath, file ) );
				}
			}
			catch ( Exception e )
			{
				Debug.Log( "***ERROR*** DeleteCampaign():: " + e.Message );
				DataStore.LogError( "DeleteCampaign() TRACE:\r\n" + e.Message );
			}
		}

		public static List<ProjectItem> GetCustomMissions()
		{
			List<ProjectItem> missions = new List<ProjectItem>();

#if UNITY_ANDROID
				string basePath = Application.persistentDataPath + "/CustomMissions";
#else

			string basePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), "ImperialCommander" );
#endif

			if ( !Directory.Exists( basePath ) )
				Directory.CreateDirectory( basePath );

			try
			{
				RecursiveFolderSearch( basePath, missions );

				return missions;
			}
			catch ( Exception e )
			{
				Debug.Log( "***ERROR*** GetCustomMissions():: " + e.Message );
				DataStore.LogError( "GetCustomMissions() TRACE:\r\n" + e.Message );
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
	}
}