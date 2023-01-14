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

		deploymentHeading.text = ui.deploymentHeading.ToUpper();
		reservedBtn.text = ui.reservedBtn.ToUpper();
		allyBtn.text = ui.allyBtn.ToUpper();
		enemyBtn.text = ui.enemyBtn.ToUpper();
		randomBtn.text = ui.randomBtn.ToUpper();
		modThreatHeading.text = ui.modThreatHeading.ToUpper();
		applyBtn.text = ui.applyBtn.ToUpper();
		roundHeading.text = ui.roundHeading.ToUpper();
		depTypeHeading.text = ui.depTypeHeading.ToUpper();
		eventHeading.text = ui.eventHeading.ToUpper();
		randomHeading.text = ui.randomHeading.ToUpper();
		randomConfirmBtn.text = ui.confirm.ToUpper();
		randomCancelBtn.text = ui.cancel.ToUpper();
		maxThreatHeading.text = ui.maxThreatHeading.ToUpper();
		endRoundBtn.text = ui.endRoundBtn.ToUpper();
		fameHeading.text = ui.fameHeading.ToUpper();
		awardsHeading.text = ui.awardsHeading.ToUpper();
		fameContinueBtn.text = ui.continueBtn.ToUpper();
		deploymentContinueBtn.text = ui.continueBtn.ToUpper();
		onslaughtDeploy.text = ui.deploy.ToUpper();
		landingDeploy1.text = ui.deploy.ToUpper();
		landingDeploy2.text = ui.deploy.ToUpper();
		chooserCloseBtn.text = ui.close.ToUpper();
		infoContinueBtn.text = ui.continueBtn.ToUpper();
		zoomCloseBtn.text = ui.close.ToUpper();
		activateContinueBtn.text = ui.continueBtn.ToUpper();
		eventContinueBtn.text = ui.continueBtn.ToUpper();

		fame1UC.text = ui.fame1UC;
		fame2UC.text = ui.fame2UC;
		fameItem1UC.text = ui.fameItem1UC;
		fameItem2UC.text = ui.fameItem2UC;
		debugThreatUC.text = ui.debugThreatUC;
		debugDepModUC.text = ui.debugDepModUC;
		debugDepHandUC.text = ui.debugDepHandUC;
		debugContinueBtn.text = ui.continueBtn.ToUpper();
		reinforceWarningUC.text = ui.reinforceWarningUC;
		deploymentWarningUC.text = ui.deploymentWarningUC;
		depWarningUC.text = ui.reinforceWarningUC;
		onslaughtIncreasedUC.text = ui.threatIncreasedUC;
		landingIncreasedUC.text = ui.threatIncreasedUC;
		reinforceIncreasedUC.text = ui.threatIncreasedUC;
		calmMessageUC.text = ui.calmMessageUC;
		optionDeployment.text = ui.optionalDeployment.ToUpper();

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
