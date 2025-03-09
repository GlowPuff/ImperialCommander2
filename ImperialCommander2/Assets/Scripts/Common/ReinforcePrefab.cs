using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This prefab is used to show reinforcements AND new deployments
/// </summary>
public class ReinforcePrefab : MonoBehaviour
{
	public Image thumbnail, colorPip, outlineImage;
	public Image[] reinforceCounts;
	public TextMeshProUGUI nameText;

	public void Init( DeploymentCard cd, int add = 0 )
	{
		//reset
		for ( int i = 0; i < 3; i++ )
		{
			reinforceCounts[i].color = Color.red;
			reinforceCounts[i].gameObject.SetActive( false );
		}
		for ( int i = 0; i < cd.size; i++ )
			reinforceCounts[i].gameObject.SetActive( true );
		nameText.text = "";
		thumbnail.sprite = null;
		colorPip.color = Color.white;

		for ( int i = 0; i < Mathf.Min( cd.size, cd.currentSize + add ); i++ )
			reinforceCounts[i].color = Color.green;

		string groupName = cd.name;
		if ( DataStore.gameType == GameType.Saga )
		{
			outlineImage.color = Saga.Utils.String2UnityColor( cd.deploymentOutlineColor );
			//get overridden name
			var ovrd = DataStore.sagaSessionData.gameVars.GetDeploymentOverride( cd.id );
			if ( ovrd != null && ovrd.isCustomDeployment )
				cd = ovrd.customCard;

			if ( ovrd != null )
				groupName = ovrd.nameOverride;
			else
				groupName = cd.name;
		}
		nameText.text = groupName;

		thumbnail.sprite = Resources.Load<Sprite>( cd.mugShotPath );
		if ( cd.isElite )
			outlineImage.color = Color.red;
		colorPip.color = DataStore.pipColors[cd.GetColorIndex()].ToColor();
	}
}
