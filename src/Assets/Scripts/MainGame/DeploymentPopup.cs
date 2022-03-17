using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeploymentPopup : MonoBehaviour
{
	public Image fader;
	public CanvasGroup cg;
	public GameObject calmPanel, reinforcePanel, landingPanel, onslaughtPanel;
	public Text depTypeText;
	public TextMeshProUGUI warning;

	//reinforce
	public ReinforcePrefab topR1, topR2, bottomR1, bottomR2;
	public GameObject topPanel, bottomPanel;//panels

	//landing
	public ReinforcePrefab landing1, landing2;
	public GameObject topLanding, bottomLanding;//panels
	public GameObject landingMessage;

	//onslaught
	public ReinforcePrefab on1R1, on1R2, on2R1, on2R2;
	public GameObject topOnslaught, bottomOnslaught;//panels
	public GameObject onR1Group, onR2Group;//containers for reinforcements
	public GameObject depPrefab, depGrid, onslaughtMessage;
	public TextMeshProUGUI onslaughtRWarning, onslaughtDWarning;

	List<CardDescriptor> groupsToDeploy;
	Action postAction;
	bool pauseKeyInput;

	public void Show( DeployMode mode, bool skipThreatIncrease, bool isOptionalDeployment, Action a = null )
	{
		gameObject.SetActive( true );
		fader.color = new Color( 0, 0, 0, 0 );
		fader.DOFade( .95f, 1 );
		cg.DOFade( 1, .5f );
		transform.GetChild( 1 ).localScale = new Vector3( .85f, .85f, .85f );
		transform.GetChild( 1 ).DOScale( 1, .5f ).SetEase( Ease.OutExpo );

		calmPanel.SetActive( false );
		reinforcePanel.SetActive( false );
		landingPanel.SetActive( false );
		onslaughtPanel.SetActive( false );

		postAction = a;
		groupsToDeploy = new List<CardDescriptor>();
		warning.gameObject.SetActive( false );

		//reset reinforce
		topPanel.SetActive( false );
		bottomPanel.SetActive( false );

		//reset landing
		topLanding.SetActive( false );
		bottomLanding.SetActive( false );

		//reset onslaught
		topOnslaught.SetActive( false );
		bottomOnslaught.SetActive( false );
		onR1Group.SetActive( false );
		onR2Group.SetActive( false );
		onslaughtRWarning.gameObject.SetActive( false );
		onslaughtDWarning.gameObject.SetActive( false );

		switch ( mode )
		{
			case DeployMode.Calm:
				calmPanel.SetActive( true );
				depTypeText.text = DataStore.uiLanguage.uiMainApp.deployModeCalm;
				HandleCalm();
				break;
			case DeployMode.Reinforcements:
				reinforcePanel.SetActive( true );
				depTypeText.text = DataStore.uiLanguage.uiMainApp.deployModeReinforcements;
				HandleReinforcements();
				break;
			case DeployMode.Landing:
				landingPanel.SetActive( true );
				depTypeText.text = DataStore.uiLanguage.uiMainApp.deployModeLanding;
				if ( isOptionalDeployment )
					depTypeText.text = DataStore.uiLanguage.uiSetup.deploymentHeading;
				HandleLanding( skipThreatIncrease, isOptionalDeployment );
				break;
			case DeployMode.Onslaught:
				onslaughtPanel.SetActive( true );
				depTypeText.text = DataStore.uiLanguage.uiMainApp.deployModeOnslaught;
				HandleOnslaught( skipThreatIncrease );
				break;
		}
	}

	public void OnClose()
	{
		FindObjectOfType<Sound>().PlaySound( FX.Click );
		fader.DOFade( 0, .5f ).OnComplete( () =>
		{
			foreach ( Transform tf in depGrid.transform )
				Destroy( tf.gameObject );
			gameObject.SetActive( false );
			//update/deploy groups
			for ( int i = 0; i < groupsToDeploy.Count; i++ )
			{
				FindObjectOfType<DeploymentGroupManager>().DeployGroup( groupsToDeploy[i] );
			}
			FindObjectOfType<DeploymentGroupManager>().UpdateGroups();

			//save state
			DataStore.sessionData.SaveSession( "Session" );

			postAction?.Invoke();
		} );
		cg.DOFade( 0, .2f );
		transform.GetChild( 1 ).DOScale( .85f, .5f ).SetEase( Ease.OutExpo );
	}

	public void HandleCalm()
	{
		/*
		Threat +Threat Level
		No deployment
		DM +2
		*/
		DataStore.sessionData.ModifyThreat( DataStore.sessionData.threatLevel );
		DataStore.sessionData.UpdateDeploymentModifier( 2 );
	}

	void HandleReinforcements()
	{
		/*
		Threat +Threat Level
		Reinforce up to 2 groups
		DM +1
		*/
		DataStore.sessionData.ModifyThreat( DataStore.sessionData.threatLevel );
		DataStore.sessionData.UpdateDeploymentModifier( 1 );

		CardDescriptor r1 = DataStore.GetReinforcement();
		if ( r1 != null )
		{
			topPanel.SetActive( true );
			topR1.Init( r1 );
			topR2.Init( r1, 1 );
			r1.currentSize += 1;
			//update threat just spent
			DataStore.sessionData.ModifyThreat( -r1.rcost );
			//Debug.Log( "new size R1:" + r1.currentSize );
		}
		CardDescriptor r2 = DataStore.GetReinforcement();
		if ( r2 != null )
		{
			bottomPanel.SetActive( true );
			bottomR1.Init( r2 );
			bottomR2.Init( r2, 1 );
			r2.currentSize += 1;
			//update threat just spent
			DataStore.sessionData.ModifyThreat( -r2.rcost );
			//Debug.Log( "new size R2:" + r2.currentSize );
		}

		if ( r1 == null && r2 == null )
		{
			warning.gameObject.SetActive( true );
			warning.text = DataStore.uiLanguage.uiMainApp.reinforceWarningUC;
		}
	}

	public void HandleLanding( bool skipThreatIncrease, bool isOptionalDeployment )
	{
		/*
		Threat +Threat Level +1
		Deploy up to 2 new groups
		“Fuzzy deployment” (see below)
		DM +1
		*/
		if ( isOptionalDeployment || skipThreatIncrease )
			landingMessage.SetActive( false );
		else// if ( !skipThreatIncrease )
		{
			DataStore.sessionData.ModifyThreat( DataStore.sessionData.threatLevel + 1 );
			DataStore.sessionData.UpdateDeploymentModifier( 1 );
			landingMessage.SetActive( true );
		}

		CardDescriptor d1 = DataStore.GetFuzzyDeployable();
		if ( d1 != null )
		{
			topLanding.SetActive( true );
			landing1.Init( d1, 3 );//make sure it shows full size
			groupsToDeploy.Add( d1 );
			//remove it from dep hand
			DataStore.deploymentHand.Remove( d1 );
			//update threat just spent
			DataStore.sessionData.ModifyThreat( -d1.cost );
		}

		CardDescriptor d2 = DataStore.GetFuzzyDeployable();
		if ( d2 != null )
		{
			bottomLanding.SetActive( true );
			landing2.Init( d2, 3 );//make sure it shows full size
			groupsToDeploy.Add( d2 );
			//remove it from dep hand
			DataStore.deploymentHand.Remove( d2 );
			//update threat just spent
			DataStore.sessionData.ModifyThreat( -d2.cost );
		}

		if ( d1 == null && d2 == null )
		{
			warning.gameObject.SetActive( true );
			warning.text = DataStore.uiLanguage.uiMainApp.deploymentWarningUC;
		}
	}

	public void HandleOnslaught( bool skipThreatIncrease )
	{
		/*
		Threat +Threat Level +2
		Reinforce up to 2 groups (cost decreased by 1, to a
		minimum of 1)
		Deploy as many new groups as possible, decreased cost:
		Tier I: no change
		Tier II: cost -1
		Tier III: cost -2
		“Fuzzy deployment” (see below)
		DM = -2
		*/

		if ( skipThreatIncrease )
			onslaughtMessage.SetActive( false );
		else
		{
			DataStore.sessionData.ModifyThreat( DataStore.sessionData.threatLevel + 2 );
			onslaughtMessage.SetActive( true );
		}

		//set deployment modifier to -2, regardless of skipThreatIncrease
		DataStore.sessionData.SetDeploymentModifier( -2 );

		CardDescriptor r1 = DataStore.GetReinforcement( true );
		if ( r1 != null )
		{
			topOnslaught.SetActive( true );
			onR1Group.SetActive( true );
			on1R1.Init( r1 );
			on1R2.Init( r1, 1 );
			r1.currentSize += 1;
			DataStore.sessionData.ModifyThreat( -(Mathf.Max( 1, r1.rcost - 1 )) );
		}

		CardDescriptor r2 = DataStore.GetReinforcement( true );
		if ( r2 != null )
		{
			onR2Group.SetActive( true );
			on2R1.Init( r2 );
			on2R2.Init( r2, 1 );
			r2.currentSize += 1;
			DataStore.sessionData.ModifyThreat( -(Mathf.Max( 1, r2.rcost - 1 )) );
		}

		if ( r1 == null && r2 == null )
		{
			onslaughtRWarning.gameObject.SetActive( true );
		}

		CardDescriptor dep;
		do
		{
			dep = DataStore.GetFuzzyDeployable( true );
			if ( dep != null )
			{
				dep.currentSize = dep.size;
				bottomOnslaught.SetActive( true );
				var go = Instantiate( depPrefab, depGrid.transform );
				go.GetComponent<ReinforcePrefab>().Init( dep );
				groupsToDeploy.Add( dep );
				//remove it from dep hand
				DataStore.deploymentHand.Remove( dep );
				if ( dep.tier == 1 )
					DataStore.sessionData.ModifyThreat( -dep.cost );
				else if ( dep.tier == 2 )
					DataStore.sessionData.ModifyThreat( -(dep.cost - 1) );
				else
					DataStore.sessionData.ModifyThreat( -(dep.cost - 2) );
			}
		}
		while ( dep != null );

		if ( depGrid.transform.childCount == 0 )
		{
			onslaughtDWarning.gameObject.SetActive( true );
		}
	}

	private void Update()
	{
		if ( Input.GetKeyDown( KeyCode.Space ) )
			OnClose();
	}
}
