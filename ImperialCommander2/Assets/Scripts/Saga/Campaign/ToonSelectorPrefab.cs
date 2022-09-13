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
			mugImage.sprite = Resources.Load<Sprite>( $"Cards/Villains/{c.id.Replace( "DG", "M" )}" );
		}

		public void InitHero( DeploymentCard c )
		{
			card = c;
			toonType = 1;
			nameText.text = c.name;
			mugImage.sprite = Resources.Load<Sprite>( $"Cards/Heroes/{c.id}" );
		}

		public void InitAlly( DeploymentCard c )
		{
			card = c;
			toonType = 2;
			nameText.text = card.name;
			mugImage.sprite = Resources.Load<Sprite>( $"Cards/Allies/{card.id.Replace( "A", "M" )}" );
		}

		public void OnAdd()
		{
			if ( toonType == 0 )
				FindObjectOfType<AddItemHeroAllyVillainPopup>().OnAddVillain( card );
			else if ( toonType == 1 )
				FindObjectOfType<AddItemHeroAllyVillainPopup>().OnAddHero( card );
			else if ( toonType == 2 )
				FindObjectOfType<AddItemHeroAllyVillainPopup>().OnAddAlly( card );
		}
	}
}
