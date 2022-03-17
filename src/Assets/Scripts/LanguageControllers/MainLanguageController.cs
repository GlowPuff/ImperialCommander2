using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainLanguageController : MonoBehaviour
{
	public TextMeshProUGUI quickMessageUC, fame1UC, fameItem1UC, fameItem2UC, fame2UC, debugThreatUC, debugDepModUC, debugDepHandUC, tooltip, onslaughtIncreasedUC, landingIncreasedUC, reinforceIncreasedUC, reinforceWarningUC, deploymentWarningUC, depWarningUC, calmMessageUC;

	public Text deploymentHeading, reservedBtn, allyBtn, enemyBtn, randomBtn, modThreatHeading, applyBtn, roundHeading, depTypeHeading, eventHeading, randomHeading, maxThreatHeading, endRoundBtn, fameHeading, awardsHeading, fameContinueBtn, randomConfirmBtn, randomCancelBtn, debugContinueBtn, deploymentContinueBtn, onslaughtDeploy, landingDeploy1, landingDeploy2, chooserCloseBtn, infoContinueBtn, zoomCloseBtn, activateContinueBtn, eventContinueBtn, coreHeading, twinHeading, hothHeading, bespinHeading, jabbaHeading, empireHeading, lothalHeading, otherHeading, optionDeployment;

	/// <summary>
	/// Sets the UI with the current language
	/// </summary>
	public void SetTranslatedUI()
	{
		UIMainApp ui = DataStore.uiLanguage.uiMainApp;

		deploymentHeading.text = ui.deploymentHeading;
		reservedBtn.text = ui.reservedBtn;
		allyBtn.text = ui.allyBtn;
		enemyBtn.text = ui.enemyBtn;
		randomBtn.text = ui.randomBtn;
		modThreatHeading.text = ui.modThreatHeading;
		applyBtn.text = ui.applyBtn;
		roundHeading.text = ui.roundHeading;
		depTypeHeading.text = ui.depTypeHeading;
		eventHeading.text = ui.eventHeading;
		randomHeading.text = ui.randomHeading;
		randomConfirmBtn.text = ui.confirm;
		randomCancelBtn.text = ui.cancel;
		maxThreatHeading.text = ui.maxThreatHeading;
		endRoundBtn.text = ui.endRoundBtn;
		fameHeading.text = ui.fameHeading;
		awardsHeading.text = ui.awardsHeading;
		fameContinueBtn.text = ui.continueBtn;
		deploymentContinueBtn.text = ui.continueBtn;
		onslaughtDeploy.text = ui.deploy;
		landingDeploy1.text = ui.deploy;
		landingDeploy2.text = ui.deploy;
		chooserCloseBtn.text = ui.close;
		infoContinueBtn.text = ui.continueBtn;
		zoomCloseBtn.text = ui.close;
		activateContinueBtn.text = ui.continueBtn;
		eventContinueBtn.text = ui.continueBtn;

		fame1UC.text = ui.fame1UC;
		fame2UC.text = ui.fame2UC;
		fameItem1UC.text = ui.fameItem1UC;
		fameItem2UC.text = ui.fameItem2UC;
		debugThreatUC.text = ui.debugThreatUC;
		debugDepModUC.text = ui.debugDepModUC;
		debugDepHandUC.text = ui.debugDepHandUC;
		debugContinueBtn.text = ui.continueBtn;
		reinforceWarningUC.text = ui.reinforceWarningUC;
		deploymentWarningUC.text = ui.deploymentWarningUC;
		depWarningUC.text = ui.reinforceWarningUC;
		onslaughtIncreasedUC.text = ui.threatIncreasedUC;
		landingIncreasedUC.text = ui.threatIncreasedUC;
		reinforceIncreasedUC.text = ui.threatIncreasedUC;
		calmMessageUC.text = ui.calmMessageUC;
		optionDeployment.text = ui.optionalDeployment;

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
