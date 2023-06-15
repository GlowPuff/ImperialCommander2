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
	public ImportItem importItemPrefab;
	public ToggleGroup toggleGroup;
	//UI
	public Text villainText, heroText, allyText, imperialText;
	public GameObject buttonContainer;
	public CanvasGroup buttonGroup;
	public Toggle firstToggle;

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
		else if ( item.name == "allies" )
			displayCharacterMode = CharacterType.Ally;
		else if ( item.name == "villains" )
			displayCharacterMode = CharacterType.Villain;
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

		List<CustomToon> imports = DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == characterType ).ToList();

		if ( characterType == CharacterType.Ally )//also show rebels
		{
			imports = imports.Concat( DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Rebel ) ).ToList();
		}

		Debug.Log( $"FOUND {DataStore.IgnoredPrefsImports.Count} IMPORTS TO EXCLUDE" );

		foreach ( var item in imports )
		{
			var import = Instantiate( importItemPrefab, container );
			import.Init( item, this );
			//only check toggle if it's imperial
			if ( characterType == CharacterType.Imperial )
			{
				import.theToggle.SetIsOnWithoutNotify( !excludedImperialsGUIDs.Contains( import.customToon.customCharacterGUID.ToString() ) );
			}
			else
			{
				//everything else is always on
				import.theToggle.SetIsOnWithoutNotify( true );
			}
		}

		if ( imports.Count > 0 )
			cardPrefab.InitCard( imports[0].deploymentCard, true );
		else
			cardPrefab.InitCard( new DeploymentCard() { name = DataStore.uiLanguage.uiSetup.choose, expansion = "Core" }, true );
	}

	/// <summary>
	/// Fired when toggle is toggled
	/// </summary>
	public void UpdateCard( DeploymentCard card, ImportItem item )
	{
		Debug.Log( "toggling" );
		cardPrefab.InitCard( card );
		//if we're NOT showing Imperials, always toggle back ON
		if ( displayCharacterMode != CharacterType.Imperial )
		{
			item.theToggle.SetIsOnWithoutNotify( true );
		}
		else
		{
			//add/remove from  unique HashSet exclusion list
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
			import.theToggle.isOn = true;
		}
	}

	public void CheckNone()
	{
		foreach ( Transform item in container )
		{
			var import = item.GetComponent<ImportItem>();
			import.theToggle.isOn = false;
		}
	}

	public void OnClose()
	{
		DataStore.IgnoredPrefsImports = excludedImperialsGUIDs.ToList();
		Debug.Log( $"ADDED {excludedImperialsGUIDs.Count} TO EXCLUSION LIST" );

		popupBase.Close();
	}
}
