using UnityEngine;
using UnityEngine.UI;

public class ExpansionController : MonoBehaviour
{
	public Text[] expText;

	public void UpdateText( int expansionIndex, int count )
	{
		var props = DataStore.uiLanguage.uiExpansions.GetType().GetFields();
		string s = props[expansionIndex].GetValue( DataStore.uiLanguage.uiExpansions ) as string;
		if ( count > 0 )
			expText[expansionIndex].text = $"{s.ToLower()} ({count})";
		else
			expText[expansionIndex].text = s.ToLower();
	}

	public void ResetText()
	{
		var props = DataStore.uiLanguage.uiExpansions.GetType().GetFields();
		for ( int i = 0; i < 8; i++ )
		{
			expText[i].text = props[i].GetValue( DataStore.uiLanguage.uiExpansions ) as string;
		}
	}
}
