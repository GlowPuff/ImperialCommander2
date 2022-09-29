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
		public Text startText, cancelText;
		public Button startButton;
		public Toggle customToggle;

		Action callback;
		string selectedExpansion;
		List<string> selectedExpansionList, expansionCode;
		bool nameGood;

		public void Show( Action onClose )
		{
			campaignNameInputField.text = "";
			startText.text = DataStore.uiLanguage.sagaUISetup.setupStartBtn;
			cancelText.text = DataStore.uiLanguage.uiSetup.cancel;
			callback = onClose;
			nameGood = false;
			selectedExpansion = "Core";
			campaignExpansionDropdown.value = 0;

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
			var c = SagaCampaign.CreateNewCampaign( campaignNameInputField.text,
				!customToggle.isOn ? selectedExpansion : "Custom" );
			FindObjectOfType<TitleController>().NavToCampaignScreen( c );
			c.SaveCampaignState();
		}

		public void Close( bool doCallback = true )
		{
			popupBase.Close();
			if ( doCallback )
				callback?.Invoke();
		}
	}
}