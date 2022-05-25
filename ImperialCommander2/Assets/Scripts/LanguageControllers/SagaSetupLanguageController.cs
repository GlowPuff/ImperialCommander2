using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SagaSetupLanguageController : MonoBehaviour
{
	public Text difficultyBtn, adaptiveBtn, initialThreatText, addtlThreatText, groupsText, ignoredBtn, villainsBtn, addAllyText, heroesText;

	public TextMeshProUGUI missionTitle;

	public void SetTranslatedUI()
	{
		SagaUISetup ui = DataStore.uiLanguage.sagaUISetup;

		groupsText.text = ui.groupsText;
		villainsBtn.text = ui.villainsBtn;

		UISetup setup = DataStore.uiLanguage.uiSetup;

		difficultyBtn.text = setup.difficulty;
		adaptiveBtn.text = setup.adaptive;
		initialThreatText.text = setup.threatLevelHeading;
		addtlThreatText.text = setup.addtlThreatHeading;
		ignoredBtn.text = setup.ignoredHeading;
		addAllyText.text = setup.addAlly;
		heroesText.text = setup.addHero;
	}
}

