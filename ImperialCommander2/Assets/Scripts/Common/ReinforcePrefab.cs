using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This prefab is used to show reinforcements AND new deployments
/// </summary>
public class ReinforcePrefab : MonoBehaviour
{
	public Image thumbnail, colorPip;
	public Image[] reinforceCounts;
	public Outline outline;
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

		nameText.text = cd.name;

		thumbnail.sprite = Resources.Load<Sprite>( cd.mugShotPath );
		if ( cd.isElite )
			outline.effectColor = Color.red;
		colorPip.color = DataStore.pipColors[cd.colorIndex].ToColor();
	}
}
