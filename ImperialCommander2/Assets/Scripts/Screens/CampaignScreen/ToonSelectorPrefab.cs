using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga
{
	public class ToonSelectorPrefab : MonoBehaviour
	{
		public TextMeshProUGUI nameText;
		public Image mugImage;

		DeploymentCard card;
		int toonType;//0=villain, 1=hero, 2=ally

		public void InitVillain( DeploymentCard c )
		{
			card = c;
			toonType = 0;
			nameText.text = card.name;
			mugImage.sprite = Resources.Load<Sprite>( c.mugShotPath );
		}

		public void InitHero( DeploymentCard c )
		{
			card = c;
			toonType = 1;
			nameText.text = c.name;
			mugImage.sprite = Resources.Load<Sprite>( c.mugShotPath );
		}

		public void InitAlly( DeploymentCard c )
		{
			card = c;
			toonType = 2;
			nameText.text = card.name;
			mugImage.sprite = Resources.Load<Sprite>( c.mugShotPath );
		}

		public void OnAdd()
		{
			if ( toonType == 0 )
				FindObjectOfType<AddCampaignItemPopup>().OnAddVillain( card );
			else if ( toonType == 1 )
				FindObjectOfType<AddCampaignItemPopup>().OnAddHero( card );
			else if ( toonType == 2 )
				FindObjectOfType<AddCampaignItemPopup>().OnAddAlly( card );
		}
	}
}
