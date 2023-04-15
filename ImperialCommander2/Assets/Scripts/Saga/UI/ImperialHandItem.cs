using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga
{
	public class ImperialHandItem : MonoBehaviour
	{
		public TextMeshProUGUI nameText;
		public Image mugOutline, mugshot;

		public void Init( DeploymentCard card )
		{
			nameText.text = card.name;

			var ovrd = DataStore.sagaSessionData.gameVars.GetDeploymentOverride( card.id );
			if ( ovrd != null )
				nameText.text = ovrd.nameOverride;

			mugOutline.color = Utils.String2UnityColor( card.deploymentOutlineColor );

			mugshot.sprite = Resources.Load<Sprite>( card.mugShotPath );
		}
	}
}
