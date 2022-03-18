using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//Handle toggling HEROES and ALLY ONLY
public class HeroAllyToggleContainer : MonoBehaviour
{
	[HideInInspector]
	public CardDescriptor selectedHero;
	public TextMeshProUGUI enemyNameText;

	List<CardDescriptor> heroCards = new List<CardDescriptor>();
	Toggle[] buttonToggles;
	ChooserMode chooserMode;
	Sound sound;

	private void Awake()
	{
		sound = FindObjectOfType<Sound>();

		buttonToggles = new Toggle[transform.childCount];
		for ( int i = 0; i < transform.childCount; i++ )
		{
			buttonToggles[i] = transform.GetChild( i ).GetComponent<Toggle>();
		}
	}

	public void ResetUI( ChooserMode mode )
	{
		chooserMode = mode;
		if ( mode == ChooserMode.Ally )//reset ally to none
			DataStore.sessionData.selectedAlly = null;

		selectedHero = null;
		enemyNameText.text = "";

		//reset to show Core expansion
		transform.parent.parent.parent.parent.Find( "expansion selector container" ).Find( "Core" ).GetComponent<Toggle>().isOn = true;
		OnChangeExpansion( "Core" );
	}

	public void OnChangeExpansion( string expansion )
	{
		EventSystem.current.SetSelectedGameObject( null );

		selectedHero = null;
		enemyNameText.text = "";

		//disable all toggle buttons
		foreach ( Transform c in transform )
		{
			c.gameObject.SetActive( false );
			c.GetComponent<Toggle>().isOn = false;
		}

		//only get card list of chosen expansion
		if ( chooserMode == ChooserMode.Hero )
			heroCards = DataStore.heroCards.cards.Where( x => x.expansion == expansion ).ToList();
		else if ( chooserMode == ChooserMode.Ally )
			heroCards = DataStore.allyCards.cards.Where( x => x.expansion == expansion ).ToList();

		Sprite thumbNail = null;

		//activate toggle btns and change label for each card in list
		for ( int i = 0; i < heroCards.Count; i++ )
		{
			var child = transform.GetChild( i );
			child.gameObject.SetActive( true );

			thumbNail = Resources.Load<Sprite>( $"Cards/Allies/{heroCards[i].id.Replace( heroCards[i].id[0], 'M' )}" );
			var thumb = child.Find( "Image" );
			thumb.GetComponent<Image>().sprite = thumbNail;
			if ( !heroCards[i].isElite )
				thumb.GetComponent<Image>().color = new Color( 1, 1, 1, 1 );
			else
				thumb.GetComponent<Image>().color = new Color( 1, .5f, .5f, 1 );
		}
	}

	public void OnToggle( int index )
	{
		EventSystem.current.SetSelectedGameObject( null );
		//checking for Active makes sure this code does NOT run when the Toggle is INACTIVE
		if ( !buttonToggles[index].gameObject.activeInHierarchy )
			return;

		sound.PlaySound( FX.Click );

		if ( buttonToggles[index].isOn )
		{
			selectedHero = heroCards[index];
			enemyNameText.text = heroCards[index].name;
		}
		else
		{
			if ( selectedHero == heroCards[index] )
				selectedHero = null;
		}
	}

	public void OnBack()
	{
		if ( chooserMode == ChooserMode.Hero )
		{
			if ( selectedHero != null && !DataStore.sessionData.selectedDeploymentCards[4].cards.Contains( selectedHero ) )
				DataStore.sessionData.selectedDeploymentCards[4].cards.Add( selectedHero );
		}
		else if ( chooserMode == ChooserMode.Ally )
		{
			if ( selectedHero != null )
				DataStore.sessionData.selectedAlly = selectedHero;
		}
		FindObjectOfType<GroupChooserScreen>().OnBack();
	}

	private void Update()
	{
		if ( Input.GetKeyDown( KeyCode.Space ) )
			OnBack();
	}
}
