using System;

namespace Saga
{
	public class AllyDeployment : EventAction
	{
		public string allyName;
		public string allyID;
		public Guid setTrigger;
		public DeploymentSpot deploymentPoint;
		public Guid specificDeploymentPoint;
		public int threatCost;
		public bool useThreat;

		public AllyDeployment()
		{

		}
	}
}
