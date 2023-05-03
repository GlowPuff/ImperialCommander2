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
			//GlowEngine.FindUnityObject<QuickMessage>().Show( "Mission Info Updated" );
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
				//GlowEngine.FindUnityObject<QuickMessage>().Show( DataStore.uiLanguage.uiMainApp.pauseDepMsgUC );
			}
			if ( mm.unpauseDeployment )
			{
				DataStore.sagaSessionData.gameVars.pauseDeployment = false;
				//GlowEngine.FindUnityObject<QuickMessage>().Show( DataStore.uiLanguage.uiMainApp.unPauseDepMsgUC );
			}
			if ( mm.pauseThreat )
			{
				DataStore.sagaSessionData.gameVars.pauseThreatIncrease = true;
				//GlowEngine.FindUnityObject<QuickMessage>().Show( DataStore.uiLanguage.uiMainApp.pauseThreatMsgUC );
			}
			if ( mm.unpauseThreat )
			{
				DataStore.sagaSessionData.gameVars.pauseThreatIncrease = false;
				//GlowEngine.FindUnityObject<QuickMessage>().Show( DataStore.uiLanguage.uiMainApp.UnPauseThreatMsgUC );
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

					ShowTextBox( $"<color=orange><uppercase><b>{DataStore.uiLanguage.sagaMainApp.endOfMissionUC}</color>\n\n{DataStore.uiLanguage.uiMainApp.fameHeading}: <color=orange>{DataStore.sagaSessionData.gameVars.fame}</color>\n\n{DataStore.uiLanguage.uiMainApp.awardsHeading}: <color=orange>{awards}</color>", () =>
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
				//FindObjectOfType<SagaEventManager>().toggleVisButton.SetActive( true );
				//FindObjectOfType<SagaController>().ToggleNavAndEntitySelection( false );
				ShowTextBox( changeObjective.longText, () =>
				{
					FindObjectOfType<SagaController>().OnChangeObjective( changeObjective.theText, () =>
					{
						//FindObjectOfType<SagaEventManager>().toggleVisButton.SetActive( false );
						//FindObjectOfType<SagaController>().ToggleNavAndEntitySelection( true );
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
			if ( eg != null )
				FindObjectOfType<SagaController>().triggerManager.FireEventGroup( eg.GUID );
			NextEventAction();
		}

		void ShowInputBox( InputPrompt ip )
		{
			Debug.Log( "ShowInputBox()::PROCESSING ShowTextBox" );
			var go = Instantiate( inputBoxPrefab, transform );
			var tb = go.transform.Find( "InputBox" ).GetComponent<InputBox>();
			tb.Show( ip, NextEventAction );
		}

		//DEPLOYMENTS
		void EnemyDeployment( EnemyDeployment ed )
		{
			Debug.Log( "SagaEventManager()::PROCESSING EnemyDeployment" );
			DeploymentCard card = DataStore.GetEnemy( ed.deploymentGroup );

			var ovrd = DataStore.sagaSessionData.gameVars.CreateDeploymentOverride( ed.deploymentGroup );
			//name
			if ( !string.IsNullOrEmpty( ed.enemyName ) )
			{
				ovrd.nameOverride = ed.enemyName;
				ed.enemyGroupData.cardName = ed.enemyName;
			}
			else
			{
				ovrd.nameOverride = card.name;
				ed.enemyGroupData.cardName = card.name;
			}
			//custom instructions
			if ( ed.useCustomInstructions )
				ovrd.SetInstructionOverride( new ChangeInstructions()
				{
					instructionType = ed.enemyGroupData.customInstructionType,
					theText = ed.enemyGroupData.customText
				} );
			//set the main override data
			ovrd.SetEnemyDeploymentOverride( ed );
			//generic mugshot (DEPRECATED)
			ovrd.useGenericMugshot = ed.useGenericMugshot;

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
				//generic mugshot
				ovrd.useGenericMugshot = ad.useGenericMugshot;
				if ( ovrd.useGenericMugshot )
					card.mugShotPath = "CardThumbnails/genericAlly";
				//name
				if ( !string.IsNullOrEmpty( ad.allyName ) )
					ovrd.nameOverride = ad.allyName;
				else
					ovrd.nameOverride = card.name;
				//trigger
				ovrd.setTrigger = ad.setTrigger;
				//event
				ovrd.setEvent = ad.setEvent;
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
					.MinusCannotRedeploy()
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

		void CustomDeployment( CustomEnemyDeployment ced )
		{
			Debug.Log( "SagaEventManager()::PROCESSING CustomDeployment" );
			var ovrd = DataStore.sagaSessionData.gameVars.CreateCustomDeploymentOverride( ced );
			if ( ced.useDeductCost )
				DataStore.sagaSessionData.ModifyThreat( -ced.groupCost );

			DeploymentCard card = DeploymentCard.CreateCustomCard( ced );
			ovrd.customCard = card;
			if ( ovrd.customType == MarkerType.Imperial )
				FindObjectOfType<SagaController>().dgManager.DeployGroup( card, true );
			else
				FindObjectOfType<SagaController>().dgManager.DeployHeroAlly( card );
			FindObjectOfType<SagaController>().dgManager.HandleMapDeployment( card, NextEventAction, ovrd );
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

			//string output = "";
			//if ( !string.IsNullOrEmpty( readied ) )
			//	output = $"{DataStore.uiLanguage.sagaMainApp.groupsReadyUC}:\r\n" + readied + "\r\n";
			//if ( !string.IsNullOrEmpty( exhausted ) )
			//	output += $"{DataStore.uiLanguage.sagaMainApp.groupsExhaustUC}:\r\n" + exhausted;

			//if ( !string.IsNullOrEmpty( output ) )
			//{
			//	ShowTextBox( output, NextEventAction );
			//}
			//else
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

		void RemoveGroup( RemoveGroup rg )
		{
			Debug.Log( "SagaEventManager()::PROCESSING RemoveGroup" );
			foreach ( var item in rg.groupsToRemove )
			{
				bool returnToHand = true;
				var ovrd = DataStore.sagaSessionData.gameVars.GetDeploymentOverride( item.id );
				//test if it can redeploy
				if ( ovrd != null && !ovrd.canRedeploy )
				{
					DataStore.sagaSessionData.CannotRedeployList.Add( item.id );
					//completely reset if it can't redeploy, so it can be manually deployed "clean" later
					DataStore.sagaSessionData.gameVars.RemoveOverride( item.id );
					returnToHand = false;
				}

				var card = DataStore.allEnemyDeploymentCards.GetDeploymentCard( item.id );
				if ( card != null )
				{
					//return it to the Hand if it can redeploy
					if ( card.id != "DG070" && card.characterType != CharacterType.Villain && returnToHand )
					{
						DataStore.deploymentHand.Add( card );
					}
					//remove it from deployed list
					DataStore.deployedEnemies.RemoveCardByID( card );
				}
				//if it is an EARNED villain, add it back into manual deploy list
				if ( DataStore.sagaSessionData.EarnedVillains.ContainsCard( card ) && !DataStore.manualDeploymentList.ContainsCard( card ) )
				{
					DataStore.manualDeploymentList.Add( card );
					DataStore.SortManualDeployList();
				}
				//finally, reset the group if needed
				if ( ovrd != null && ovrd.canRedeploy )
				{
					if ( ovrd.useResetOnRedeployment )
						DataStore.sagaSessionData.gameVars.RemoveOverride( ovrd.ID );
					else if ( !ovrd.useResetOnRedeployment )
						ovrd.ResetDP();
				}

				if ( DataStore.deployedEnemies.Count == 0 )
					FindObjectOfType<SagaController>().eventManager.CheckIfEventsTriggered();

				//remove icon from the enemy column
				FindObjectOfType<SagaController>().dgManager.RemoveGroup( card.id );

				//remove any override
				//DataStore.sagaSessionData.gameVars.RemoveOverride( item.id );
				//if ( card != null )
				//{
				//	//remove it from the hand
				//	DataStore.deploymentHand.Add( card );
				//	//remove it from the deployment list
				//	DataStore.deployedEnemies.Remove( card );
				//	//remove icon from the enemy column
				//	FindObjectOfType<SagaController>().dgManager.RemoveGroup( card.id );
				//}
			}

			foreach ( var item in rg.allyGroupsToRemove )
			{
				//remove any override
				DataStore.sagaSessionData.gameVars.RemoveOverride( item.id );
				var card = DataStore.allyCards.GetDeploymentCard( item.id );
				if ( card != null )
				{
					//remove it from the deployed heroes list
					DataStore.deployedHeroes.Remove( card );
					//remove icon from the ally column
					FindObjectOfType<SagaController>().dgManager.RemoveGroup( card.id );
				}
			}

			NextEventAction();
		}

		//MAPS & TOKENS
		void MapManagement( MapManagement mm )
		{
			Debug.Log( "SagaEventManager()::PROCESSING MapManagement" );
			//activate map section
			if ( mm.mapSection != Guid.Empty )
			{
				var tiles = FindObjectOfType<SagaController>().tileManager.ActivateMapSection( mm.mapSection );
				FindObjectOfType<TileManager>().CamToSection( mm.mapSection );
				var tmsg = string.Join( ", ", tiles.Item1 );
				var emsg = DataStore.uiLanguage.sagaMainApp.mmAddEntitiesUC + ":\n\n";
				var emsg2 = string.Join( "\n", tiles.Item2 );
				emsg = string.IsNullOrEmpty( emsg2.Trim() ) ? "" : emsg + emsg2;

				ShowTextBox( $"{DataStore.uiLanguage.sagaMainApp.mmAddTilesUC}:\n\n<color=orange>{tmsg}</color>\n\n{emsg}", () =>
				{
					//see if there is an optional deployment waiting for an Active DP
					if ( DataStore.sagaSessionData.gameVars.delayOptionalDeployment )
					{
						var dp = FindObjectOfType<SagaController>().mapEntityManager.GetActiveDeploymentPoint( null );
						if ( dp != Guid.Empty )
						{
							DataStore.sagaSessionData.gameVars.delayOptionalDeployment = false;
							FindObjectOfType<SagaController>().deploymentPopup.Show( DeployMode.Landing, false, true, NextEventAction );
						}
						else
							NextEventAction();
					}
					else
						NextEventAction();
				} );
			}
			//deactivate map section
			if ( mm.mapSectionRemove != Guid.Empty )
			{
				var tiles = FindObjectOfType<SagaController>().tileManager.DeactivateMapSection( mm.mapSectionRemove );
				FindObjectOfType<TileManager>().CamToSection( mm.mapSectionRemove );
				ShowTextBox( $"{DataStore.uiLanguage.sagaMainApp.mmRemoveTilesUC}:\n\n<color=orange>{string.Join( ", ", tiles )}</color>", () =>
					{
						NextEventAction();
					} );
			}
			//activate tile
			if ( mm.mapTile != Guid.Empty )
			{
				string t = FindObjectOfType<SagaController>().tileManager.ActivateTile( mm.mapTile );

				FindObjectOfType<TileManager>().CamToTile( mm.mapTile );
				ShowTextBox( $"{DataStore.uiLanguage.sagaMainApp.mmAddTilesUC}:\n\n<color=orange>{t}</color>", () =>
				{
					NextEventAction();
				} );
			}
			//deactivate tile
			if ( mm.mapTileRemove != Guid.Empty )
			{
				string t = FindObjectOfType<SagaController>().tileManager.DeactivateTile( mm.mapTileRemove );

				FindObjectOfType<TileManager>().CamToTile( mm.mapTileRemove );
				ShowTextBox( $"{DataStore.uiLanguage.sagaMainApp.mmRemoveTilesUC}:\n\n<color=orange>{t}</color>", () =>
				{
					NextEventAction();
				} );
			}
		}

		void ModifyMapEntity( ModifyMapEntity mod )
		{
			var em = FindObjectOfType<MapEntityManager>();
			em.ModifyPrefabs( mod, () =>
			{
				//see if there is an optional deployment waiting for an Active DP
				if ( DataStore.sagaSessionData.gameVars.delayOptionalDeployment )
				{
					var dp = FindObjectOfType<SagaController>().mapEntityManager.GetActiveDeploymentPoint( null );
					if ( dp != Guid.Empty )
					{
						DataStore.sagaSessionData.gameVars.delayOptionalDeployment = false;
						FindObjectOfType<SagaController>().deploymentPopup.Show( DeployMode.Landing, false, true, NextEventAction );
					}
					else
						NextEventAction();
				}
				else
					NextEventAction();
			} );
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