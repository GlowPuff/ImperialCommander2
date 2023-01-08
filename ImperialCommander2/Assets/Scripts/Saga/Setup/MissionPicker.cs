using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.UI;

namespace Saga
{
	public class MissionPicker : MonoBehaviour
	{
		public GameObject missionItemPrefab, folderItemPrefab;
		public Transform missionContainer;
		//UI
		public TextMeshProUGUI missionNameText, missionDescriptionText, additionalInfoText;
		public Button tilesButton, modeToggleButton;
		public TileInfoPopup tileInfoPopup;
		public Text modeToggleBtnText;
		public CanvasGroup canvasGroup;
		public Transform busyIcon;
		public GameObject busyPanel;

		[HideInInspector]
		public ProjectItem selectedMission;
		[HideInInspector]
		public bool isBusy = false;
		[HideInInspector]
		public PickerMode pickerMode { get; set; }

		string basePath;
		ProjectItem[] projectItems;
		ToggleGroup toggleGroup;
		List<string> expansionsAvailable = new List<string>();
		Stack<string> folderBreadcrumbs = new Stack<string>();

		private void Start()
		{
			pickerMode = PickerMode.BuiltIn;
			selectedMission = null;
			toggleGroup = missionContainer.GetComponent<ToggleGroup>();

			//basePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), "ImperialCommander" );

#if UNITY_ANDROID
			// /storage/emulated/0/Android/data/com.GlowPuff.ImperialCommander2/files
			//string customPath = "/storage/emulated/0/ImperialCommander2";
			string customPath = Path.Combine( Application.persistentDataPath, "CustomMissions" );
#else
			string customPath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), "ImperialCommander" );
#endif
			//make sure the custom folder exists
			try
			{
				if ( !Directory.Exists( customPath ) )
				{
					var dinfo = Directory.CreateDirectory( customPath );
					if ( dinfo == null )
					{
						Utils.LogError( "Could not create the Mission project folder.\r\nTried to create: " + customPath );
						FindObjectOfType<SagaSetup>()?.errorPanel?.Show( $"Could not create the Mission project folder.\r\nTried to create: {customPath}" );
					}
				}
			}
			catch ( Exception )
			{
				Utils.LogError( "Could not create the Mission project folder.\r\nTried to create: " + customPath );
				FindObjectOfType<SagaSetup>()?.errorPanel?.Show( $"Could not create the Mission project folder.\r\nTried to create: {customPath}" );
			}

			busyIcon.DORotate( new Vector3( 0, 0, 360 ), 1f, RotateMode.FastBeyond360 ).SetEase( Ease.InOutQuad ).SetLoops( -1 );

