using System;
using TMPro;
using UnityEngine;

namespace Saga
{
	public class ForceMissionItemPrefab : MonoBehaviour
	{
		public TextMeshProUGUI threatInfo, addForcedMission;

		Action callback;

		public void Init( Action addCallback )
		{
			threatInfo.text = DataStore.uiLanguage.uiCampaign.threatInfoUC;
			addForcedMission.text = DataStore.uiLanguage.uiCampaign.addForcedMissionUC;

			callback = addCallback;
		}

		public void OnAddClick()
		{
			callback?.Invoke();
		}
	}
}