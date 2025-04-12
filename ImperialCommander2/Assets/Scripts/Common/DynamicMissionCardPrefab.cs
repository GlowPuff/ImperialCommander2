using System;
using System.Linq;
using Saga;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DynamicMissionCardPrefab : MonoBehaviour
{
	public TextMeshProUGUI descriptionText, titleTagsText, rewardText, heroVillainText, typePageExpansionText, dateText;
	public GameObject heroBox, dateBox, rewardBox;
	public Image expansionImage, cardImage, mugshot;
	public Sprite[] expansionSprites;

	private MissionCard missionCard;

	//dynamic card background color = 007CC1

	public void InitCard( MissionCard card )
	{
		missionCard = card;

		Func<string[], string, string> parse = ( string[] toParse, string sep ) =>
		 {
			 string t = "";
			 for ( int i = 0; i < toParse.Length; i++ )
			 {
				 t += toParse[i];
				 if ( i < toParse.Length - 1 )
					 t += $"{sep}";
			 }
			 return t;
		 };

		//card color
		if ( missionCard.missionType.Any( x => x == MissionType.Finale ) )
			cardImage.color = Color.yellow;
		else if ( missionCard.missionType.Any( x => x == MissionType.Story ) )
			cardImage.color = new Color( 0, 164f / 255f, 1 );
		else if ( missionCard.missionType.Any( x => x == MissionType.Personal ) )
			cardImage.color = Color.red;
		else if ( missionCard.missionType.Any( x => x == MissionType.Ally ) )
			cardImage.color = Color.green;
		else if ( missionCard.missionType.Any( x => x == MissionType.Agenda ) )
			cardImage.color = new Color( 0, 82f / 255f, 128f / 255f );
		else if ( missionCard.missionType.Any( x => x == MissionType.Threat ) )
			cardImage.color = new Color( 1, 142f / 255f, 0 );
		else
			cardImage.color = Color.gray;

		//description + bonus text
		if ( missionCard.expansion == Expansion.Other && FileManager.importedCampaigns.FirstOrDefault( x => x.campaignName == missionCard.expansionText ) != null )
			descriptionText.text = missionCard.descriptionText;
		else
			descriptionText.text = missionCard.descriptionText.Replace( "<i>", "" ).Replace( "</i>", "" ).Replace( "\n", "\n\n" );
		descriptionText.text += $"\n\n<color=orange>{missionCard.bonusText}</color>";

		//tags
		titleTagsText.text = $"{missionCard.name}\n<size=20><color=orange>{parse( missionCard.tagsText, " - " )}";

		//reward
		rewardText.text = $"{DataStore.uiLanguage.uiMainApp.rewardUC}: " + missionCard.rebelRewardText + missionCard.imperialRewardText;
		rewardBox.SetActive( !string.IsNullOrEmpty( missionCard.rebelRewardText ) || !string.IsNullOrEmpty( missionCard.imperialRewardText ) );

		//hero/villain name
		heroVillainText.text = missionCard.heroText + missionCard.villainText + missionCard.allyText;
		heroBox.SetActive( !string.IsNullOrEmpty( heroVillainText.text ) );
		if ( !string.IsNullOrEmpty( missionCard.heroText ) || !string.IsNullOrEmpty( missionCard.allyText ) )
			heroBox.GetComponent<Image>().color = new Color( 0, 1, 160f / 255f );
		else
			heroBox.GetComponent<Image>().color = new Color( 1, 40f / 255f, 0 );
		//hero icon?
		if ( !string.IsNullOrEmpty( missionCard.heroText ) )
			mugshot.sprite = Resources.Load<Sprite>( $"CardThumbnails/StockHero{missionCard.hero.GetDigits()}" );
		//villain icon?
		else if ( missionCard.villain.Length > 0 )
			mugshot.sprite = Resources.Load<Sprite>( $"CardThumbnails/StockVillain{missionCard.villain[0].GetDigits()}" );
		//ally icon?
		else if ( missionCard.ally.Length > 0 )
			mugshot.sprite = Resources.Load<Sprite>( $"CardThumbnails/StockAlly{missionCard.ally[0].GetDigits()}" );

		//page # and expansion
		string page = missionCard.page > 0 ? $"{DataStore.uiLanguage.uiMainApp.pageUC} {missionCard.page}, " : "";
		typePageExpansionText.text = $"{page}{missionCard.expansionText}";

		//timeline
		dateBox.SetActive( missionCard.timePeriod.Length > 0 );
		if ( missionCard.timePeriod.Length > 0 )
			dateText.text = $"Time Period: {missionCard.timePeriod[0]}-{missionCard.timePeriod[1]}";

		//expansion icon
		expansionImage.sprite = expansionSprites[(int)missionCard.expansion];
		if ( missionCard.expansion == Expansion.Other && !missionCard.missionType.Contains( MissionType.Agenda ) )
			expansionImage.sprite = expansionSprites[8];
		if ( missionCard.expansion == Expansion.Other && FileManager.importedCampaigns.FirstOrDefault( x => x.campaignName == missionCard.expansionText ) != null )
		{
			Texture2D tex = new Texture2D( 2, 2 );
			if ( tex.LoadImage( FileManager.importedCampaigns.FirstOrDefault( x => x.campaignName == missionCard.expansionText ).iconBytesBuffer ) )
			{
				Sprite iconSprite = Sprite.Create( tex, new Rect( 0, 0, tex.width, tex.height ), new Vector2( 0, 0 ), 100f );
				expansionImage.sprite = iconSprite;
			}
		}
	}
}
