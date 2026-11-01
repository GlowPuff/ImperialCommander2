using System.Collections.Generic;

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
	public UITacticalAI uiTacticalAI;
	public UIHelpOverlay uiHelpOverlay;

	public UIHelpOverlay uiHelpOverlayBackup;//the English version to fall back to for missing entries in translations
}

// Player-facing texts of the tactical AI layer (activation popups, doctrine
// orders, focus explanations). All keys live in Languages/<Lang>/ui.json.
public class UITacticalAI
{
	public string actionFormat, doctrineHeader, docAggressive, docGuard, docBlocker, docHunter, docSupport, docAmbusher;
	// M13a (playtest 28.10): popup labelling of the Wenn-Dann tree. A plan
	// with exactly ONE action block gets the unnumbered solo line
	// (actionSoloFormat, {0} = block text); plans with >=2 keep "Aktion {n}".
	// M13c (Thomas 30.10): the "Diese Karte zeigt..." lead and the standalone
	// "Grundregel:" lead (doctrineTreeLead) are REMOVED - the basic attack
	// rule now rides inside the merged info block (ordInfoRule).
	public string actionSoloFormat;
	public string moveFocus, moveFocusNoTarget, moveGuard, moveBlocker, moveSupport, moveAmbusher;
	public string attackFocus, attackNoTarget, attackGuard, attackGuardNoTarget, attackAmbush, attackAmbushNoTarget, attackSupport, attackSupportNoTarget;
	public string attack2Move, attack2MoveNoTarget, attack2MoveGuard, attack2MoveAmbush, attack2MoveBlocker;
	// M13t (Thomas 30.10.): Stativ/Tripod emplacement action trees - printed
	// rule: movement and attack are mutually exclusive per activation, so the
	// guard tree must never suggest a shuffle while a post/target exists.
	// Missing keys fall back to the plain guard trees (never empty rows).
	public string moveGuardTripod, attackGuardTripod, attackGuardTripodNoTarget, attack2MoveGuardTripod;
	public string attackSecondIntro, attackSecondTail;
	// Noldorion playtest 2.11 (PR #194): assertive second-attack block for
	// cards that PRINT Assault (multiple attack actions per activation).
	public string assaultSecondAttack;
	public string overlordFallback;
	// M13c (Thomas 30.10): shuffleNote DELETED ("(afterwards: shuffle this
	// group's deck)" does not exist in this app's flow - honest removal).
	public string specialAsPrinted, restLine, interactLine;
	public string focusPrefix, focusWounded, focusFirepower;
	// M6 overlord explain parts (public threat economy + aim marks + risk).
	// M13c (Thomas 30.10): ordRoundFocus + focusNoChance REMOVED from the
	// focus-target WHY line (the "all groups converge...; no chance" tail).
	public string ordThreat, ordMarked;
	// M11d: light strain-pressure note in the round-focus WHY line
	public string ordStrainPressure;
	// M11h: fuzzy strain-usage prior in the round-focus WHY line - args
	// {0} = completed round, {1} = reachable buffer; wording says
	// "wahrscheinlich/probably" (a prior, never marker knowledge).
	public string ordStrainFuzzy;
	// M11e: heading line for the printed {A} option block in the activation popup
	public string ordOptionPrinted;
	// M13n (Thomas 29.10): general RR reminder block printed under the action
	// blocks, BEFORE the gated explanation: attack = weapon/ability use, and
	// the door law (RR DOORS: opening a door is an INTERACT - one of the five
	// action types - so it costs an action; locked doors only per mission).
	// M13n + M13c: the general RR info block under the action blocks - ONE
	// merged paragraph: basic attack rule, attack line, door law.
	public string ordInfoRule, ordInfoAttack, ordInfoDoor;
	public string ordRiskLabel, ordRiskPress, ordRiskHold, ordRiskGuardHold, ordRiskDisengage;
	public string whyPrefix, whyRank;
	public string setHeading, setMode, setSkill, setExplain, setInfo, setOn, setOff, setRowFormat;
	// M13h (Thomas 28.10): the setCardSkills note field is REMOVED - the note
	// line it fed is gone from the settings screen (stale 'S4' wording).

