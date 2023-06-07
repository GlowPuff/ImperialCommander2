using Saga;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ImportItem : MonoBehaviour
{
	ImportPanel importPanel;
	ImportCampaignPanel importCampaignPanel;

	[HideInInspector]
	public CustomToon customToon;
	[HideInInspector]
	public CampaignPackage customPackage;

	public TextMeshProUGUI nameText, subnameText;
	public Toggle theToggle;

	public void Init( CustomToon tc, ImportPanel parentPanel )
	{
		customToon = tc;
		importPanel = parentPanel;

		nameText.text = customToon.deploymentCard.name;
		subnameText.text = customToon.deploymentCard.subname;
		theToggle.group = importPanel.toggleGroup;
	}

	public void Init( CampaignPackage package, ImportCampaignPanel parentPanel )
	{
		customPackage = package;
		importCampaignPanel = parentPanel;

		nameText.text = !string.IsNullOrEmpty( customPackage.campaignName?.Trim() ) ? customPackage.campaignName.Trim() : DataStore.uiLanguage.uiMainApp.noneUC;
		subnameText.text = $"<color=green>{DataStore.uiLanguage.uiCampaign.itemsUC}</color>: <color=yellow>{customPackage.campaignMissionItems.Count}</color>";
		theToggle.group = importCampaignPanel.toggleGroup;
	}

	public void OnToggle()
	{
		importPanel?.UpdateCard( customToon.deploymentCard );
		if ( theToggle.isOn )
			importCampaignPanel?.ToggleSelected( customPackage );
		else
			importCampaignPanel?.ToggleSelected( null );
	}
}
