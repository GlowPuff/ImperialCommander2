using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Saga
{
	public class Vector
	{
		public float X;
		public float Y;
		public float Z;
		public Vector( float x = 0, float y = 0, float z = 0 )
		{
			X = x;
			Y = y;
			Z = z;
		}

		public static implicit operator Vector( string d )
		{
			string[] v = d.Split( new char[] { ',' } );
			Vector v1 = new Vector( float.Parse( v[0] ), float.Parse( v[1] ) );
			return v1;
		}
	}

	public struct CampaignModify
	{
		public MissionType missionType;
		public int threatValue;
		public bool agendaToggle;
		public string missionID;
		public string expansionCode;
		public string[] itemTierArray;
	}

	public class SagaGameVars
	{
		public int round;
		public int eventsTriggered;
		public int currentThreat;
		public int deploymentModifier;
		public int fame;
		public bool pauseDeployment;
		public bool pauseThreatIncrease;
		public bool isNewGame = true;
		public string currentMissionInfo;
		//temporary event conditions
		public bool isEndTurn = false;
		public bool isStartTurn = false;
		public string currentObjective;
		public DeploymentCard activatedGroup = null;
		//keep track of the end of current round events
		//keep track of events that have already fired (for use with certain TriggeredBy)
		//keep track of any enemy group data overrides (instructions, custom enemy deployment event action, etc)
		public Dictionary<Guid, int> endCurrentRoundEvents { get; } = new Dictionary<Guid, int>();
		public List<Guid> firedEvents { get; } = new List<Guid>();
		public List<DeploymentGroupOverride> dgOverrides = new List<DeploymentGroupOverride>();
		public DeploymentGroupOverride dgOverridesAll = null;
		public Dictionary<Guid, int> highlightLifeTimes = new Dictionary<Guid, int>();

		public SagaGameVars()
		{

		}

		public void AddEndCurrentRoundEvent( Guid guid )
		{
			if ( !endCurrentRoundEvents.ContainsKey( guid ) )
			{
				Debug.Log( "AddEndCurrentRoundEvent()::EVENT ADDED" );
				endCurrentRoundEvents.Add( guid, round );
				//return true;
			}
			else//update it
			{
				Debug.Log( "AddEndCurrentRoundEvent()::EVENT UPDATED" );
				endCurrentRoundEvents[guid] = round;
				//return false;
			}

			//Debug.Log( "AddEndCurrentRoundEvent()::END CURRENT ROUND EVENT ALREADY QUEUED" );
			//return false;
		}

		public bool ShouldFireEndCurrentRoundEvent( Guid guid )
		{
			if ( endCurrentRoundEvents.ContainsKey( guid ) )
				return endCurrentRoundEvents[guid] == round;
			else
				return false;
		}

		public void AddFiredEvent( Guid guid )
		{
			if ( !firedEvents.Contains( guid ) )
				firedEvents.Add( guid );
		}

		/// <summary>
		/// Returns the ALL DeploymentGroupOverride unless id is specified, null if it doesn't exist
		/// </summary>
		public DeploymentGroupOverride GetDeploymentOverride( string id = "" )
		{
			if ( !string.IsNullOrEmpty( id ) )
			{
				return dgOverrides.Where( x => x.ID == id ).FirstOr( null );
			}
			else if ( id == null )
				return null;
			else
				return dgOverridesAll;
		}

		/// <summary>
		/// Create and return a new override, otherwise return existing override
		/// </summary>
		public DeploymentGroupOverride CreateDeploymentOverride( string id = "" )
		{
			if ( string.IsNullOrEmpty( id ) )
			{
				return dgOverridesAll ?? (dgOverridesAll = new DeploymentGroupOverride( "" ));
			}
			else if ( !string.IsNullOrEmpty( id ) )
			{
				if ( dgOverrides.Any( x => x.ID == id ) )
					return dgOverrides.Where( x => x.ID == id ).First();
				else
				{
					var ovrd = new DeploymentGroupOverride( id );
					dgOverrides.Add( ovrd );
					return ovrd;
				}
			}
			return null;
		}

		public DeploymentGroupOverride CreateCustomDeploymentOverride( CustomEnemyDeployment ced )
		{
			if ( dgOverrides.Any( x => x.ID == ced.enemyGroupData.cardID ) )
				return dgOverrides.Where( x => x.ID == ced.enemyGroupData.cardID ).First();
			else
			{
				var ovrd = new DeploymentGroupOverride( ced.enemyGroupData.cardID );
				dgOverrides.Add( ovrd );
				ovrd.isCustom = true;
				ovrd.customType = ced.customType;
				//set name
				ovrd.nameOverride = ced.enemyGroupData.cardName;
				//set egd
				ovrd.SetEnemyDeploymentOverride( ced.enemyGroupData );
				//reposition instructions
				ovrd.repositionInstructions = ced.repositionInstructions;
				//set thumbnail
				ovrd.thumbnailGroupImperial = ced.thumbnailGroupImperial;
				ovrd.thumbnailGroupRebel = ced.thumbnailGroupRebel;
				//bonuses
				ovrd.customBonuses = ced.bonuses.Split( '\n' );
				//deployment
				ovrd.canReinforce = ced.canReinforce;
				ovrd.canRedeploy = ced.canRedeploy;
				ovrd.canBeDefeated = ced.canBeDefeated;
				ovrd.useResetOnRedeployment = ced.useResetOnRedeployment;

				return ovrd;
			}
		}

		public void RemoveOverride( string id )
		{
			if ( string.IsNullOrEmpty( id ) )
				return;
			int idx = dgOverrides.FindIndex( x => { return x.ID == id; } );
			if ( idx >= 0 )
			{
				Debug.Log( $"RemoveOverride()::{id}" );
				dgOverrides.RemoveAt( idx );
			}
		}

		public void RemoveAllOverrides()
		{
			dgOverridesAll = null;
			dgOverrides.Clear();
		}
	}

	public class SagaSetupOptions
	{
		/*
    difficulty
    adaptive difficulty
    hero selection
    ally selection
    initial threat level / additional threat
    earned villains
    ignored groups
*/
		public Difficulty difficulty;
		public bool useAdaptiveDifficulty;
		public int threatLevel;
		public int addtlThreat;
		public ProjectItem projectItem;
		public bool isTutorial;
		public bool isDebugging;//testing a mission from the command line should not save state
		public int tutorialIndex;

		public SagaSetupOptions()
		{
			Reset();
		}

		public void Reset()
		{
			projectItem = null;
			difficulty = Difficulty.Medium;
			useAdaptiveDifficulty = false;
			threatLevel = 3;
			addtlThreat = 0;
			isTutorial = false;
			isDebugging = false;
			tutorialIndex = 0;
		}

		public string ToggleDifficulty()
		{
			if ( difficulty == Difficulty.Easy )
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
	}

	//public class DeploymentColor
	//{
	//	public string name;
	//	public Color color;

	//	public DeploymentColor( string n, Color c )
	//	{
	//		name = n;
	//		color = c;
	//	}
	//}

	public class EntityModifier
	{
		public Guid sourceGUID;
		public bool hasColor;
		public bool hasProperties;
		public EntityProperties entityProperties;
	}

	public class ButtonAction
	{
		public string buttonText;
		public Guid triggerGUID;
		public Guid eventGUID;
	}

	public class DPData
	{
		public Guid GUID;
	}

	public class GroupAbility
	{
		public string name;
		public string text;
	}

	public class ProjectItem : IComparable<ProjectItem>
	{
		public string Title;
		public string Date;
		public string Description;
		public string AdditionalInfo;
		public string fileName;
		public string relativePath;
		public string fileVersion;
		public long timeTicks;
		public string missionID;
		public string missionGUID;
		public string fullPathWithFilename;
		public PickerMode pickerMode;

		public int CompareTo( ProjectItem other ) => timeTicks > other.timeTicks ? -1 : timeTicks < other.timeTicks ? 1 : 0;
	}

	public class EnemyGroupData
	{
		public Guid GUID;
		public string cardName;
		public string cardID;
		public CustomInstructionType customInstructionType;
		public string customText;
		public List<DPData> pointList = new List<DPData>();
		public GroupPriorityTraits groupPriorityTraits;
		public Guid defeatedTrigger;
		public Guid defeatedEvent;
		public bool useGenericMugshot;
		public bool useInitialGroupCustomName;

		public EnemyGroupData()
		{

		}

		//public EnemyGroupData( DeploymentCard dc, DeploymentPoint dp )
		//{
		//	GUID = Guid.NewGuid();
		//	cardName = dc.name;
		//	cardID = dc.id;
		//	customText = "";
		//	customInstructionType = CustomInstructionType.Replace;
		//	groupPriorityTraits = new GroupPriorityTraits();
		//	for ( int i = 0; i < dc.size; i++ )
		//	{
		//		pointList.Add( new DPData() { GUID = dp.GUID } );
		//	}
		//}

		//public void SetDP( Guid guid )
		//{
		//	int c = pointList.Count;
		//	pointList.Clear();
		//	for ( int i = 0; i < c; i++ )
		//	{
		//		pointList.Add( new DPData() { GUID = guid } );
		//	}
		//}
	}

	public class InputRange
	{
		public string theText;
		public int fromValue;
		public int toValue;
		public Guid triggerGUID;
		public Guid eventGUID;

		public InputRange()
		{

		}
	}

	public class TriggerManagerState
	{
		public List<TriggerState> triggerStateList = new List<TriggerState>();
		public List<EventGroup> eventGroupList = new List<EventGroup>();
	}

	public class EntityManagerState
	{
		[JsonConverter( typeof( MapEntityConverter ) )]
		public List<IMapEntity> mapEntities = new List<IMapEntity>();
	}

	public class TileManagerState
	{
		public List<MapSection> mapSections;
		public List<TileDescriptor> tileDescriptors;
	}
}