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
		public static IEnumerable<ProjectItem> GetProjects()
		{
#if UNITY_ANDROID
			string basePath = Application.persistentDataPath + "/CustomMissions";
#else
			string basePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), "ImperialCommander" );
#endif

			try
			{
				//make sure the project folder exists
				if ( !Directory.Exists( basePath ) )
				{
					var dinfo = Directory.CreateDirectory( basePath );
					if ( dinfo == null )
					{
						Utils.LogError( "Could not create the Mission project folder.\r\nTried to create: " + basePath );
						return null;
					}
				}
			}
			catch ( Exception )
			{
				Utils.LogError( "Could not create the Mission project folder.\r\nTried to create: " + basePath );
				return null;
			}

			List<ProjectItem> items = new List<ProjectItem>();
			DirectoryInfo di = new DirectoryInfo( basePath );
			FileInfo[] files = di.GetFiles().Where( file => file.Extension == ".json" ).ToArray();

			try
			{
				//find mission files
				foreach ( FileInfo fi in files )
				{
					var pi = CreateProjectItem( fi.FullName );
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

		public static ProjectItem CreateProjectItem( string filename )
		{
			ProjectItem projectItem = new ProjectItem();
			FileInfo fi = new FileInfo( filename );

			string[] text = File.ReadAllLines( filename );
			foreach ( var line in text )
			{
				//manually parse each line
				string[] split = line.Split( ':' );
				if ( split.Length == 2 )
				{
					projectItem.fileName = fi.Name;
					//projectItem.relativePath = Path.GetRelativePath( basePath, new DirectoryInfo( filename ).FullName );

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
				}
			}

			return projectItem;
		}

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
		public static Mission LoadMission( string filename )
		{
			string json = "";

			try
			{
				using ( StreamReader sr = new StreamReader( filename ) )
				{
					json = sr.ReadToEnd();
				}

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
				return null;
			}
		}
	}
}
