using System.Collections.Generic;

public class UILanguage
{
	public UITitle uiTitle;
	public UISetup uiSetup;
	public UIMainApp uiMainApp;
	public UISettings uiSettings;
	public UIExpansions uiExpansions;
	public UIDeploymentGroups uiDeploymentGroups;
	public List<MissionCard> uiMissionCards;
}

public class UISettings
{
	public string settingsHeading, music, sound, bloom, vignette, quit, returnBtn, ok;
}

public class UITitle
{
	public string menuHeading, newGameBtn, continueBtn, campaignsBtn, optionsBtn, supportUC, docsUC;
}

public class UISetup
{
	public string settingsHeading, chooseMission, viewCardBtn, missionInfoBtn, threatLevelHeading, addtlThreatHeading, deploymentHeading, yes, no, back, difficulty, easy, normal, hard, imperials, mercenaries, adaptive, groupsHeading, choose, zoom, initialHeading, reservedHeading, villainsHeading, ignoredHeading, addHero, addAlly, threatCostHeading, cancel, continueBtn, saved, loaded, selected, enemyChooser, missionChooser, heroAllyChooser, adaptiveInfoUC, chooseHeroesHeading;
}

public class UIMainApp
{
	public string eliteUpgradeMsgUC, eliteDowngradeMsgUC, restoredMsgUC, restoreErrorMsgUC, pauseDepMsgUC, unPauseDepMsgUC, pauseThreatMsgUC, UnPauseThreatMsgUC, deploymentHeading, reservedBtn, allyBtn, enemyBtn, randomBtn, modThreatHeading, applyBtn, roundHeading, depTypeHeading, eventHeading, randomHeading, maxThreatHeading, endRoundBtn, fameHeading, awardsHeading, fame1UC, fameItem1UC, fameItem2UC, fame2UC, continueBtn, debugThreatUC, debugDepModUC, debugDepHandUC, tooltipRulesUC, tooltipInfoUC, tooltipPauseDepUC, tooltipPauseThreatUC, tooltipOpDepUC, tooltipSettingsUC, tooltipImpHandUC, tooltipActivateUC, tooltipFameUC, confirm, cancel, deploy, threatIncreasedUC, reinforceWarningUC, deploymentWarningUC, calmMessageUC, close, deployModeCalm, deployModeReinforcements, deployModeLanding, deployModeOnslaught, fameIncreasedUC, noRandomMatchesUC, depCostUC, noAbilitiesUC, ignoredAbilitiesUC, noKeywordsUC, noneUC, rewardUC, pageUC, optionalDeployment;
}

public class UIExpansions
{
	public string core, twin, hoth, bespin, jabba, empire, lothal, other;
}

public class UIDeploymentGroups
{
	public List<CardLanguage> enemyCards;
	public List<CardLanguage> villainCards;
	public List<CardLanguage> allyCards;
	public List<CardLanguage> heroCards;
}