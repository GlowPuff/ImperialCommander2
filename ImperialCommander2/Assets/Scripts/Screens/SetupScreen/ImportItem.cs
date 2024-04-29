using Saga;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class is used on ImportItem AND ImportItemButton prefabs
/// </summary>
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
		if ( theToggle != null )//null if this is on a ImportItemButton prefab
			theToggle.group = importPanel.toggleGroup;
	}

	public void Init( CampaignPackage package, ImportCampaignPanel parentPanel )
	{
		customPackage = package;
		importCampaignPanel = parentPanel;

		nameText.text = !string.IsNullOrEmpty( customPackage.campaignName?.Trim() ) ? customPackage.campaignName.Trim() : DataStore.uiLanguage.uiMainApp.noneUC;
		subnameText.text = $"<color=green>{DataStore.uiLanguage.uiCampaign.itemsUC}</color>: <color=yellow>{customPackage.campaignMissionItems.Count}</color>";
		//toggle is never null for campaign package prefab (ImportItem)
		theToggle.group = importCampaignPanel.toggleGroup;
	}

	public void OnToggle()
	{
		//only used for imported imperials
		importPanel?.UpdateCard( customToon.deploymentCard, this );

		//only used for imported campaign packages
		if ( theToggle != null && theToggle.isOn )
			importCampaignPanel?.ToggleSelected( customPackage );
		else
			importCampaignPanel?.ToggleSelected( null );
	}

	public void OnClick()
	{
		//only used for imported heroes/allies/villains
		importPanel?.UpdateCard( customToon.deploymentCard, this );
	}

	public void ToggleIsOn( bool isOn )
	{
		//only Imperials have an active Toggle button
		if ( theToggle != null )
			theToggle.isOn = isOn;
	}
}
