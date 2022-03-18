using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DiceRoller : MonoBehaviour
{
	public CanvasGroup cg, cg2, cardcg;
	public Image fader, attackRanged, attackMelee, defenseIcon, bgIconColor, outlineColor;
	public GameObject container, diceObject;
	public Text okBtn;
	public GameObject addAttackContainer, addDefenseContainer;
	public DynamicCardPrefab dynamicCard;
	[HideInInspector]

	CardDescriptor card;
	Action<bool> callback;
	GridLayoutGroup gridLayout;

	public void Show( CardDescriptor cd, bool isAttack, Action<bool> ac = null )
	{
		callback = ac;
		okBtn.text = DataStore.uiLanguage.uiSettings.ok;
		gridLayout = container.GetComponent<GridLayoutGroup>();
		dynamicCard.InitCard( cd );

		addAttackContainer.SetActive( false );
		addDefenseContainer.SetActive( false );
		if ( isAttack )
			addAttackContainer.SetActive( true );
		else
			addDefenseContainer.SetActive( true );

		foreach ( Transform item in container.transform )
		{
			Destroy( item.gameObject );
		}

		card = cd;
		attackMelee.gameObject.SetActive( false );
		attackRanged.gameObject.SetActive( false );
		defenseIcon.gameObject.SetActive( false );

		if ( isAttack )
		{
			//set the attack type icon
			if ( cd.attackType == AttackType.Melee )
				attackMelee.gameObject.SetActive( true );
			else
				attackRanged.gameObject.SetActive( true );
			bgIconColor.color = new Color( 135f / 255f, 21f / 255f, 0 );
			outlineColor.color = new Color( 1, 40f / 255f, 0 );

			if ( cd.attacks != null )
			{
				for ( int i = 0; i < cd.attacks.Length; i++ )
				{
					CreateDice( card.attacks[i] );
				}
			}
		}
		else
		{
			defenseIcon.gameObject.SetActive( true );
			bgIconColor.color = new Color( 0, 21f / 135f / 255f, 85f / 255f );
			outlineColor.color = new Color( 0, 1, 0 );

			if ( cd.defense != null )
			{
				for ( int i = 0; i < cd.defense.Length; i++ )
				{
					CreateDice( card.defense[i] );
				}
			}
		}

		gameObject.SetActive( true );
		fader.color = new Color( 0, 0, 0, 0 );
		fader.DOFade( .95f, .5f );
		cg.DOFade( 1, .5f );
		cg2.DOFade( 1, .5f );
		cardcg.DOFade( 1, .5f );
		transform.GetChild( 1 ).localScale = new Vector3( .85f, .85f, .85f );
		transform.GetChild( 1 ).DOScale( 1, .5f ).SetEase( Ease.OutExpo );
	}

	public void OnOK()
	{
		FindObjectOfType<Sound>().PlaySound( FX.Click );
		fader.DOFade( 0, .5f ).OnComplete( () =>
		{
			callback?.Invoke( true );
			gameObject.SetActive( false );
		} );
		cg.DOFade( 0, .2f );
		cg2.DOFade( 0, .2f );
		cardcg.DOFade( 0, .2f );
		transform.GetChild( 1 ).DOScale( .85f, .5f ).SetEase( Ease.OutExpo );
	}

	private void Update()
	{
		if ( Input.GetKeyDown( KeyCode.Space ) )
			OnOK();

		if ( container.transform.childCount > 4 )
			gridLayout.childAlignment = TextAnchor.UpperCenter;
		else
			gridLayout.childAlignment = TextAnchor.MiddleCenter;
	}

	private void CreateDice( DiceColor dc )
	{
		var go = Instantiate( diceObject );
		go.layer = 5;
		go.transform.SetParent( container.transform );
		go.transform.localScale = Vector3.one;
		go.transform.localEulerAngles = Vector3.zero;
		go.transform.localPosition = Vector3.zero;

		Dice dice = go.GetComponent<Dice>();
		dice.diceColor = dc;
	}

	public void AddDice( int c )
	{
		if ( container.transform.childCount < 8 )
			CreateDice( (DiceColor)c );
	}
}
