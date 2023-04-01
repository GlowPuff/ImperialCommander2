using System;
using UnityEngine;
using UnityEngine.UI;

namespace Saga
{
	public class ContinueCampaignPanel : MonoBehaviour
	{
		public PopupBase popupBase;
		public Text startText, cancelText, titleText;
		public CampaignTogglePrefab campaignTogglePrefab;
		public GameObject toggleContainer;
		public Button startButton;

		Action callback;
		Guid selectedCampaign;

		public void Show( Action onClose )
		{
			startText.text = DataStore.uiLanguage.sagaUISetup.setupStartBtn;
			cancelText.text = DataStore.uiLanguage.uiSetup.cancel;
			titleText.text = DataStore.uiLanguage.uiTitle.loadCampaign;
			callback = onClose;

			selectedCampaign = Guid.Empty;

			//popuplate existing campaigns
			foreach ( Transform item in toggleContainer.transform )
				Destroy( item.gameObject );
			var clist = FileManager.GetCampaigns();
			var toggleGroup = toggleContainer.GetComponent<ToggleGroup>();
			foreach ( var item in clist )
			{
				var go = Instantiate( campaignTogglePrefab, toggleContainer.transform );
				go.GetComponent<CampaignTogglePrefab>().Init( item, toggleGroup, OnToggleCallback );
			}

			startButton.interactable = false;

			popupBase.Show();
		}

		public void StartCampaign()
		{
			Close( false );
			if ( selectedCampaign != Guid.Empty )
			{
				var c = SagaCampaign.LoadCampaignState( selectedCampaign );
				c.FixExpansionCodes();
				FindObjectOfType<TitleController>().NavToCampaignScreen( c );
			}
		}

		public void Close( bool doCallback = true )
		{
			popupBase.Close();
			if ( doCallback )
				callback?.Invoke();
		}

		void OnToggleCallback( CampaignTogglePrefab t )
		{
			startButton.interactable = t.GetComponent<Toggle>().isOn;
			if ( t.GetComponent<Toggle>().isOn )
				selectedCampaign = t.campaignGUID;
		}
	}
}