using System.Collections.Generic;

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
public enum SessionMode { Classic, Saga, Campaign }
public enum GameRoundTest { Current, PlusOne }

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