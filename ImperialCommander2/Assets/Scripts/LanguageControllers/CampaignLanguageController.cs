using TMPro;
using UnityEngine;

public class CampaignLanguageController : MonoBehaviour
{
	public TextMeshProUGUI campaignNameUC, creditsUC, fameUC, awardsUC, itemsUC, rewardsUC, villainsUC, alliesUC, xpUC, missionTypeMockupUC, missionNameMockupUC, itemMockupUC;

	public void SetTranslatedUI()
	{
		UICampaign ui = DataStore.uiLanguage.uiCampaign;
		ui.BuildMissionTypeStrings();

		campaignNameUC.text = ui.campaignNameUC;
		creditsUC.text = ui.creditsUC + ":";
		awardsUC.text = ui.awardsUC + ":";
		fameUC.text = ui.fameUC + ":";
		itemsUC.text = ui.itemsUC;
		rewardsUC.text = ui.rewardsUC;
		villainsUC.text = ui.villainsUC;
		alliesUC.text = ui.alliesUC;
		xpUC.text = ui.xpUC + ":";
		missionTypeMockupUC.text = ui.missionTypeMockupUC;
		missionNameMockupUC.text = ui.missionNameMockupUC;
		itemMockupUC.text = ui.itemMockupUC;
	}
}
