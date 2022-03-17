using DG.Tweening;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventPopup : MonoBehaviour
{
	public Text eventTitle;
	public TextMeshProUGUI eventFlavor;
	public Image fader;
	public CanvasGroup cg;
	public TMP_FontAsset tmpImpeprialFont;

	CardEvent cardEvent;
	CardDescriptor allyToAdd, enemyToAdd, rebel1, rebel2;
	Action callback;

	public void Show( CardEvent ce, Action cb = null )
	{
		gameObject.SetActive( true );
		fader.color = new Color( 0, 0, 0, 0 );
		fader.DOFade( .95f, 1 );
		cg.DOFade( 1, .5f );
		transform.GetChild( 0 ).localScale = new Vector3( .85f, .85f, .85f );
		transform.GetChild( 0 ).DOScale( 1, .5f ).SetEase( Ease.OutExpo );

		callback = cb;
		cardEvent = ce;
		eventTitle.text = ce.eventName.ToLower();
		eventFlavor.text = ce.eventFlavor;
		allyToAdd = null;
		enemyToAdd = null;

		//pick 2 rebels/allies
		var hlist = DataStore.deployedHeroes.GetHealthy();
		//make sure there are valid heroes/allies to target
		if ( hlist != null && hlist.Count > 0 )
		{
			int[] rnd = GlowEngine.GenerateRandomNumbers( hlist.Count() );
			rebel1 = hlist[rnd[0]];
			if ( hlist.Count > 1 )
				rebel2 = hlist[rnd[1]];
			else
				rebel2 = rebel1;
		}
		else
		{
			rebel1 = new CardDescriptor() { name = "None" };
			rebel2 = rebel1;
		}

		foreach ( var s in ce.content )
			ParseCard( s );
	}

	public void OnClose()
	{
		FindObjectOfType<Sound>().PlaySound( FX.Click );
		fader.DOFade( 0, .5f ).OnComplete( () =>
		{
			Transform content = transform.Find( "Panel/content" );
			foreach ( Transform tf in content )
				Destroy( tf.gameObject );
			//handle deployment
			if ( allyToAdd != null )
				FindObjectOfType<DeploymentGroupManager>().DeployHeroAlly( allyToAdd );
			if ( enemyToAdd != null )
			{
				if ( DataStore.sessionData.gameVars.pauseDeployment )
				{
					GlowEngine.FindObjectsOfTypeSingle<QuickMessage>().Show( $"Imperial Deployment is <color=\"red\">PAUSED</color>.  The requested group [{enemyToAdd.name}] will not be deployed." );
				}
				else
				{
					FindObjectOfType<DeploymentGroupManager>().DeployGroup( enemyToAdd );
				}
			}

			gameObject.SetActive( false );

			callback?.Invoke();
		} );
		cg.DOFade( 0, .2f );
		transform.GetChild( 0 ).DOScale( .85f, .5f ).SetEase( Ease.OutExpo );
	}

	void ParseCard( string item )
	{
		Transform content = transform.Find( "Panel/content" );
		GameObject go = new GameObject( "content item" );
		go.layer = 5;
		go.transform.SetParent( content );
		go.transform.localScale = Vector3.one;
		go.transform.localEulerAngles = Vector3.zero;

		TextMeshProUGUI nt = go.AddComponent<TextMeshProUGUI>();
		nt.color = Color.white;
		nt.fontSize = 27;

		//replace glyphs
		item = item.Replace( "{H}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">H</font></color>" );
		item = item.Replace( "{C}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">C</font></color>" );
		item = item.Replace( "{J}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">J</font></color>" );
		item = item.Replace( "{K}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">K</font></color>" );
		item = item.Replace( "{B}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">B</font></color>" );

		//add bullets
		if ( item.Contains( "{-}" ) )
		{
			nt.color = new Color( 0, 0.6440244f, 1, 1 );
			nt.margin = new Vector4( 25, 0, 0, 0 );
			item = item.Replace( "{-}", " ■ " );
		}
		//handle rebels
		if ( item.Contains( "{R1}" ) )
		{
			item = item.Replace( "{R1}", "<color=#00A4FF>" + rebel1.name + "</color>" );
		}
		if ( item.Contains( "{R2}" ) )
		{
			item = item.Replace( "{R2}", "<color=#89A4FF>" + rebel2.name + "</color>" );
		}

		//special rules
		if ( cardEvent.eventRule == "R8" && item.Contains( "{V}" ) )
		{
			enemyToAdd = HandleR8();
			if ( enemyToAdd != null )
			{
				item = item.Replace( "{V}", enemyToAdd.name );
				DataStore.sessionData.ModifyThreat( -Math.Min( 7, enemyToAdd.cost ) );
			}
			else
				item = item.Replace( "{V}", $"<color=red>{DataStore.uiLanguage.uiMainApp.noneUC}</color>" );
		}
		else if ( cardEvent.eventRule == "R13" )
		{
			if ( !DataStore.deployedEnemies.Any( x => x.id == "DG072" ) )//vader
			{
				//add to deployment hand
				DataStore.deploymentHand.Add( DataStore.villainCards.cards.Where( x => x.id == "DG072" ).First() );
				//reduce its cost by 2
				DataStore.deploymentHand.Where( x => x.id == "DG072" ).First().cost -= 2;
			}
		}
		else if ( cardEvent.eventRule == "R18" && item.Contains( "{CR}" ) )
		{
			enemyToAdd = HandleR18();
			if ( enemyToAdd != null )
			{
				item = item.Replace( "{CR}", enemyToAdd.name );
				DataStore.sessionData.ModifyThreat( -Math.Min( 5, enemyToAdd.cost ) );
			}
			else
				item = item.Replace( "{CR}", $"<color=red>{DataStore.uiLanguage.uiMainApp.noneUC}</color>" );
		}
		else if ( cardEvent.eventRule == "R23" )
		{
			allyToAdd = HandleR23();
			if ( allyToAdd != null )
			{
				item = item.Replace( "{A}", "<color=#00A4FF>" + allyToAdd.name );
				DataStore.sessionData.ModifyThreat( allyToAdd.cost / 2 );
			}
			else
				item = item.Replace( "{A}", $"<color=red>{DataStore.uiLanguage.uiMainApp.noneUC}</color>" );
		}

		nt.text = item;
		var rt = go.GetComponent<RectTransform>();
		rt.sizeDelta = new Vector2( 900, 100 );
	}

	CardDescriptor HandleR23()
	{
		//filter ally list by owned expansions + Other, minus anything already deployed
		var alist =
			DataStore.allyCards.cards
			.OwnedPlusOther()
			.MinusDeployed();
		//sanity check for empty list
		if ( alist.Count() == 0 )
		{
			return null;
		}
		else
		{
			int[] rnd = GlowEngine.GenerateRandomNumbers( alist.Count() );
			return alist.ToList()[rnd[0]];
		}
	}

	CardDescriptor HandleR8()
	{
		/*•	If there is a villain in the deployment hand, choose that villain.
		•	If there are any earned villains, select one of those villains randomly.
		•	If there are no earned villains, select a villain randomly.
		•	Threat cost for the villain may not be higher than the current threat amount plus 7.  After deployment, decrease threat by the villain’s threat cost, to a maximum of 7. (If the villain is cheaper than 7 threat, decrease threat by that amount.)
		*/

		//try from deployment hand, minus deployed
		int[] rnd;
		var v = DataStore.deploymentHand
			.GetVillains()
			.MinusDeployed()//shouldn't be necessary
			.Where( x => x.cost <= DataStore.sessionData.threatLevel + 7 ).ToList();
		if ( v.Count > 0 )
		{
			rnd = GlowEngine.GenerateRandomNumbers( v.Count );
			return v[rnd[0]];
		}

		//try earned villains, minus deployed
		v = DataStore.sessionData
			.EarnedVillains
			.MinusDeployed()
			.Where( x => x.cost <= DataStore.sessionData.threatLevel + 7 ).ToList();
		if ( v.Count > 0 )
		{
			rnd = GlowEngine.GenerateRandomNumbers( v.Count );
			return v[rnd[0]];
		}

		//else random villain owned+other, minus deployed/ignored/faction
		v = DataStore.villainCards.cards
			.OwnedPlusOther()
			.FilterByFaction()
			.MinusIgnored()
			.MinusDeployed()
			.Where( x => x.cost <= DataStore.sessionData.threatLevel + 7 ).ToList();
		if ( v.Count > 0 )
		{
			rnd = GlowEngine.GenerateRandomNumbers( v.Count );
			//add it to earned list, per the rules for this event
			DataStore.sessionData.EarnedVillains.Add( v[rnd[0]] );
			return v[rnd[0]];
		}
		//bust
		return null;
	}

	CardDescriptor HandleR18()
	{
		//Randomly select a creature, based on the available groups
		var clist = DataStore.deploymentCards.cards.Where( x =>
		x.id == "DG017" ||
		x.id == "DG018" ||
		x.id == "DG060" ||
		x.id == "DG061" ||
		x.id == "DG028" ||
		x.id == "DG029"
		).ToList()
		.Owned()
		.FilterByFaction()
		.MinusDeployed()
		.MinusReserved()
		.MinusIgnored();
		if ( clist.Count > 0 )
		{
			int[] rnd = GlowEngine.GenerateRandomNumbers( clist.Count );
			//if it's in deployment hand, remove it
			if ( DataStore.deploymentHand.Contains( clist[rnd[0]] ) )
				DataStore.deploymentHand.Remove( clist[rnd[0]] );
			return clist[rnd[0]];
		}
		else
			return null;
	}

	private void Update()
	{
		if ( Input.GetKeyDown( KeyCode.Space ) )
			OnClose();
	}
}
