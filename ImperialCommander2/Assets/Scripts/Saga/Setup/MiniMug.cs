using UnityEngine;

namespace Saga
{
	public class MiniMug : MonoBehaviour
	{
		[HideInInspector]
		public DeploymentCard card;

		public void Click()
		{
			FindObjectOfType<SagaSetup>().RemoveHero( card );
		}
	}
}
