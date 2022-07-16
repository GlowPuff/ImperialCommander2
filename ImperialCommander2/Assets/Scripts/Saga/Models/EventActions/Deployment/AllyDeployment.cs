﻿using System;

namespace Saga
{
	public class AllyDeployment : EventAction
	{
		public string allyName;
		public string allyID;
		public Guid setTrigger;
		public Guid setEvent;
		public DeploymentSpot deploymentPoint;
		public Guid specificDeploymentPoint;
		public int threatCost;
		public bool useThreat;
		public bool useGenericMugshot;

		public AllyDeployment()
		{

		}
	}
}
