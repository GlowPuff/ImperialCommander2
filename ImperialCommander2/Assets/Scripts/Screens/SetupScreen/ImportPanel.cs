using System.Collections.Generic;
using System.Linq;
using Saga;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Modifies the current session with global imports (Imperials)
/// </summary>
public class ImportPanel : MonoBehaviour
{
	public PopupBase popupBase;
	public Text continueText;
	public DynamicCardPrefab cardPrefab;
	public Transform container;
	public ImportItem importItemPrefab, importItemButtonPrefab;
	public ToggleGroup toggleGroup;
	//UI
	public Text villainText, heroText, allyText, imperialText;
	public GameObject buttonContainer;
	public CanvasGroup buttonGroup;
	public Toggle firstToggle;
	public HelpPanel helpPanel;

	HashSet<string> excludedImperialsGUIDs;
	CharacterType displayCharacterMode;

	public void Show()
	{
		EventSystem.current.SetSelectedGameObject( null );
		popupBase.Show();

		continueText.text = DataStore.uiLanguage.uiSetup.continueBtn;
		villainText.text = DataStore.uiLanguage.uiCampaign.villainsUC.ToUpper();
		heroText.text = DataStore.uiLanguage.uiCampaign.heroUC.ToUpper();
		allyText.text = DataStore.uiLanguage.uiCampaign.alliesUC.ToUpper();
		imperialText.text = DataStore.uiLanguage.uiSetup.imperials;//already uppercase

		excludedImperialsGUIDs = new HashSet<string>( DataStore.IgnoredPrefsImports );

		//make sure Imperial tab selected
		firstToggle.SetIsOnWithoutNotify( true );
		ChangeData( firstToggle );
	}

	/// <summary>
	/// use Toggle name, 2=Imperials, 0=Heroes, 1=Allies/Neutral, 3=Villains
	/// </summary>
	public void ChangeData( Toggle item )
	{
		//only process if switched ON
		if ( !item.isOn )
			return;

		if ( item.name == "imperials" )
			displayCharacterMode = CharacterType.Imperial;
		else if ( item.name == "heroes" )
			displayCharacterMode = CharacterType.Hero;

		buttonGroup.interactable = displayCharacterMode == CharacterType.Imperial;
		FilterImports( displayCharacterMode );
	}

	void FilterImports( CharacterType characterType )
	{
		foreach ( Transform item in container )
		{
			Destroy( item.gameObject );
		}

		List<CustomToon> imports = new List<CustomToon>();
		//for Imperials, gather Imperials AND villains
		if ( characterType == CharacterType.Imperial )
		{
			imports = DataStore.globalImportedCharacters
				.Where( x => x.deploymentCard.characterType == CharacterType.Imperial )
				.Concat( DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Villain ) )
				.ToList();
			Debug.Log( $"FOUND {DataStore.IgnoredPrefsImports.Count} IMPORTS TO EXCLUDE" );
		}
		//for Heroes, gather Heroes AND Allies AND Rebels
		else if ( characterType == CharacterType.Hero )
		{
			imports = DataStore.globalImportedCharacters
				.Where( x => x.deploymentCard.characterType == CharacterType.Hero )
				.Concat( DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Ally ) )
				.Concat( DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Rebel ) )
				.ToList();
		}

		foreach ( var item in imports )
		{
			ImportItem import = null;
			//only Imperials use the checkbox prefab
			if ( item.deploymentCard.characterType == CharacterType.Imperial )
				import = Instantiate( importItemPrefab, container );
			else//everything else uses the button prefab
				import = Instantiate( importItemButtonPrefab, container );

			import.Init( item, this );
			//only check toggle if it's imperial
			if ( item.deploymentCard.characterType == CharacterType.Imperial )
			{
				import.theToggle.SetIsOnWithoutNotify( !excludedImperialsGUIDs.Contains( import.customToon.customCharacterGUID.ToString() ) );
			}
		}

		if ( imports.Count > 0 )
		{
			cardPrefab.gameObject.SetActive( true );
			cardPrefab.InitCard( imports[0].deploymentCard, true );
		}
		else
			cardPrefab.gameObject.SetActive( false );
	}

	/// <summary>
	/// Fired when toggle is toggled
	/// </summary>
	public void UpdateCard( DeploymentCard card, ImportItem item )
	{
		//all card types can show their card
		cardPrefab.InitCard( card );

		//but only Imperials have a toggle
		if ( card.characterType == CharacterType.Imperial )
		{
			//add/remove from unique HashSet exclusion list
			if ( !item.theToggle.isOn )
			{
				excludedImperialsGUIDs.Add( item.customToon.customCharacterGUID.ToString() );
			}
			else
			{
				excludedImperialsGUIDs.Remove( item.customToon.customCharacterGUID.ToString() );
			}
		}
	}

	public void CheckAll()
	{
		foreach ( Transform item in container )
		{
			var import = item.GetComponent<ImportItem>();
			import.ToggleIsOn( true );
		}
	}

	public void CheckNone()
	{
		foreach ( Transform item in container )
		{
			var import = item.GetComponent<ImportItem>();
			import.ToggleIsOn( false );
		}
	}

	public void OnClose()
	{
		DataStore.IgnoredPrefsImports = excludedImperialsGUIDs.ToList();
		Debug.Log( $"ADDED {excludedImperialsGUIDs.Count} TO EXCLUSION LIST" );

		popupBase.Close();
	}

	public void OnHelpClick()
	{
		helpPanel.Show();
	}
}
