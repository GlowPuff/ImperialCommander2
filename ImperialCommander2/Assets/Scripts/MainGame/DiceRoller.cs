using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiceRoller : MonoBehaviour
{
	public CanvasGroup cg, cg2, cardcg, mainContainerCG;
	public Image fader, attackRanged, attackMelee, defenseIcon, bgIconColor, outlineColor;
	public GameObject container, diceObject, modifierBox, visToggleBtn;
	public Text okBtn;
	public GameObject addAttackContainer, addDefenseContainer;
	public DynamicCardPrefab dynamicCard;
	public TextMeshProUGUI modText;
	public HelpPanel helpPanel;

	DeploymentCard card;
	Action<bool> callback;
	GridLayoutGroup gridLayout;
	bool visible, spaceListen;

	public void Show( DeploymentCard cd, bool isAttack, Action<bool> ac = null )
	{
		spaceListen = true;
		visible = true;
		callback = ac;
		okBtn.text = DataStore.uiLanguage.uiSettings.ok.ToUpper();
		gridLayout = container.GetComponent<GridLayoutGroup>();
		dynamicCard.InitCard( cd, true );

		visToggleBtn.SetActive( true );
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

		if ( DataStore.gameType == GameType.Saga )
		{
			//check for modififier override
			var ovrd = DataStore.sagaSessionData.gameVars.GetDeploymentOverride( cd.id );
			if ( ovrd != null && ovrd.showMod && !string.IsNullOrEmpty( ovrd.modification.Trim() ) )
			{
				modifierBox.SetActive( true );
				modText.text = Saga.Utils.ReplaceGlyphs( ovrd.modification );
			}
			else
				modifierBox.SetActive( false );
		}

		gameObject.SetActive( true );
		fader.color = new Color( 0, 0, 0, 0 );
		fader.DOFade( .95f, .5f );
		cg.DOFade( 1, .5f );
		cg2.DOFade( 1, .5f );
		cardcg.DOFade( 1, .5f );
		mainContainerCG.DOFade( 1, .5f );
		transform.GetChild( 1 ).localScale = new Vector3( .85f, .85f, .85f );
		transform.GetChild( 1 ).DOScale( 1, .5f ).SetEase( Ease.OutExpo );
	}

	public void OnOK()
	{
		if ( !visible )
			return;

		FindObjectOfType<Sound>().PlaySound( FX.Click );
		visToggleBtn.SetActive( false );
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
		if ( spaceListen && Input.GetKeyDown( KeyCode.Space ) )
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

	public void OnHideToggle()
	{
		visible = !visible;
		if ( !visible )
		{
			mainContainerCG.DOFade( 0, .5f );
			fader.DOFade( 0, .5f );
		}
		else
		{
			mainContainerCG.DOFade( 1, .5f );
			fader.DOFade( .95f, .5f );
		}
	}

	public void OnHelpClick()
	{
		spaceListen = false;
		helpPanel.Show( () => spaceListen = true );
	}
}
