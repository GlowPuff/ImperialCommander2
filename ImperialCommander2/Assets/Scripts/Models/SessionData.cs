using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject( MemberSerialization.OptOut )]
public class SessionData
{
	public int stateManagementVersion = 3;
	public Difficulty difficulty;
	public int threatLevel;
	public int addtlThreat;
	public AllyRules allyRules;
	public YesNo optionalDeployment;
	public YesNo allyThreatCost;
	public bool includeImperials, includeMercs;
	public Expansion selectedMissionExpansion;
	public string selectedMissionID;
	public string selectedMissionName;
	//0=starting, 1=reserved, 2=villains, 3=ignored, 4=heroes
	public List<DeploymentCard>[] selectedDeploymentCards { get; set; }
	public DeploymentCard selectedAlly;
	public GameVars gameVars;
	public bool useAdaptiveDifficulty;

	//using a default value allows the older JSON state which is missing these values to deserialize properly, otherwise they would be NULL
	//[DefaultValue( false )]
	//[JsonProperty( DefaultValueHandling = DefaultValueHandling.Populate )]

	[JsonIgnore]
	public List<DeploymentCard> MissionStarting
	{
		get { return selectedDeploymentCards[0]; }
	}
	[JsonIgnore]
	public List<DeploymentCard> MissionReserved
	{
		get { return selectedDeploymentCards[1]; }
	}
	[JsonIgnore]
	public List<DeploymentCard> EarnedVillains
	{
		get { return selectedDeploymentCards[2]; }
	}
	[JsonIgnore]
	public List<DeploymentCard> MissionIgnored
	{
		get { return selectedDeploymentCards[3]; }
	}
	[JsonIgnore]
	public List<DeploymentCard> MissionHeroes
	{
		get { return selectedDeploymentCards[4]; }
	}

	public class GameVars
	{
		public int round;
		public int eventsTriggered;
		public int currentThreat;
		public int deploymentModifier;
		public int fame;
		public bool pauseDeployment;
		public bool pauseThreatIncrease;
		public bool isNewGame = true;

		public GameVars()
		{

		}
	}

	public SessionData()
	{
		//DataStore.InitData() has already been called at this point to load data
		difficulty = Difficulty.NotSet;
		threatLevel = addtlThreat = 0;
		allyRules = AllyRules.Normal;
		optionalDeployment = YesNo.No;
		allyThreatCost = YesNo.No;
		selectedMissionExpansion = Expansion.Core;
		selectedMissionID = "core1";
		selectedMissionName = DataStore.missionCards["Core"][0].name;
		includeImperials = true;
		includeMercs = true;

		selectedDeploymentCards = new List<DeploymentCard>[5];
		for ( int i = 0; i < 5; i++ )
			selectedDeploymentCards[i] = new List<DeploymentCard>();
		selectedAlly = null;

		//ignore "Other" expansion enemy groups by default
		selectedDeploymentCards[3].AddRange( DataStore.deploymentCards.Where( x => x.expansion == "Other" ) );
		gameVars = new GameVars();
	}

	/// <summary>
	/// Only called when starting a NEW CLASSIC game
	/// </summary>
	public void InitGameVars()
	{
		gameVars.round = 1;
		gameVars.eventsTriggered = 0;
		gameVars.fame = 0;

		gameVars.currentThreat = 0;
		if ( allyThreatCost == YesNo.Yes && selectedAlly != null )
			gameVars.currentThreat += selectedAlly.cost;
		gameVars.currentThreat += addtlThreat;

		gameVars.deploymentModifier = 0;
		if ( difficulty == Difficulty.Hard )
			gameVars.deploymentModifier = 2;

		gameVars.pauseDeployment = false;
		gameVars.pauseThreatIncrease = false;
	}

	public string ToggleDifficulty()
	{
		if ( difficulty == Difficulty.NotSet )
			difficulty = Difficulty.Medium;
		else if ( difficulty == Difficulty.Easy )
			difficulty = Difficulty.Medium;
		else if ( difficulty == Difficulty.Medium )
			difficulty = Difficulty.Hard;
		else
			difficulty = Difficulty.Easy;

		if ( difficulty == Difficulty.Easy )
			return DataStore.uiLanguage.uiSetup.easy;
		else if ( difficulty == Difficulty.Medium )
			return DataStore.uiLanguage.uiSetup.normal;
		else
			return DataStore.uiLanguage.uiSetup.hard;
	}

	/// <summary>
	/// Deprecated
	/// </summary>
	public string ToggleRules()
	{
		if ( allyRules == AllyRules.NotSet )
			allyRules = AllyRules.Normal;
		else if ( allyRules == AllyRules.Normal )
			allyRules = AllyRules.Lothal;
		else
			allyRules = AllyRules.Normal;

		return allyRules == AllyRules.Normal ? DataStore.uiLanguage.uiSetup.normal : "lothal";
	}

