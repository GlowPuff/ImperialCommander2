using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//Handle toggling MISSIONS ONLY
public class MissionToggleContainer : MonoBehaviour
{
	List<MissionCard> missionCards = new List<MissionCard>();
	Toggle[] buttonToggles;
	Expansion selectedExpansion;
	Sound sound;

	private void Awake()
	{
		sound = FindObjectOfType<Sound>();

		buttonToggles = new Toggle[40];
		for ( int i = 0; i < transform.childCount; i++ )
			buttonToggles[i] = transform.GetChild( i ).GetComponent<Toggle>();
	}

	public void ResetUI()
	{
		//reset to core expansion by default
		missionCards = DataStore.missionCards["Core"];
		transform.parent.parent.parent.parent.Find( "expansion selector container" ).Find( "Core" ).GetComponent<Toggle>().isOn = true;
		//...and show data for that expansion
		OnChangeExpansion( "Core" );
	}

	public void OnChangeExpansion( string expansion )
	{
		Enum.TryParse( expansion, out selectedExpansion );

		if ( DataStore.missionCards.TryGetValue( expansion, out missionCards ) )
		{
			//foreach ( Transform c in transform )
			//default select first in group
			transform.GetChild( 0 ).GetComponent<Toggle>().isOn = true;
			for ( int i = 1; i < transform.childCount; i++ )
			{
				transform.GetChild( i ).gameObject.SetActive( false );
				transform.GetChild( i ).GetComponent<Toggle>().isOn = false;
			}

			for ( int i = 0; i < missionCards.Count(); i++ )
			{
				//switch on if previously selected
				//do it while Toggle is INACTIVE so OnToggle code doesn't run
				if ( DataStore.sessionData.selectedMissionName.ToLower() == missionCards[i].name.ToLower() )
				{
					buttonToggles[i].isOn = true;
				}

				var child = transform.GetChild( i );
				child.GetComponent<Toggle>().isOn = false;
				var label = child.Find( "Label" );
				label.GetComponent<Text>().text = missionCards[i].name.ToLower();

				child.gameObject.SetActive( true );
			}
		}
	}

	public void OnToggle( int index )
	{
		//checking for Active makes sure this code does NOT run when the Toggle is INACTIVE
		if ( !buttonToggles[index].gameObject.activeInHierarchy || missionCards.Count == 0 )
			return;

		sound.PlaySound( FX.Click );

		if ( buttonToggles[index].isOn )
		{
			DataStore.sessionData.selectedMissionID = missionCards[index].id;
			DataStore.sessionData.selectedMissionName = missionCards[index].name;
			DataStore.sessionData.selectedMissionExpansion = selectedExpansion;
		}
		else//if nothing selected, reset to core1, mission 1
		{
			DataStore.sessionData.selectedMissionID = "core1";
			DataStore.sessionData.selectedMissionName = DataStore.missionCards["Core"][0].name;
			DataStore.sessionData.selectedMissionExpansion = Expansion.Core;
		}
	}
}
