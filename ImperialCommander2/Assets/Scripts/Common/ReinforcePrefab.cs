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

	public void Init( CardDescriptor cd, int add = 0 )
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

		if ( DataStore.villainCards.cards.Contains( cd ) )
		{
			thumbnail.sprite = Resources.Load<Sprite>( $"Cards/Villains/{cd.id.Replace( "DG", "M" )}" );
			outline.effectColor = Color.red;
		}
		else
		{
			thumbnail.sprite = Resources.Load<Sprite>( $"Cards/Enemies/{cd.expansion}/{cd.id.Replace( "DG", "M" )}" );
			if ( cd.name.Contains( "Elite" ) )
				outline.effectColor = Color.red;
		}
		colorPip.color = DataStore.pipColors[cd.colorIndex].ToColor();
	}
}
