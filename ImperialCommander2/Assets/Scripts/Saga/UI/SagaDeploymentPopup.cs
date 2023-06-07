using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga
{
	public class SagaDeploymentPopup : MonoBehaviour
	{
		public Image fader;
		public CanvasGroup cg;
		public GameObject calmPanel, reinforcePanel, landingPanel, onslaughtPanel;
		public Text depTypeText;
		public TextMeshProUGUI warning;

		//calm
		public TextMeshProUGUI calmText;

		//reinforce
		public ReinforcePrefab topR1, topR2, bottomR1, bottomR2;
		public GameObject topPanel, bottomPanel;//panels
		public TextMeshProUGUI reinforceMessage;

		//landing
		public ReinforcePrefab landing1, landing2;
		public GameObject topLanding, bottomLanding;//panels
		public GameObject landingMessage;
		public TextMeshProUGUI landingMessageText;

		//onslaught
		public ReinforcePrefab on1R1, on1R2, on2R1, on2R2;
		public GameObject topOnslaught, bottomOnslaught;//panels
		public GameObject onR1Group, onR2Group;//containers for reinforcements
		public GameObject depPrefab, depGrid, onslaughtMessage;
		public TextMeshProUGUI onslaughtRWarning, onslaughtDWarning, onslaughtMessageText, onslaughtWarningText;

		List<DeploymentCard> groupsToDeploy;
		Action postAction;
		DeploymentGroupOverride deploymentOverride;
		bool acceptInput = true;

		public void Show( DeployMode mode, bool skipThreatIncrease, bool isOptionalDeployment, Action callback = null, DeploymentGroupOverride ovrd = null )
		{
			acceptInput = true;
			gameObject.SetActive( true );
			fader.color = new Color( 0, 0, 0, 0 );
			fader.DOFade( .75f, 1 );//.95
			cg.DOFade( 1, .5f );
			transform.GetChild( 1 ).localScale = new Vector3( .85f, .85f, .85f );
			transform.GetChild( 1 ).DOScale( 1, .5f ).SetEase( Ease.OutExpo );

			calmPanel.SetActive( false );
			reinforcePanel.SetActive( false );
			landingPanel.SetActive( false );
			onslaughtPanel.SetActive( false );

			deploymentOverride = ovrd;
			postAction = callback;
			groupsToDeploy = new List<DeploymentCard>();
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
					calmText.text = DataStore.uiLanguage.uiMainApp.calmMessageUC;
					HandleCalm();
					break;
				case DeployMode.Reinforcements:
					reinforcePanel.SetActive( true );
					depTypeText.text = DataStore.uiLanguage.uiMainApp.deployModeReinforcements;
					reinforceMessage.text = DataStore.uiLanguage.uiMainApp.threatIncreasedUC;
					HandleReinforcements();
					break;
				case DeployMode.Landing:
					landingPanel.SetActive( true );
					depTypeText.text = DataStore.uiLanguage.uiMainApp.deployModeLanding;
					landingMessageText.text = DataStore.uiLanguage.uiMainApp.threatIncreasedUC;
					if ( isOptionalDeployment )
						depTypeText.text = DataStore.uiLanguage.uiSetup.deploymentHeading;
					HandleLanding( skipThreatIncrease, isOptionalDeployment );
					break;
				case DeployMode.Onslaught:
					onslaughtPanel.SetActive( true );
					depTypeText.text = DataStore.uiLanguage.uiMainApp.deployModeOnslaught;
					onslaughtMessageText.text = DataStore.uiLanguage.uiMainApp.threatIncreasedUC;
					onslaughtRWarning.text = DataStore.uiLanguage.uiMainApp.reinforceWarningUC;
					onslaughtDWarning.text = DataStore.uiLanguage.uiMainApp.deploymentWarningUC;
					onslaughtWarningText.text = DataStore.uiLanguage.uiMainApp.reinforceWarningUC;
					HandleOnslaught( skipThreatIncrease );
					break;
			}

			DataStore.sagaSessionData.missionLogger.LogEvent( MissionLogType.DeploymentEvent, depTypeText.text );
		}

		public void OnClose()
		{
			if ( !acceptInput )
				return;
			acceptInput = false;

			FindObjectOfType<Sound>().PlaySound( FX.Click );
			cg.DOFade( 0, .2f );
			transform.GetChild( 1 ).DOScale( .85f, .5f ).SetEase( Ease.OutExpo );
			fader.DOFade( 0, .5f ).OnComplete( () =>
			{
				//destroy onslaught group icons
				foreach ( Transform tf in depGrid.transform )
					Destroy( tf.gameObject );
				gameObject.SetActive( false );
				//update/deploy groups
				if ( groupsToDeploy.Count > 0 )
				{
					if ( deploymentOverride == null )
						FindObjectOfType<DeploymentManager>().DeployGroupList( groupsToDeploy, false, postAction );
					else
						FindObjectOfType<DeploymentManager>().DeployGroupListWithOverride( groupsToDeploy, deploymentOverride, postAction );
				}
				else
				{
					postAction?.Invoke();
				}
				FindObjectOfType<DeploymentManager>().UpdateGroups();

				//save state
				//DataStore.sagaSessionData.SaveSession( "Session" );
			} );
		}

		public void HandleCalm()
		{
			/*
			Threat +Threat Level
			No deployment
			DM +2
			*/
			DataStore.sagaSessionData.ModifyThreat( DataStore.sagaSessionData.setupOptions.threatLevel );
			DataStore.sagaSessionData.UpdateDeploymentModifier( 2 );
		}

		void HandleReinforcements()
		{
			/*
			Threat +Threat Level
			Reinforce up to 2 groups
			DM +1
			*/
			DataStore.sagaSessionData.ModifyThreat( DataStore.sagaSessionData.setupOptions.threatLevel );
			DataStore.sagaSessionData.UpdateDeploymentModifier( 1 );

			DeploymentCard r1 = DataStore.GetReinforcement( DataStore.sagaSessionData.gameVars.currentThreat );
			if ( r1 != null )
			{
				topPanel.SetActive( true );
				topR1.Init( r1 );
				topR2.Init( r1, 1 );
				r1.currentSize += 1;
				//update threat just spent
				DataStore.sagaSessionData.ModifyThreat( -r1.rcost );
				//Debug.Log( "new size R1:" + r1.currentSize );
			}

			DeploymentCard r2 = DataStore.GetReinforcement( DataStore.sagaSessionData.gameVars.currentThreat );
			if ( r2 != null )
			{
				bottomPanel.SetActive( true );
				bottomR1.Init( r2 );
				bottomR2.Init( r2, 1 );
				r2.currentSize += 1;
				//update threat just spent
				DataStore.sagaSessionData.ModifyThreat( -r2.rcost );
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
				DataStore.sagaSessionData.ModifyThreat( DataStore.sagaSessionData.setupOptions.threatLevel + 1 );
				DataStore.sagaSessionData.UpdateDeploymentModifier( 1 );
				landingMessage.SetActive( true );
			}

			DeploymentCard d1 = DataStore.GetFuzzyDeployable( DataStore.sagaSessionData.gameVars.currentThreat );
			if ( d1 != null )
			{
				d1.ResetWoundTracker();
				topLanding.SetActive( true );
				landing1.Init( d1, 3 );//make sure it shows full size
				groupsToDeploy.Add( d1 );
				//remove it from dep hand
				DataStore.deploymentHand.Remove( d1 );
				//if this is an optional deployment event action, check for cost
				if ( deploymentOverride != null )
				{
					if ( deploymentOverride.useThreat )
					{
						DataStore.sagaSessionData.ModifyThreat( -(Mathf.Clamp( d1.cost + deploymentOverride.threatCost, 0, 100 )) );
					}
				}
				else//update threat just spent
					DataStore.sagaSessionData.ModifyThreat( -d1.cost );
			}

			DeploymentCard d2 = DataStore.GetFuzzyDeployable( DataStore.sagaSessionData.gameVars.currentThreat );
			if ( d2 != null )
			{
				d2.ResetWoundTracker();
				bottomLanding.SetActive( true );
				landing2.Init( d2, 3 );//make sure it shows full size
				groupsToDeploy.Add( d2 );
				//remove it from dep hand
				DataStore.deploymentHand.Remove( d2 );
				//if this is an optional deployment event action, check for cost
				if ( deploymentOverride != null )
				{
					if ( deploymentOverride.useThreat )
					{
						DataStore.sagaSessionData.ModifyThreat( -(Mathf.Clamp( d2.cost + deploymentOverride.threatCost, 0, 100 )) );
					}
				}
				else//update threat just spent
					DataStore.sagaSessionData.ModifyThreat( -d2.cost );
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
				DataStore.sagaSessionData.ModifyThreat( DataStore.sagaSessionData.setupOptions.threatLevel + 2 );
				onslaughtMessage.SetActive( true );
			}

			//set deployment modifier to -2, regardless of skipThreatIncrease
			DataStore.sagaSessionData.SetDeploymentModifier( -2 );

			DeploymentCard r1 = DataStore.GetReinforcement( DataStore.sagaSessionData.gameVars.currentThreat, true );
			if ( r1 != null )
			{
				topOnslaught.SetActive( true );
				onR1Group.SetActive( true );
				on1R1.Init( r1 );
				on1R2.Init( r1, 1 );
				r1.currentSize += 1;
				DataStore.sagaSessionData.ModifyThreat( -(Mathf.Max( 1, r1.rcost - 1 )) );
			}

			DeploymentCard r2 = DataStore.GetReinforcement( DataStore.sagaSessionData.gameVars.currentThreat, true );
			if ( r2 != null )
			{
				onR2Group.SetActive( true );
				on2R1.Init( r2 );
				on2R2.Init( r2, 1 );
				r2.currentSize += 1;
				DataStore.sagaSessionData.ModifyThreat( -(Mathf.Max( 1, r2.rcost - 1 )) );
			}

			if ( r1 == null && r2 == null )
			{
				onslaughtRWarning.gameObject.SetActive( true );
			}

			DeploymentCard dep;
			do
			{
				dep = DataStore.GetFuzzyDeployable( DataStore.sagaSessionData.gameVars.currentThreat, true );
				if ( dep != null )
				{
					dep.currentSize = dep.size;
					bottomOnslaught.SetActive( true );
					var go = Instantiate( depPrefab, depGrid.transform );
					go.GetComponent<ReinforcePrefab>().Init( dep );
					go.transform.localScale = new Vector3( .8f, .8f, .8f );
					groupsToDeploy.Add( dep );
					//remove it from dep hand
					DataStore.deploymentHand.Remove( dep );
					if ( dep.tier == 1 )
						DataStore.sagaSessionData.ModifyThreat( -dep.cost );
					else if ( dep.tier == 2 )
						DataStore.sagaSessionData.ModifyThreat( -(dep.cost - 1) );
					else
						DataStore.sagaSessionData.ModifyThreat( -(dep.cost - 2) );
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
}
