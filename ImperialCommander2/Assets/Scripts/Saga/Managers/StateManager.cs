using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Saga
{
	/// <summary>
	/// Saga mode game save/restore
	/// </summary>
	public class StateManager
	{
		public ManagerStates managerStates;

		public static void SaveState()
		{
			Debug.Log( "SaveSession()::SAVING SESSION" );

			string basePath = Path.Combine( Application.persistentDataPath, "SagaSession" );

			try
			{
				if ( !Directory.Exists( basePath ) )
					Directory.CreateDirectory( basePath );

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

				Debug.Log( "***SESSION SAVED***" );
			}
			catch ( Exception e )
			{
				Debug.Log( "***ERROR*** SaveSession(Saga):: " + e.Message );
				File.WriteAllText( Path.Combine( basePath, "error_log.txt" ), "SaveState() TRACE:\r\n" + e.Message );
			}
		}

		public bool LoadSession()
		{
			string basePath = Path.Combine( Application.persistentDataPath, "SagaSession" );

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

				Debug.Log( "***SESSION LOADED***" );
				return true;
			}
			catch ( Exception e )
			{
				Debug.Log( "***ERROR*** LoadState:: " + e.Message );
				File.WriteAllText( Path.Combine( basePath, "error_log.txt" ), "RESTORE STATE TRACE:\r\n" + e.Message );
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
