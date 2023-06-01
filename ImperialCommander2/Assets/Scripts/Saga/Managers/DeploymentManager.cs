using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Saga
{
	public class DeploymentManager : MonoBehaviour
	{
		public Transform heroContainer, enemyContainer;
		public GameObject hgPrefab;
		public GameObject dgPrefab;

		Sound sound;

		private void Awake()
		{
			sound = FindObjectOfType<Sound>();
		}

		public void DeployHeroAlly( DeploymentCard cd )
		{
			if ( DataStore.deployedHeroes.ContainsCard( cd ) )
			{
				Debug.Log( cd.name + " already deployed" );
				return;
			}

			DataStore.sagaSessionData.missionLogger.LogEvent( MissionLogType.GroupDeployment, cd.name );

			//a new healthy hero/ally
			cd.heroState = new HeroState();
			cd.heroState.Init();// DataStore.sagaSessionData.MissionHeroes.Count );

			var go = Instantiate( hgPrefab, heroContainer );
			go.GetComponent<SagaHGPrefab>().Init( cd );
			if ( !DataStore.deployedHeroes.ContainsCard( cd ) )
				DataStore.deployedHeroes.Add( cd );
			sound.PlaySound( FX.Computer );
		}

		/// <summary>
		/// Creates the icons and handles visual map deployment
		/// </summary>
		public void DeployStartingGroups( Action callback = null )
		{
			//create the icons
			foreach ( var cd in DataStore.sagaSessionData.MissionStarting )
			{
				cd.currentSize = cd.size;
				cd.hasActivated = false;
				var go = Instantiate( dgPrefab, enemyContainer );
				go.GetComponent<SagaDGPrefab>().Init( cd );
				DataStore.deployedEnemies.Add( cd );
			}

			//handle visual map deployment
			StartCoroutine( HandleMultipleMapDeployment( DataStore.sagaSessionData.MissionStarting.ToArray(), callback ) );

			sound.PlaySound( FX.Deploy );
		}

		public void RestoreState()
		{
			//restore enemy groups
			for ( int i = 0; i < DataStore.deployedEnemies.Count; i++ )
			{
				var go = Instantiate( dgPrefab, enemyContainer );
				go.GetComponent<SagaDGPrefab>().Init( DataStore.deployedEnemies[i] );
				go.GetComponent<SagaDGPrefab>().SetGroupSize( DataStore.deployedEnemies[i].currentSize );
			}

			//restore heroes and allies
			for ( int i = 0; i < DataStore.deployedHeroes.Count; i++ )
			{
				var go = Instantiate( hgPrefab, heroContainer );
				go.GetComponent<SagaHGPrefab>().Init( DataStore.deployedHeroes[i] );
			}
		}

		/// <summary>
		/// Add icon to the game, (Optionally) does RNG elite up/downgrade, removes it from the hand
		/// </summary>
		public void DeployGroup( DeploymentCard cardDescriptor, bool skipEliteModify = false )
		{
			DataStore.sagaSessionData.missionLogger.LogEvent( MissionLogType.GroupDeployment, cardDescriptor.name );
			DoDeployGroup( cardDescriptor, skipEliteModify );
		}

		/// <summary>
		/// Add icon to the game, (Optionally) does RNG elite up/downgrade, removes it from the hand, also handles visible map deployment
		/// </summary>
		public void DeployGroupList( List<DeploymentCard> cardDescriptor, bool skipEliteModify = false, Action callback = null )
		{
			for ( int i = 0; i < cardDescriptor.Count; i++ )
			{
				DoDeployGroup( cardDescriptor[i], skipEliteModify );
			}

			StartCoroutine( HandleMultipleMapDeployment( cardDescriptor.ToArray(), callback ) );
		}

		public void DeployGroupListWithOverride( List<DeploymentCard> cardDescriptor, DeploymentGroupOverride ovrd, Action callback = null )
		{
			Debug.Log( "OPTIONAL DEPLOYMENT" );
			for ( int i = 0; i < cardDescriptor.Count; i++ )
			{
				DoDeployGroup( cardDescriptor[i], true );
			}

			StartCoroutine( HandleMultipleMapDeployment( cardDescriptor.ToArray(), callback, ovrd ) );
		}

		/// <summary>
		/// Add icon to the game, (Optionally) does RNG elite up/downgrade, removes it from the hand
		/// </summary>
		void DoDeployGroup( DeploymentCard cardDescriptor, bool skipEliteModify = false )
		{
			cardDescriptor.hasActivated = false;
			// EASY: Any time an Elite group is deployed, it has a 15% chance to be downgraded to a normal group without refunding of threat. ( If the respective normal group is still available.)
			if ( DataStore.sagaSessionData.setupOptions.difficulty == Difficulty.Easy
				&& !skipEliteModify
				&& cardDescriptor.isElite
				&& GlowEngine.RandomBool( 15 ) )
			{
				//see if normal version exists, include dep hand
				var nonE = DataStore.GetNonEliteVersion( cardDescriptor );
				if ( nonE != null )
				{
					Debug.Log( "DeployGroup EASY mode Elite downgrade: " + nonE.name );
					cardDescriptor = nonE;
					//GlowEngine.FindUnityObject<QuickMessage>().Show( DataStore.uiLanguage.uiMainApp.eliteDowngradeMsgUC );
				}
			}

			//Hard: Threat increase x1.3 Any time a normal group is deployed, it has a 15 % chance to be upgraded to an Elite group at no additional threat cost. ( If the respective normal group is still available.) Deployment Modifier starts at 2 instead of 0.
			if ( DataStore.sagaSessionData.setupOptions.difficulty == Difficulty.Hard
				&& !skipEliteModify
				&& !cardDescriptor.isElite
				&& GlowEngine.RandomBool( 15 ) )
			{
				//see if elite version exists, include dep hand
				var elite = DataStore.GetEliteVersion( cardDescriptor );
				if ( elite != null )
				{
					Debug.Log( "DeployGroup HARD mode Elite upgrade: " + elite.name );
					cardDescriptor = elite;
					//GlowEngine.FindUnityObject<QuickMessage>().Show( DataStore.uiLanguage.uiMainApp.eliteUpgradeMsgUC );
				}
				else
					Debug.Log( "SKIPPED: " + cardDescriptor.name );
			}

			if ( DataStore.deployedEnemies.ContainsCard( cardDescriptor ) )
			{
				Debug.Log( $"{cardDescriptor.name}::{cardDescriptor.id} already deployed" );
				GlowEngine.FindUnityObject<QuickMessage>().Show( $"Tried to Deploy {cardDescriptor.name} [{cardDescriptor.id}], but it's already deployed." );
				return;
			}

			cardDescriptor.currentSize = cardDescriptor.size;
			var go = Instantiate( dgPrefab, enemyContainer );
			go.GetComponent<SagaDGPrefab>().Init( cardDescriptor );

			//add it to deployed enemies
			DataStore.deployedEnemies.Add( cardDescriptor );
			//if it's FROM the dep hand, remove it
			//should have already been removed *IF* it's from DeploymentPopup
			//otherwise it just got (up/down)graded to/from Elite or it's from the event action
			DataStore.deploymentHand.Remove( cardDescriptor );
		}

		/// <summary>
		/// updates current deploy size
		/// </summary>
		public void UpdateGroups()
		{
			foreach ( Transform enemy in enemyContainer )
			{
				enemy.GetComponent<SagaDGPrefab>().UpdateCount();
			}
		}

		public List<DeploymentCard> GetNonExhaustedGroups()
		{
			var cd = new List<DeploymentCard>();
			foreach ( Transform c in enemyContainer )
			{
				var pf = c.GetComponent<SagaDGPrefab>();
				if ( !pf.IsExhausted )
					cd.Add( pf.Card );
			}
			return cd;
		}

		public void ExhaustGroup( string id )
		{
			foreach ( Transform c in enemyContainer )
			{
				var pf = c.GetComponent<SagaDGPrefab>();
				if ( pf.Card.id == id )
				{
					pf.ToggleExhausted( true );
					return;
				}
			}
		}

		public void RemoveGroup( string id )
		{
			foreach ( Transform c in enemyContainer )
			{
				var pf = c.GetComponent<SagaDGPrefab>();
				if ( pf.Card.id == id )
				{
					pf.RemoveSelf();
					return;
				}
			}
			foreach ( Transform c in heroContainer )
			{
				var pf = c.GetComponent<SagaHGPrefab>();
				if ( pf.Card.id == id )
				{
					pf.RemoveSelf();
					return;
				}
			}
		}

		public void ReadyGroup( string id )
		{
			foreach ( Transform c in enemyContainer )
			{
				var pf = c.GetComponent<SagaDGPrefab>();
				if ( pf.Card.id == id )
				{
					pf.ToggleExhausted( false );
					return;
				}
			}
		}

		public void ReadyAllGroups()
		{
			foreach ( Transform c in enemyContainer )
			{
				var pf = c.GetComponent<SagaDGPrefab>();
				pf.ToggleExhausted( false );
			}
			foreach ( Transform c in heroContainer )
			{
				var pf = c.GetComponent<SagaHGPrefab>();
				pf.ResetActivation();
			}
		}

		/// <summary>
		/// Checks for a group override before handling visual deployment on map, deploymentOverride is from optional deployment event action (null in other cases)
		/// </summary>
		IEnumerator HandleMultipleMapDeployment( DeploymentCard[] enemyToAdd, Action callback = null, DeploymentGroupOverride ovrd = null )
		{
			foreach ( DeploymentCard enemy in enemyToAdd )
			{
				bool done = false;
				var dpovrd = DataStore.sagaSessionData.gameVars.GetDeploymentOverride( enemy.id );
				if ( ovrd != null )
				{
					dpovrd = ovrd;
					dpovrd.nameOverride = enemy.name;
					dpovrd.ID = enemy.id;
				}

				HandleMapDeployment( enemy, () =>
					{
						done = true;
					}, dpovrd );

				while ( !done )
					yield return null;
			}

			callback?.Invoke();
		}

		/// <summary>
		/// Does the visual map deployment - uses override if exists, highlights DP, camera to DP, ovrd is from optional deployment event action
		/// </summary>
		public void HandleMapDeployment( DeploymentCard enemyToAdd, Action callback = null, DeploymentGroupOverride ovrd = null )
		{
			string cardID = enemyToAdd.id;
			sound.playDeploymentSound( cardID );

			//first, see if there is override data for this group
			//var ovrd = DataStore.sagaSessionData.gameVars.GetDeploymentOverride( enemyToAdd.id );
			if ( ovrd != null )
			{
				if ( ovrd.isCustomDeployment )
					cardID = "Custom";

				if ( ovrd.deploymentPoint == DeploymentSpot.Active )
				{
					Debug.Log( "EnemyDeployment::ACTIVE DP" );
					var adp = FindObjectOfType<MapEntityManager>().GetActiveDeploymentPoint( enemyToAdd );
					if ( adp != Guid.Empty )
					{
						FindObjectOfType<SagaEventManager>().toggleVisButton.SetActive( true );
						//FindObjectOfType<SagaController>().ToggleNavAndEntitySelection( false );
						FindObjectOfType<MapEntityManager>().ToggleHighlightDeploymentPoint( adp, true );
						FindObjectOfType<CameraController>().MoveToEntity( adp );
						//string enemyName = ovrd.useGenericMugshot ? "Rebel" : ovrd.nameOverride;
						string enemyName = ovrd.nameOverride;
						FindObjectOfType<SagaEventManager>().ShowTextBox( $"{DataStore.uiLanguage.sagaMainApp.deployMessageUC}:\n\n<color=white>{enemyName}</color>", () =>
						{
							// <color=orange>[{cardID}]</color>
							FindObjectOfType<SagaEventManager>().toggleVisButton.SetActive( false );
							//FindObjectOfType<SagaController>().ToggleNavAndEntitySelection( true );
							FindObjectOfType<MapEntityManager>().ToggleHighlightDeploymentPoint( adp, false );
							callback?.Invoke();
						} );
					}
					else
					{
						GlowEngine.FindUnityObject<QuickMessage>().Show( DataStore.uiLanguage.sagaMainApp.noDPWarningUC );
						callback?.Invoke();
					}
				}
				else if ( ovrd.deploymentPoint == DeploymentSpot.None )
				{
					Debug.Log( "EnemyDeployment::NONE DP" );
					FindObjectOfType<SagaEventManager>().toggleVisButton.SetActive( true );
					//string enemyName = ovrd.useGenericMugshot ? "Rebel" : ovrd.nameOverride;
					string enemyName = ovrd.nameOverride;
					FindObjectOfType<SagaEventManager>().ShowTextBox( $"{DataStore.uiLanguage.sagaMainApp.deployMessageUC}:\n\n<color=white>{enemyName}</color>", () =>
					{
						// <color=orange>[{cardID}]</color>
						FindObjectOfType<SagaEventManager>().toggleVisButton.SetActive( false );
						callback?.Invoke();
					} );
				}
				else//multiple specific DPs
				{
					Debug.Log( "EnemyDeployment::MULTIPLE DPs" );
					DoMultipleDeployment( enemyToAdd, ovrd, callback );
					//StartCoroutine( NavToDeployment( ovrd, callback ) );//also invokes callback when finished
				}
			}
			else//no override, just use Active DP
			{
				var adp = FindObjectOfType<MapEntityManager>().GetActiveDeploymentPoint( enemyToAdd );
				if ( adp != Guid.Empty )
				{
					FindObjectOfType<SagaEventManager>().toggleVisButton.SetActive( true );
					//FindObjectOfType<SagaController>().ToggleNavAndEntitySelection( false );
					FindObjectOfType<MapEntityManager>().ToggleHighlightDeploymentPoint( adp, true );
					FindObjectOfType<CameraController>().MoveToEntity( adp );
					//string enemyName = ovrd.useGenericMugshot ? "Rebel" : enemyToAdd.name;
					FindObjectOfType<SagaEventManager>().ShowTextBox( $"{DataStore.uiLanguage.sagaMainApp.deployMessageUC}:\n\n<color=white>{enemyToAdd.name}</color>", () =>
					{
						// <color=orange>[{cardID}]</color>
						FindObjectOfType<SagaEventManager>().toggleVisButton.SetActive( false );
						//FindObjectOfType<SagaController>().ToggleNavAndEntitySelection( true );
						FindObjectOfType<MapEntityManager>().ToggleHighlightDeploymentPoint( adp, false );
						callback?.Invoke();
					} );
				}
				else
				{
					GlowEngine.FindUnityObject<QuickMessage>().Show( DataStore.uiLanguage.sagaMainApp.noDPWarningUC );
					callback?.Invoke();
				}
			}
		}

		/// <summary>
		/// Handle deployments over multiple DPs
		/// </summary>
		void DoMultipleDeployment( DeploymentCard enemyToAdd, DeploymentGroupOverride ovrd, Action callback = null )
		{
			string cardID = ovrd.ID;
			if ( ovrd.isCustomDeployment )
				cardID = "Custom";
			FindObjectOfType<SagaEventManager>().toggleVisButton.SetActive( true );
			var adp = FindObjectOfType<MapEntityManager>().GetActiveDeploymentPoint( enemyToAdd );

			//show all DPs used
			var allDPs = ovrd.GetDeploymentPoints();
			foreach ( var dp in allDPs )
			{
				Guid guid = dp == Guid.Empty ? adp : dp;
				FindObjectOfType<MapEntityManager>().ToggleHighlightDeploymentPoint( guid, true );
			}

			if ( allDPs.Length > 0 )
				FindObjectOfType<CameraController>().MoveToEntity( allDPs[0] );

			//string enemyName = ovrd.useGenericMugshot ? "Rebel" : ovrd.nameOverride;
			string enemyName = ovrd.nameOverride;
			FindObjectOfType<SagaEventManager>().ShowTextBox( $"{DataStore.uiLanguage.sagaMainApp.deployMessageUC}:\n\n<color=white>{enemyName}</color>", () =>
			{
				// <color=orange>[{cardID}]</color>
				//hide all DPs used
				foreach ( var dp in ovrd.GetDeploymentPoints() )
				{
					Guid guid = dp == Guid.Empty ? adp : dp;
					FindObjectOfType<MapEntityManager>().ToggleHighlightDeploymentPoint( guid, false );
				}
				FindObjectOfType<SagaEventManager>().toggleVisButton.SetActive( false );
				callback?.Invoke();
			} );
		}

		//IEnumerator NavToDeployment( DeploymentGroupOverride ovrd, Action callback = null )
		//{
		//	FindObjectOfType<SagaController>().ToggleNavAndEntitySelection( false );
		//	FindObjectOfType<SagaEventManager>().toggleVisButton.SetActive( true );
		//	var adp = FindObjectOfType<MapEntityManager>().GetActiveDeploymentPoint();

		//	foreach ( var dp in ovrd.GetDeploymentPoints() )
		//	{
		//		var done = false;
		//		//if the dp is empty, use the Active DP
		//		Guid guid = dp == Guid.Empty ? adp : dp;
		//		FindObjectOfType<MapEntityManager>().ToggleHighlightDeploymentPoint( guid, true );
		//		FindObjectOfType<CameraController>().MoveToEntity( guid );
		//		FindObjectOfType<SagaEventManager>().ShowTextBox( $"{DataStore.uiLanguage.sagaMainApp.deployMessageUC}:\n\n<color=white>{ovrd.nameOverride}</color> <color=orange>[{ovrd.ID}]</color>", () =>
		//		{
		//			FindObjectOfType<MapEntityManager>().ToggleHighlightDeploymentPoint( guid, false );
		//			done = true;
		//		} );

		//		while ( !done )
		//			yield return null;
		//	}

		//	FindObjectOfType<SagaController>().ToggleNavAndEntitySelection( true );
		//	FindObjectOfType<SagaEventManager>().toggleVisButton.SetActive( false );
		//	callback?.Invoke();
		//}
	}
}
