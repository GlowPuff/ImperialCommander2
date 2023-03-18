using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga
{
	public class MissionPickerItem : MonoBehaviour
	{
		public TextMeshProUGUI missionNameText, versionText;
		public Toggle pickerToggle;

		ProjectItem projectItem;
		bool pauseClick;

		public void Init( ProjectItem pi, bool ison, PickerMode mode )
		{
			pi.pickerMode = mode;
			projectItem = pi;
			missionNameText.text = pi.Title;
			if ( mode == PickerMode.Custom )
				versionText.text = pi.fileName;
			else
				versionText.text = pi.missionID;

			//setting isOn triggers OnClick()
			pauseClick = true;
			pickerToggle.isOn = ison;
			pauseClick = false;
		}

		public void OnClick()
		{
			var s = FindObjectOfType<SagaSetup>();
			if ( s != null && !pauseClick && pickerToggle.isOn )
				s.missionPicker.OnMissionSelected( projectItem );
		}
	}
}
