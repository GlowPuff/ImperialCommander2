using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga
{
	public class MissionPickerItem : MonoBehaviour
	{
		public TextMeshProUGUI missionNameText, versionText;

		ProjectItem projectItem;

		public void Init( ProjectItem pi, bool ison )
		{
			projectItem = pi;
			missionNameText.text = pi.Title;
			versionText.text = $"Version: {pi.fileVersion}";
			GetComponent<Toggle>().isOn = ison;
		}

		public void OnClick()
		{
			if ( GetComponent<Toggle>().isOn )
				FindObjectOfType<SagaSetup>().missionPicker.OnMissionSelected( projectItem );
		}
	}
}
