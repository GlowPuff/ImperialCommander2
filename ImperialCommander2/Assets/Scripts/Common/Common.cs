using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum FX { Click, CopyThat, Droid, Computer, Deploy, Drill, Trouble, Vader, SetBlasters, Restricted, DropWeapons, None }
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
	public static List<CardDescriptor> Owned( this List<CardDescriptor> thisCD )
	{
		return thisCD.Where( x => DataStore.ownedExpansions.Contains( (Expansion)Enum.Parse( typeof( Expansion ), x.expansion ) ) ).ToList();
	}

	public static List<CardDescriptor> OwnedPlusOther( this List<CardDescriptor> thisCD )
	{
		return thisCD.Where( x => DataStore.ownedExpansions.Contains( (Expansion)Enum.Parse( typeof( Expansion ), x.expansion ) ) || x.expansion == "Other" ).ToList();
	}

	public static List<CardDescriptor> MinusEarnedVillains( this List<CardDescriptor> thisCD )
	{
		return thisCD.Where( x => !DataStore.sessionData.EarnedVillains.Contains( x ) ).ToList();
	}

	public static List<CardDescriptor> MinusIgnored( this List<CardDescriptor> thisCD )
	{
		return thisCD.Where( x => !DataStore.sessionData.MissionIgnored.Contains( x ) ).ToList();
	}

	public static List<CardDescriptor> MinusStarting( this List<CardDescriptor> thisCD )
	{
		return thisCD.Where( x => !DataStore.sessionData.MissionStarting.Contains( x ) ).ToList();
	}

	public static List<CardDescriptor> MinusReserved( this List<CardDescriptor> thisCD )
	{
		return thisCD.Where( x => !DataStore.sessionData.MissionReserved.Contains( x ) ).ToList();
	}

	public static List<CardDescriptor> MinusDeployed( this List<CardDescriptor> thisCD )
	{
		return thisCD.Where( x => !DataStore.deployedEnemies.Contains( x ) ).ToList();
	}

	public static List<CardDescriptor> MinusInDeploymentHand( this List<CardDescriptor> thisCD )
	{
		return thisCD.Where( x => !DataStore.deploymentHand.Contains( x ) ).ToList();
	}

	/// <summary>
	/// Filters and returns just the villains in the supplied list
	/// </summary>
	public static List<CardDescriptor> GetVillains( this List<CardDescriptor> thisCD )
	{
		return thisCD.Where( x => DataStore.villainCards.cards.Contains( x ) ).ToList();
	}

	public static List<CardDescriptor> FilterByFaction( this List<CardDescriptor> thisCD )
	{
		if ( DataStore.sessionData.includeImperials && !DataStore.sessionData.includeMercs )
			return thisCD.Where( x => x.faction == "Imperial" ).ToList();
		else if ( DataStore.sessionData.includeMercs && !DataStore.sessionData.includeImperials )
			return thisCD.Where( x => x.faction == "Mercenary" ).ToList();
		else
			return thisCD.Where( x => x.faction == "Imperial" || x.faction == "Mercenary" ).ToList();
	}

	public static List<CardDescriptor> GetHeroesAndAllies( this List<CardDescriptor> thisCD )
	{
		return thisCD.Where( x => x.id[0] == 'H' || x.id[0] == 'A' ).ToList();
	}

	/// <summary>
	/// returns list of healthy heroes/allies
	/// </summary>
	public static List<CardDescriptor> GetHealthy( this List<CardDescriptor> thisCD )
	{
		if ( thisCD.Any( x => !x.isDummy && x.heroState.isHealthy ) )
			return thisCD.Where( x => !x.isDummy && x.heroState.isHealthy ).ToList();
		else
			return null;
	}

	public static List<CardDescriptor> GetUnhealthy( this List<CardDescriptor> thisCD )
	{
		if ( thisCD.Any( x => !x.isDummy && !x.heroState.isHealthy ) )
			return thisCD.Where( x => !x.isDummy && !x.heroState.isHealthy ).ToList();
		else
			return null;
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
}
