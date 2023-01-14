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

		settingsHeader.text = ui.settingsHeading.ToUpper();
		chooseMissionBtn.text = ui.chooseMission.ToUpper();
		viewCardBtn.text = ui.viewCardBtn.ToUpper();
		missionInfoBtn.text = ui.missionInfoBtn.ToUpper();
		threatLevel.text = ui.threatLevelHeading.ToUpper();
		addtlThreat.text = ui.addtlThreatHeading.ToUpper();
		deploymentHeader.text = ui.deploymentHeading.ToUpper();
		opdepBtn.text = ui.no.ToUpper();
		difficultyBtn.text = ui.difficulty.ToUpper();
		imperialsHeader.text = ui.imperials.ToUpper();
		mercenariesHeader.text = ui.mercenaries.ToUpper();
		adaptiveHeader.text = ui.adaptive.ToUpper();
		groupsHeading.text = ui.groupsHeading.ToUpper();
		initialBtn.text = ui.choose.ToUpper();
		reservedBtn.text = ui.choose.ToUpper();
		villainsBtn.text = ui.choose.ToUpper();
		ignoredBtn.text = ui.choose.ToUpper();
		initialHeading.text = ui.initialHeading.ToUpper();
		reservedHeading.text = ui.reservedHeading.ToUpper();
		villainsHeading.text = ui.villainsHeading.ToUpper();
		ignoredHeading.text = ui.ignoredHeading.ToUpper();
		addHeroBtn.text = ui.addHero.ToUpper();
		addAllyBtn.text = ui.addAlly.ToUpper();
		threatCostHeading.text = ui.threatCostHeading.ToUpper();
		threatCostBtn.text = ui.no.ToUpper();
		cancelBtn.text = ui.cancel.ToUpper();
		continueBtn.text = ui.continueBtn.ToUpper();
		adaptiveInfo.text = ui.adaptiveInfoUC;
		enemyChooserHeading.text = ui.enemyChooser.ToUpper();
		missionChooserHeading.text = ui.missionChooser.ToUpper();
		enemyBackBtn.text = ui.back.ToUpper();
		missionBackBtn.text = ui.back.ToUpper();
		heroAllyBackBtn.text = ui.back.ToUpper();
		zoomBackBtn.text = ui.back.ToUpper();
		enemyZoomBtn.text = ui.zoom.ToUpper();
		chooseHeroesHeading.text = ui.chooseHeroesHeading.ToUpper();
		heroBackBtn.text = ui.back.ToUpper();

		UIExpansions uie = DataStore.uiLanguage.uiExpansions;
		coreHeading.text = uie.core.ToUpper();
		twinHeading.text = uie.twin.ToUpper();
		hothHeading.text = uie.hoth.ToUpper();
		bespinHeading.text = uie.bespin.ToUpper();
		jabbaHeading.text = uie.jabba.ToUpper();
		empireHeading.text = uie.empire.ToUpper();
		lothalHeading.text = uie.lothal.ToUpper();
		otherHeading.text = uie.other.ToUpper();
	}
}
