using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga
{
	public class SagaLanguageController : MonoBehaviour
	{
		public Text endRoundBtn, roundHeading, onslaughtDeploy, landingDeploy1, landingDeploy2, fameHeading, awardsHeading, fameContinueBtn, depTypeHeading, eventHeading, eventContinueBtn, deploymentContinueBtn, activateContinueBtn;

		public TextMeshProUGUI fameItem1UC, fameItem2UC, fame1UC, fame2UC, onslaughtIncreasedUC, landingIncreasedUC, reinforceIncreasedUC, reinforceWarningUC, deploymentWarningUC, depWarningUC, calmMessageUC, woundUC, withdrawUC, exhaustUC, heroDefeatUC, enemyDefeatUC;

		public void SetTranslatedUI()
		{
			SagaMainApp uiSaga = DataStore.uiLanguage.sagaMainApp;
			UIMainApp uiMain = DataStore.uiLanguage.uiMainApp;

			fameContinueBtn.text = uiMain.continueBtn.ToUpper();
			deploymentContinueBtn.text = uiMain.continueBtn.ToUpper();
			activateContinueBtn.text = uiMain.continueBtn.ToUpper();
			eventContinueBtn.text = uiMain.continueBtn.ToUpper();

			fameHeading.text = uiMain.fameHeading.ToUpper();
			awardsHeading.text = uiMain.awardsHeading.ToUpper();
			fame1UC.text = uiMain.fame1UC;
			fame2UC.text = uiMain.fame2UC;
			fame1UC.text = uiMain.fame1UC;
			fame2UC.text = uiMain.fame2UC;
			fameItem1UC.text = uiMain.fameItem1UC;
			fameItem2UC.text = uiMain.fameItem2UC;

			onslaughtDeploy.text = uiMain.deploy.ToUpper();
			landingDeploy2.text = uiMain.deploy.ToUpper();
			landingDeploy1.text = uiMain.deploy.ToUpper();
			landingDeploy2.text = uiMain.deploy.ToUpper();

			roundHeading.text = uiMain.roundHeading.ToUpper();
			endRoundBtn.text = uiMain.endRoundBtn.ToUpper();

			woundUC.text = uiSaga.woundUC;
			//withdrawUC.text = uiSaga.withdrawUC;
			exhaustUC.text = uiSaga.exhaustUC;
			heroDefeatUC.text = uiSaga.defeatUC;
			enemyDefeatUC.text = uiSaga.defeatUC;
		}
	}
}
