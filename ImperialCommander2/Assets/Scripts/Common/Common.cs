using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum FX { Click, CopyThat, Droid, Computer, Deploy, Drill, Trouble, Vader, SetBlasters, Restricted, DropWeapons, Notify, Lightspeed, None }
public enum Difficulty { NotSet, Easy, Medium, Hard }
public enum AllyRules { NotSet, Normal, Lothal }
public enum YesNo { NotSet, Yes, No }
public enum Faction { Imperial, Mercenary }
public enum Expansion { Core, Twin, Hoth, Bespin, Jabba, Empire, Lothal, Other }
public enum ChooserMode { DeploymentGroups, Missions, Hero, Ally, Villain }
public enum DeployMode { Calm, Reinforcements, Landing, Onslaught }
public enum SettingsCommand { Quit, ReturnTitles }
public enum HeroHealth { Healthy, Wounded, Defeated }
public enum NetworkStatus { Busy, UpToDate, Error, WrongVersion }
public enum DiceColor { White, Black, Yellow, Red, Green, Blue, Grey }
public enum AttackType { Ranged, Melee, None }
public enum FigureSize { Small1x1, Medium1x2, Large2x2, Huge2x3 }
public enum GroupTraits { Trooper, Leader, HeavyWeapon, Guardian, Brawler, Droid, Vehicle, Hunter, Creature, Smuggler, Spy, ForceUser, Wookiee, Hero }
public enum MissionType { Story, Side, Agenda, Threat, Forced, Other, Finale, General, Personal, Villain, Ally }
public enum GameType { Classic, Saga }

public class HealthState
{
	public List<int> enemySizes = new List<int>();
	public List<HeroHealth> heroStates = new List<HeroHealth>();
}

public class GitHubResponse
{
	public string tag_name;
	public string body;
}

public static class Extensions
{
	/// <summary>
	/// Match a card in a list by its ID
	/// </summary>
	public static bool ContainsCard( this List<DeploymentCard> thisCD, DeploymentCard comp )
	{
		return thisCD.Any( x => x.id == comp.id );
	}
	public static DeploymentCard GetDeploymentCard( this List<DeploymentCard> thisCD, string cardID )
	{
		return thisCD.Where( x => x.id == cardID.ToUpper() ).FirstOr( null );
	}
	public static List<DeploymentCard> Owned( this List<DeploymentCard> thisCD )
	{
		return thisCD.Where( x => DataStore.ownedExpansions.Contains( (Expansion)Enum.Parse( typeof( Expansion ), x.expansion ) ) ).ToList();
	}

	public static List<DeploymentCard> OwnedPlusOther( this List<DeploymentCard> thisCD )
	{
		return thisCD.Where( x => DataStore.ownedExpansions.Contains( (Expansion)Enum.Parse( typeof( Expansion ), x.expansion ) ) || x.expansion == "Other" ).ToList();
	}

	public static List<DeploymentCard> MinusEarnedVillains( this List<DeploymentCard> thisCD )
	{
		if ( DataStore.gameType == GameType.Classic )
			return thisCD.Where( x => !DataStore.sessionData.EarnedVillains.ContainsCard( x ) ).ToList();
		else
			return thisCD.Where( x => !DataStore.sagaSessionData.EarnedVillains.ContainsCard( x ) ).ToList();
	}

	public static List<DeploymentCard> MinusIgnored( this List<DeploymentCard> thisCD )
	{
		if ( DataStore.gameType == GameType.Classic )
			return thisCD.Where( x => !DataStore.sessionData.MissionIgnored.ContainsCard( x ) ).ToList();
		else
			return thisCD.Where( x => !DataStore.sagaSessionData.MissionIgnored.ContainsCard( x ) ).ToList();
	}

	public static List<DeploymentCard> MinusStarting( this List<DeploymentCard> thisCD )
	{
		if ( DataStore.gameType == GameType.Classic )
			return thisCD.Where( x => !DataStore.sessionData.MissionStarting.ContainsCard( x ) ).ToList();
		else
			return thisCD.Where( x => !DataStore.sagaSessionData.MissionStarting.ContainsCard( x ) ).ToList();
	}

	public static List<DeploymentCard> MinusReserved( this List<DeploymentCard> thisCD )
	{
		if ( DataStore.gameType == GameType.Classic )
			return thisCD.Where( x => !DataStore.sessionData.MissionReserved.ContainsCard( x ) ).ToList();
		else
			return thisCD.Where( x => !DataStore.sagaSessionData.MissionReserved.ContainsCard( x ) ).ToList();
	}

	public static List<DeploymentCard> MinusDeployed( this List<DeploymentCard> thisCD )
	{
		return thisCD.Where( x => !DataStore.deployedEnemies.ContainsCard( x ) ).ToList();
	}

