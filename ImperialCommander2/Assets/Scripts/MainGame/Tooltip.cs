using TMPro;
using UnityEngine;

public class Tooltip : MonoBehaviour
{
	public TextMeshProUGUI tmp;

	public void Show( string t )
	{
		string tt = "";
		switch ( t )
		{
			case "MissionInfo":
				tt = DataStore.uiLanguage.uiMainApp.tooltipInfoUC; break;
			case "MissionRules":
				tt = DataStore.uiLanguage.uiMainApp.tooltipRulesUC; break;
			case "PauseDeploy":
				tt = DataStore.uiLanguage.uiMainApp.tooltipPauseDepUC; break;
			case "PauseThreat":
				tt = DataStore.uiLanguage.uiMainApp.tooltipPauseThreatUC; break;
			case "OptionalDeploy":
				tt = DataStore.uiLanguage.uiMainApp.tooltipOpDepUC; break;
			case "Settings":
				tt = DataStore.uiLanguage.uiMainApp.tooltipSettingsUC; break;
			case "Debug":
				tt = DataStore.uiLanguage.uiMainApp.tooltipImpHandUC; break;
			case "Activate":
				tt = DataStore.uiLanguage.uiMainApp.tooltipActivateUC; break;
			case "Fame":
				tt = DataStore.uiLanguage.uiMainApp.tooltipFameUC; break;
			default:
				tt = "Unknown string code: " + t; break;
		}

		gameObject.SetActive( true );
		tmp.text = tt;
	}

	public void Hide()
	{
		gameObject.SetActive( false );
	}
}
