using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EnemyActivationPopup : MonoBehaviour
{
	public Image fader;
	public CanvasGroup cg;
	public TextMeshProUGUI bonusNameText, bonusText, ignoreText;
	public Text enemyName, continueText;
	public Image thumbnail, colorPip;
	public CardZoom cardZoom;
	public DynamicCardPrefab cardPrefab;
	public DiceRoller diceRoller;

	CardInstruction cardInstruction;
	CardDescriptor cardDescriptor;
	string rebel1;
	bool spaceListen;

	public void Show( CardDescriptor cd )
	{
		EventSystem.current.SetSelectedGameObject( null );
		//Debug.Log( "Showing: " + cd.name + " / " + cd.id );
		//clear values
		thumbnail.color = new Color( 1, 1, 1, 0 );
		bonusNameText.text = "";
		bonusText.text = "";
		enemyName.text = "";
		ignoreText.text = "";
		spaceListen = true;
		colorPip.color = DataStore.pipColors[cd.colorIndex].ToColor();
		continueText.text = DataStore.uiLanguage.uiSetup.continueBtn;

		cardDescriptor = cd;

		cardInstruction = DataStore.activationInstructions.Where( x => x.instID == cd.id ).FirstOr( null );
		if ( cardInstruction == null )
		{
			Debug.Log( "cardInstruction is NULL: " + cd.id );
			GlowEngine.FindObjectsOfTypeSingle<QuickMessage>().Show( "EnemyActivationPopup: cardInstruction is NULL: " + cd.id );
			return;
		}

		cardPrefab.InitCard( cd );

		//== no longer an issue
		//if ( cardInstruction == null )
		//{
		//	//not all elites have their own instruction, resulting in null found, so get its regular version instruction set by name instead
		//	int idx = cd.name.IndexOf( '(' );
		//	if ( idx > 0 )
		//	{
		//		string nonelite = cd.name.Substring( 0, idx ).Trim();
		//		cardInstruction = DataStore.activationInstructions.Where( x => x.instName == nonelite ).FirstOr( null );
		//		Debug.Log( "TRYING REGULAR INSTRUCTION" );
		//		if ( cardInstruction == null )
		//		{
		//			Debug.Log( "CAN'T FIND INSTRUCTION FOR: " + cd.id + "/" + nonelite );
		//			return;
		//		}
		//	}
		//}

		gameObject.SetActive( true );
		fader.color = new Color( 0, 0, 0, 0 );
		fader.DOFade( .95f, 1 );
		cg.DOFade( 1, .5f );
		transform.GetChild( 1 ).localScale = new Vector3( .85f, .85f, .85f );
		transform.GetChild( 1 ).DOScale( 1, .5f ).SetEase( Ease.OutExpo );

		SetThumbnail( cd );
		enemyName.text = cd.name.ToLower();
		if ( !string.IsNullOrEmpty( cd.ignored ) )
			ignoreText.text = $"<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">F</font></color>" + cd.ignored;
		else
			ignoreText.text = "";

		if ( !cardDescriptor.hasActivated )
		{
			//if multiple card instructions, pick 1
			int[] rnd = GlowEngine.GenerateRandomNumbers( cardInstruction.content.Count );
			InstructionOption io = cardInstruction.content[rnd[0]];

			CardDescriptor potentialRebel = FindRebel();
			if ( potentialRebel != null )
				rebel1 = potentialRebel.name;
			else
				rebel1 = DataStore.uiLanguage.uiMainApp.noneUC;

			ParseInstructions( io );
			ParseBonus( cd.id );

			//save this card's activation state
			cardDescriptor.hasActivated = true;
			cardDescriptor.rebelName = rebel1;
			cardDescriptor.instructionOption = io;
			cardDescriptor.bonusName = bonusNameText.text;
			cardDescriptor.bonusText = bonusText.text;
		}
		else
		{
			CardDescriptor potentialRebel = FindRebel();
			if ( cardDescriptor.rebelName != null )
				rebel1 = cardDescriptor.rebelName;
			else if ( potentialRebel != null )
				rebel1 = potentialRebel.name;
			else
				rebel1 = DataStore.uiLanguage.uiMainApp.noneUC;

			if ( cardDescriptor.instructionOption != null )
				ParseInstructions( cardDescriptor.instructionOption );
			else
			{
				InstructionOption io = cardInstruction.content[GlowEngine.GenerateRandomNumbers( cardInstruction.content.Count )[0]];
				ParseInstructions( io );
				cardDescriptor.instructionOption = io;
			}

			if ( cardDescriptor.bonusName != null && cardDescriptor.bonusText != null )
			{
				bonusNameText.text = cardDescriptor.bonusName;
				bonusText.text = cardDescriptor.bonusText;
			}
			else
			{
				ParseBonus( cd.id );
				cardDescriptor.bonusName = bonusNameText.text;
				cardDescriptor.bonusText = bonusText.text;
			}
		}
	}

	void SetThumbnail( CardDescriptor cd )
	{
		//set thumbnail for villain
		if ( DataStore.villainCards.cards.Any( x => x.id == cd.id ) )
			thumbnail.sprite = Resources.Load<Sprite>( $"Cards/Villains/{cd.id.Replace( "DG", "M" )}" );
		else//regular enemy
		{
			thumbnail.sprite = Resources.Load<Sprite>( $"Cards/Enemies/{cd.expansion}/{cd.id.Replace( "DG", "M" )}" );
			thumbnail.GetComponent<Outline>().effectColor = new Color( 0, 0.6440244f, 1, 1 );
		}
		thumbnail.DOFade( 1, .25f );
	}

	void ParseBonus( string id )
	{
		bonusNameText.text = "";
		bonusText.text = "";
		BonusEffect be = DataStore.bonusEffects.Where( x => x.bonusID == id ).FirstOr( null );
		if ( be == null || be.effects.Count == 0 )
			return;

		//first choose a random bonus
		int[] rnd = GlowEngine.GenerateRandomNumbers( be.effects.Count );
		string e = be.effects[rnd[0]];
		//get the bonus name
		int idx = e.IndexOf( ':' );
		bonusNameText.text = e.Substring( 0, idx );
		bonusText.text = ReplaceGlyphs( e.Substring( idx + 1 ) ).Trim();

		//At each activation, there’s a 25% chance that no bonus effect will be applied
		if ( DataStore.sessionData.difficulty == Difficulty.Easy )
		{
			if ( GlowEngine.RandomBool( 25 ) )
			{
				Debug.Log( "EASY MODE: applied 25% chance bonus skipped" );
				bonusNameText.text = "";
				bonusText.text = "";
			}
		}
	}

	void ParseInstructions( InstructionOption op )
	{
		Transform content = transform.Find( "Panel/content" );

		for ( int i = 0; i < op.instruction.Count; i++ )
		{
			string item = op.instruction[i];

			GameObject go = new GameObject( "content item" );
			go.layer = 5;
			go.transform.SetParent( content );
			go.transform.localScale = Vector3.one;
			go.transform.localEulerAngles = Vector3.zero;

			TextMeshProUGUI nt = go.AddComponent<TextMeshProUGUI>();
			nt.color = Color.white;
			nt.fontSize = 25;

			//replace glyphs
			item = ReplaceGlyphs( item );

			//add bullets
			if ( item.Contains( "{-}" ) )
			{
				nt.color = new Color( 0, 0.6440244f, 1, 1 );
				//nt.margin = new Vector4( 25, 0, 0, 0 );
				//item = item.Replace( "{-}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">U</font></color> " );
				item = item.Replace( "{-}", " ■ " );
			}
			//orange highlight
			if ( item.Contains( "{O}" ) )
			{
				item = item.Replace( "{O}", "" );
				nt.color = new Color( 1, 0.5586207f, 0, 1 );
			}

			nt.text = item;
			var rt = go.GetComponent<RectTransform>();
			rt.sizeDelta = new Vector2( 1100, 100 );
		}
	}

	string ReplaceGlyphs( string item )
	{
		item = item.Replace( "{H}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">H</font></color>" );
		item = item.Replace( "{C}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">C</font></color>" );
		item = item.Replace( "{J}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">J</font></color>" );
		item = item.Replace( "{K}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">K</font></color>" );
		item = item.Replace( "{A}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">A</font></color>" );
		item = item.Replace( "{Q}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">Q</font></color>" );
		item = item.Replace( "{g}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">g</font></color>" );
		item = item.Replace( "{h}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">h</font></color>" );
		item = item.Replace( "{E}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">E</font></color>" );
		item = item.Replace( "{G}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">G</font></color>" );
		item = item.Replace( "{f}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">f</font></color>" );
		item = item.Replace( "{b}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">b</font></color>" );
		item = item.Replace( "{B}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">B</font></color>" );
		item = item.Replace( "{I}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">I</font></color>" );
		item = item.Replace( "{P}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">P</font></color>" );
		item = item.Replace( "{F}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">F</font></color>" );

		if ( item.Contains( "{R1}" ) )
		{
			item = item.Replace( "{R1}", "<color=#00A4FF>" + rebel1 + "</color>" );
		}

		return item;
	}

	CardDescriptor FindRebel()
	{
		var hlist = DataStore.deployedHeroes.GetHealthy();
		var ulist = DataStore.deployedHeroes.GetUnhealthy();
		CardDescriptor r = null;

		if ( hlist != null )
		{
			//Debug.Log( "healthy HEROES: " + hlist.Count );
			int[] rnd = GlowEngine.GenerateRandomNumbers( hlist.Count() );
			r = hlist[rnd[0]];
		}
		else if ( ulist != null )
		{
			//Debug.Log( "UNhealthy HEROES: " + ulist.Count );
			int[] rnd = GlowEngine.GenerateRandomNumbers( ulist.Count() );
			r = ulist[rnd[0]];
		}

		return r;
	}

	public void OnViewCard()
	{
		spaceListen = false;
		EventSystem.current.SetSelectedGameObject( null );

		CardViewPopup cardViewPopup = GlowEngine.FindObjectsOfTypeSingle<CardViewPopup>();
		cardViewPopup.Show( cardDescriptor, OnReturn );
	}

	public void OnReturn( bool value = true )
	{
		spaceListen = value;
	}

	public void OnClose()
	{
		FindObjectOfType<Sound>().PlaySound( FX.Click );
		fader.DOFade( 0, .5f ).OnComplete( () =>
		{
			Transform content = transform.Find( "Panel/content" );
			foreach ( Transform tf in content )
				Destroy( tf.gameObject );
			gameObject.SetActive( false );
		} );
		cg.DOFade( 0, .2f );
		transform.GetChild( 1 ).DOScale( .85f, .5f ).SetEase( Ease.OutExpo );
	}

	private void Update()
	{
		if ( spaceListen && Input.GetKeyDown( KeyCode.Space ) )
			OnClose();
	}

	public void OnRollAttackDice()
	{
		if ( cardDescriptor.attacks == null )
			return;

		FindObjectOfType<Sound>().PlaySound( FX.Click );
		spaceListen = false;
		EventSystem.current.SetSelectedGameObject( null );
		diceRoller.Show( cardDescriptor, true, OnReturn );
	}

	public void OnRollDefenseDice()
	{
		if ( cardDescriptor.defense == null )
			return;

		FindObjectOfType<Sound>().PlaySound( FX.Click );
		spaceListen = false;
		EventSystem.current.SetSelectedGameObject( null );
		diceRoller.Show( cardDescriptor, false, OnReturn );
	}
}
