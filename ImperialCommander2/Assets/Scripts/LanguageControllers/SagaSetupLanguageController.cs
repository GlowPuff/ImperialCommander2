using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SagaSetupLanguageController : MonoBehaviour
{
	public Text difficultyBtn, adaptiveBtn, initialThreatText, addtlThreatText, groupsText, ignoredBtn, villainsBtn, addAllyText, heroesText, tilesBtn, heroCloseBtn, groupCloseBtn, tilesCloseBtn, setupCancelBtn, setupStartBtn, officialCustomBtn, missionCardBtn, campaignTilesButton;

	public TextMeshProUGUI missionTitle;

	public void SetTranslatedUI()
	{
		SagaUISetup ui = DataStore.uiLanguage.sagaUISetup;
		groupsText.text = ui.groupsText.ToUpper();
		villainsBtn.text = ui.villainsBtn.ToUpper();
		tilesBtn.text = ui.tilesBtn.ToUpper();
		campaignTilesButton.text = ui.tilesBtn;
		setupStartBtn.text = ui.setupStartBtn.ToUpper();
		officialCustomBtn.text = ui.officialBtn.ToUpper();
		missionCardBtn.text = ui.missionCardBtn.ToUpper();

		UISetup setup = DataStore.uiLanguage.uiSetup;
		difficultyBtn.text = setup.difficulty.ToUpper();
		adaptiveBtn.text = setup.adaptive.ToUpper();
		initialThreatText.text = setup.threatLevelHeading.ToUpper();
		addtlThreatText.text = setup.addtlThreatHeading.ToUpper();
		ignoredBtn.text = setup.ignoredHeading.ToUpper();
		addAllyText.text = setup.addAlly.ToUpper();
		heroesText.text = setup.addHero.ToUpper();
		setupCancelBtn.text = setup.cancel.ToUpper();

		UIMainApp uiMain = DataStore.uiLanguage.uiMainApp;
		heroCloseBtn.text = uiMain.close.ToUpper();
		groupCloseBtn.text = uiMain.close.ToUpper();
		tilesCloseBtn.text = uiMain.close.ToUpper();
	}
}

