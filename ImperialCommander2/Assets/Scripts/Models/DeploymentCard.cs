using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;

public class DeploymentCards
{
	public List<CardDescriptor> cards = new List<CardDescriptor>();
}

public class CardDescriptor : IEquatable<CardDescriptor>
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

	//start v.1.0.21 additions
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
	//==end v.1.0.21 additions
	//==


	//==upkeep properties
	public int currentSize;
	public int colorIndex;
	//start v.1.0.17 additions
	[DefaultValue( false )]
	[JsonProperty( DefaultValueHandling = DefaultValueHandling.Populate )]
	public bool hasActivated;
	public string bonusName, bonusText, rebelName;
	public InstructionOption instructionOption;

	//start v.1.0.20 additions
	public bool isDummy;
	public HeroState heroState;
	//==end v.1.0.20 additions
	//==end upkeep

	public bool Equals( CardDescriptor obj )
	{
		if ( obj == null )
			return false;
		CardDescriptor objAsPart = obj as CardDescriptor;
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