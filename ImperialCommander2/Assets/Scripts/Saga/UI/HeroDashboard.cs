using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroDashboard : MonoBehaviour
{
	public PopupBase popupBase;
	public GameObject misionInfoPanel, famePanel, logPanel;
	public Toggle missionInfoToggle;
	public TextMeshProUGUI missionInfoText, logText;
	public ScrollRect logScrollRect, infoScrollRect;
	public Text fameText, awardText;

	string missionInfo;

	public void Show( string info )
	{
		popupBase.Show();

		missionInfo = info;

		misionInfoPanel.SetActive( true );
		famePanel.SetActive( false );
		logPanel.SetActive( false );

		missionInfoToggle.isOn = true;
		missionInfoText.text = missionInfo;
	}

	public void Close()
	{
		popupBase.Close();
	}

	public void OnTabClick( Toggle t )
	{
		misionInfoPanel.SetActive( false );
		famePanel.SetActive( false );
		logPanel.SetActive( false );

		if ( t.isOn )
		{
			switch ( t.name )
			{
				case "info":
					misionInfoPanel.SetActive( true );
					missionInfoText.text = missionInfo;
					infoScrollRect.verticalNormalizedPosition = 1;//scroll to top
					break;
				case "fame":
					famePanel.SetActive( true );
					UpdateFame();
					break;
				case "log":
					logPanel.SetActive( true );
					logText.text = DataStore.sagaSessionData.missionLogger.missionLogText;
					logScrollRect.verticalNormalizedPosition = 0;//scroll to bottom
					break;
			}
		}
	}

	void UpdateFame()
	{
		int currentFame = DataStore.sagaSessionData.gameVars.fame;
		int currentRound = DataStore.sagaSessionData.gameVars.round;

		fameText.text = "<color=#00A4FF>" + DataStore.uiLanguage.uiMainApp.fameHeading.ToUpper() + "</color> <color=#00FFA0>" + currentFame.ToString() + "</color>";

		//AWARD value based on FAME divided by 12, rounded down (for every 12 Fame you earn, you gain 1 Reward
		int awards = Mathf.FloorToInt( currentFame / 12 );
		//reset to 0 at round 8+
		if ( currentRound >= 8 )
			awards = 0;
		awardText.text = "<color=#00A4FF>" + DataStore.uiLanguage.uiMainApp.awardsHeading.ToUpper() + "</color> <color=#00FFA0>" + awards.ToString() + "</color>";
	}
}
