using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GenericChooser : MonoBehaviour
{
	public CanvasGroup cg;
	public Image fader;
	public Sound sound;
	public CardDescriptor selectedCard;
	public TextMeshProUGUI nameText;
	public Text closeText;

	Image[] images;
	string imagePath;
	List<CardDescriptor> cardDescriptors;
	ChooserMode chooserMode;
	List<CardDescriptor> cardSet;
	int selectedIndex = -1;
	Action<CardDescriptor> callBack;

	public void Show( ChooserMode mode, List<CardDescriptor> cards, Action<CardDescriptor> callback )
	{
		gameObject.SetActive( true );
		fader.color = new Color( 0, 0, 0, 0 );
		fader.DOFade( .95f, 1 );
		cg.alpha = 0;
		cg.DOFade( 1, .5f );

		closeText.text = DataStore.uiLanguage.uiMainApp.close;

		//hide expansion buttons not owned, skipping Core and Other
		Transform exp = transform.Find( "Panel/expansion selector container" );
		//toggle Core button ON
		exp.GetChild( 0 ).GetComponent<Toggle>().isOn = true;
		for ( int i = 1; i < exp.childCount - 1; i++ )
		{
			if ( DataStore.ownedExpansions.Contains( (Expansion)Enum.Parse( typeof( Expansion ), exp.GetChild( i ).name ) ) )
				exp.GetChild( i ).gameObject.SetActive( true );
			else
				exp.GetChild( i ).gameObject.SetActive( false );
		}

		callBack = callback;
		chooserMode = mode;

		//add custom group IF mode != ally/hero
		CardDescriptor custom = new CardDescriptor() { cost = 0, expansion = "Other", name = "Custom Group", faction = "None", id = "DG070", ignored = "", priority = 2, rcost = 0, size = 1, tier = 1 };

		cardDescriptors = new List<CardDescriptor>( cards );
		if ( mode == ChooserMode.DeploymentGroups && !cardDescriptors.Any( x => x.id == "DG070" ) )
			cardDescriptors.Add( custom );

		OnChangeExpansion( "Core" );
	}

	public void OnClose()
	{
		sound.PlaySound( FX.Click );
		fader.DOFade( 0, .25f ).OnComplete( () => gameObject.SetActive( false ) );
		cg.DOFade( 0, .25f );
		callBack.Invoke( selectedCard );
	}

	public void OnToggle( int index )
	{
		EventSystem.current.SetSelectedGameObject( null );
		Transform grid = transform.Find( "Panel/mugshot grid" );
		if ( !grid.GetChild( index ).gameObject.activeInHierarchy )
			return;

		sound.PlaySound( FX.Click );
		if ( selectedIndex == index )
		{
			selectedIndex = -1;
			selectedCard = null;
			nameText.text = "";
		}
		else
		{
			selectedIndex = index;
			selectedCard = cardSet[index];
			nameText.text = cardSet[index].name;
			Debug.Log( cardSet[index].name );
		}
		//Debug.Log( "selectedIndex: " + selectedIndex );
	}

	public void OnChangeExpansion( string expansion )
	{
		sound.PlaySound( FX.Click );

		selectedIndex = -1;
		selectedCard = null;
		nameText.text = "";

		//toggle off and hide all buttons
		Transform grid = transform.Find( "Panel/mugshot grid" );
		foreach ( Transform c in grid )
		{
			c.gameObject.SetActive( false );
			c.GetComponent<Toggle>().isOn = false;
		}

		//filter to selected expansion
		cardSet = new List<CardDescriptor>();
		cardSet = cardDescriptors.Where( x => x.expansion == expansion ).ToList();
		images = new Image[cardSet.Count];

		if ( cardSet.Count > 40 )
		{
			Debug.Log( "ERROR: Requested number of thumbnails exceeds number of images available." );
			return;
		}

		//Debug.Log( "CARD COUNT: " + cardSet.Count );

		//populate thumbnail with images
		for ( int i = 0; i < cardSet.Count; i++ )
		{
			if ( chooserMode == ChooserMode.DeploymentGroups )
			{
				if ( DataStore.villainCards.cards.Contains( cardSet[i] ) )
					imagePath = $"Cards/Villains/{cardSet[i].id.Replace( "DG", "M" )}";
				else
					imagePath = $"Cards/Enemies/{cardSet[i].expansion}/{cardSet[i].id.Replace( "DG", "M" )}";
			}
			else if ( chooserMode == ChooserMode.Ally )
				imagePath = $"Cards/Allies/{cardSet[i].id.Replace( "A", "M" )}";

			images[i] = grid.GetChild( i ).Find( "Image" ).GetComponent<Image>();
			images[i].sprite = Resources.Load<Sprite>( imagePath );
			if ( cardSet[i].isElite )
				images[i].color = new Color( 1, .29f, .29f, 1 );
			else
				images[i].color = Color.white;
			//set active LAST so OnToggle won't run
			grid.GetChild( i ).gameObject.SetActive( true );

			//var child = grid.GetChild( i );
			//child.gameObject.SetActive( true );
			//var label = child.Find( "Label" );
			//label.GetComponent<Text>().text = enemyCards[i].name.ToLower();
		}
	}

	private void Update()
	{
		if ( Input.GetKeyDown( KeyCode.Space ) )
			OnClose();
	}
}
