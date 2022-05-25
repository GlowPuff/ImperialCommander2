using System.IO;
using TMPro;
using UnityEngine;

namespace Saga
{
	public class MissionPickerFolder : MonoBehaviour
	{
		public TextMeshProUGUI folderNameText;

		DirectoryInfo dInfo;

		public void Init( DirectoryInfo di )
		{
			dInfo = di;
			folderNameText.text = dInfo.Name;
		}

		public void OnClick()
		{
			FindObjectOfType<SagaSetup>().missionPicker.OnChangeFolder( dInfo.FullName );
		}
	}
}