	public string ToggleDeployment()
	{
		if ( optionalDeployment == YesNo.Yes )
			optionalDeployment = YesNo.No;
		else
			optionalDeployment = YesNo.Yes;

		return optionalDeployment == YesNo.Yes ? DataStore.uiLanguage.uiSetup.yes : DataStore.uiLanguage.uiSetup.no;
	}

	public string ToggleThreatCost()
	{
		if ( allyThreatCost == YesNo.Yes )
			allyThreatCost = YesNo.No;
		else
			allyThreatCost = YesNo.Yes;

		return allyThreatCost == YesNo.Yes ? DataStore.uiLanguage.uiSetup.yes : DataStore.uiLanguage.uiSetup.no;
	}

	public void ToggleImperials( bool isOn )
	{
		includeImperials = isOn;
	}

	public void ToggleMercs( bool isOn )
	{
		includeMercs = isOn;
	}

	public void ToggleHero( string id )
	{
		int idx = selectedDeploymentCards[4].FindIndex( x => x.id == id );
		selectedDeploymentCards[4].RemoveAt( idx );
	}

	/// <summary>
	/// Positive or negative number to add/decrease
	/// </summary>
	public void ModifyThreat( int amount, bool force = false )
	{
		//the only time ModifyThreat has "force=true" is when the user applies a custom amount of threat
		//in that case, do NOT apply the difficulty modifier - apply the direct amount requested
		if ( amount > 0 && !force )
		{
			//round to nearest whole number, with X.5 rounding UP
			if ( difficulty == Difficulty.Easy )
				amount = (int)Math.Round( amount * .7f, 0, MidpointRounding.AwayFromZero );
			else if ( difficulty == Difficulty.Hard )
				amount = (int)Math.Round( amount * 1.3f, 0, MidpointRounding.AwayFromZero );
		}
		//Debug.Log( "UpdateThreat() amount: " + amount );

		//only pause modification of threat when "amount" is POSITIVE
		//threat COSTS (negative) should ALWAYS modify (subtract) threat
		if ( amount > 0 && gameVars.pauseThreatIncrease && !force )
		{
			Debug.Log( "THREAT PAUSED" );
			return;
		}

		gameVars.currentThreat = Math.Max( 0, gameVars.currentThreat + amount );
		Debug.Log( "UpdateThreat(): current=" + gameVars.currentThreat );
	}

	public void UpdateDeploymentModifier( int amount )
	{
		gameVars.deploymentModifier += amount;
		Debug.Log( "Update DeploymentModifier: " + gameVars.deploymentModifier );
	}

	public void SetDeploymentModifier( int amount )
	{
		gameVars.deploymentModifier = amount;
		Debug.Log( "Set DeploymentModifier: " + gameVars.deploymentModifier );
	}

	public void SaveSession( string baseFolder )
	{
		string basePath = Path.Combine( Application.persistentDataPath, baseFolder );

		try
		{
			if ( !Directory.Exists( basePath ) )
				Directory.CreateDirectory( basePath );

			//save the session data
			gameVars.isNewGame = false;//mark this as a saved session
			string output = JsonConvert.SerializeObject( this, Formatting.Indented );
			string outpath = Path.Combine( basePath, "sessiondata.json" );
			using ( var stream = File.CreateText( outpath ) )
			{
				stream.Write( output );
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

			//remaining events (so same events aren't triggered again when loading a session
			outpath = Path.Combine( basePath, "events.json" );
			output = JsonConvert.SerializeObject( DataStore.cardEvents, Formatting.Indented );
			using ( var stream = File.CreateText( outpath ) )
			{
				stream.Write( output );
			}

			Debug.Log( "***SESSION SAVED (Classic)***" );
		}
		catch ( Exception e )
		{
			Debug.Log( "***ERROR*** SaveSession:: " + e.Message );
			DataStore.LogError( "SaveSession() TRACE:\r\n" + e.Message );
		}
	}

	public bool SaveDefaults()
	{
		string basePath = Path.Combine( Application.persistentDataPath, "Defaults" );

		try
		{
			if ( !Directory.Exists( basePath ) )
				Directory.CreateDirectory( basePath );

			//save the session data
			string output = JsonConvert.SerializeObject( this, Formatting.Indented );
			string outpath = Path.Combine( basePath, "sessiondata.json" );
			using ( var stream = File.CreateText( outpath ) )
			{
				stream.Write( output );
			}
			return true;
		}
		catch ( Exception e )
		{
			Debug.Log( "***ERROR*** SaveDefaults:: " + e.Message );
			DataStore.LogError( "SaveDefaults() TRACE:\r\n" + e.Message );
			return false;
		}
	}
}
