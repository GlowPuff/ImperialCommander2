using System.Collections.Generic;
using System.IO;
using System.Linq;
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

			string expansion = "";
			//expected format: Expansion #
			switch ( bname.Split( ' ' )[0] )
			{
				case "Core":
					expansion = Capitalize( DataStore.uiLanguage.uiExpansions.core );
					break;
				case "Twin":
					expansion = Capitalize( DataStore.uiLanguage.uiExpansions.twin );
					break;
				case "Hoth":
					expansion = Capitalize( DataStore.uiLanguage.uiExpansions.hoth );
					break;
				case "Bespin":
					expansion = Capitalize( DataStore.uiLanguage.uiExpansions.bespin );
					break;
				case "Jabba":
					expansion = Capitalize( DataStore.uiLanguage.uiExpansions.jabba );
					break;
				case "Empire":
					expansion = Capitalize( DataStore.uiLanguage.uiExpansions.empire );
					break;
				case "Lothal":
					expansion = Capitalize( DataStore.uiLanguage.uiExpansions.lothal );
					break;
				case "Other":
					expansion = Capitalize( DataStore.uiLanguage.uiExpansions.other );
					break;
				default:
					expansion = "Unknown Expansion::" + bname[0];
					break;
			}
			folderNameText.text = expansion;
		}

		string Capitalize( string s )
		{
			string[] parts = s.Split( ' ' );
			List<string> output = new List<string>();
			foreach ( var item in parts )
			{
				if ( item.Length == 1 )
					output.Add( char.ToUpperInvariant( item[0] ).ToString() );
				else
					output.Add( char.ToUpperInvariant( item[0] ).ToString() + item.Substring( 1 ) );
			}
			return output.Aggregate( "", ( acc, cur ) => acc + " " + cur );
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
