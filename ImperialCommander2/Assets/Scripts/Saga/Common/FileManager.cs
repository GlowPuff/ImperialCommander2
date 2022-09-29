using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Saga
{
	public class FileManager
	{
		/// <summary>
		/// Return ProjectItem info for missions in Project folder
		/// </summary>
		//		public static IEnumerable<ProjectItem> GetProjects()
		//		{
		//#if UNITY_ANDROID
		//			string basePath = Path.Combine( Application.persistentDataPath, "CustomMissions" );
		//#else
		//			string basePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), "ImperialCommander" );
		//#endif

		//			try
		//			{
		//				//make sure the project folder exists
		//				if ( !Directory.Exists( basePath ) )
		//				{
		//					var dinfo = Directory.CreateDirectory( basePath );
		//					if ( dinfo == null )
		//					{
		//						Utils.LogError( "Could not create the Mission project folder.\r\nTried to create: " + basePath );
		//						return null;
		//					}
		//				}
		//			}
		//			catch ( Exception )
		//			{
		//				Utils.LogError( "Could not create the Mission project folder.\r\nTried to create: " + basePath );
		//				return null;
		//			}

		//			List<ProjectItem> items = new List<ProjectItem>();
		//			DirectoryInfo di = new DirectoryInfo( basePath );
		//			FileInfo[] files = di.GetFiles().Where( file => file.Extension == ".json" ).ToArray();

		//			try
		//			{
		//				//find mission files
		//				foreach ( FileInfo fi in files )
		//				{
		//					var pi = CreateProjectItem( fi.FullName );
		//					items.Add( pi );
		//				}
		//				items.Sort();
		//				return items;
		//			}
		//			catch ( Exception )
		//			{
		//				return null;
		//			}
		//		}

		//public static ProjectItem CreateProjectItem( string filename )
		//{
		//	ProjectItem projectItem = new ProjectItem();
		//	FileInfo fi = new FileInfo( filename );

		//	string[] text = File.ReadAllLines( filename );
		//	foreach ( var line in text )
		//	{
		//		//manually parse each line
		//		string[] split = line.Split( ':' );
		//		if ( split.Length == 2 )
		//		{
		//			projectItem.fileName = fi.Name;
		//			//projectItem.relativePath = Path.GetRelativePath( basePath, new DirectoryInfo( filename ).FullName );

		//			split[0] = split[0].Replace( "\"", "" ).Replace( ",", "" ).Trim();
		//			split[1] = split[1].Replace( "\"", "" ).Replace( ",", "" ).Trim();
		//			if ( split[0] == "missionName" )
		//				projectItem.Title = split[1];
		//			if ( split[0] == "saveDate" )
		//				projectItem.Date = split[1];
		//			if ( split[0] == "fileVersion" )
		//				projectItem.fileVersion = split[1];
		//			if ( split[0] == "timeTicks" )
		//				projectItem.timeTicks = long.Parse( split[1] );
		//		}
		//	}

		//	return projectItem;
		//}

		public static Mission LoadMissionFromString( string json )
		{
			try
			{
				var m = JsonConvert.DeserializeObject<Mission>( json );
				Debug.Log( "Loaded Mission: " + m.fileName );
				return m;
			}
			catch ( Exception e )
			{
				Utils.LogError( "LoadMissionFromString()::Could not load the Mission. Exception: " + e.Message );
				return null;
			}
		}

		/// <summary>
		/// Loads a mission from its FULL PATH .json.
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

		public static Mission LoadMission( string filename )
		{
			return LoadMission( filename, out string foo );
		}

		public static List<SagaCampaign> GetCampaigns()
		{
			List<SagaCampaign> clist = new List<SagaCampaign>();
			string basePath = Path.Combine( Application.persistentDataPath, "SagaCampaigns" );

			string json = "";
			List<string> filenames = Directory.GetFiles( basePath ).Where( x =>
			{
				FileInfo fi = new FileInfo( x );
				return fi.Extension == ".json";
			} ).ToList();

			try
			{
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
				File.WriteAllText( Path.Combine( Application.persistentDataPath, "error_log.txt" ), "LOAD CAMPAIGNS TRACE:\r\n" + e.Message );
				return null;
			}
		}

		public static void DeleteCampaign( Guid guid )
		{
			string basePath = Path.Combine( Application.persistentDataPath, "SagaCampaigns" );
			DirectoryInfo di = new DirectoryInfo( basePath );

			try
			{

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
				File.WriteAllText( Path.Combine( Application.persistentDataPath, "error_log.txt" ), "DeleteCampaign() TRACE:\r\n" + e.Message );
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

			var di = new DirectoryInfo( basePath );
			var folders = di.GetDirectories();

			foreach ( var folder in folders )
			{
				//iterate each folder
				ProjectItem[] projectItems = GetProjects( basePath ).ToArray();
				missions.AddRange( projectItems );
			}

			return missions;
		}

		public static IEnumerable<ProjectItem> GetProjects( string pathToUse )
		{
			List<ProjectItem> items = new List<ProjectItem>();
			DirectoryInfo di = new DirectoryInfo( pathToUse );
			FileInfo[] files = di.GetFiles().Where( file => file.Extension == ".json" ).ToArray();

			try
			{
				//find mission files
				foreach ( FileInfo fi in files )
				{
					//FileInfo fi = new FileInfo( filename );
					string[] text = File.ReadAllLines( fi.FullName );
					//var pi = CreateProjectItem( fi.FullName );
					var pi = CreateProjectItem( text, fi.Name, fi.FullName );
					items.Add( pi );
				}
				items.Sort();
				return items;
			}
			catch ( Exception )
			{
				return null;
			}
		}

		public static ProjectItem CreateProjectItem( string[] text, string fileName = "", string fullName = "" )
		{
			ProjectItem projectItem = new ProjectItem();

			foreach ( var line in text )
			{
				//manually parse each line
				string[] split = line.Split( ':' );
				if ( split.Length == 2 )
				{
					projectItem.fileName = fileName;//fi.Name;

					split[0] = split[0].Replace( "\"", "" ).Replace( ",", "" ).Trim();
					split[1] = split[1].Replace( "\"", "" ).Replace( ",", "" ).Trim();
					if ( split[0] == "missionName" )
						projectItem.Title = split[1];
					if ( split[0] == "saveDate" )
						projectItem.Date = split[1];
					if ( split[0] == "fileVersion" )
						projectItem.fileVersion = split[1];
					if ( split[0] == "timeTicks" )
						projectItem.timeTicks = long.Parse( split[1] );
					if ( split[0] == "missionDescription" && !string.IsNullOrEmpty( split[1] ) )
					{
						string[] aiSplit = line.Split( ':' );
						aiSplit[0] = aiSplit[0].Replace( "\"", "" ).Trim();
						aiSplit[1] = aiSplit[1].Replace( "\"", "" ).Trim();
						projectItem.Description = aiSplit[1].Substring( 0, aiSplit[1].Length - 2 );
					}
					if ( split[0] == "missionID" )
						projectItem.missionID = split[1];
					if ( split[0] == "missionGUID" )
						projectItem.missionGUID = split[1];
					if ( split[0] == "additionalMissionInfo" && !string.IsNullOrEmpty( split[1] ) )
					{
						string[] aiSplit = line.Split( ':' );
						aiSplit[0] = aiSplit[0].Replace( "\"", "" ).Trim();
						aiSplit[1] = aiSplit[1].Replace( "\"", "" ).Trim();
						projectItem.AdditionalInfo = aiSplit[1].Substring( 0, aiSplit[1].Length - 2 );
					}
				}
				else if ( split.Length > 2 )//mission name with a colon
				{
					for ( int i = 0; i < split.Length; i++ )
						split[i] = split[i].Replace( "\"", "" ).Replace( ",", "" ).Trim();
					if ( split[0] == "missionName" && !string.IsNullOrEmpty( split[1] ) )
					{
						int idx = line.IndexOf( ':' );
						int c = line.LastIndexOf( ',' );
						string mname = line.Substring( idx + 1, c - idx - 1 ).Replace( "\"", "" ).Trim();
						projectItem.Title = mname;
					}
				}
			}

			projectItem.fullPathWithFilename = fullName;

			return projectItem;
		}
	}
}