using System.Collections.Generic;
using UnityEngine;

public class DeploymentGroupManager : MonoBehaviour
{
	public Transform gridContainer, heroContainer;
	public GameObject dgPrefab, hgPrefab;

	Sound sound;

	private void Awake()
	{
		sound = FindObjectOfType<Sound>();
	}

	/// <summary>
	/// deploys hero/ally to hero box and adds it to deployed hero list
	/// </summary>
	public void DeployHeroAlly( CardDescriptor cd )
	{
		if ( DataStore.deployedHeroes.Contains( cd ) )
		{
			Debug.Log( cd.name + " already deployed" );
			return;
		}

		//a new healthy hero/ally
		cd.heroState = new HeroState();
		cd.heroState.Init( DataStore.sessionData.MissionHeroes.Count );

		var go = Instantiate( hgPrefab, heroContainer );
		go.GetComponent<HGPrefab>().Init( cd );
		if ( !DataStore.deployedHeroes.Contains( cd ) )
			DataStore.deployedHeroes.Add( cd );
		sound.PlaySound( FX.Computer );
	}

	public void DeployStartingGroups()
	{
		foreach ( var cd in DataStore.sessionData.MissionStarting )
		{
			cd.currentSize = cd.size;
			cd.hasActivated = false;
			var go = Instantiate( dgPrefab, gridContainer );
			go.GetComponent<DGPrefab>().Init( cd );
			DataStore.deployedEnemies.Add( cd );
		}
		var rt = gridContainer.GetComponent<RectTransform>();
		rt.localPosition = new Vector3( 20, -3000, 0 );
		sound.PlaySound( FX.Deploy );
	}

	public void RestoreState()
	{
		//restore enemy groups
		for ( int i = 0; i < DataStore.deployedEnemies.Count; i++ )
		{
			var go = Instantiate( dgPrefab, gridContainer );
			go.GetComponent<DGPrefab>().Init( DataStore.deployedEnemies[i] );
			go.GetComponent<DGPrefab>().SetGroupSize( DataStore.deployedEnemies[i].currentSize );
		}
		var rt = gridContainer.GetComponent<RectTransform>();
		rt.localPosition = new Vector3( 20, -3000, 0 );

		//restore heroes and allies
		for ( int i = 0; i < DataStore.deployedHeroes.Count; i++ )
		{
			var go = Instantiate( hgPrefab, heroContainer );
			go.GetComponent<HGPrefab>().Init( DataStore.deployedHeroes[i] );
		}
	}

	/// <summary>
	/// Takes an enemy or villain, applies difficulty modifier, deploys, removes from dep hand, adds to deployed list
	/// </summary>
	public void DeployGroup( CardDescriptor cardDescriptor, bool skipEliteModify = false )
	{
		cardDescriptor.hasActivated = false;
		// EASY: Any time an Elite group is deployed, it has a 15% chance to be downgraded to a normal group without refunding of threat. ( If the respective normal group is still available.)
		if ( DataStore.sessionData.difficulty == Difficulty.Easy &&
			!skipEliteModify &&
			cardDescriptor.isElite &&
			GlowEngine.RandomBool( 15 ) )
		{
			//see if normal version exists, include dep hand
			var nonE = DataStore.GetNonEliteVersion( cardDescriptor );
			if ( nonE != null )
			{
				Debug.Log( "DeployGroup EASY mode Elite downgrade: " + nonE.name );
				cardDescriptor = nonE;
				GlowEngine.FindObjectsOfTypeSingle<QuickMessage>().Show( DataStore.uiLanguage.uiMainApp.eliteDowngradeMsgUC );
			}
		}

		//Hard: Threat increase x1.3 Any time a normal group is deployed, it has a 15 % chance to be upgraded to an Elite group at no additional threat cost. ( If the respective normal group is still available.) Deployment Modifier starts at 2 instead of 0.
		if ( DataStore.sessionData.difficulty == Difficulty.Hard &&
			!skipEliteModify &&
			!cardDescriptor.isElite &&
			GlowEngine.RandomBool( 15 ) )
		{
			//see if elite version exists, include dep hand
			var elite = DataStore.GetEliteVersion( cardDescriptor );
			if ( elite != null )
			{
				Debug.Log( "DeployGroup HARD mode Elite upgrade: " + elite.name );
				cardDescriptor = elite;
				GlowEngine.FindObjectsOfTypeSingle<QuickMessage>().Show( DataStore.uiLanguage.uiMainApp.eliteUpgradeMsgUC );
			}
			else
				Debug.Log( "SKIPPED: " + cardDescriptor.name );
		}

		if ( DataStore.deployedEnemies.Contains( cardDescriptor ) )
		{
			Debug.Log( cardDescriptor.name + " already deployed" );
			return;
		}

		cardDescriptor.currentSize = cardDescriptor.size;
		var go = Instantiate( dgPrefab, gridContainer );
		go.GetComponent<DGPrefab>().Init( cardDescriptor );

		//add it to deployed enemies
		DataStore.deployedEnemies.Add( cardDescriptor );
		//if it's FROM the dep hand, remove it
		//should have already been removed *IF* it's from DeploymentPopup
		//otherwise it just got (up/down)graded to/from Elite
		DataStore.deploymentHand.Remove( cardDescriptor );

		sound.playDeploymentSound( cardDescriptor.id );

		//var rt = gridContainer.GetComponent<RectTransform>();
		//rt.localPosition = new Vector3( 20, -3000, 0 );
	}

	/// <summary>
	/// updates current deploy size
	/// </summary>
	public void UpdateGroups()
	{
		foreach ( Transform enemy in gridContainer )
		{
			enemy.GetComponent<DGPrefab>().UpdateCount();
		}
	}

	public List<CardDescriptor> GetNonExhaustedGroups()
	{
		var cd = new List<CardDescriptor>();
		foreach ( Transform c in gridContainer )
		{
			var pf = c.GetComponent<DGPrefab>();
			if ( !pf.IsExhausted )
				cd.Add( pf.Card );
		}
		return cd;
	}

	public void ExhaustGroup( string id )
	{
		foreach ( Transform c in gridContainer )
		{
			var pf = c.GetComponent<DGPrefab>();
			if ( pf.Card.id == id )
			{
				pf.ToggleExhausted( true );
				return;
			}
		}
	}

	public void ReadyAllGroups()
	{
		foreach ( Transform c in gridContainer )
		{
			var pf = c.GetComponent<DGPrefab>();
			pf.ToggleExhausted( false );
		}
		foreach ( Transform c in heroContainer )
		{
			var pf = c.GetComponent<HGPrefab>();
			pf.ResetActivation();
		}
	}
}
