using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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

	//public struct RedButtonColor
	//{
	//	public Color normalColor { get { return new Color( 1, 40f / 255f, 0 ); } }
	//	public Color highlightedColor { get { return new Color( 1, 142f / 255f, 0 ); } }
	//	public Color pressedColor { get { return new Color( 135f / 255f, 21f / 255f, 0 ); } }
	//	public Color selectedColor { get { return new Color( 1, 40f / 255f, 0 ); } }
	//	public Color disabledColor { get { return new Color( 110f / 255f, 18f / 255f, 0 ); } }
	//}

	//public struct GreenButtonColor
	//{
	//	public Color normalColor { get { return new Color( 0, 1f, 160f / 255f ); } }
	//	public Color highlightedColor { get { return new Color( 0, 1f, 0 ); } }
	//	public Color pressedColor { get { return new Color( 0, 135f / 255f, 85f / 255f ); } }
	//	public Color selectedColor { get { return new Color( 0, 1f, 160f / 255f ); } }
	//	public Color disabledColor { get { return new Color( 0, 115f / 255f, 72f / 255f ); } }
	//}

	public struct CampaignModify
	{
		public MissionType missionType;
		public int threatValue;
		public bool agendaToggle;
		public string missionID;
		public string expansionCode;
		public string[] itemTierArray;
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

	//public class SagaDeploymentCard
	//{
	//	public DeploymentCard deploymentCard;
	//	public EnemyGroupData enemyGroupData;
	//}

	//public class CardLanguage
	//{
	//	public string id;
	//	public string name;
	//	public string subname;
	//	public string ignored;
	//	public string[] traits;
	//	public string[] surges;
	//	public string[] keywords;
	//	public GroupAbility[] abilities;
	//}
}