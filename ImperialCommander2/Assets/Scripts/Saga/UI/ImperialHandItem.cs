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

			if ( card.isElite )
				mugOutline.color = new Color( 1, 40f / 255f, 0 );

			mugshot.sprite = Resources.Load<Sprite>( card.mugShotPath );
		}
	}
}
