using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugObject : MonoBehaviour
{
	public Image thumb;
	public TextMeshProUGUI cardName, cardCost, cardCostHeading;

	DeploymentCard cardDescriptor;

	public void Init( DeploymentCard cd )
	{
		cardDescriptor = cd;

		thumb.sprite = Resources.Load<Sprite>( cd.mugShotPath );
		cardName.text = cd.name;
		cardCost.text = cd.cost.ToString();
		cardCostHeading.text = DataStore.uiLanguage.uiMainApp.depCostUC + ":";
	}

	public void OnRemove()
	{
		DataStore.deploymentHand.Remove( cardDescriptor );
		//add card into manual deployment list, then sort list
		if ( !DataStore.manualDeploymentList.ContainsCard( cardDescriptor ) )
		{
			DataStore.manualDeploymentList.Add( cardDescriptor );
			DataStore.SortManualDeployList();
		}

		Destroy( gameObject );
	}
}
