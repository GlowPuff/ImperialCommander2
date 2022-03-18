using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RandomDeployPopup : MonoBehaviour
{
	public Image fader;
	public CanvasGroup cg;
	public MWheelHandler mWheelHandler;

	public void Show()
	{
		gameObject.SetActive( true );
		fader.color = new Color( 0, 0, 0, 0 );
		fader.DOFade( .95f, 1 );
		cg.DOFade( 1, .5f );
		transform.GetChild( 1 ).localScale = new Vector3( .85f, .85f, .85f );
		transform.GetChild( 1 ).DOScale( 1, .5f ).SetEase( Ease.OutExpo );

		mWheelHandler.ResetWheeler();
	}

	public void OnCancel()
	{
		FindObjectOfType<Sound>().PlaySound( FX.Click );
		fader.DOFade( 0, .5f ).OnComplete( () =>
		{
			gameObject.SetActive( false );
		} );
		cg.DOFade( 0, .2f );
		transform.GetChild( 1 ).DOScale( .85f, .5f ).SetEase( Ease.OutExpo );
	}

	public void OnConfirm()
	{
		int c = mWheelHandler.wheelValue;
		CardDescriptor cd = null;
		List<CardDescriptor> list = new List<CardDescriptor>();
		do
		{
			var p = DataStore.deploymentCards.cards
				.OwnedPlusOther()
				.MinusDeployed()
				.MinusInDeploymentHand()
				.MinusStarting()
				.MinusReserved()
				.MinusIgnored()
				.FilterByFaction()
				.Concat( DataStore.sessionData.EarnedVillains )
				.Where( x => x.cost <= c && !list.Contains( x ) )
				.ToList();
			if ( p.Count > 0 )
			{
				int[] rnd = GlowEngine.GenerateRandomNumbers( p.Count );
				cd = p[rnd[0]];
				list.Add( cd );
				c -= cd.cost;
			}
			else
				cd = null;
		} while ( cd != null );

		//deploy any groups picked
		foreach ( var card in list )
			FindObjectOfType<DeploymentGroupManager>().DeployGroup( card, true );

		string s = DataStore.uiLanguage.uiMainApp.noRandomMatchesUC.Replace( "{d}", mWheelHandler.wheelValue.ToString() );
		if ( list.Count == 0 )
			GlowEngine.FindObjectsOfTypeSingle<QuickMessage>().Show( $"<color=\"orange\">{s}</color>" );

		OnCancel();
	}
}
