using System;

namespace Saga
{
	public class Terminal : IMapEntity
	{
		public Guid GUID { get; set; }
		public string name { get; set; }
		public EntityType entityType { get; set; }
		public Vector entityPosition { get; set; }
		public float entityRotation { get; set; }
		public bool hasProperties { get; }
		public bool hasColor { get; }
		public EntityProperties entityProperties { get; set; }
		public Guid mapSectionOwner { get; set; }

		//console props
		public string deploymentColor
		{
			get { return entityProperties.entityColor; }
			set
			{
				entityProperties.entityColor = value;
			}
		}
	}
}