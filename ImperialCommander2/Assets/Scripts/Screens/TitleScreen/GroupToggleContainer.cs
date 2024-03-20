using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// DEPRECATED, CLASSIC MODE IS NOW REMOVED
/// </summary>

//Handle toggling ENEMY GROUPS and VILLAINS ONLY
public class GroupToggleContainer : MonoBehaviour
{
	public ExpansionController expansionController;
	public DynamicCardPrefab cardPrefab;

	List<DeploymentCard> deploymentCards;//?
	List<DeploymentCard> enemyCards;
	int groupIndex;//0=starting, 1=reserved, 2=villains, 3=ignored
	Toggle[] buttonToggles;
	Expansion selectedExpansion;
	Sound sound;

	private void Awake()
	{
		sound = FindObjectOfType<Sound>();

		buttonToggles = new Toggle[transform.childCount];
		for ( int i = 0; i < transform.childCount; i++ )
			buttonToggles[i] = transform.GetChild( i ).GetComponent<Toggle>();
	}

	public void OnToggle( Toggle toggle )
	{
		int index = int.Parse( toggle.name.Substring( 8 ).TrimEnd( ')' ) );

		EventSystem.current.SetSelectedGameObject( null );
		//checking for Active makes sure this code does NOT run when the Toggle is INACTIVE
		if ( !buttonToggles[index].gameObject.activeInHierarchy )
			return;

		sound.PlaySound( FX.Click );
		cardPrefab.gameObject.SetActive( true );

		var id = int.Parse( enemyCards[index].id.Substring( 2 ).TrimStart( '0' ) );

		if ( enemyCards[index].id == "DG070" )
			cardPrefab.gameObject.SetActive( false );


		//if ( buttonToggles[index].isOn )
		//{
		//	DataStore.sessionData.selectedDeploymentCards[groupIndex].Add( enemyCards[index] );
		//	cardPrefab.InitCard( enemyCards[index] );
		//}
		//else
		//{
		//	DataStore.sessionData.selectedDeploymentCards[groupIndex].Remove( enemyCards[index] );
		//	cardPrefab.gameObject.SetActive( false );
		//}

		expansionController.UpdateText( (int)selectedExpansion, buttonToggles.Count( x => x.isOn ) );
	}

	public void OnChangeExpansion( string expansion )
	{
		EventSystem.current.SetSelectedGameObject( null );
		Enum.TryParse( expansion, out selectedExpansion );

		cardPrefab.gameObject.SetActive( false );
		foreach ( Transform c in transform )
		{
			c.gameObject.SetActive( false );
			c.GetComponent<Toggle>().isOn = false;
			c.GetComponent<Toggle>().interactable = true;
		}

		//0=starting, 1=reserved, 2=villains, 3=ignored
		if ( groupIndex == 0 || groupIndex == 1 )
		{
			deploymentCards = new List<DeploymentCard>( DataStore.deploymentCards.Concat( DataStore.villainCards ).ToList() );
		}
		else if ( groupIndex == 2 )
			deploymentCards = DataStore.villainCards;
		else if ( groupIndex == 3 )
			deploymentCards = DataStore.deploymentCards;

		DeploymentCard custom = new DeploymentCard() { cost = 0, expansion = "Other", name = "Custom Group", faction = "None", id = "DG070", ignored = "", priority = 2, rcost = 0, size = 1, tier = 1, mugShotPath = "CardThumbnails/customToken", isElite = false };

		enemyCards = deploymentCards.Where( x => x.expansion == expansion ).ToList();
		if ( expansion == "Other" )
			enemyCards.Add( custom );
		//List<DeploymentCard> prevSelected = DataStore.sessionData.selectedDeploymentCards[groupIndex];

		for ( int i = 0; i < enemyCards.Count; i++ )
		{
			var child = transform.GetChild( i );
			//switch on if previously selected
			//do it while Toggle is INACTIVE so OnToggle code doesn't run
			//if ( prevSelected.ContainsCard( enemyCards[i] ) )
			//	buttonToggles[i].isOn = true;
			child.gameObject.SetActive( true );//re-enable the Toggle

			var id = int.Parse( enemyCards[i].id.Substring( 2 ).TrimStart( '0' ) );

			//set the thumbnail texture
			var thumb = child.Find( "Image" );
			if ( id == 70 )
				thumb.GetComponent<Image>().sprite = Resources.Load<Sprite>( custom.mugShotPath );
			else
				thumb.GetComponent<Image>().sprite = Resources.Load<Sprite>( enemyCards[i].mugShotPath );
			if ( enemyCards[i].isElite || id > 70 )
				thumb.GetComponent<Image>().color = new Color( 1, .5f, .5f, 1 );
			else
				thumb.GetComponent<Image>().color = new Color( 1, 1, 1, 1 );

			//if an enemy is already in another group index (Initial, Reserved, etc), disable the toggle so the enemy can't be added to 2 different groups
			//ie: can't put same enemy into both Initial and Reserved
			if ( IsInGroup( enemyCards[i] ) )
			{
				buttonToggles[i].interactable = false;
				if ( !enemyCards[i].isElite )
					thumb.GetComponent<Image>().color = new Color( 1, 1, 1, .35f );
				else
					thumb.GetComponent<Image>().color = new Color( 1, .5f, .5f, .35f );
			}
		}

		expansionController.UpdateText( (int)selectedExpansion, buttonToggles.Count( x => x.isOn ) );
	}

	public void ResetUI( int dataGroupIndex )
	{
		cardPrefab.gameObject.SetActive( false );
		//0=starting, 1=reserved, 2=villains, 3=ignored, 4=heroes
		groupIndex = dataGroupIndex;
		transform.parent.parent.parent.parent.Find( "expansion selector container" ).Find( "Core" ).GetComponent<Toggle>().isOn = true;
		OnChangeExpansion( "Core" );
	}

	public bool IsInGroup( DeploymentCard cd )
	{
		bool found = false;

		for ( int i = 0; i < 5; i++ )
		{
			if ( groupIndex != i )
			{
				//if ( DataStore.sessionData.selectedDeploymentCards[i].ContainsCard( cd ) )
				//	found = true;
			}
		}

		return found;
	}
}
