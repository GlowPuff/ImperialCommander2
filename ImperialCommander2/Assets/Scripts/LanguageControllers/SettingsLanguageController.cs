using UnityEngine;
using UnityEngine.UI;

public class SettingsLanguageController : MonoBehaviour
{
	public Text settingsHeading, music, sound, bloom, vignette, ambient, quitBtn, returnBtn, okBtn, quickClose;

	/// <summary>
	/// Sets the UI with the current language
	/// </summary>
	public void SetTranslatedUI()
	{
		UISettings ui = DataStore.uiLanguage.uiSettings;

		settingsHeading.text = ui.settingsHeading.ToUpper();
		music.text = ui.music.ToUpper();
		sound.text = ui.sound.ToUpper();
		bloom.text = ui.bloom.ToUpper();
		vignette.text = ui.vignette.ToUpper();
		quitBtn.text = ui.quit.ToUpper();
		returnBtn.text = ui.returnBtn.ToUpper();
		okBtn.text = ui.ok.ToUpper();
		ambient.text = "AMBIENT";
		quickClose.text = ui.quickClose.ToUpper();
	}
}
