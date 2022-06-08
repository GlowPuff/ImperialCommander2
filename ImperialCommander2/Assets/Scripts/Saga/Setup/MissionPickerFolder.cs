using System.IO;
using TMPro;
using UnityEngine;

namespace Saga
{
	public class MissionPickerFolder : MonoBehaviour
	{
		public TextMeshProUGUI folderNameText;

		DirectoryInfo dInfo;
		string builtInFolder;
		PickerMode pickerMode;

		public void Init( DirectoryInfo di )
		{
			pickerMode = PickerMode.Custom;
			dInfo = di;
			folderNameText.text = dInfo.Name;
		}

		public void InitBuiltin( string bname )
		{
			pickerMode = PickerMode.BuiltIn;
			builtInFolder = bname;
			folderNameText.text = bname;
		}

		public void OnClick()
		{
			if ( pickerMode == PickerMode.Custom )
				FindObjectOfType<SagaSetup>().missionPicker.OnChangeFolder( dInfo.FullName );
			else
				FindObjectOfType<SagaSetup>().missionPicker.OnChangeBuiltinFolder( builtInFolder );
		}
	}
}