	// skill ladder DISPLAY names (dev 18: recruit..5-star general, Thomas).
	// Stored/logged token stays s1..s5 - these are labels only (PR rule:
	// display via language files).
	public string skillS1, skillS2, skillS3, skillS4, skillS5;
	public string badgeAuto, badgeOff;
	// M11f (Einsatzkarte): plain-text mission badge that replaces the legacy
	// difficulty toggle while the tactical planner is in charge. {0} = ladder
	// rung number, {1} = skillS name. The (auto) variant marks ""/AUTO tokens
	// that resolve to the global ladder rung.
	public string badgeMissionFormat, badgeMissionAutoFormat, badgeMissionOff;
	// Personas (M1/M3, Thomas 02.11.): the persona REPLACED the class-deck
	// and agenda-card system - its ui.json keys (impBuyLog/agendaBuy*/imp*)
	// are removed with it, 11-language parity re-checked. Vocabulary:
	// picker words, rank/bonus chips (activation-card line), QM
	// announcements, the three persona name/motto/signature rows (keys are
	// the asset's nameKey/mottoKey/signaturText), settings row heading.
	public string personaFollowDefault, personaNone, personaRank;
	public string personaBonusCost, personaBonusSubsidy, personaBonusDoctrine, personaBonusGuard;
	public string personaRankUpQM, personaRankDownQM, personaSubsidyQM;
	public string personaButton, personaPopupHeader, setPersona;
	public string personaPlainNote, personaRankColumn, personaCycleNote;
	public string personaCaptTrooper, personaCaptTrooperMotto, personaCaptIsb, personaCaptIsbMotto;
	public string personaCaptHutt, personaCaptHuttMotto;
	public string personaCaptSignature, personaIsbSignature, personaHuttSignature;
	// Noldorion review 2.11 (PR #194): popup note for non-villain groups
	// whose printed abilities stay in the human lane (the mode never
	// pre-decides "may" abilities it cannot see the board for). (The card
	// announce line ordCardEffect is GONE - personas own that slot now.)
	public string ordAbilitiesLane;
	// Noldorion direction reply 2.11: "the power score needs to be very
	// transparent" - EXPLAIN=VOLL discloses the real focus computation:
	// score + what the number measures (posture-dependent) + wound economy
	// + decision basis + runner-up. Numbers are the fixture-pinned sim
	// values (FocusPick.AsDict), every input is a public table artifact.
	public string ordFocusDetail;
	// Basis/measure labels for the disclosure line (code maps the machine
	// vocabulary; unknown basis falls through as the raw token).
	public string ordFocusMeasureDamage, ordFocusMeasureRunner;
	public string ordFocusBasisMark, ordFocusBasisChip, ordFocusBasisConditioned;
	public string ordFocusBasisEconomy, ordFocusBasisPrinted;
	// Punkt 2 (Thomas 01.11.): doctrine phase disclosure - one-shot QM at
	// the defend->hunt flip and the EXPLAIN line naming both phases.
	public string ordPhaseShiftQM, ordPhaseExplain, ordPhaseDefend, ordPhaseHunt;
	public string expNone, expPostHoc, expFull;
}

public class UISettings
{
	public string settingsHeading, music, sound, bloom, vignette, quit, returnBtn, ok, quickClose, ambient, zoomButtons, topdownView, roundLimitOn, roundLimitOff, roundLimitDangerous, roundLimitLabel, skipWarpIntroLabel, enemyGroupsColor, colorRegular, colorElite, colorVillain, audioHeader, uiHeader, graphicsHeader, mapperHeader, mapActivateImperialsLabel, mapToggleCamViewLabel, mapToggleMapVisibilityLabel, mapNavForwardLabel, mapNavBackLabel, mapNavLeftLabel, mapNavRightLabel, mapNavCWLabel, mapNavCCWLabel, reset, pressKeyMessage;
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
	// M13c optional-deployment revival keys (last field block): ui.json sagaMainApp, 11 languages.
	public string tooltipHideUIUC, roundIncreasedUC, endOfMissionUC, deployMessageUC, noDPWarningUC, mmAddTilesUC, mmRemoveTilesUC, mmCoreTileNameUC, mmTwinTileNameUC, mmHothTileNameUC, mmBespinTileNameUC, mmJabbaTileNameUC, mmEmpireTileNameUC, mmLothalTileNameUC, mmAddEntitiesUC, groupsReadyUC, groupsExhaustUC, repositionTargetUC, doorsUC, cratesUC, terminalsUC, tokensUC, woundUC, withdrawUC, exhaustUC, defeatUC, imperialMenu, medpacInfoUC, cannotDefeatUC, missionLogTitle, optionalDeployTitleUC, optionalDeployPickGroupUC, optionalDeployPickDpUC, optionalDeployNoGroupsUC, optionalDeployCostFormatUC, optionalDeployDpFormatUC, optionalDeployConfirmUC, optionalTacticalAnnounceUC;
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