using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Saga
{
	public class Mission
	{
		public string languageID;
		public MissionProperties missionProperties;
		public Guid missionGUID;
		/// <summary>
		/// JUST the filename+extension
		/// </summary>
		public string fileName;
		/// <summary>
		/// folder path+filename RELATIVE to SpecialFolder.MyDocuments
		/// </summary>
		public string relativePath;

		/// <summary>
		///	increment this each time file format gets updated
		/// </summary>
		public string fileVersion;

		public string saveDate;
		/// <summary>
		/// File save time so recent list can sort by recent first
		/// </summary>
		public long timeTicks;

		public List<MapSection> mapSections;
		public List<Trigger> globalTriggers;
		public List<MissionEvent> globalEvents;

		[JsonConverter( typeof( MapEntityConverter ) )]
		public List<IMapEntity> mapEntities;
		public List<EnemyGroupData> initialDeploymentGroups;
		public List<EnemyGroupData> reservedDeploymentGroups;
		public List<EventGroup> eventGroups;
		public List<EntityGroup> entityGroups;

		public bool TriggerExists( Trigger t )
		{
			bool g = globalTriggers.Any( x => x.GUID == t.GUID );
			bool m = mapSections.Any( x => x.triggers.Any( xt => xt.GUID == t.GUID ) );
			return g || m;
		}

		public bool TriggerExists( Guid guid )
		{
			bool g = globalTriggers.Any( x => x.GUID == guid );
			bool m = mapSections.Any( x => x.triggers.Any( xt => xt.GUID == guid ) );
			return g || m;
		}

		public bool EventExists( Guid guid )
		{
			bool g = globalEvents.Any( x => x.GUID == guid );
			bool m = mapSections.Any( x => x.missionEvents.Any( xt => xt.GUID == guid ) );
			return g || m;
		}

		public Trigger GetTriggerFromGUID( Guid guid )
		{
			if ( globalTriggers.Any( x => x.GUID == guid ) )
				return globalTriggers.First( x => x.GUID == guid );
			else if ( mapSections.Any( x => x.triggers.Any( xt => xt.GUID == guid ) ) )
				return mapSections.First( x => x.triggers.Any( xt => xt.GUID == guid ) ).triggers.First( x => x.GUID == guid );
			else
				return null;
		}

		/// <summary>
		/// Seed is CASE INSENSITIVE
		/// </summary>
		public Trigger GetTriggerFromName( string n )
		{
			if ( globalTriggers.Any( x => x.name.ToLower() == n.ToLower() ) )
				return globalTriggers.First( x => x.name.ToLower() == n.ToLower() );
			else if ( mapSections.Any( x => x.triggers.Any( xt => xt.name.ToLower() == n.ToLower() ) ) )
				return mapSections.First( x => x.triggers.Any( xt => xt.name.ToLower() == n.ToLower() ) ).triggers.First( x => x.name.ToLower() == n.ToLower() );
			else
				return null;
		}

		public MissionEvent GetEventFromGUID( Guid guid )
		{
			if ( globalEvents.Any( x => x.GUID == guid ) )
				return globalEvents.First( x => x.GUID == guid );
			else if ( mapSections.Any( x => x.missionEvents.Any( xt => xt.GUID == guid ) ) )
				return mapSections.First( x => x.missionEvents.Any( xt => xt.GUID == guid ) ).missionEvents.First( x => x.GUID == guid );
			else
				return null;
		}

		public bool EntityExists( Guid guid )
		{
			if ( guid == Guid.Empty )
				return true;
			return mapEntities.Any( x => x.GUID == guid );
		}

		public IMapEntity GetEntityFromGUID( Guid guid )
		{
			return mapEntities.Where( x => x.GUID == guid ).FirstOr( null );
		}
	}
}
