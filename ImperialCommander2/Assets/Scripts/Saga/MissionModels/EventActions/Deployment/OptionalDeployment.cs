using System;

namespace Saga
{
	public class OptionalDeployment : EventAction
	{
		public DeploymentSpot deploymentPoint;
		public int threatCost;
		public bool useThreat;
		public Guid specificDeploymentPoint;
		public bool isOnslaught;

		public OptionalDeployment()
		{

		}
	}
}
