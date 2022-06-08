using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga
{
	public class MissionPickerItem : MonoBehaviour
	{
		public TextMeshProUGUI missionNameText, versionText;

		ProjectItem projectItem;

		public void Init( ProjectItem pi, bool ison, PickerMode mode )
		{
			pi.pickerMode = mode;
			projectItem = pi;
			missionNameText.text = pi.Title;
			if ( mode == PickerMode.Custom )
				versionText.text = $"Version: {pi.fileVersion}";
			else
				versionText.text = pi.missionID;

			GetComponent<Toggle>().isOn = ison;
		}

		public void OnClick()
		{
			if ( GetComponent<Toggle>().isOn )
				FindObjectOfType<SagaSetup>().missionPicker.OnMissionSelected( projectItem );
		}
	}
}
