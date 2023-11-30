using UnityEngine;

public class HelpMeta : MonoBehaviour
{
	public string elementID;
	public bool isHidden = false;
	[HideInInspector]
	public HelpPanel helpPanel;

	public void OnClick()
	{
		helpPanel.OnHelpRequest( elementID );
	}
}
