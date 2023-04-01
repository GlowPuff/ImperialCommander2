using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetupLanguageController : MonoBehaviour
{
	public Text settingsHeader, chooseMissionBtn, viewCardBtn, missionInfoBtn, threatLevel, addtlThreat, deploymentHeader, opdepBtn, difficultyBtn, imperialsHeader, mercenariesHeader, adaptiveHeader, groupsHeading, initialBtn, reservedBtn, villainsBtn, ignoredBtn, initialHeading, reservedHeading, villainsHeading, ignoredHeading, addHeroBtn, addAllyBtn, threatCostHeading, threatCostBtn, cancelBtn, continueBtn, prefsStatus, enemyChooserHeading, missionChooserHeading, enemyBackBtn, missionBackBtn, heroAllyBackBtn, zoomBackBtn, enemyZoomBtn, chooseHeroesHeading, heroBackBtn, coreHeading, twinHeading, hothHeading, bespinHeading, jabbaHeading, empireHeading, lothalHeading, otherHeading;

	public TextMeshProUGUI adaptiveInfo;

	/// <summary>
	/// Sets the UI with the current language
	/// </summary>
	public void SetTranslatedUI()
	{
		UISetup ui = DataStore.uiLanguage.uiSetup;

		settingsHeader.text = ui.settingsHeading;
		chooseMissionBtn.text = ui.chooseMission;
		viewCardBtn.text = ui.viewCardBtn;
		missionInfoBtn.text = ui.missionInfoBtn;
		threatLevel.text = ui.threatLevelHeading;
		addtlThreat.text = ui.addtlThreatHeading;
		deploymentHeader.text = ui.deploymentHeading;
		opdepBtn.text = ui.no;
		difficultyBtn.text = ui.difficulty;
		imperialsHeader.text = ui.imperials;
		mercenariesHeader.text = ui.mercenaries;
		adaptiveHeader.text = ui.adaptive;
		groupsHeading.text = ui.groupsHeading;
		initialBtn.text = ui.choose;
		reservedBtn.text = ui.choose;
		villainsBtn.text = ui.choose;
		ignoredBtn.text = ui.choose;
		initialHeading.text = ui.initialHeading;
		reservedHeading.text = ui.reservedHeading;
		villainsHeading.text = ui.villainsHeading;
		ignoredHeading.text = ui.ignoredHeading;
		addHeroBtn.text = ui.addHero;
		addAllyBtn.text = ui.addAlly;
		threatCostHeading.text = ui.threatCostHeading;
		threatCostBtn.text = ui.no;
		cancelBtn.text = ui.cancel;
		continueBtn.text = ui.continueBtn;
		adaptiveInfo.text = ui.adaptiveInfoUC;
		enemyChooserHeading.text = ui.enemyChooser;
		missionChooserHeading.text = ui.missionChooser;
		enemyBackBtn.text = ui.back;
		missionBackBtn.text = ui.back;
		heroAllyBackBtn.text = ui.back;
		zoomBackBtn.text = ui.back;
		enemyZoomBtn.text = ui.zoom;
		chooseHeroesHeading.text = ui.chooseHeroesHeading;
		heroBackBtn.text = ui.back;

		UIExpansions uie = DataStore.uiLanguage.uiExpansions;
		coreHeading.text = uie.core;
		twinHeading.text = uie.twin;
		hothHeading.text = uie.hoth;
		bespinHeading.text = uie.bespin;
		jabbaHeading.text = uie.jabba;
		empireHeading.text = uie.empire;
		lothalHeading.text = uie.lothal;
		otherHeading.text = uie.other;
	}
}
