using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Saga
{
	/// <summary>
	/// Saga mode game state save/restore
	/// </summary>
	public class StateManager
	{
		public ManagerStates managerStates;

		public static void RemoveState( SessionMode sessionMode )
		{
			Debug.Log( $"RemoveState()::REMOVING SESSION (Mode = {sessionMode})" );

			try
			{
				string basePath;
				if ( sessionMode == SessionMode.Saga )
					basePath = FileManager.sagaSessionPath;
				else
					basePath = FileManager.campaignSessionPath;

				if ( Directory.Exists( basePath ) )
					Directory.Delete( basePath, true );
				Debug.Log( $"***SESSION REMOVED (Mode = {sessionMode})***" );
			}
			catch ( Exception e )
			{
				Utils.LogError( "RemoveState()::" + e.Message );
			}
		}

		/// <summary>
		/// Saves the GAME state, NOT the campaign state
		/// </summary>
		/// <param name="sessionMode">Determines the FOLDER that's used to save to</param>
		public static void SaveState( SessionMode sessionMode )
		{
			Debug.Log( $"SaveSession()::SAVING SESSION (Mode = {sessionMode})" );

			string basePath;
			if ( sessionMode == SessionMode.Saga )
				basePath = FileManager.sagaSessionPath;
			else
				basePath = FileManager.campaignSessionPath;

			bool exists = true;
			if ( !Directory.Exists( basePath ) )
				exists = FileManager.SetupDefaultFolders();

			if ( !exists || !Directory.Exists( basePath ) )
			{
				Utils.LogError( $"SaveState()::FOLDER DOESN'T EXIST AND CAN'T BE CREATED. UNABLE TO SAVE STATE\nPATH: '{basePath}'" );
				return;
			}

			try
			{
				//save the session data
				DataStore.sagaSessionData.gameVars.isNewGame = false;//mark this as a saved session
				string output = JsonConvert.SerializeObject( DataStore.sagaSessionData, Formatting.Indented );
				string outpath = Path.Combine( basePath, "sessiondata.json" );
				using ( var stream = File.CreateText( outpath ) )
				{
					stream.Write( output );
				}

				//save the mission JSON
				outpath = Path.Combine( basePath, "mission.json" );
				using ( var stream = File.CreateText( outpath ) )
				{
					stream.Write( DataStore.sagaSessionData.missionStringified );
				}

				//==save card lists
				//deployment hand
				outpath = Path.Combine( basePath, "deploymenthand.json" );
				output = JsonConvert.SerializeObject( DataStore.deploymentHand, Formatting.Indented );
				using ( var stream = File.CreateText( outpath ) )
				{
					stream.Write( output );
				}

				//manual deployment deck
				outpath = Path.Combine( basePath, "manualdeployment.json" );
				output = JsonConvert.SerializeObject( DataStore.manualDeploymentList, Formatting.Indented );
				using ( var stream = File.CreateText( outpath ) )
				{
					stream.Write( output );
				}

				//deployed enemies
				outpath = Path.Combine( basePath, "deployedenemies.json" );
				output = JsonConvert.SerializeObject( DataStore.deployedEnemies, Formatting.Indented );
				using ( var stream = File.CreateText( outpath ) )
				{
					stream.Write( output );
				}

				//deployed heroes
				outpath = Path.Combine( basePath, "heroesallies.json" );
				output = JsonConvert.SerializeObject( DataStore.deployedHeroes, Formatting.Indented );
				using ( var stream = File.CreateText( outpath ) )
				{
					stream.Write( output );
				}

				//deploymentCards, so it doesn't have to be rebuilt
				outpath = Path.Combine( basePath, "deploymentcards.json" );
				output = JsonConvert.SerializeObject( DataStore.deploymentCards, Formatting.Indented );
				using ( var stream = File.CreateText( outpath ) )
				{
					stream.Write( output );
				}

				//MANAGER STATES
				//trigger manager
				outpath = Path.Combine( basePath, "triggermanager.json" );
				output = GlowEngine.FindUnityObject<TriggerManager>().GetState();
				using ( var stream = File.CreateText( outpath ) )
				{
					stream.Write( output );
				}

				//entity manager
				outpath = Path.Combine( basePath, "entitymanager.json" );
				output = GlowEngine.FindUnityObject<MapEntityManager>().GetState();
				using ( var stream = File.CreateText( outpath ) )
				{
					stream.Write( output );
				}

				//tile manager
				outpath = Path.Combine( basePath, "tilemanager.json" );
				output = GlowEngine.FindUnityObject<TileManager>().GetState();
				using ( var stream = File.CreateText( outpath ) )
				{
					stream.Write( output );
				}

				Debug.Log( $"***SESSION SAVED (Mode = {sessionMode})***" );
			}
			catch ( Exception e )
			{
				Utils.LogError( "SaveSession(Saga)::" + e.Message );
			}
		}

		/// <summary>
		/// Loads the GAME state, NOT the campaign state
		/// </summary>
		/// <param name="sessionMode">Determines the FOLDER that's used to load from</param>
		public bool LoadState( SessionMode sessionMode )
		{
			string basePath;
			if ( sessionMode == SessionMode.Saga )
				basePath = FileManager.sagaSessionPath;
			else
				basePath = FileManager.campaignSessionPath;

			string json = "";
			try
			{
				//session
				string path = Path.Combine( basePath, "sessiondata.json" );
				using ( StreamReader sr = new StreamReader( path ) )
				{
					json = sr.ReadToEnd();
				}
				DataStore.sagaSessionData = JsonConvert.DeserializeObject<SagaSession>( json );

				//mission
				path = Path.Combine( basePath, "mission.json" );
				using ( StreamReader sr = new StreamReader( path ) )
				{
					json = sr.ReadToEnd();
				}
				DataStore.mission = JsonConvert.DeserializeObject<Mission>( json );


				//deployment hand
				path = Path.Combine( basePath, "deploymenthand.json" );
				using ( StreamReader sr = new StreamReader( path ) )
				{
					json = sr.ReadToEnd();
				}
				DataStore.deploymentHand = JsonConvert.DeserializeObject<List<DeploymentCard>>( json );

				//manual deployment deck
				path = Path.Combine( basePath, "manualdeployment.json" );
				using ( StreamReader sr = new StreamReader( path ) )
				{
					json = sr.ReadToEnd();
				}
				DataStore.manualDeploymentList = JsonConvert.DeserializeObject<List<DeploymentCard>>( json );

				//deployed enemies
				path = Path.Combine( basePath, "deployedenemies.json" );
				using ( StreamReader sr = new StreamReader( path ) )
				{
					json = sr.ReadToEnd();
				}
				DataStore.deployedEnemies = JsonConvert.DeserializeObject<List<DeploymentCard>>( json );

				//deployed heroes
				path = Path.Combine( basePath, "heroesallies.json" );
				using ( StreamReader sr = new StreamReader( path ) )
				{
					json = sr.ReadToEnd();
				}
				DataStore.deployedHeroes = JsonConvert.DeserializeObject<List<DeploymentCard>>( json );

				//deploymentCards
				path = Path.Combine( basePath, "deploymentcards.json" );
				using ( StreamReader sr = new StreamReader( path ) )
				{
					json = sr.ReadToEnd();
				}
				DataStore.deploymentCards = JsonConvert.DeserializeObject<List<DeploymentCard>>( json );

				//manager states
				managerStates = new ManagerStates();
				//trigger manager
				path = Path.Combine( basePath, "triggermanager.json" );
				using ( StreamReader sr = new StreamReader( path ) )
				{
					json = sr.ReadToEnd();
				}
				managerStates.triggerManagerState = JsonConvert.DeserializeObject<TriggerManagerState>( json );

				//entity manager
				path = Path.Combine( basePath, "entitymanager.json" );
				using ( StreamReader sr = new StreamReader( path ) )
				{
					json = sr.ReadToEnd();
				}
				managerStates.entityManagerState = JsonConvert.DeserializeObject<EntityManagerState>( json );

				//tile manager
				path = Path.Combine( basePath, "tilemanager.json" );
				using ( StreamReader sr = new StreamReader( path ) )
				{
					json = sr.ReadToEnd();
				}
				managerStates.tileManagerState = JsonConvert.DeserializeObject<TileManagerState>( json );

				//set card text translations
				DataStore.SetCardTranslations( DataStore.deploymentHand );
				DataStore.SetCardTranslations( DataStore.manualDeploymentList );
				DataStore.SetCardTranslations( DataStore.deployedEnemies );
				DataStore.SetCardTranslations( DataStore.deployedHeroes );
				DataStore.SetCardTranslations( DataStore.deploymentCards );

				Debug.Log( $"***SESSION LOADED (Mode = {sessionMode}, {DataStore.mission.missionProperties.missionName})***" );
				return true;
			}
			catch ( Exception e )
			{
				Utils.LogError( "LoadState()::" + e.Message );
				return false;
			}
		}
	}

	public class ManagerStates
	{
		public TriggerManagerState triggerManagerState;
		public EntityManagerState entityManagerState;
		public TileManagerState tileManagerState;
	}
}
