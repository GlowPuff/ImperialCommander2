using System;
using System.Collections.Generic;
using System.Linq;
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

	public struct CampaignModify
	{
		public MissionType missionType;
		public int threatValue;
		public bool agendaToggle;
		public string missionID;
		public string expansionCode;
		public string[] itemTierArray;
	}

	public class EntityModifier
	{
		public Guid GUID;
		public Guid sourceGUID;
		public bool hasColor;
		public bool hasProperties;
		public EntityProperties entityProperties;
	}

	public class ButtonAction
	{
		public Guid GUID;
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
		public string stringifiedMission;
		public bool hasTranslation;
		public string expansion;

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
		public Guid GUID;
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

	public class ThumbnailData
	{
		public List<Thumbnail> Other, Rebel, Imperial, Mercenary, StockImperial, StockAlly, StockHero, StockVillain;
		List<Thumbnail> None = new List<Thumbnail>( new Thumbnail[] { new Thumbnail() { Name = "Select a Thumbnail", ID = "None" } } );
		public Thumbnail NoneThumb => None[0];

		public ThumbnailData()
		{
			Other = new List<Thumbnail>();
			Rebel = new List<Thumbnail>();
			Imperial = new List<Thumbnail>();
			Mercenary = new List<Thumbnail>();
			StockImperial = new List<Thumbnail>();
			StockAlly = new List<Thumbnail>();
			StockHero = new List<Thumbnail>();
			StockVillain = new List<Thumbnail>();
		}

		public List<Thumbnail> Filter( ThumbType ttype )
		{
			switch ( ttype )
			{
				case ThumbType.All:
					return None.Concat( Other ).Concat( Rebel ).Concat( Imperial ).Concat( Mercenary ).Concat( StockImperial ).Concat( StockAlly ).Concat( StockHero ).Concat( StockVillain ).ToList();
				case ThumbType.Other:
					return None.Concat( Other ).ToList();
				case ThumbType.Rebel:
					return None.Concat( Rebel ).ToList();
				case ThumbType.Imperial:
					return None.Concat( Imperial ).ToList();
				case ThumbType.Mercenary:
					return None.Concat( Mercenary ).ToList();
				case ThumbType.StockImperial:
					return None.Concat( StockImperial ).ToList();
				case ThumbType.StockAlly:
					return None.Concat( StockAlly ).ToList();
				case ThumbType.StockHero:
					return None.Concat( StockHero ).ToList();
				case ThumbType.StockVillain:
					return None.Concat( StockVillain ).ToList();
				default:
					return Other;
			}
		}
	}

	public class Thumbnail
	{
		public string Name { get; set; }//full name of icon's character
		public string ID { get; set; }//basically the filename
	}
}