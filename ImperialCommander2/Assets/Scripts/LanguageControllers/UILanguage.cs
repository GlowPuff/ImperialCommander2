﻿using System.Collections.Generic;

public class UILanguage
{
	public UITitle uiTitle;
	public UISetup uiSetup;
	public SagaUISetup sagaUISetup;
	public UIMainApp uiMainApp;
	public SagaMainApp sagaMainApp;
	public UISettings uiSettings;
	public UIExpansions uiExpansions;
	public UIDeploymentGroups uiDeploymentGroups;
	public List<MissionCard> uiMissionCards;
	public UICampaign uiCampaign;
	public UILogger uiLogger;
	public UIHelpOverlay uiHelpOverlay;

	public UIHelpOverlay uiHelpOverlayBackup;//the English version to fall back to for missing entries in translations
}

public class UISettings
{
	public string settingsHeading, music, sound, bloom, vignette, quit, returnBtn, ok, quickClose, ambient, zoomButtons, topdownView, roundLimitOn, roundLimitOff, roundLimitDangerous, roundLimitLabel, skipWarpIntroLabel, enemyGroupsColor, colorRegular, colorElite, colorVillain;
}

public class UITitle
{
	public string menuHeading, newGameBtn, continueBtn, campaignsBtn, optionsBtn, supportUC, docsUC, newCampaign, loadCampaign, confirmDelete, delete, expansions, tutorialUC, saga, campaigns, classic;
}

public class UISetup
{
	public string settingsHeading, chooseMission, viewCardBtn, missionInfoBtn, threatLevelHeading, addtlThreatHeading, deploymentHeading, yes, no, back, difficulty, easy, normal, hard, imperials, mercenaries, adaptive, groupsHeading, choose, zoom, initialHeading, reservedHeading, villainsHeading, ignoredHeading, addHero, addAlly, threatCostHeading, cancel, continueBtn, saved, loaded, selected, enemyChooser, missionChooser, heroAllyChooser, adaptiveInfoUC, chooseHeroesHeading, appMissionFormat;
}

public class SagaUISetup
{
	public string groupsText, villainsBtn, tilesBtn, setupStartBtn, officialBtn, customBtn, missionCardBtn, campaignJournalUC, importBtn;
}

public class SagaMainApp
{
	public string tooltipHideUIUC, roundIncreasedUC, endOfMissionUC, deployMessageUC, noDPWarningUC, mmAddTilesUC, mmRemoveTilesUC, mmCoreTileNameUC, mmTwinTileNameUC, mmHothTileNameUC, mmBespinTileNameUC, mmJabbaTileNameUC, mmEmpireTileNameUC, mmLothalTileNameUC, mmAddEntitiesUC, groupsReadyUC, groupsExhaustUC, repositionTargetUC, doorsUC, cratesUC, terminalsUC, tokensUC, woundUC, withdrawUC, exhaustUC, defeatUC, imperialMenu, medpacInfoUC, cannotDefeatUC, missionLogTitle;
}

public class UIMainApp
{
	public string eliteUpgradeMsgUC, eliteDowngradeMsgUC, restoredMsgUC, restoreErrorMsgUC, pauseDepMsgUC, unPauseDepMsgUC, pauseThreatMsgUC, UnPauseThreatMsgUC, deploymentHeading, reservedBtn, allyBtn, enemyBtn, randomBtn, modThreatHeading, applyBtn, roundHeading, depTypeHeading, eventHeading, randomHeading, maxThreatHeading, endRoundBtn, fameHeading, awardsHeading, fame1UC, fameItem1UC, fameItem2UC, fame2UC, continueBtn, debugThreatUC, debugDepModUC, debugDepHandUC, tooltipRulesUC, tooltipInfoUC, tooltipPauseDepUC, tooltipPauseThreatUC, tooltipOpDepUC, tooltipSettingsUC, tooltipImpHandUC, tooltipActivateUC, tooltipFameUC, tooltipDashboardUC, tooltipMedPacUC, confirm, cancel, deploy, threatIncreasedUC, reinforceWarningUC, deploymentWarningUC, calmMessageUC, close, deployModeCalm, deployModeReinforcements, deployModeLanding, deployModeOnslaught, fameIncreasedUC, noRandomMatchesUC, depCostUC, noAbilitiesUC, ignoredAbilitiesUC, noKeywordsUC, noneUC, rewardUC, pageUC, optionalDeployment;
}

public class UIExpansions
{
	public string core, twin, hoth, bespin, jabba, empire, lothal, other, figurepacks;
}

public class UIDeploymentGroups
{
	public List<CardLanguage> enemyCards;
	public List<CardLanguage> villainCards;
	public List<CardLanguage> allyCards;
	public List<CardLanguage> heroCards;
}

public class UICampaign
{
	public string threatInfoUC, modeIntroductionUC, modeStoryUC, modeSideUC, forcedUC, modeInterludeUC, modeFinaleUC, campaignNameUC, customCampaign, addForcedMissionUC, tierUC, selectMissionUC, customUC, creditsUC, fameUC, awardsUC, campaignSetup, itemsUC, rewardsUC, villainsUC, alliesUC, threat, agendaUC, modifyUC, removeUC, otherUC, campaignUC, generalUC, heroUC, personalUC, campaignSetupUC, campaignDescriptionUC, sagaDescriptionUC, classicDescriptionUC, agendaMission, agendaImperialUC, agendaRebelUC, xpUC, costUC, missionTypeMockupUC, missionNameMockupUC, itemMockupUC;

	public string[] missionTypeStrings;

	public void BuildMissionTypeStrings()
	{
		missionTypeStrings = new string[6];
		missionTypeStrings[0] = modeStoryUC;
		missionTypeStrings[1] = modeSideUC;
		missionTypeStrings[2] = forcedUC;
		missionTypeStrings[3] = modeIntroductionUC;
		missionTypeStrings[4] = modeInterludeUC;
		missionTypeStrings[5] = modeFinaleUC;
	}
}

public class UILogger
{
	public string textLabel, inputPromptLabel, selectionPromptLabel, selectionLabel, groupActivationLabel, groupDeployedLabel, groupRemovedLabel, groupDefeatedLabel, deploymentEventLabel, inputValueLabel;
}

public class UIHelpOverlay
{
	public HelpOverlayPanel[] helpOverlayPanels;
}

public class HelpOverlayPanel
{
	public string panelHelpID;
	public HelpOverlayItem[] helpItems;
}

public class HelpOverlayItem
{
	public string id;
	public string helpText;
}