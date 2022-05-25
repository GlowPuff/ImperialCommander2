using System;

namespace Saga
{
	public class RandomDeployment : EventAction
	{
		public ThreatModifierType threatType;
		public int fixedValue;
		public int threatLevel;
		public DeploymentSpot deploymentPoint;
		public Guid specificDeploymentPoint;

		public RandomDeployment()
		{

		}
	}
}
