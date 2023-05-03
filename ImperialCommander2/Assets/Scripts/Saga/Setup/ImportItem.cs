using Saga;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ImportItem : MonoBehaviour
{
	ImportPanel importPanel;

	[HideInInspector]
	public CustomToon customToon;

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

	public void OnToggle()
	{
		importPanel.UpdateCard( customToon.deploymentCard );
	}
}
