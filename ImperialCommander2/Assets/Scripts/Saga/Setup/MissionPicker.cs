using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Saga
{
	public class MissionPicker : MonoBehaviour
	{
		public GameObject missionItemPrefab, folderItemPrefab;
		public Transform missionContainer;
		//UI
		public TextMeshProUGUI missionNameText, missionDescriptionText;
		public Button tilesButton;
		public TileInfoPopup tileInfoPopup;

		[HideInInspector]
		public ProjectItem selectedMission;

		string currentFolder, basePath, prevFolder;
		ProjectItem[] projectItems;
		ToggleGroup toggleGroup;

		private void Start()
		{
			selectedMission = null;
			toggleGroup = missionContainer.GetComponent<ToggleGroup>();

			basePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), "ImperialCommander" );

			//make sure the project folder exists
			if ( !Directory.Exists( basePath ) )
			{
				var dinfo = Directory.CreateDirectory( basePath );
				if ( dinfo == null )
				{
					Utils.LogError( "Could not create the Mission project folder.\r\nTried to create: " + basePath );
				}
			}

			OnChangeFolder( basePath );
		}

		public void OnChangeFolder( string path )
		{
			selectedMission = null;
			OnMissionSelected( null );
			prevFolder = currentFolder;
			currentFolder = path;
			//populate mission picker items
			for ( int i = 1; i < missionContainer.childCount; i++ )
			{
				//get rid of all items except the UP FOLDER item
				Destroy( missionContainer.GetChild( i ).gameObject );
			}

			//disable UP folder if we're at the root
			if ( basePath == currentFolder )
				missionContainer.GetChild( 0 ).gameObject.SetActive( false );
			else
				missionContainer.GetChild( 0 ).gameObject.SetActive( true );

			//add folders first
			var di = new DirectoryInfo( currentFolder );
			var folders = di.GetDirectories();
			foreach ( var folder in folders )
			{
				var fi = Instantiate( folderItemPrefab, missionContainer );
				fi.GetComponent<MissionPickerFolder>().Init( folder );
			}

			//then add files
			projectItems = GetProjects( currentFolder ).ToArray();
			bool first = true;
			foreach ( var item in projectItems )
			{
				var picker = Instantiate( missionItemPrefab, missionContainer );
				var pi = picker.GetComponent<MissionPickerItem>();
				pi.GetComponent<Toggle>().group = toggleGroup;
				pi.Init( item, first );
				first = false;
			}

			if ( projectItems.Length > 0 )
			{
				selectedMission = projectItems[0];
				OnMissionSelected( selectedMission );
			}
		}

		public void OnMissionSelected( ProjectItem pi )
		{
			EventSystem.current.SetSelectedGameObject( null );
			if ( pi != null )
			{
				selectedMission = pi;
				missionNameText.text = pi?.Title;
				missionDescriptionText.text = pi?.Description;
				tilesButton.interactable = true;
			}
			else
			{
				selectedMission = null;
				missionNameText.text = "";
				missionDescriptionText.text = "";
				tilesButton.interactable = false;
			}
		}

		public void OnFolderUp()
		{
			EventSystem.current.SetSelectedGameObject( null );
			OnMissionSelected( null );
			OnChangeFolder( prevFolder );
		}

		public void OnTiles()
		{
			EventSystem.current.SetSelectedGameObject( null );
			Mission m = FileManager.LoadMission( selectedMission.fullPathWithFilename );
			List<string> tiles = new List<string>();
			if ( m != null )
			{
				foreach ( var section in m.mapSections )
				{
					foreach ( var tile in section.mapTiles )
						tiles.Add( tile.expansion + " " + tile.tileID );
				}
				tileInfoPopup.Show( tiles.ToArray() );
			}
		}

		IEnumerable<ProjectItem> GetProjects( string pathToUse )
		{
			List<ProjectItem> items = new List<ProjectItem>();
			DirectoryInfo di = new DirectoryInfo( pathToUse );
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
					if ( split[0] == "missionDescription" )
						projectItem.Description = split[1];
					if ( split[0] == "missionID" )
						projectItem.missionID = split[1];
					if ( split[0] == "missionGUID" )
						projectItem.missionGUID = split[1];
					if ( split[0] == "missionName" )
						projectItem.Title = split[1];
				}
			}

			projectItem.fullPathWithFilename = fi.FullName;

			//process auto-description for known missions
			if ( projectItem.missionID != "Custom" )
			{
				string[] id = projectItem.missionID.Split( ' ' );
				var card = DataStore.missionCards[id[0]].Where( x => x.id == $"{id[0]}{id[1]}" ).FirstOr( null );
				if ( card != null )
					projectItem.Description = card.descriptionText;
				else
					projectItem.Description = "Error parsing description.";
			}

			return projectItem;
		}
	}
}