			basePath = "BuiltIn";
			StartCoroutine( GetMissionsAvailable() );
		}

		IEnumerator GetMissionsAvailable()
		{
			isBusy = true;
			foreach ( var item in Enum.GetValues( typeof( Expansion ) ) )
			{
				AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync( item.ToString() );
				while ( !handle.IsDone )
					yield return null;

				if ( handle.Result.Count > 0 )
					expansionsAvailable.Add( item.ToString() );
				Addressables.Release( handle );
			}
			isBusy = false;

			Debug.Log( "GetMissionsAvailable()::FOUND " + expansionsAvailable.Aggregate( "", ( acc, next ) => acc + next + ", " ) );
			OnChangeBuiltinFolder( basePath );
		}

		/// <summary>
		/// Change folder for CUSTOM mission mode
		/// </summary>
		public void OnChangeFolder( string path )
		{
			selectedMission = null;
			OnMissionSelected( null );
			if ( !folderBreadcrumbs.Contains( path ) )
				folderBreadcrumbs.Push( path );
			//populate mission picker items
			for ( int i = 1; i < missionContainer.childCount; i++ )
			{
				//get rid of all items except the UP FOLDER item
				Destroy( missionContainer.GetChild( i ).gameObject );
			}

			//disable UP folder if we're at the root
			if ( basePath == folderBreadcrumbs.Peek() )
				missionContainer.GetChild( 0 ).gameObject.SetActive( false );
			else
				missionContainer.GetChild( 0 ).gameObject.SetActive( true );

			//add folders first
			if ( Directory.Exists( folderBreadcrumbs.Peek() ) )
			{
				var di = new DirectoryInfo( folderBreadcrumbs.Peek() );
				var folders = di.GetDirectories();
				try
				{
					foreach ( var folder in folders )
					{
						var fi = Instantiate( folderItemPrefab, missionContainer );
						fi.GetComponent<MissionPickerFolder>().Init( folder );
					}

					//then add files
					projectItems = FileManager.GetProjects( folderBreadcrumbs.Peek() ).ToArray();
					//sort alphabetically
					projectItems = projectItems.Where( x => x.missionGUID != null ).OrderBy( x => x.Title ).ToArray();
					bool first = true;
					foreach ( var item in projectItems )
					{
						var picker = Instantiate( missionItemPrefab, missionContainer );
						var pi = picker.GetComponent<MissionPickerItem>();
						pi.GetComponent<Toggle>().group = toggleGroup;
						pi.Init( item, first, PickerMode.Custom );
						first = false;
					}

					if ( projectItems.Length > 0 )
					{
						selectedMission = projectItems[0];
						OnMissionSelected( selectedMission );
					}
				}
				catch ( Exception )
				{
					Utils.LogError( $"OnChangeFolder()::Error iterating over '{path}'" );
					FindObjectOfType<SagaSetup>().errorPanel.Show( $"OnChangeFolder()::Error iterating over '{path}'" );
				}
			}
		}

		public void OnChangeBuiltinFolder( string expansion )
		{
			selectedMission = null;
			OnMissionSelected( null );
			folderBreadcrumbs.Push( (expansion.ToString()) );

			//populate mission picker items
			for ( int i = 1; i < missionContainer.childCount; i++ )
			{
				//get rid of all items except the UP FOLDER item
				Destroy( missionContainer.GetChild( i ).gameObject );
			}
			//disable UP folder if we're at the root
			if ( basePath == folderBreadcrumbs.Peek() )
				missionContainer.GetChild( 0 ).gameObject.SetActive( false );
			else
				missionContainer.GetChild( 0 ).gameObject.SetActive( true );

			//if this is the top, add folders
			if ( folderBreadcrumbs.Peek() == "BuiltIn" )
			{
				foreach ( var folder in expansionsAvailable )
				{
					var fi = Instantiate( folderItemPrefab, missionContainer );
					fi.GetComponent<MissionPickerFolder>().InitBuiltin( folder.ToString() );
				}
			}
			else//otherwise we're in a built-in subfolder, so populate with missions
			{
				AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync( expansion );
				handle.Completed += ( x ) =>
				{
					StartCoroutine( CreateBuiltInPickersFromAddressables( x.Result ) );
					Addressables.Release( handle );
				};
			}
		}

		IEnumerator CreateBuiltInPickersFromAddressables( IList<IResourceLocation> locations )
		{
			isBusy = true;
			Dictionary<string, string[]> missionList = new Dictionary<string, string[]>();
			List<ProjectItem> piList = new List<ProjectItem>();
			AsyncOperationHandle<TextAsset> loadHandle;

			foreach ( var item in locations )
			{
				if ( Utils.AssetExists( item ) )
				{
					loadHandle = Addressables.LoadAssetAsync<TextAsset>( item );
				}
				else
				{
					FindObjectOfType<SagaSetup>().errorPanel.Show( $"CreateBuiltInPickersFromAddressables()::Asset key doesn't exist:\n{item.PrimaryKey}" );
					isBusy = false;
					yield break;
				}

				while ( !loadHandle.IsDone )
					yield return null;

				if ( loadHandle.Status == AsyncOperationStatus.Succeeded )
				{
					string[] loadedMission = loadHandle.Result.text.Split( '\n' );
					missionList.Add( item.PrimaryKey, loadedMission );
				}
				else
				{
					Addressables.Release( loadHandle );
					FindObjectOfType<SagaSetup>().errorPanel.Show( $"CreateBuiltInPickersFromAddressables()::Error in LoadAssetAsync():\n{item.PrimaryKey}" );
					isBusy = false;
					yield break;
				}

				Addressables.Release( loadHandle );
			}

			try
			{
				//create project items from mission list
				//the Keys are CORE1, JABBA4, etc
				foreach ( var item in missionList.Keys )
				{
					piList.Add( CreateProjectItem( missionList[item], item, item ) );
				}
			}
			catch ( Exception e )
			{
				FindObjectOfType<SagaSetup>().errorPanel.Show( $"CreateBuiltInPickersFromAddressables()::Error creating in CreateProjectItem():\n{e.Message}" );
				isBusy = false;
				yield break;
			}

			//sort the pi list
			piList.Sort( ( ProjectItem i1, ProjectItem i2 ) =>
			{
				//Debug.Log( $"SORTING::{i1.Title}.......{i2.Title}" );
				if ( i1.missionID != "Custom" && i2.missionID != "Custom" )
				{
					int n1 = int.Parse( i1.missionID.Split( ' ' )[1] );
					int n2 = int.Parse( i2.missionID.Split( ' ' )[1] );
					if ( n1 < n2 )
						return -1;
					else if ( n1 > n2 )
						return 1;
					else
						return 0;
				}
				else
				{
					Debug.Log( $"ERROR PARSING MISSION::i1={i1.missionID}, i2={i2.missionID}" );
					return 1;
				}
			} );

			bool first = true;
			foreach ( var item in piList )
			{
				var picker = Instantiate( missionItemPrefab, missionContainer );
				var pi = picker.GetComponent<MissionPickerItem>();
				pi.GetComponent<Toggle>().group = toggleGroup;
				pi.Init( item, first, PickerMode.BuiltIn );
				first = false;
			}

			isBusy = false;
		}

		public void OnChangeMode()
		{
			folderBreadcrumbs.Clear();

			if ( pickerMode == PickerMode.Custom )
			{
				pickerMode = PickerMode.BuiltIn;
				modeToggleBtnText.text = DataStore.uiLanguage.sagaUISetup.officialBtn;
				basePath = "BuiltIn";
				OnChangeBuiltinFolder( basePath );
			}
			else
			{
				pickerMode = PickerMode.Custom;
				modeToggleBtnText.text = DataStore.uiLanguage.sagaUISetup.customBtn;
#if UNITY_ANDROID
				basePath = Application.persistentDataPath + "/CustomMissions";
#else
				basePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), "ImperialCommander" );
