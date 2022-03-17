using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CardViewPopup : MonoBehaviour
{
	public CanvasGroup cg, cg2;
	public Image fader;
	public DynamicCardPrefab dynamicCard;
	public DynamicMissionCardPrefab dynamicMissionCard;

	Action<bool> callback;
	CardDescriptor card;

	public void Show( CardDescriptor cd, Action<bool> action = null )
	{
		card = cd;
		dynamicCard.gameObject.SetActive( true );
		dynamicCard.InitCard( cd );
		callback = action;

		gameObject.SetActive( true );
		fader.color = new Color( 0, 0, 0, 0 );
		fader.DOFade( .95f, .5f );
		cg.DOFade( 1, .5f );
		transform.GetChild( 1 ).localScale = new Vector3( .85f, .85f, .85f );
		transform.GetChild( 1 ).DOScale( 1, .5f ).SetEase( Ease.OutExpo );
	}

	public void ShowMissionCard( MissionCard cd, Action<bool> action = null )
	{
		dynamicMissionCard.gameObject.SetActive( true );
		dynamicMissionCard.InitCard( cd );
		callback = action;

		gameObject.SetActive( true );
		fader.color = new Color( 0, 0, 0, 0 );
		fader.DOFade( .95f, .5f );
		cg2.DOFade( 1, .5f );
		transform.GetChild( 2 ).localScale = new Vector3( .85f, .85f, .85f );
		transform.GetChild( 2 ).DOScale( 1, .5f ).SetEase( Ease.OutExpo );
	}

	public void OnOK()
	{
		FindObjectOfType<Sound>().PlaySound( FX.Click );
		fader.DOFade( 0, .5f ).OnComplete( () =>
		{
			callback?.Invoke( true );
			gameObject.SetActive( false );
			dynamicCard?.gameObject.SetActive( false );
			dynamicMissionCard?.gameObject.SetActive( false );
		} );
		cg?.DOFade( 0, .2f );
		cg2?.DOFade( 0, .2f );
		transform.GetChild( 1 ).DOScale( .85f, .5f ).SetEase( Ease.OutExpo );
		transform.GetChild( 2 )?.DOScale( .85f, .5f ).SetEase( Ease.OutExpo );
	}

	public void OnRollAttack()
	{
		OnOK();
		DiceRoller diceRoller = GlowEngine.FindObjectsOfTypeSingle<DiceRoller>();
		diceRoller.Show( card, true );
	}

	public void OnRollDefense()
	{
		OnOK();
		DiceRoller diceRoller = GlowEngine.FindObjectsOfTypeSingle<DiceRoller>();
		diceRoller.Show( card, false );
	}

	private void Update()
	{
		if ( Input.GetKeyDown( KeyCode.Space ) )
			OnOK();
	}
}
