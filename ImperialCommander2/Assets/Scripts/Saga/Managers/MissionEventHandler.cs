using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace Saga
{
	public partial class SagaEventManager
	{
		public void NotifyMissionInfoChanged()
		{
			infoButtonTX.DOScale( 1.2f, .15f ).SetLoops( -1, LoopType.Yoyo );
			GlowEngine.FindUnityObject<QuickMessage>().Show( "Mission Info Updated" );
		}

		/// <summary>
		/// Parses text for glyphs, NOT an event action, does NOT call NextEventAction()
		/// </summary>
		public void ShowTextBox( string m, Action callback = null )
		{
			Debug.Log( "SagaEventManager()::PROCESSING ShowTextBox" );
			if ( string.IsNullOrEmpty( m?.Trim() ) )
			{
				Debug.Log( "ShowTextBox()::NO TEXT" );
				callback?.Invoke();
				return;
			}
			var go = Instantiate( textBoxPrefab, transform );
			var tb = go.transform.Find( "TextBox" ).GetComponent<TextBox>();
			tb.Show( m, callback );
		}

		//GENERAL
		void MissionManagement( MissionManagement mm )
		{
			Debug.Log( "SagaEventManager()::PROCESSING MissionManagement" );
			var sc = FindObjectOfType<SagaController>();

			if ( mm.incRoundCounter )
			{
				sc.IncreaseRound();
				CheckIfEventsTriggered();
				GlowEngine.FindUnityObject<QuickMessage>().Show( DataStore.uiLanguage.sagaMainApp.roundIncreasedUC );
			}
			if ( mm.pauseDeployment )
			{
				DataStore.sagaSessionData.gameVars.pauseDeployment = true;
				GlowEngine.FindUnityObject<QuickMessage>().Show( DataStore.uiLanguage.uiMainApp.pauseDepMsgUC );
			}
			if ( mm.unpauseDeployment )
			{
				DataStore.sagaSessionData.gameVars.pauseDeployment = false;
				GlowEngine.FindUnityObject<QuickMessage>().Show( DataStore.uiLanguage.uiMainApp.unPauseDepMsgUC );
			}
			if ( mm.pauseThreat )
			{
				DataStore.sagaSessionData.gameVars.pauseThreatIncrease = true;
				GlowEngine.FindUnityObject<QuickMessage>().Show( DataStore.uiLanguage.uiMainApp.pauseThreatMsgUC );
			}
			if ( mm.unpauseThreat )
			{
				DataStore.sagaSessionData.gameVars.pauseThreatIncrease = false;
				GlowEngine.FindUnityObject<QuickMessage>().Show( DataStore.uiLanguage.uiMainApp.UnPauseThreatMsgUC );
			}
			if ( mm.endMission )
			{
				//don't process any more events or event actions after this one
				endProcessingCallback = null;
				eventQueue.Clear();
				eventActionQueue.Clear();

				if ( !DataStore.sagaSessionData.setupOptions.useAdaptiveDifficulty )
				{
					GlowEngine.FindUnityObject<QuickMessage>().Show( DataStore.uiLanguage.sagaMainApp.endOfMissionUC );
					GlowTimer.SetTimer( 3, () =>
					{
						//load title screen
						FindObjectOfType<SagaController>().EndMission();
					} );
				}
				else
				{
					int awards = Mathf.FloorToInt( DataStore.sagaSessionData.gameVars.fame / 12 );
					if ( DataStore.sagaSessionData.gameVars.round >= 8 )
						awards = 0;

					ShowTextBox( $"<color=orange><bold>{DataStore.uiLanguage.sagaMainApp.endOfMissionUC}</bold></color>\n\n{DataStore.uiLanguage.uiMainApp.fameHeading}: <color=orange><bold>{DataStore.sagaSessionData.gameVars.fame}</bold></color>\n\n{DataStore.uiLanguage.uiMainApp.awardsHeading}: <color=orange><bold>{awards}</bold></color>", () =>
					{
						//load title screen
						FindObjectOfType<SagaController>().EndMission();
					} );
				}
			}

			NextEventAction();
		}

		void ChangeMissionInfo( ChangeMissionInfo ci )
		{
			Debug.Log( "SagaEventManager()::PROCESSING ChangeMissionInfo" );
			FindObjectOfType<Sound>().PlaySound( FX.Notify );
			DataStore.sagaSessionData.gameVars.currentMissionInfo = ci.theText;
			NotifyMissionInfoChanged();
			NextEventAction();
		}

		void ShowChangeObjective( ChangeObjective changeObjective )
		{
			Debug.Log( "SagaEventManager()::PROCESSING ChangeObjective" );
			//objective bar handles glyphs itself
			if ( !string.IsNullOrEmpty( changeObjective.longText ) )
			{
				FindObjectOfType<SagaEventManager>().toggleVisButton.SetActive( true );
				FindObjectOfType<SagaController>().ToggleNavAndEntitySelection( false );
				ShowTextBox( changeObjective.longText, () =>
				{
					FindObjectOfType<SagaController>().OnChangeObjective( changeObjective.theText, () =>
					{
						FindObjectOfType<SagaEventManager>().toggleVisButton.SetActive( false );
						FindObjectOfType<SagaController>().ToggleNavAndEntitySelection( true );
						NextEventAction();
					} );
				} );
			}
			else
			{
				FindObjectOfType<SagaController>().OnChangeObjective( changeObjective.theText, () => NextEventAction() );
			}
		}

		void ModifyVariable( ModifyVariable mv )
		{
			Debug.Log( "SagaEventManager()::PROCESSING ModifyVariable" );
			foreach ( var tm in mv.triggerList )
			{
				FindObjectOfType<TriggerManager>().ModifyVariable( tm );
			}
			CheckIfEventsTriggered( () => NextEventAction() );
		}

		void ModifyThreat( ModifyThreat mt )
		{
			Debug.Log( "SagaEventManager()::PROCESSING ModifyVariable" );
			if ( mt.threatModifierType == ThreatModifierType.Fixed )
				DataStore.sagaSessionData.ModifyThreat( mt.fixedValue );
			else if ( mt.threatModifierType == ThreatModifierType.Multiple )
				DataStore.sagaSessionData.ModifyThreat( DataStore.sagaSessionData.setupOptions.threatLevel * mt.threatValue );
			NextEventAction();
		}

		void ActivateEventGroup( ActivateEventGroup ag )
		{
			Debug.Log( "SagaEventManager()::PROCESSING ActivateEventGroup" );
			EventGroup eg = DataStore.mission.eventGroups.Where( x => x.GUID == ag.eventGroupGUID ).FirstOr( null );
			FindObjectOfType<SagaController>().triggerManager.FireEventGroup( eg.GUID );
			NextEventAction();
		}

		//DEPLOYMENTS
		void EnemyDeployment( EnemyDeployment ed )
		{
			Debug.Log( "SagaEventManager()::PROCESSING EnemyDeployment" );
			DeploymentCard card = DataStore.GetEnemy( ed.deploymentGroup );

			//check for existing group reset
			var reset = DataStore.sagaSessionData.gameVars.GetDeploymentOverride( ed.deploymentGroup );
			if ( reset != null )
			{
				if ( reset.useResetGroup )
				{

					NextEventAction();
					return;
				}
			}

			var ovrd = DataStore.sagaSessionData.gameVars.CreateDeploymentOverride( ed.deploymentGroup );
			//name
			if ( !string.IsNullOrEmpty( ed.enemyName ) )
				ovrd.nameOverride = ed.enemyName;
			else
				ovrd.nameOverride = card.name;
			//custom instructions
			if ( ed.useCustomInstructions )
				ovrd.SetInstructionOverride( new ChangeInstructions()
				{
					instructionType = ed.enemyGroupData.customInstructionType,
					theText = ed.enemyGroupData.customText
				} );
			//set the main override data
			ovrd.SetEnemyDeploymentOverride( ed );

			//check if this deployment uses threat cost, and apply any modification
			if ( ed.useThreat )
			{
				Debug.Log( $"EnemyDeployment::APPLYING THREAT COST ({card.cost})::MODIFIER ({ed.threatCost})" );
				DataStore.sagaSessionData.ModifyThreat( -(Mathf.Clamp( card.cost + ed.threatCost, 0, 100 )) );
			}

			//finally, do the actual deployment
			//deploy this group to the board, unmodified by elite RNG
			FindObjectOfType<SagaController>().dgManager.DeployGroup( card, true );
			FindObjectOfType<SagaController>().dgManager.HandleMapDeployment( card, NextEventAction, ovrd );
		}

		void AllyDeployment( AllyDeployment ad )
		{
			Debug.Log( "SagaEventManager()::PROCESSING AllyDeployment" );
			DeploymentCard card = DataStore.allyCards.Where( x => x.id == ad.allyID ).FirstOr( null );
			if ( card != null )
			{
				var ovrd = DataStore.sagaSessionData.gameVars.CreateDeploymentOverride( ad.allyID );
				//name
				if ( !string.IsNullOrEmpty( ad.allyName ) )
					ovrd.nameOverride = ad.allyName;
				else
					ovrd.nameOverride = card.name;
				//trigger
				ovrd.setTrigger = ad.setTrigger;
				//dp
				ovrd.deploymentPoint = ad.deploymentPoint;
				ovrd.specificDeploymentPoint = ad.specificDeploymentPoint;
				//threat cost to ADD
				if ( ad.useThreat )
				{
					Debug.Log( $"EnemyDeployment::APPLYING THREAT COST ({card.cost})::MODIFIER ({ad.threatCost})" );
					DataStore.sagaSessionData.ModifyThreat( Mathf.Clamp( card.cost + ad.threatCost, 0, 100 ) );
				}
				//finally, do the actual deployment
				FindObjectOfType<SagaController>().dgManager.DeployHeroAlly( card );
				FindObjectOfType<SagaController>().dgManager.HandleMapDeployment( card, NextEventAction, ovrd );
			}
			else
				NextEventAction();
		}

		void OptionalDeployment( OptionalDeployment od )
		{
			Debug.Log( "SagaEventManager()::PROCESSING OptionalDeployment" );
			DeploymentGroupOverride ovrd = new DeploymentGroupOverride( "" );
			ovrd.deploymentPoint = od.deploymentPoint;
			ovrd.specificDeploymentPoint = od.specificDeploymentPoint;
			ovrd.useThreat = od.useThreat;
			ovrd.threatCost = od.threatCost;
			if ( od.isOnslaught )
				FindObjectOfType<SagaController>().deploymentPopup.Show( DeployMode.Onslaught, true, true, NextEventAction, ovrd );
			else
				FindObjectOfType<SagaController>().deploymentPopup.Show( DeployMode.Landing, true, true, NextEventAction, ovrd );
		}

		void RandomDeployment( RandomDeployment rd )
		{
			Debug.Log( "SagaEventManager()::PROCESSING RandomDeployment" );
			DeploymentCard cd = null;
			List<DeploymentCard> list = new List<DeploymentCard>();

			DeploymentGroupOverride ovrd = new DeploymentGroupOverride( "" );
			ovrd.deploymentPoint = rd.deploymentPoint;
			ovrd.specificDeploymentPoint = rd.specificDeploymentPoint;
			int threatLimit = 0;

			if ( rd.threatType == ThreatModifierType.Fixed )
				threatLimit = Math.Abs( rd.fixedValue );
			else
				threatLimit = Math.Abs( rd.threatLevel ) * DataStore.sagaSessionData.setupOptions.threatLevel;

			Debug.Log( $"THREAT COST LIMIT::{threatLimit}" );

			do
			{
				var p = DataStore.deploymentCards
					.OwnedPlusOther()
					.MinusDeployed()
					.MinusInDeploymentHand()
					.MinusStarting()
					.MinusReserved()
					.MinusIgnored()
					.FilterByFaction()
					.Concat( DataStore.sagaSessionData.EarnedVillains )
					.Where( x => x.cost <= threatLimit && !list.ContainsCard( x ) )
					.ToList();
				if ( p.Count > 0 )
				{
					int[] rnd = GlowEngine.GenerateRandomNumbers( p.Count );
					cd = p[rnd[0]];
					list.Add( cd );
					threatLimit -= cd.cost;
				}
				else
					cd = null;
			} while ( cd != null );

			if ( list.Count > 0 )
			{
				//deploy any groups picked (skips elite upgrade RNG)
				FindObjectOfType<SagaController>().dgManager.DeployGroupListWithOverride( list, ovrd, NextEventAction );
			}
			else
				NextEventAction();
		}

		void AddGroupToHand( AddGroupDeployment ag )
		{
			foreach ( var dg in ag.groupsToAdd )
			{
				var group = DataStore.allEnemyDeploymentCards.Where( x => x.id == dg.id ).FirstOr( null );
				if ( group != null )
				{
					DataStore.deploymentHand.Add( group );
					Debug.Log( $"SagaEventManager()::AddGroupDeployment::ADDED GROUP TO HAND::{group.name}, {group.id}" );
				}
			}
			NextEventAction();
		}

		//GROUP MANIPULATIONS
		void ChangeGroupInstructions( ChangeInstructions ci )
		{
			Debug.Log( "SagaEventManager()::PROCESSING ChangeInstructions" );
			if ( ci.groupsToAdd.Count == 0 )//apply to ALL
			{
				var ovrd = DataStore.sagaSessionData.gameVars.CreateDeploymentOverride();
				ovrd.SetInstructionOverride( ci );
			}
			else//apply to specific groups
			{
				foreach ( var dg in ci.groupsToAdd )
				{
					var ovrd = DataStore.sagaSessionData.gameVars.CreateDeploymentOverride( dg.id );
					ovrd.SetInstructionOverride( ci );
				}
			}
			NextEventAction();
		}

		void ChangeGroupTarget( ChangeTarget ct )
		{
			if ( ct.groupType == GroupType.All )//apply to ALL
			{
				Debug.Log( "SagaEventManager()::PROCESSING ChangeTarget::ALL" );
				var ovrd = DataStore.sagaSessionData.gameVars.CreateDeploymentOverride();
				ovrd.SetTargetOverride( ct );
			}
			else
			{
				foreach ( var dg in ct.groupsToAdd )
				{
					Debug.Log( $"SagaEventManager()::PROCESSING ChangeTarget::SPECIFIC::{dg.id}::{dg.name}" );
					var ovrd = DataStore.sagaSessionData.gameVars.CreateDeploymentOverride( dg.id );
					ovrd.SetTargetOverride( ct );
				}
			}
			NextEventAction();
		}

		void ChangeGroupStatus( ChangeGroupStatus cs )
		{
			string readied = "", exhausted = "";
			foreach ( var grp in cs.readyGroups )
			{
				readied += $"<color=orange>{grp.name} [{grp.id}]</color> \r\n";
				FindObjectOfType<SagaController>().dgManager.ReadyGroup( grp.id );
			}
			foreach ( var grp in cs.exhaustGroups )
			{
				exhausted += $"<color=orange>{grp.name} [{grp.id}]</color> \r\n";
				FindObjectOfType<SagaController>().dgManager.ExhaustGroup( grp.id );
			}

			string output = "";
			if ( !string.IsNullOrEmpty( readied ) )
				output = $"{DataStore.uiLanguage.sagaMainApp.groupsReadyUC}:\r\n" + readied + "\r\n";
			if ( !string.IsNullOrEmpty( exhausted ) )
				output += $"{DataStore.uiLanguage.sagaMainApp.groupsExhaustUC}:\r\n" + exhausted;

			if ( !string.IsNullOrEmpty( output ) )
			{
				ShowTextBox( output, NextEventAction );
			}
			else
				NextEventAction();
		}

		void ChangeReposition( ChangeReposition cr )
		{
			Debug.Log( "SagaEventManager()::PROCESSING ChangeReposition" );
			if ( cr.useSpecific )
			{
				foreach ( var item in cr.repoGroups )
				{
					var ovrd = DataStore.sagaSessionData.gameVars.CreateDeploymentOverride( item.id );
					ovrd.repositionInstructions = cr.theText;
				}
			}
			else//apply to all
			{
				var ovrd = DataStore.sagaSessionData.gameVars.CreateDeploymentOverride();
				ovrd.repositionInstructions = cr.theText;
			}

			NextEventAction();
		}

		void ResetGroup( ResetGroup rg )
		{
			Debug.Log( "SagaEventManager()::PROCESSING ResetGroup" );
			if ( rg.resetAll )
			{
				DataStore.sagaSessionData.gameVars.RemoveAllOverrides();
			}
			else
			{
				foreach ( var item in rg.groupsToAdd )
				{
					DataStore.sagaSessionData.gameVars.RemoveOverride( item.id );
				}
			}

			NextEventAction();
		}

		//MAPS & TOKENS
		void MapManagement( MapManagement mm )
		{
			Debug.Log( "SagaEventManager()::PROCESSING MapManagement" );
			if ( mm.mapSection != Guid.Empty )
			{
				var tiles = FindObjectOfType<SagaController>().tileManager.ActivateMapSection( mm.mapSection );
				FindObjectOfType<TileManager>().CamToSection( mm.mapSection );
				ShowTextBox( $"{DataStore.uiLanguage.sagaMainApp.mmAddTilesUC}:\n\n<color=orange>{string.Join( ", ", tiles.Item1 )}</color>\n\n{DataStore.uiLanguage.sagaMainApp.mmAddEntitiesUC}:\n\n{string.Join( "\n", tiles.Item2 )}", () =>
				{
					NextEventAction();
				} );
			}
			if ( mm.mapSectionRemove != Guid.Empty )
			{
				var tiles = FindObjectOfType<SagaController>().tileManager.DeactivateMapSection( mm.mapSectionRemove );
				FindObjectOfType<TileManager>().CamToSection( mm.mapSectionRemove );
				ShowTextBox( $"{DataStore.uiLanguage.sagaMainApp.mmRemoveTilesUC}:\n\n<color=orange>{string.Join( ", ", tiles )}</color>", () =>
					{
						NextEventAction();
					} );
			}
			if ( mm.mapTile != Guid.Empty )
			{
				string t = FindObjectOfType<SagaController>().tileManager.ActivateTile( mm.mapTile );

				FindObjectOfType<TileManager>().CamToTile( t.Replace( " ", "_" ) );
				ShowTextBox( $"{DataStore.uiLanguage.sagaMainApp.mmAddTilesUC}:\n\n<color=orange>{t}</color>", () =>
				{
					NextEventAction();
				} );
			}
			if ( mm.mapTileRemove != Guid.Empty )
			{
				string t = FindObjectOfType<SagaController>().tileManager.DeactivateTile( mm.mapTileRemove );
				//FindObjectOfType<TileManager>().CamToTile( t.Replace( " ", "_" ) );
				ShowTextBox( $"{DataStore.uiLanguage.sagaMainApp.mmRemoveTilesUC}:\n\n<color=orange>{t}</color>", () =>
				{
					NextEventAction();
				} );
			}
		}

		void ModifyMapEntity( ModifyMapEntity mod )
		{
			var em = FindObjectOfType<MapEntityManager>();
			em.ModifyPrefabs( mod );

			NextEventAction();
		}

		/// <summary>
		/// this is a special case because it can be called directly WITHOUT an event to fire it
		/// does NOT call NextEventAction(), parses text for glyphs
		/// </summary>
		public void ShowPromptBox( QuestionPrompt prompt, Action callback = null )
		{
			Debug.Log( "SagaEventManager()::PROCESSING QuestionPrompt" );

			if ( string.IsNullOrEmpty( prompt.theText?.Trim() ) )
			{
				Debug.Log( "ShowPromptBox()::NO TEXT" );
				callback?.Invoke();
				return;
			}
			var go = Instantiate( promptBoxPrefab, transform );
			var tb = go.transform.Find( "TextBox" ).GetComponent<PromptBox>();
			tb.Show( prompt, callback );
		}
	}
}