using System;
using Newtonsoft.Json;

public class DeploymentCard : IEquatable<DeploymentCard>
{
	//== data from JSON
	[JsonIgnore]
	public string name;
	public string id;
	public int tier;
	public string faction;
	public int priority;
	public int cost;
	public int rcost;
	public int size;
	public int fame;
	public int reimb;
	public string expansion;
	[JsonIgnore]
	public string ignored;
	public bool isElite;
	public bool isHero;
	[JsonIgnore]
	public string subname;
	public int health;
	public int speed;
	[JsonIgnore]
	public string[] traits;
	[JsonIgnore]
	public string[] surges;
	[JsonIgnore]
	public string[] keywords;
	[JsonIgnore]
	public GroupAbility[] abilities;
	public DiceColor[] defense;
	public DiceColor[] attacks;
	public AttackType attackType;
	public FigureSize miniSize;
	public GroupTraits[] groupTraits;
	public GroupTraits[] preferredTargets;
	//==

	//==upkeep properties
	public int currentSize;
	public int colorIndex;
	//[DefaultValue( false )]
	//[JsonProperty( DefaultValueHandling = DefaultValueHandling.Populate )]
	public bool hasActivated = false;
	public string bonusName, bonusText, rebelName;
	public InstructionOption instructionOption;
	public bool isDummy;
	public HeroState heroState;
	public bool hasDeployed = false;
	//==end upkeep

	//public Saga.EnemyGroupData enemyGroupData { get; set; }

	public bool Equals( DeploymentCard obj )
	{
		if ( obj == null )
			return false;
		DeploymentCard objAsPart = obj as DeploymentCard;
		if ( objAsPart == null )
			return false;
		else
			return id == objAsPart.id;
	}
}

public class GroupAbility
{
	public string name;
	public string text;
}

public class CardLanguage
{
	public string id;
	public string name;
	public string subname;
	public string ignored;
	public string[] traits;
	public string[] surges;
	public string[] keywords;
	public GroupAbility[] abilities;
}

public class DCPointer
{
	public string name;
	public string id;
}
