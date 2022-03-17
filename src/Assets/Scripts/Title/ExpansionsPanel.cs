using System;
using UnityEngine;
using UnityEngine.UI;

public class ExpansionsPanel : MonoBehaviour
{
	public void ActivatePanel()
	{
		foreach ( Transform t in transform )
		{
			if ( t.name == "Button" )
				continue;

			if ( DataStore.ownedExpansions.Contains( (Expansion)Enum.Parse( typeof( Expansion ), t.name ) ) )
				t.GetComponent<Toggle>().isOn = true;
			else
				t.GetComponent<Toggle>().isOn = false;
		}
	}
}
