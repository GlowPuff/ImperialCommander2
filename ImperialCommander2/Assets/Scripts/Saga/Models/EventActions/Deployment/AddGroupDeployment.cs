using System.Collections.Generic;

namespace Saga
{
	public class AddGroupDeployment : EventAction
	{
		public List<DeploymentCard> groupsToAdd { get; set; } = new List<DeploymentCard>();

		public AddGroupDeployment()
		{

		}
	}
}
