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

		//M13c/M13g: tactical optional deployment -> the chosen DP marker is
		//revealed (Thomas 28.10: the marker IS the placement instruction for
		//the human Imperator at the open screen; deployments are ALWAYS OPEN
		//- exactly like the printed rules, which never hide a deployment).
		bool tacticalOptionalReveal = false;

		//M13c: manual (legacy-mode) optional placement window state.
		GameObject manualPanel;
		TextMeshProUGUI manualHint, manualDpLabel;
		Button manualConfirm;
		List<DeploymentCard> manualCandidates;
		List<DeploymentPoint> manualDps;
		DeploymentCard manualGroup;
		DeploymentPoint manualPickedDp;
		//M13f: true ONLY for the DM-button optional flows (manual window or
		//tactical fuzzy path), driven by the explicit buttonFlow parameter of
		//Show() - NEVER inferred from ovrd. The button passes ovrd NON-null
		//(ImperialPopup.cs) while the event rails pass it null
		//(MissionEventHandler.cs) - an `ovrd == null` predicate as used in
		//M13c was INVERTED against the real callers (QA finding).
		//Event-sourced optional deployments (Aufstellungs-Ereignis,
		//delayOptionalDeployment) are NOT touched: no extra save, no announce -
		//their chain calls NextEventAction itself.
		bool isOptionalFlow = false;

		/// <summary>
		/// M13f: buttonFlow is the CALLER's explicit statement that this Show()
		/// comes from the DM's optional-deployment button (ImperialPopup).
		/// It is never inferred from ovrd: the button passes a NON-null ovrd
		/// while the delayOptionalDeployment event rails omit it entirely
		/// (MissionEventHandler.cs :354/:356/:724/:797 all stay buttonFlow=false
		/// = pre-M13c behaviour, no extra save, no announce).
		/// </summary>
		public void Show( DeployMode mode, bool skipThreatIncrease, bool isOptionalDeployment, Action callback = null, DeploymentGroupOverride ovrd = null, bool manual = false, bool buttonFlow = false )
		{
			acceptInput = true;
			gameObject.SetActive( true );

			// Personas (M2): the persona's mission subsidy is granted and
			// announced HERE - at popup open the hand and the live threat
			// exist, so the credit is spendable in the opening deployment
			// (once per mission identity, see PersonaState).
			TacticalAI.PersonaState.EnsureMissionSubsidy();
			fader.color = new Color( 0, 0, 0, 0 );
			//M13c: in the manual placement window the DM must be able to CLICK
			//the map behind the popup - the fader stays visual-only there.
			fader.raycastTarget = !manual;
			fader.DOFade( manual ? .25f : .75f, 1 );//.95
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
			//M13c: reset the manual-mode state + marker-reveal flag on every Show.
			tacticalOptionalReveal = isOptionalDeployment && manual == false
				&& TacticalAI.TacticalAIBridge.OptionalDeployPath() == TacticalAI.Core.OptionalDeploy.PathTactical;
			isOptionalFlow = isOptionalDeployment && buttonFlow;//explicit DM-button flow flag, never ovrd-inference (M13f)
			ResetManual();

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
					if ( manual )
						HandleManual( ovrd );
					else
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
				//M13c: restore the modal default for the next Show()
				fader.raycastTarget = true;
				//M13f: closing the window - CONFIRM or ABORT (CLOSE button /
				//space) - now also tears down the manual picker: DP markers
				//off, map hook released, strip destroyed. QA corpus 'Marker
				//verschwindet nach Aufstellungsphase wieder' holds for the
				//abort path too; before this, markers stayed lit until the
				//next Show()/ResetManual.
				ResetManual();
				//destroy onslaught group icons
				foreach ( Transform tf in depGrid.transform )
					Destroy( tf.gameObject );
				gameObject.SetActive( false );

				//M13f: the tactical announce runs as the deploy-COMPLETION
				//callback, not beside it: the per-card deploy TextBox (and
				//the Active-branch highlight around it, DeploymentManager.cs
				//:311 on / :320 off) must finish first, and only then is
				//every card's actually-used DP recorded. The event rails are
				//untouched: isOptionalFlow=false -> afterDeploy ==
				//postAction, byte-for-byte the pre-M13c wiring.
				Action afterDeploy = postAction;
				if ( isOptionalFlow && tacticalOptionalReveal && groupsToDeploy.Count > 0 )
				{
					var prev = postAction;
					afterDeploy = () =>
					{
						AnnounceTacticalLandings();
						prev?.Invoke();
					};
				}

				//update/deploy groups
				if ( groupsToDeploy.Count > 0 )
				{
					if ( deploymentOverride == null )
						FindObjectOfType<DeploymentManager>().DeployGroupList( groupsToDeploy, false, afterDeploy );
					else
						FindObjectOfType<DeploymentManager>().DeployGroupListWithOverride( groupsToDeploy, deploymentOverride, afterDeploy );
				}
				else
				{
					afterDeploy?.Invoke();
				}
				FindObjectOfType<DeploymentManager>().UpdateGroups();

				//M13c/M13f: the DM-button optional flows (manual window +
				//tactical fuzzy path) persist the mission state right after
				//the logical deploy (DoDeployGroup runs synchronously inside
				//the DeployGroupList* calls above). GroupDeployment log lines
				//are written by DeploymentManager on deploy; the placement
				//announcement is chained to afterDeploy.
				if ( isOptionalFlow )
					DataStore.sagaSessionData.SaveState();
				isOptionalFlow = false;

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
			int threatGain = DataStore.sagaSessionData.setupOptions.threatLevel;
			int depMod = 2;
			if ( Utils.IsRoundLimitReachedWithSetting( 2 ) )
			{
				Debug.Log( "HandleCalm()::Danger Mode, Threat Gain*2 and Modifier+2" );
				threatGain *= 2;
				depMod += 2;
			}
			DataStore.sagaSessionData.ModifyThreat( threatGain );
			DataStore.sagaSessionData.UpdateDeploymentModifier( depMod );
		}

		void HandleReinforcements()
		{
			/*
			Threat +Threat Level
			Reinforce up to 2 groups
			DM +1
			*/
			int threatGain = DataStore.sagaSessionData.setupOptions.threatLevel;
			int depMod = 1;
			if ( Utils.IsRoundLimitReachedWithSetting( 2 ) )
			{
				threatGain *= 2;
				depMod += 2;
			}

			DataStore.sagaSessionData.ModifyThreat( threatGain );
			DataStore.sagaSessionData.UpdateDeploymentModifier( depMod );

			DeploymentCard r1 = DataStore.GetReinforcement( DataStore.sagaSessionData.gameVars.currentThreat );
			if ( r1 != null )
			{
				topPanel.SetActive( true );
				topR1.Init( r1 );
				topR2.Init( r1, 1 );
				r1.currentSize += 1;
				//update threat just spent
				DataStore.sagaSessionData.ModifyThreat( -TacticalAI.PersonaState.CostFor( r1, r1.rcost ) );
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
				DataStore.sagaSessionData.ModifyThreat( -TacticalAI.PersonaState.CostFor( r2, r2.rcost ) );
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
				int threatGain = DataStore.sagaSessionData.setupOptions.threatLevel + 1;
				int depMod = 1;
				if ( Utils.IsRoundLimitReachedWithSetting( 2 ) )
				{
					Debug.Log( "HandleLanding()::Danger Mode, Threat Gain*2 and Modifier+2" );
					threatGain *= 2;
					depMod += 2;
				}

				DataStore.sagaSessionData.ModifyThreat( threatGain );
				DataStore.sagaSessionData.UpdateDeploymentModifier( depMod );
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
						// Personas (M2): the persona discount rides the PRINTED
					// cost, the event modifier is added on top (public math,
					// PersonaState.CostFor - printed value stays untouched).
					DataStore.sagaSessionData.ModifyThreat( -(Mathf.Clamp( TacticalAI.PersonaState.CostFor( d1, d1.cost ) + deploymentOverride.threatCost, 0, 100 )) );
					}
				}
				else//update threat just spent
					DataStore.sagaSessionData.ModifyThreat( -TacticalAI.PersonaState.CostFor( d1, d1.cost ) );
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
						// Personas (M2): discount on the printed cost (see d1).
					DataStore.sagaSessionData.ModifyThreat( -(Mathf.Clamp( TacticalAI.PersonaState.CostFor( d2, d2.cost ) + deploymentOverride.threatCost, 0, 100 )) );
					}
				}
				else//update threat just spent
					DataStore.sagaSessionData.ModifyThreat( -TacticalAI.PersonaState.CostFor( d2, d2.cost ) );
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
				int threatGain = DataStore.sagaSessionData.setupOptions.threatLevel + 2;
				if ( Utils.IsRoundLimitReachedWithSetting( 2 ) )
				{
					Debug.Log( "HandleOnslaught()::Danger Mode, Threat Gain*2" );
					threatGain *= 2;
				}

				DataStore.sagaSessionData.ModifyThreat( threatGain );
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
				// Personas (M2): persona discount FIRST, then the printed
				// onslaught reduction (-1, floored at 1) - both public.
				DataStore.sagaSessionData.ModifyThreat( -(Mathf.Max( 1, TacticalAI.PersonaState.CostFor( r1, r1.rcost ) - 1 )) );
			}

			DeploymentCard r2 = DataStore.GetReinforcement( DataStore.sagaSessionData.gameVars.currentThreat, true );
			if ( r2 != null )
			{
				onR2Group.SetActive( true );
				on2R1.Init( r2 );
				on2R2.Init( r2, 1 );
				r2.currentSize += 1;
				// Personas (M2): see r1.
				DataStore.sagaSessionData.ModifyThreat( -(Mathf.Max( 1, TacticalAI.PersonaState.CostFor( r2, r2.rcost ) - 1 )) );
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

		// ---------------------------------------------------------------- M13c

		/// <summary>
		/// M13f Scope B (reworked after QA): tactical-mode optional deployment
		/// announcement, run as the deploy-completion callback. The DP named
		/// here is the DP the deploy ACTUALLY used, recorded by
		/// DeploymentManager.LastActiveDeploymentPoint on its ovrd.Active
		/// deploy path (DeploymentManager.cs:303-328) - NOT a fresh
		/// GetActiveDeploymentPoint re-roll: in the deterministic bridge case
		/// (MapEntityManager.cs:526-528) a re-roll repeats the same pick, but
		/// the legacy RNG fallback (MapEntityManager.cs:530) could otherwise
		/// name a DIFFERENT DP than the one deployed to. One DP truth, no
		/// second pick. The pulsing marker (ShowDeploymentPoints,
		/// MapEntityManager.cs:491) is lit for exactly the duration of each
		/// announce TextBox - on when the line opens, off when it closes -
		/// sequentially, one TextBox at a time. Since M13g EVERY deployment -
		/// optional or not - shows its DP highlight: the ovrd==null branch in
		/// DeploymentManager runs the legacy reveal (highlight + camera)
		/// VERBATIM; deployments are always open (Thomas 28.10).
		/// </summary>
		void AnnounceTacticalLandings()
		{
			var scm = FindObjectOfType<SagaController>();
			if ( scm == null ) return;
			var dm = FindObjectOfType<DeploymentManager>();
			var lines = new List<string>();
			var dps = new List<Guid>();
			foreach ( var card in groupsToDeploy )
			{
				//the DP the deploy really used; the resolver call below is
				//only a defensive fallback for paths that recorded nothing.
				Guid dp = dm != null ? dm.LastActiveDeploymentPoint( card.id ) : Guid.Empty;
				if ( dp == Guid.Empty )
					dp = scm.mapEntityManager.GetActiveDeploymentPoint( card );
				string dpName = dp == Guid.Empty ? "" : (scm.mapEntityManager.GetEntity( dp )?.name ?? "");
				string line = TacticalAI.Core.OptionalDeploy.AnnounceLine(
					OptionalDeployUi.AnnounceFormat, card.name, dpName );
				if ( string.IsNullOrEmpty( line ) ) continue;
				lines.Add( line );
				dps.Add( dp );
			}
			AnnounceStep( scm, lines, dps, 0 );
		}

		void AnnounceStep( SagaController scm, List<string> lines, List<Guid> dps, int index )
		{
			if ( index >= lines.Count )
				return;
			var em = FindObjectOfType<SagaEventManager>();
			if ( em == null )
				return;
			Guid dp = dps[index];
			if ( dp != Guid.Empty )
				scm.mapEntityManager.ShowDeploymentPoints( new[] { dp }, true );
			em.ShowTextBox( lines[index], () =>
			{
				if ( dp != Guid.Empty )
					scm.mapEntityManager.ShowDeploymentPoints( new[] { dp }, false );
				AnnounceStep( scm, lines, dps, index + 1 );
			} );
		}

		/// <summary>
		/// M13c Scope C: the revived manual placement window for legacy-mode
		/// campaigns (Thomas 28.10). Compact strip (the map must stay visible
		/// and clickable - no dark fader): pick a group, the valid DPs of the
		/// card light up via the existing highlight mechanism, click one, pay
		/// the printed cost on confirm. Built at runtime exactly like the
		/// established tacai badges/panels (new GameObject + Button + TMP),
		/// so no scene edit is needed.
		/// </summary>
		void HandleManual( DeploymentGroupOverride ovrd )
		{
			landingPanel.SetActive( false );//the strip replaces the fuzzy landing panel
			//the shared `warning` label is a sibling of the panels, not a child
			//of Landing - it stays usable for both manual failure states

			//fuzzy candidate pool for the DM button = deployment hand (first,
			//printed-cost groups) + manual deploy list, minus deployed - the
			//same pools the legacy popup already treats as deployable now.
			var deployedIds = new List<string>();
			foreach ( var d in DataStore.deployedEnemies ) deployedIds.Add( d.id );
			var handIds = new List<string>();
			foreach ( var c in DataStore.deploymentHand ) handIds.Add( c.id );
			var manualIds = new List<string>();
			foreach ( var c in DataStore.manualDeploymentList ) manualIds.Add( c.id );
			var ids = TacticalAI.Core.OptionalDeploy.CandidateIds( handIds, manualIds, deployedIds );

			manualCandidates = new List<DeploymentCard>();
			foreach ( var id in ids )
			{
				var card = DataStore.deploymentHand.GetDeploymentCard( id )
					?? DataStore.manualDeploymentList.GetDeploymentCard( id );
				if ( card != null ) manualCandidates.Add( card );
			}

			//strip background (blocks stray clicks, unlike the full-screen fader)
			manualPanel = new GameObject( "OptionalDeployStrip", typeof( RectTransform ) );
			manualPanel.transform.SetParent( transform, false );
			var pRect = (RectTransform)manualPanel.transform;
			pRect.anchorMin = new Vector2( .5f, .5f );
			pRect.anchorMax = new Vector2( .5f, .5f );
			pRect.sizeDelta = new Vector2( 660, 580 );
			pRect.anchoredPosition = new Vector2( 0, -40 );
			var bg = manualPanel.AddComponent<Image>();
			bg.color = new Color( .04f, .04f, .06f, .95f );

			MakeLabel( manualPanel.transform, "OptionalDeployHeading", OptionalDeployUi.Title.ToUpperInvariant(),
				26, new Vector2( 0, 258 ), new Vector2( 620, 40 ), true );
			manualHint = MakeLabel( manualPanel.transform, "OptionalDeployHint", OptionalDeployUi.PickGroup,
				20, new Vector2( 0, 214 ), new Vector2( 620, 34 ), false );

			//scrollable group list
			var svGo = new GameObject( "OptionalDeployScroll", typeof( RectTransform ),
				typeof( ScrollRect ), typeof( Image ), typeof( RectMask2D ) );
			svGo.transform.SetParent( manualPanel.transform, false );
			var svRect = (RectTransform)svGo.transform;
			svRect.anchorMin = new Vector2( .5f, .5f );
			svRect.anchorMax = new Vector2( .5f, .5f );
			svRect.sizeDelta = new Vector2( 620, 340 );
			svRect.anchoredPosition = new Vector2( 0, 24 );
			svGo.GetComponent<Image>().color = new Color( 1, 1, 1, .04f );
			var scroll = svGo.GetComponent<ScrollRect>();
			scroll.horizontal = false;

			var viewGo = new GameObject( "Viewport", typeof( RectTransform ), typeof( RectMask2D ) );
			viewGo.transform.SetParent( svGo.transform, false );
			var viewRect = (RectTransform)viewGo.transform;
			viewRect.anchorMin = Vector2.zero;
			viewRect.anchorMax = Vector2.one;
			viewRect.sizeDelta = Vector2.zero;

			var contentGo = new GameObject( "Content", typeof( RectTransform ), typeof( VerticalLayoutGroup ) );
			contentGo.transform.SetParent( viewGo.transform, false );
			var cRect = (RectTransform)contentGo.transform;
			cRect.anchorMin = new Vector2( 0, 1 );
			cRect.anchorMax = new Vector2( 0, 1 );
			cRect.pivot = new Vector2( .5f, 1 );
			cRect.sizeDelta = new Vector2( 610, 0 );
			var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
			vlg.spacing = 6;
			vlg.childControlHeight = true;
			vlg.childControlWidth = true;
			vlg.childForceExpandWidth = true;
			contentGo.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			scroll.viewport = viewRect;
			scroll.content = cRect;

			if ( manualCandidates.Count == 0 )
			{
				ShowWarning( OptionalDeployUi.NoGroups );
			}
			else
			{
				foreach ( var card in manualCandidates )
				{
					var row = MakeButton( contentGo.transform, card.name,
						string.Format( OptionalDeployUi.CostFormat,
							DataStore.uiLanguage.uiMainApp.depCostUC,
							TacticalAI.Core.OptionalDeploy.DisplayedCost( card.cost ) ) );
					row.GetComponent<Button>().onClick.AddListener( () => OnManualGroupPicked( card ) );
				}
			}

			manualDpLabel = MakeLabel( manualPanel.transform, "OptionalDeployDp", "",
				20, new Vector2( 0, -162 ), new Vector2( 620, 34 ), false );

			manualConfirm = MakeButton( manualPanel.transform, DataStore.uiLanguage.uiMainApp.deploy, "" );
			var cRect2 = (RectTransform)manualConfirm.transform;
			cRect2.sizeDelta = new Vector2( 300, 48 );
			manualConfirm.onClick.AddListener( ConfirmManual );

			var closeBtn = MakeButton( manualPanel.transform, DataStore.uiLanguage.uiMainApp.close, "" );
			var clRect = (RectTransform)closeBtn.transform;
			clRect.sizeDelta = new Vector2( 300, 48 );
			closeBtn.onClick.AddListener( OnClose );

			//both buttons at the strip bottom, side by side
			cRect2.anchoredPosition = new Vector2( -155, -240 );
			clRect.anchoredPosition = new Vector2( 155, -240 );
			manualConfirm.interactable = false;
		}

		TextMeshProUGUI MakeLabel( Transform parent, string name, string text, int size, Vector2 pos, Vector2 sizeDelta, bool bold )
		{
			var go = new GameObject( name, typeof( RectTransform ) );
			go.transform.SetParent( parent, false );
			var r = (RectTransform)go.transform;
			r.anchorMin = new Vector2( .5f, .5f );
			r.anchorMax = new Vector2( .5f, .5f );
			r.sizeDelta = sizeDelta;
			r.anchoredPosition = pos;
			var t = go.AddComponent<TextMeshProUGUI>();
			t.text = text;
			t.fontSize = size;
			t.alignment = TextAlignmentOptions.Center;
			t.color = Color.white;
			t.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
			return t;
		}

		Button MakeButton( Transform parent, string line1, string line2 )
		{
			var go = new GameObject( "OptionalDeployButton", typeof( RectTransform ), typeof( Image ), typeof( Button ) );
			go.transform.SetParent( parent, false );
			var r = (RectTransform)go.transform;
			r.sizeDelta = new Vector2( 590, 44 );
			go.GetComponent<Image>().color = new Color( .18f, .22f, .30f, .96f );
			var lbl = MakeLabel( go.transform, "Text (TMP)", string.IsNullOrEmpty( line2 ) ? line1 : $"{line1}\n<size=70%>{line2}</size>",
				20, Vector2.zero, new Vector2( 580, 42 ), false );
			lbl.enableWordWrapping = true;
			lbl.overflowMode = TextOverflowModes.Ellipsis;
			return go.GetComponent<Button>();
		}

		void ShowWarning( string msg )
		{
			warning.gameObject.SetActive( true );
			warning.text = msg;
		}

		void OnManualGroupPicked( DeploymentCard card )
		{
			if ( !acceptInput ) return;
			ClearManualMarkers();
			manualGroup = card;
			manualPickedDp = null;
			manualConfirm.interactable = false;
			manualDpLabel.text = "";
			warning.gameObject.SetActive( false );

			manualDps = new List<DeploymentPoint>();
			var valid = FindObjectOfType<SagaController>().mapEntityManager.GetValidDeploymentPoints( card );
			foreach ( var e in valid )
			{
				var dp = e as DeploymentPoint;
				if ( dp != null ) manualDps.Add( dp );
			}
			if ( manualDps.Count == 0 )
			{
				//existing warning key, reused verbatim (Scope C)
				ShowWarning( DataStore.uiLanguage.sagaMainApp.noDPWarningUC );
				manualHint.text = OptionalDeployUi.PickGroup;
				return;
			}
			//valid DPs light up = exactly what the DM may click, for exactly
			//as long as this picking phase lasts
			FindObjectOfType<SagaController>().mapEntityManager.ShowDeploymentPoints(
				System.Linq.Enumerable.Select( manualDps, d => d.GUID ), true );
			var mem = FindObjectOfType<SagaController>().mapEntityManager;
			mem.OnDeploymentPointPicked = OnManualDpPicked;
			manualHint.text = OptionalDeployUi.PickDp;
		}

		void OnManualDpPicked( DeploymentPoint dp )
		{
			if ( !acceptInput || dp == null || manualGroup == null ) return;
			//still-valid check: only DPs from the filtered list may confirm
			if ( manualDps == null || !manualDps.Contains( dp ) ) return;
			manualPickedDp = dp;
			manualDpLabel.text = string.Format( OptionalDeployUi.DpFormat, dp.name ?? "" );
			manualConfirm.interactable = TacticalAI.Core.OptionalDeploy.CanConfirmManual(
				manualGroup.id, manualGroup.cost, true, true );
			FindObjectOfType<Sound>().PlaySound( FX.Click );
		}

		void ConfirmManual()
		{
			if ( !acceptInput ) return;
			if ( manualGroup == null || manualPickedDp == null ) return;
			//Core confirm law (pinned): group + printed cost + picked, valid DP
			if ( !TacticalAI.Core.OptionalDeploy.CanConfirmManual(
				manualGroup.id, manualGroup.cost, true, manualDps.Contains( manualPickedDp ) ) )
				return;

			int spend = TacticalAI.Core.OptionalDeploy.ThreatSpend( manualGroup.cost,
				deploymentOverride != null ? deploymentOverride.threatCost : 0 );
			Debug.Log( $"M13c manual optional deploy::{manualGroup.name} @ {manualPickedDp.name}::threat -{spend}" );

			//pay the printed cost (EnemyDeployment cost law)
			DataStore.sagaSessionData.ModifyThreat( -spend );
			manualGroup.ResetWoundTracker();
			DataStore.deploymentHand.Remove( manualGroup );

			//specific-DP override, passed inline (like the event-sourced
			//RandomDeployment path) - no permanent override registered
			deploymentOverride = new DeploymentGroupOverride( "" );
			deploymentOverride.deploymentPoint = DeploymentSpot.Specific;
			deploymentOverride.specificDeploymentPoint = manualPickedDp.GUID;
			deploymentOverride.pointList.Clear();
			deploymentOverride.pointList.Add( new DPData { GUID = manualPickedDp.GUID } );
			groupsToDeploy.Add( manualGroup );
			OnClose();
		}

		void ClearManualMarkers()
		{
			var scm = FindObjectOfType<SagaController>();
			if ( scm != null && manualDps != null && manualDps.Count > 0 )
				scm.mapEntityManager.ShowDeploymentPoints(
					System.Linq.Enumerable.Select( manualDps, d => d.GUID ), false );
		}

		void ResetManual()
		{
			ClearManualMarkers();
			var mem = FindObjectOfType<SagaController>()?.mapEntityManager;
			if ( mem != null ) mem.OnDeploymentPointPicked = null;
			manualGroup = null;
			manualPickedDp = null;
			manualDps = null;
			manualCandidates = null;
			manualHint = null;
			manualDpLabel = null;
			manualConfirm = null;
			if ( manualPanel != null ) Destroy( manualPanel );
			manualPanel = null;
		}

		private void Update()
		{
			if ( Input.GetKeyDown( KeyCode.Space ) )
				OnClose();
		}
	}
}
