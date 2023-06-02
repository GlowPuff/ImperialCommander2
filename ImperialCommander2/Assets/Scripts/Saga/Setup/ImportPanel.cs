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

	public void Show()
	{
		EventSystem.current.SetSelectedGameObject( null );
		popupBase.Show();

		continueText.text = DataStore.uiLanguage.uiSetup.continueBtn;

		FilterImports( CharacterType.Imperial );
	}

	//public void ShowViewer(List<DeploymentCard> selected, CharacterType ctype, Action cb<List<DeploymentCard>> = null )
	//{
	//	EventSystem.current.SetSelectedGameObject( null );
	//	popupBase.Show();

	//selectedGroups = new List<DeploymentCard>( selected );
	//	continueText.text = DataStore.uiLanguage.uiSetup.continueBtn;
	//	callback = cb;

	//	FilterImports( ctype );

	//	cardPrefab.InitCard( new DeploymentCard() { name = DataStore.uiLanguage.uiSetup.choose, expansion = "Core" } );
	//}

	void FilterImports( CharacterType characterType )
	{
		foreach ( Transform item in container )
		{
			Destroy( item.gameObject );
		}

		List<CustomToon> imports = DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == characterType ).ToList();

		Debug.Log( $"FOUND {DataStore.IgnoredPrefsImports.Count} IMPORTS TO EXCLUDE" );

		foreach ( var item in imports )
		{
			var import = Instantiate( importItemPrefab, container );
			import.Init( item, this );
			import.theToggle.isOn = !DataStore.IgnoredPrefsImports.Contains( import.customToon.customCharacterGUID.ToString() );
		}

		if ( imports.Count > 0 )
			cardPrefab.InitCard( imports[0].deploymentCard, true );
		else
			cardPrefab.InitCard( new DeploymentCard() { name = DataStore.uiLanguage.uiSetup.choose, expansion = "Core" }, true );
	}

	public void UpdateCard( DeploymentCard card )
	{
		cardPrefab.InitCard( card );
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
		List<string> excluded = new List<string>();
		//add excluded imports
		foreach ( Transform item in container )
		{
			var import = item.GetComponent<ImportItem>();
			if ( !import.theToggle.isOn )
				excluded.Add( import.customToon.customCharacterGUID.ToString() );
		}

		DataStore.IgnoredPrefsImports = excluded;
		Debug.Log( $"ADDED {excluded.Count} TO EXCLUSION LIST" );

		popupBase.Close();
	}
}
