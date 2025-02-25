using UnityEngine;
using UnityEngine.UI;

public class SettingsLanguageController : MonoBehaviour
{
	public Text settingsHeading, music, sound, bloom, vignette, ambient, quitBtn, returnBtn, okBtn, quickClose, zoomButtons, topdownView, roundLimitOn, roundLimitOff, roundLimitDangerous, roundLimitLabel, skipWarpIntroLabel, enemyGroupsColor, colorRegular, colorElite, colorVillain;

	/// <summary>
	/// Sets the UI with the current language
	/// </summary>
	public void SetTranslatedUI()
	{
		UISettings ui = DataStore.uiLanguage.uiSettings;

		settingsHeading.text = ui.settingsHeading;
		music.text = ui.music;
		sound.text = ui.sound;
		bloom.text = ui.bloom;
		vignette.text = ui.vignette;
		quitBtn.text = ui.quit;
		returnBtn.text = ui.returnBtn;
		okBtn.text = ui.ok;
		ambient.text = ui.ambient;
		quickClose.text = ui.quickClose;
		zoomButtons.text = ui.zoomButtons;
		topdownView.text = ui.topdownView;
		roundLimitOn.text = ui.roundLimitOn;
		roundLimitOff.text = ui.roundLimitOff;
		roundLimitDangerous.text = ui.roundLimitDangerous;
		roundLimitLabel.text = ui.roundLimitLabel;
		skipWarpIntroLabel.text = ui.skipWarpIntroLabel;
		enemyGroupsColor.text = ui.enemyGroupsColor;
		colorRegular.text = ui.colorRegular;
		colorElite.text = ui.colorElite;
		colorVillain.text = ui.colorVillain;

	}
}
