using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Saga
{
	public class ModifyCustomPropsPopup : MonoBehaviour
	{
		public PopupBase popupBase;
		public Text cancelButtonText, continueButtonText, threatValue, threatText;
		public TMP_Dropdown tierDropdown;
		public Toggle agendaToggle;
		public MWheelHandler wheeler;
		public TextMeshProUGUI agendaText;

		Action<CampaignModify> addCallback;
		CampaignModify campaignModify;

		public void Show( CampaignModify modifier, Action<CampaignModify> callback )
		{
			EventSystem.current.SetSelectedGameObject( null );
			popupBase.Show();

			//translations
			threatText.text = DataStore.uiLanguage.uiCampaign.threat;
			agendaText.text = $"+ {DataStore.uiLanguage.uiCampaign.agendaUC}";
			cancelButtonText.text = DataStore.uiLanguage.uiSetup.cancel;
			continueButtonText.text = DataStore.uiLanguage.uiSetup.continueBtn;
			var s = modifier.itemTierArray.Select( x => $"{DataStore.uiLanguage.uiCampaign.tierUC} " + x );
			tierDropdown.itemText.text = s.Aggregate( ( acc, cur ) => acc + ", " + cur );

			wheeler.ResetWheeler( modifier.threatValue );
			tierDropdown.value = GetValue( modifier.itemTierArray );
			agendaToggle.isOn = modifier.agendaToggle;

			addCallback = callback;
			campaignModify = modifier;
		}

		string[] GetItems( int value )
		{
			string[] items = new string[0];
			switch ( value )
			{
				case 0:
					items = new string[] { "1" };
					break;
				case 1:
					items = new string[] { "2" };
					break;
				case 2:
					items = new string[] { "3" };
					break;
				case 3:
					items = new string[] { "1", "2" };
					break;
				case 4:
					items = new string[] { "2", "3" };
					break;
				case 5:
					items = new string[] { "1", "2", "3" };
					break;
			}
			return items;
		}

		int GetValue( string[] array )
		{
			if ( array.Contains( "1" ) && array.Contains( "2" ) && array.Contains( "3" ) )
				return 5;
			if ( array.Contains( "2" ) && array.Contains( "3" ) )
				return 4;
			if ( array.Contains( "1" ) && array.Contains( "2" ) )
				return 3;
			if ( array.Contains( "3" ) )
				return 2;
			if ( array.Contains( "2" ) )
				return 1;
			if ( array.Contains( "1" ) )
				return 0;

			return 0;
		}

		public void OnContinue()
		{
			popupBase.Close();

			campaignModify.itemTierArray = GetItems( tierDropdown.value );
			campaignModify.threatValue = wheeler.wheelValue;
			campaignModify.agendaToggle = agendaToggle.isOn;

			addCallback?.Invoke( campaignModify );
		}

		public void Cancel()
		{
			popupBase.Close();
		}
	}
}