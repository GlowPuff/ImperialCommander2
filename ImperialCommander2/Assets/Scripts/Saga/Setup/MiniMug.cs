using UnityEngine;
using UnityEngine.UI;

namespace Saga
{
	/// <summary>
	/// Mugshot toggle button for heroes
	/// </summary>
	public class MiniMug : MonoBehaviour
	{
		[HideInInspector]
		public DeploymentCard card;
		public Image mugImage;

		public void Init( DeploymentCard cd )
		{
			card = cd;
			mugImage.sprite = Resources.Load<Sprite>( cd.mugShotPath );
		}

		public void Click()
		{
			FindObjectOfType<SagaSetup>().RemoveHero( card );
		}
	}
}