#endif
				OnChangeFolder( basePath );
			}

			FindObjectOfType<SagaSetup>().OnModeChange( pickerMode );
		}

		public void OnMissionSelected( ProjectItem pi )
		{
			EventSystem.current.SetSelectedGameObject( null );
			if ( pi != null )
			{
				selectedMission = pi;
				missionNameText.text = pi?.Title;
				missionDescriptionText.text = pi?.Description;
				additionalInfoText.text = pi?.AdditionalInfo;
				if ( pi.missionID != "Custom" )//official mission
				{
					var expansion = pi.missionID.Split( ' ' )[0].ToLower();
					var id = pi.missionID.Split( ' ' )[1].ToLower();
					var presets = DataStore.missionPresets[expansion];
					var mp = presets.Where( x => x.id.ToLower() == $"{expansion}{id}" ).FirstOr( null );
					FindObjectOfType<SagaSetup>().OnMissionSelected( mp );
				}
				else//custom mission
					FindObjectOfType<SagaSetup>().OnMissionSelected( pi );
			}
			else
			{
				selectedMission = null;
				missionNameText.text = "";
				missionDescriptionText.text = "";
				additionalInfoText.text = "";
			}
		}

		public void OnFolderUp()
		{
			EventSystem.current.SetSelectedGameObject( null );
			OnMissionSelected( null );
			if ( pickerMode == PickerMode.Custom )
			{
				folderBreadcrumbs.Pop();
				OnChangeFolder( folderBreadcrumbs.Peek() );
			}
			else
				OnChangeBuiltinFolder( "BuiltIn" );
		}

		public void OnTiles()
		{
			EventSystem.current.SetSelectedGameObject( null );
			Mission m = null;
			Action doneAction = () =>
			{
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
			};

			if ( pickerMode == PickerMode.Custom )
			{
				m = FileManager.LoadMission( selectedMission.fullPathWithFilename );
				if ( m != null )
					doneAction();
				else
					FindObjectOfType<SagaSetup>().errorPanel.Show( $"OnTiles()::Mission is null\n'{selectedMission.fullPathWithFilename}'" );
			}
			else
			{
				isBusy = true;
				try
				{
					if ( Utils.AssetExists( selectedMission.fullPathWithFilename ) )
					{
						AsyncOperationHandle<TextAsset> loadHandle = Addressables.LoadAssetAsync<TextAsset>( selectedMission.fullPathWithFilename );
						loadHandle.Completed += ( x ) =>
						{
							if ( x.Status == AsyncOperationStatus.Succeeded )
							{
								m = FileManager.LoadMissionFromString( x.Result.text );
								doneAction();
							}
							else
								FindObjectOfType<SagaSetup>().errorPanel.Show( $"OnTiles()::Mission is null\n'{selectedMission.fullPathWithFilename}'" );

							Addressables.Release( loadHandle );
							isBusy = false;
						};
					}
					else
						FindObjectOfType<SagaSetup>().errorPanel.Show( $"OnTiles()::Can't find asset\n'{selectedMission.fullPathWithFilename}'" );
				}
				catch ( Exception e )
				{
					FindObjectOfType<SagaSetup>().errorPanel.Show( $"OnTiles()::Exception:\n'{e.Message}'\n'{selectedMission.fullPathWithFilename}'" );
					isBusy = false;
				}
			}
		}

		public ProjectItem CreateProjectItem( string[] text, string fileName = "", string fullName = "" )
		{
			ProjectItem projectItem = new ProjectItem();
			//FileInfo fi = new FileInfo( filename );

			//string[] text = File.ReadAllLines( filename );
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
						projectItem.Description = aiSplit[1].Substring( 0, aiSplit[1].Length - 1 );
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
						var ainfo = aiSplit[1].Substring( 0, aiSplit[1].Length - 1 );
						ainfo = ainfo == "null" ? "" : ainfo;
						projectItem.AdditionalInfo = ainfo;
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
					if ( split[0] == "missionDescription" && !string.IsNullOrEmpty( split[1] ) )
					{
						int idx = line.IndexOf( ':' );
						int c = line.LastIndexOf( ',' );
						string mname = line.Substring( idx + 1, c - idx - 1 ).Replace( "\"", "" ).Trim();
						projectItem.Description = mname;
					}
					if ( split[0] == "additionalMissionInfo" && !string.IsNullOrEmpty( split[1] ) )
					{
						int idx = line.IndexOf( ':' );
						int c = line.LastIndexOf( ',' );
						string mname = line.Substring( idx + 1, c - idx - 1 ).Replace( "\"", "" ).Trim();
						projectItem.AdditionalInfo = mname;
					}
				}
			}

			projectItem.fullPathWithFilename = fullName;//fi.FullName;

			//process auto-description and translated title for known missions
			if ( projectItem.missionID != "Custom" )
			{
				string[] id = projectItem.missionID.Split( ' ' );
				var card = DataStore.missionCards[id[0]].Where( x => x.id == $"{id[0]}{id[1]}" ).FirstOr( null );
				if ( card != null )
					projectItem.Description = card.descriptionText;
				else
					projectItem.Description = "Error parsing description.";

				//for built-in missions, get TRANSLATED mission title also
				if ( pickerMode == PickerMode.BuiltIn )
				{
					if ( card != null )
						projectItem.Title = card.name;
				}
			}

			return projectItem;
		}

		private void Update()
		{
			tilesButton.interactable = selectedMission != null && !isBusy;
			modeToggleButton.interactable = !isBusy;
			canvasGroup.interactable = !isBusy;
			busyPanel.SetActive( isBusy );
		}
	}
}