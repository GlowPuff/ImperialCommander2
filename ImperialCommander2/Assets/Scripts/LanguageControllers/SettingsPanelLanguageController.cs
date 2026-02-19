using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelLanguageController : MonoBehaviour
{
	public Text settingsHeading, music, sound, ambient, bloom, vignette, topdownView, quitBtn, returnBtn, okBtn, quickClose, zoomButtons, skipWarpIntroLabel, roundLimitOn, roundLimitOff, roundLimitDangerous, roundLimitLabel, enemyGroupsColor, colorRegular, colorElite, colorVillain, audioHeader, uiHeader, graphicsHeader, mapperHeader, mapActivateImperialsLabel, mapToggleCamViewLabel, mapToggleMapVisibilityLabel, mapNavForwardLabel, mapNavBackLabel, mapNavLeftLabel, mapNavRightLabel, mapNavCWLabel, mapNavCCWLabel, reset;

	public void SetTranslatedUI()
	{
		UISettings ui = DataStore.uiLanguage.uiSettings;

		settingsHeading.text = ui.settingsHeading;
		quitBtn.text = ui.quit;
		returnBtn.text = ui.returnBtn;
		okBtn.text = ui.ok;

		music.text = ui.music;
		sound.text = ui.sound;
		ambient.text = ui.ambient;

		quickClose.text = ui.quickClose;
		zoomButtons.text = ui.zoomButtons;
		skipWarpIntroLabel.text = ui.skipWarpIntroLabel;
		roundLimitOn.text = ui.roundLimitOn;
		roundLimitOff.text = ui.roundLimitOff;
		roundLimitDangerous.text = ui.roundLimitDangerous;
		roundLimitLabel.text = ui.roundLimitLabel;

		bloom.text = ui.bloom;
		vignette.text = ui.vignette;
		topdownView.text = ui.topdownView;

		enemyGroupsColor.text = ui.enemyGroupsColor;
		colorRegular.text = ui.colorRegular;
		colorElite.text = ui.colorElite;
		colorVillain.text = ui.colorVillain;

		audioHeader.text = ui.audioHeader;
		uiHeader.text = ui.uiHeader;
		graphicsHeader.text = ui.graphicsHeader;
		mapperHeader.text = ui.mapperHeader;
		reset.text = ui.reset;

		mapActivateImperialsLabel.text = ui.mapActivateImperialsLabel;
		mapToggleCamViewLabel.text = ui.mapToggleCamViewLabel;
		mapToggleMapVisibilityLabel.text = ui.mapToggleMapVisibilityLabel;
		mapNavForwardLabel.text = ui.mapNavForwardLabel;
		mapNavBackLabel.text = ui.mapNavBackLabel;
		mapNavLeftLabel.text = ui.mapNavLeftLabel;
		mapNavRightLabel.text = ui.mapNavRightLabel;
		mapNavCWLabel.text = ui.mapNavCWLabel;
		mapNavCCWLabel.text = ui.mapNavCCWLabel;
	}
}