	public static List<DeploymentCard> MinusInDeploymentHand( this List<DeploymentCard> thisCD )
	{
		return thisCD.Where( x => !DataStore.deploymentHand.ContainsCard( x ) ).ToList();
	}

	/// <summary>
	/// Filters and returns just the villains in the supplied list
	/// </summary>
	public static List<DeploymentCard> GetVillains( this List<DeploymentCard> thisCD )
	{
		return thisCD.Where( x => DataStore.villainCards.ContainsCard( x ) ).ToList();
	}

	public static List<DeploymentCard> FilterByFaction( this List<DeploymentCard> thisCD )
	{
		if ( DataStore.gameType == GameType.Classic )
		{
			if ( DataStore.sessionData.includeImperials && !DataStore.sessionData.includeMercs )
				return thisCD.Where( x => x.faction == "Imperial" ).ToList();
			else if ( DataStore.sessionData.includeMercs && !DataStore.sessionData.includeImperials )
				return thisCD.Where( x => x.faction == "Mercenary" ).ToList();
			else
				return thisCD.Where( x => x.faction == "Imperial" || x.faction == "Mercenary" ).ToList();
		}
		else
		{
			if ( DataStore.mission.missionProperties.factionImperial && !DataStore.mission.missionProperties.factionMercenary )
				return thisCD.Where( x => x.faction == "Imperial" ).ToList();
			else if ( DataStore.mission.missionProperties.factionMercenary && !DataStore.mission.missionProperties.factionImperial )
				return thisCD.Where( x => x.faction == "Mercenary" ).ToList();
			else
				return thisCD.Where( x => x.faction == "Imperial" || x.faction == "Mercenary" ).ToList();
		}
	}

	public static List<DeploymentCard> GetHeroesAndAllies( this List<DeploymentCard> thisCD )
	{
		return thisCD.Where( x => x.id[0] == 'H' || x.id[0] == 'A' ).ToList();
	}

	/// <summary>
	/// returns list of healthy heroes/allies
	/// </summary>
	public static List<DeploymentCard> GetHealthy( this List<DeploymentCard> thisCD )
	{
		if ( thisCD.Any( x => !x.isDummy && x.heroState.isHealthy ) )
			return thisCD.Where( x => !x.isDummy && x.heroState.isHealthy ).ToList();
		else
			return null;
	}

	public static List<DeploymentCard> GetUnhealthy( this List<DeploymentCard> thisCD )
	{
		if ( thisCD.Any( x => !x.isDummy && !x.heroState.isHealthy ) )
			return thisCD.Where( x => !x.isDummy && !x.heroState.isHealthy ).ToList();
		else
			return null;
	}

	/// <summary>
	/// Filters and returns a randomly selected group (if>1) that has any of the provided traits
	/// </summary>
	public static List<DeploymentCard> WithTraits( this List<DeploymentCard> thisCD, GroupTraits[] trait )
	{
		if ( trait.Length == 0 || thisCD is null )
			return null;
		var list = (from dc in thisCD from tr in trait where dc.groupTraits.ToList().Contains( tr ) select dc).ToList();
		if ( list.Count > 0 )
		{
			Debug.Log( "WithTraits()::MATCHING TRAITS FOUND" );
			return list;
		}
		else
		{
			Debug.Log( "WithTraits()::NO MATCHING TRAITS FOUND" );
			return null;
		}
	}
	public static List<DeploymentCard> MinusCannotRedeploy( this List<DeploymentCard> thisCD )
	{
		return thisCD.Where( x => !DataStore.sagaSessionData.CannotRedeployList.Contains( x.id ) ).ToList();
	}

	public static List<DeploymentCard> MinusElite( this List<DeploymentCard> thisCD )
	{
		return thisCD.Where( x => !x.isElite ).ToList();
	}

	public static List<DeploymentCard> OnlyElite( this List<DeploymentCard> thisCD )
	{
		return thisCD.Where( x => x.isElite ).ToList();
	}

	public static T FirstOr<T>( this IEnumerable<T> thisEnum, T def )
	{
		foreach ( var item in thisEnum )
			return item;
		return def;
	}

	public static Color ToColor( this Vector3 c )
	{
		return new Color( c.x, c.y, c.z, 1 );
	}

	public static Color ToColor( this Vector4 c )
	{
		return new Color( c.x, c.y, c.z, c.w );
	}

	public static Vector3 ToUnityV3( this Saga.Vector v )
	{
		return new Vector3( v.X, v.Y, v.Z );
	}

	public static Saga.Vector ToSagaVector( this Vector3 v )
	{
		return new Saga.Vector( v.x, v.y, v.z );
	}
}