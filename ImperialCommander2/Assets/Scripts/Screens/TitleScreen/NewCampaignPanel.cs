using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga
{
	public class NewCampaignPanel : MonoBehaviour
	{
		public PopupBase popupBase;
		public TMP_Dropdown campaignExpansionDropdown;
		public TMP_InputField campaignNameInputField;
		public TextMeshProUGUI placeholderText, importedCampaignNameText;
		public Text startText, cancelText, importCampaignBtn;
		public Button startButton;
		public Toggle customToggle;
		public ImportCampaignPanel campaignPanel;
		public Image packageSprite;

		Action callback;
		string selectedExpansion;
		List<string> selectedExpansionList, expansionCode;
		bool nameGood, importFuncDoRemove;
		CampaignPackage selectedCampaignPackage;

		public void Show( Action onClose )
		{
			campaignNameInputField.text = "";
			startText.text = DataStore.uiLanguage.sagaUISetup.setupStartBtn;
			cancelText.text = DataStore.uiLanguage.uiSetup.cancel;
			placeholderText.text = DataStore.uiLanguage.uiCampaign.campaignNameUC;
			importCampaignBtn.text = DataStore.uiLanguage.sagaUISetup.importBtn;

			selectedCampaignPackage = null;
			importFuncDoRemove = false;
			callback = onClose;
			nameGood = false;
			selectedExpansion = "Core";
			campaignExpansionDropdown.value = 0;

			packageSprite.gameObject.SetActive( false );

			//populate expansion dropdown
			campaignExpansionDropdown.ClearOptions();
			selectedExpansionList = DataStore.ownedExpansions.Select( x => DataStore.missionCards[x.ToString()][0].expansionText ).ToList();
			expansionCode = DataStore.ownedExpansions.Select( x => x.ToString() ).ToList();
			campaignExpansionDropdown.AddOptions( selectedExpansionList );

			popupBase.Show();
		}

		public void OnEditCampaignName()
		{
			if ( !string.IsNullOrEmpty( campaignNameInputField.text ) )
				nameGood = true;
			else
				nameGood = false;

			startButton.interactable = nameGood;
		}

		public void OnExpansionChanged()
		{
			selectedExpansion = expansionCode[campaignExpansionDropdown.value];

			startButton.interactable = nameGood;
		}

		public void OnCustomToggle()
		{
			campaignExpansionDropdown.interactable = !customToggle.isOn;
		}

		public void StartCampaign()
		{
			//create and save the new campaign
			Close( false );
			if ( selectedCampaignPackage != null )
			{
				var c = SagaCampaign.CreateNewImportedCampaign( campaignNameInputField.text, selectedCampaignPackage );
				c.SaveCampaignState();
				FindObjectOfType<TitleController>().NavToCampaignScreen( c );
			}
			else
			{
				var c = SagaCampaign.CreateNewCampaign( campaignNameInputField.text,
					!customToggle.isOn ? selectedExpansion : "Custom" );
				c.SaveCampaignState();
				FindObjectOfType<TitleController>().NavToCampaignScreen( c );
			}
		}

		public void OnImportCampaign()
		{
			if ( importFuncDoRemove )
				OnRemoveImportedCampaign();
			else
			{
				campaignPanel.Show( () =>
				{
					if ( campaignPanel.selectedPackage != null )
					{
						selectedCampaignPackage = campaignPanel.selectedPackage;
						importedCampaignNameText.text = selectedCampaignPackage.campaignName;
						campaignExpansionDropdown.interactable = false;
						customToggle.interactable = false;
						importCampaignBtn.text = DataStore.uiLanguage.uiCampaign.removeUC.ToUpper();
						importFuncDoRemove = true;
						//generate icon sprite from loaded bytes
						Texture2D tex = new Texture2D( 2, 2 );
						if ( tex.LoadImage( selectedCampaignPackage.iconBytesBuffer ) )
						{
							packageSprite.gameObject.SetActive( true );
							Sprite iconSprite = Sprite.Create( tex, new Rect( 0, 0, tex.width, tex.height ), new Vector2( 0, 0 ), 100f );
							packageSprite.sprite = iconSprite;
						}
					}
					else
						selectedCampaignPackage = null;
				} );
			}
		}

		public void OnRemoveImportedCampaign()
		{
			packageSprite.gameObject.SetActive( false );
			importedCampaignNameText.text = "...";
			campaignExpansionDropdown.interactable = true;
			customToggle.interactable = true;
			importCampaignBtn.text = DataStore.uiLanguage.sagaUISetup.importBtn;
			importFuncDoRemove = false;
			selectedCampaignPackage = null;
		}

		public void Close( bool doCallback = true )
		{
			popupBase.Close();
			if ( doCallback )
				callback?.Invoke();
		}
	}
}