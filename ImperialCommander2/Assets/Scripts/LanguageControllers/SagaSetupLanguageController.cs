using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SagaSetupLanguageController : MonoBehaviour
{
	public Text difficultyBtn, adaptiveBtn, initialThreatText, addtlThreatText, groupsText, ignoredBtn, villainsBtn, addAllyText, heroesText, tilesBtn, heroCloseBtn, groupCloseBtn, tilesCloseBtn, setupCancelBtn, setupStartBtn, officialCustomBtn, missionCardBtn, campaignTilesButton, importBtn;

	public TextMeshProUGUI missionTitle;

	public void SetTranslatedUI()
	{
		SagaUISetup ui = DataStore.uiLanguage.sagaUISetup;
		groupsText.text = ui.groupsText;
		villainsBtn.text = ui.villainsBtn;
		tilesBtn.text = ui.tilesBtn;
		campaignTilesButton.text = ui.tilesBtn;
		setupStartBtn.text = ui.setupStartBtn;
		officialCustomBtn.text = ui.officialBtn;
		missionCardBtn.text = ui.missionCardBtn;
		importBtn.text = ui.importBtn;

		UISetup setup = DataStore.uiLanguage.uiSetup;
		difficultyBtn.text = setup.difficulty;
		adaptiveBtn.text = setup.adaptive;
		initialThreatText.text = setup.threatLevelHeading;
		addtlThreatText.text = setup.addtlThreatHeading;
		ignoredBtn.text = setup.ignoredHeading;
		addAllyText.text = setup.addAlly;
		heroesText.text = setup.addHero;
		setupCancelBtn.text = setup.cancel;

		UIMainApp uiMain = DataStore.uiLanguage.uiMainApp;
		heroCloseBtn.text = uiMain.close;
		groupCloseBtn.text = uiMain.close;
		tilesCloseBtn.text = uiMain.close;
	}

	public void UpdateIgnoredCount( int c )
	{
		UISetup setup = DataStore.uiLanguage.uiSetup;
		ignoredBtn.text = $"{setup.ignoredHeading} <color=red>{c}</color>";
	}
}

