using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga
{
	public class CampaignTogglePrefab : MonoBehaviour
	{
		public TextMeshProUGUI nameText, expansionText;
		[HideInInspector]
		public Guid campaignGUID;

		Action<CampaignTogglePrefab> callback;

		public void Init( SagaCampaign c, ToggleGroup tg, Action<CampaignTogglePrefab> cb )
		{
			nameText.text = c.campaignName;
			if ( c.campaignExpansionCode != "Custom" )
				expansionText.text = DataStore.translatedExpansionNames[c.campaignExpansionCode];
			else
				expansionText.text = "Custom";
			campaignGUID = c.GUID;
			callback = cb;
			GetComponent<Toggle>().group = tg;
		}

		public void OnToggle()
		{
			callback?.Invoke( this );
		}

		public void OnDelete()
		{
			var confirmDeletePopup = GlowEngine.FindUnityObject<ConfirmDeletePopup>();

			confirmDeletePopup.Show( nameText.text, () =>
				{
					FileManager.DeleteCampaign( campaignGUID );
					Destroy( gameObject );
					FindObjectOfType<ContinueCampaignPanel>().startButton.interactable = false;
					FindObjectOfType<TitleController>().DeleteCampaignState( campaignGUID );
				} );
		}
	}
}
