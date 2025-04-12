using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Saga
{
	public class ImportCampaignPanel : MonoBehaviour
	{
		public PopupBase popupBase;
		public Text importBtnText, cancelBtnText;
		public Transform container;
		public ImportItem importItemPrefab;
		public ToggleGroup toggleGroup;
		public Button importButton;
		public CampaignPackage selectedPackage;

		Action callback = null;

		public void Show( Action cb )
		{
			importBtnText.text = DataStore.uiLanguage.sagaUISetup.importBtn;
			cancelBtnText.text = DataStore.uiLanguage.uiMainApp.cancel;
			callback = cb;

			selectedPackage = null;

			popupBase.Show();

			PopulateList();
		}

		private async void PopulateList()
		{
			foreach ( Transform item in container )
			{
				Destroy( item.gameObject );
			}

			try
			{
				var imports = new List<CampaignPackage>();
				await Task.Run( () =>
				{

					if ( DataStore.Language.ToUpper() == "EN" )
					{
						//do a quick load without deserializing any of the missions
						imports = FileManager.GetCampaignPackageList( true );
					}
					else
					{
						//loading the missions in order to also extract the translated names
						imports = FileManager.GetCampaignPackageList( false );
					}
				} );

				foreach ( var item in imports )
				{
					var import = Instantiate( importItemPrefab, container );
					import.Init( item, this );
				}
			}
			catch ( Exception e )
			{
				Utils.LogError( $"PopulateList()::Error populating campaign imports list\n{e.Message}" );
				foreach ( Transform item in container )
				{
					Destroy( item.gameObject );
				}
			}
		}

		public void ToggleSelected( CampaignPackage p )
		{
			selectedPackage = p;
			importButton.interactable = p != null;
		}

		public void OnImport()
		{
			popupBase.Close();
			callback?.Invoke();
		}

		public void Close()//cancel
		{
			selectedPackage = null;
			popupBase.Close();
			callback?.Invoke();
		}
	}
}
