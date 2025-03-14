﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DG.Tweening;
using Saga;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EnemyActivationPopup : MonoBehaviour
{
	public Image fader;
	public CanvasGroup cg;
	public TextMeshProUGUI bonusNameText, bonusText, ignoreText, modText;
	public Text enemyName, continueText;
	public Image thumbnail, colorPip;
	public DynamicCardPrefab cardPrefab;
	public DiceRoller diceRoller;
	public GameObject modifierBox;
	public Transform instructionContentContainer;
	public HPTrackerContainer hpTrackerContainer;
	[HideInInspector]
	public bool isActive = false;
	public Image outlineColor;
	public HelpPanel helpPanel;

	CardInstruction cardInstruction;
	DeploymentCard cardDescriptor;
	string rebel1;
	bool spaceListen;
	Action callback;

	public void Show( DeploymentCard cd, Difficulty difficulty, Action cb = null )
	{
		EventSystem.current.SetSelectedGameObject( null );
		//Debug.Log( "Showing: " + cd.name + " / " + cd.id );
		isActive = true;
		//clear values
		callback = cb;
		thumbnail.color = new Color( 1, 1, 1, 0 );
		bonusNameText.text = "";
		bonusText.text = "";
		enemyName.text = "";
		ignoreText.text = "";
		spaceListen = true;
		colorPip.color = DataStore.pipColors[cd.GetColorIndex()].ToColor();
		continueText.text = DataStore.uiLanguage.uiMainApp.continueBtn;

		cardDescriptor = cd;

		cardPrefab.InitCard( cd, true );

		gameObject.SetActive( true );
		fader.color = new Color( 0, 0, 0, 0 );
		fader.DOFade( .95f, 1 );
		cg.DOFade( 1, .5f );
		transform.GetChild( 1 ).localScale = new Vector3( .85f, .85f, .85f );
		transform.GetChild( 1 ).DOScale( 1, .5f ).SetEase( Ease.OutExpo );

		//mugshot
		thumbnail.sprite = Resources.Load<Sprite>( cardDescriptor.mugShotPath );
		thumbnail.DOFade( 1, .25f );
		//outline
		outlineColor.color = Utils.String2UnityColor( cardDescriptor.deploymentOutlineColor );
		//name
		enemyName.text = cd.name.ToUpper();

		if ( !string.IsNullOrEmpty( cd.ignored ) )
			ignoreText.text = $"<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">F</font></color>" + cd.ignored;
		else
			ignoreText.text = "";

		ShowSaga( cd, difficulty );
	}

	void ShowSaga( DeploymentCard cd, Difficulty difficulty )
	{
		DataStore.sagaSessionData.missionLogger.LogEvent( MissionLogType.GroupActivation, $"{cd.name}" );
		//handle wound tracker
		//reset wound tracker
		if ( hpTrackerContainer != null )
			hpTrackerContainer.ResetTracker( cd );

		//check for name override
		modifierBox.SetActive( false );
		var ovrd = DataStore.sagaSessionData.gameVars.GetDeploymentOverride( cd.id );
		if ( ovrd != null )
		{
			//name override
			enemyName.text = ovrd.nameOverride.ToUpper();
			//check for modififier override
			if ( ovrd.showMod && !string.IsNullOrEmpty( ovrd.modification.Trim() ) )
			{
				modifierBox.SetActive( true );
				modText.text = ReplaceGlyphs( ovrd.modification );
			}
			else
				modifierBox.SetActive( false );
		}

		//rebel target
		if ( cardDescriptor.hasActivated && cardDescriptor.rebelName != null )
		{
			Debug.Log( "***RE-USING PREVIOUS ACTIVATION DATA::rebelName***" );
			rebel1 = cardDescriptor.rebelName;
		}
		else
		{
			DeploymentCard potentialRebel = FindRebelSaga();
			if ( potentialRebel != null )
			{
				rebel1 = potentialRebel.name;
				//check for a target name override, id will be null if this is using "Other" as a target
				//sending null as the id will return a null instead of the All override
				var povrd = DataStore.sagaSessionData.gameVars.GetDeploymentOverride( potentialRebel.id );
				if ( povrd != null )
					rebel1 = povrd.nameOverride;
			}
			else
				rebel1 = DataStore.uiLanguage.uiMainApp.noneUC;

			//store activation data
			cardDescriptor.rebelName = rebel1;
		}

		//instruction
		if ( cardDescriptor.hasActivated && cardDescriptor.instructionOption != null )
		{
			Debug.Log( "***RE-USING PREVIOUS ACTIVATION DATA::instructionOption***" );
			ParseInstructions( cardDescriptor.instructionOption.instruction );
		}
		else
		{
			cardInstruction = DataStore.GetActivationInstruction( cd.id );
			InstructionOption savedInstruction = null;
			if ( cardInstruction != null )
			{
				//if multiple card instructions, pick 1
				int[] rnd = GlowEngine.GenerateRandomNumbers( cardInstruction.content.Count );
				InstructionOption io = cardInstruction.content[rnd[0]];
				List<string> instructions = io.instruction;
				//check for instruction/repositioning override, which are appended to 'instructions' list
				instructions = GetModifiedInstructions( cd.id, instructions );
				instructions = GetModifiedRepositioning( cd.id, instructions );
				savedInstruction = new InstructionOption() { instruction = instructions };
				//rebel1 has been set, now it's safe to parse instructions that use it for targeting
				ParseInstructions( instructions );
				//store activation data
				cardDescriptor.instructionOption = savedInstruction;
			}
			else if ( ovrd != null && ovrd.isCustomDeployment )
			{
				if ( ovrd.changeInstructions != null )
					ParseInstructions( ovrd.changeInstructions.theText.Split( '\n' ).ToList() );
			}
		}

		//bonus
		if ( cardDescriptor.hasActivated
			&& cardDescriptor.bonusName != null
			&& cardDescriptor.bonusText != null )
		{
			Debug.Log( "***RE-USING PREVIOUS ACTIVATION DATA::bonus***" );
			bonusNameText.text = cardDescriptor.bonusName;
			bonusText.text = cardDescriptor.bonusText;
		}
		else
		{
			ParseBonusSaga( cd.id, difficulty );
			cardDescriptor.bonusName = bonusNameText.text;
			cardDescriptor.bonusText = bonusText.text;
		}

		cardDescriptor.hasActivated = true;
	}

	void ParseBonusSaga( string id, Difficulty difficulty )
	{
		bonusNameText.text = "";
		bonusText.text = "";

		var ovrd = DataStore.sagaSessionData.gameVars.GetDeploymentOverride( cardDescriptor.id );
		if ( ovrd != null && !ovrd.isCustomDeployment && ovrd.useGenericMugshot )
			return;

		if ( ovrd != null && ovrd.isCustomDeployment )
		{
			string e = ovrd.customBonuses[GlowEngine.GenerateRandomNumbers( ovrd.customBonuses.Length )[0]];
			if ( !string.IsNullOrEmpty( e ) )
			{
				try
				{
					//get the bonus name
					string[] b = e.Split( ':' );
					if ( b.Length == 2 )
					{
						bonusNameText.text = b[0];//e.Substring( 0, idx );
						bonusText.text = ReplaceGlyphs( b[1] ).Trim();//e.Substring( idx + 1 ) ).Trim();
					}
					else if ( b.Length == 1 )
					{
						bonusNameText.text = "";
						bonusText.text = ReplaceGlyphs( b[0] ).Trim();
					}
					//int idx = e.IndexOf( ':' );
				}
				catch ( Exception ex )
				{
					bonusNameText.text = "Error";
					bonusText.text = "Error - see log";
					Utils.LogWarning( $"ParseBonusSaga()::Error parsing ID={id}\n{ex.Message}" );
				}
			}
		}
		else
			ParseBonus( id, difficulty );
	}

	void ParseBonus( string id, Difficulty difficulty )
	{
		bonusNameText.text = "";
		bonusText.text = "";
		BonusEffect be = DataStore.GetBonusEffect( id );
		if ( be == null || be.effects.Count == 0 )
			return;

		try
		{
			//first choose a random bonus
			int[] rnd = GlowEngine.GenerateRandomNumbers( be.effects.Count );
			string e = be.effects[rnd[0]];
			//get the bonus name
			int idx = e.IndexOf( ':' );
			bonusNameText.text = e.Substring( 0, idx );
			bonusText.text = ReplaceGlyphs( e.Substring( idx + 1 ) ).Trim();
		}
		catch ( Exception ex )
		{
			bonusNameText.text = "Error";
			bonusText.text = "Error - see log";
			Utils.LogWarning( $"ParseBonus()::Error parsing ID={id}\n{ex.Message}" );
		}

		//At each activation, there’s a 25% chance that no bonus effect will be applied
		if ( difficulty == Difficulty.Easy )
		{
			if ( GlowEngine.RandomBool( 25 ) )
			{
				Debug.Log( "EASY MODE: applied 25% chance bonus skipped" );
				bonusNameText.text = "";
				bonusText.text = "";
			}
		}
	}

	void ParseInstructions( List<string> instruction )
	{
		foreach ( Transform tf in instructionContentContainer )
			Destroy( tf.gameObject );

		for ( int i = 0; i < instruction.Count; i++ )
		{
			string item = instruction[i];

			GameObject go = new GameObject( "instruction item" );
			go.layer = 5;
			go.transform.SetParent( instructionContentContainer );
			go.transform.localScale = Vector3.one;
			go.transform.localEulerAngles = Vector3.zero;

			TextMeshProUGUI nt = go.AddComponent<TextMeshProUGUI>();
			nt.color = Color.white;
			nt.fontSize = 25;

			//replace glyphs
			item = ReplaceGlyphs( item );

			//add bullets (light blue text)
			if ( item.Contains( "{-}" ) )
			{
				//nt.color = new Color( 0, 0.6440244f, 1, 1 );
				nt.color = new Color( 0.4980392f, 0.8266184f, 1, 1 );
				//nt.margin = new Vector4( 25, 0, 0, 0 );
				//item = item.Replace( "{-}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">U</font></color> " );
				item = item.Replace( "{-}", " \u25A0 " );
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
		item = item.Replace( "{V}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">V</font></color>" );
		item = item.Replace( "{D}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">D</font></color>" );
		//interferes with {O}range code, not used for activations anyways
		//item = item.Replace( "{O}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">O</font></color>" );
		item = item.Replace( "{R}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">R</font></color>" );
		item = item.Replace( "{S}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">S</font></color>" );
		item = item.Replace( "{U}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">U</font></color>" );
		item = item.Replace( "{W}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">W</font></color>" );
		item = item.Replace( "{X}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">X</font></color>" );
		item = item.Replace( "{c}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">c</font></color>" );
		item = item.Replace( "{e}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">e</font></color>" );
		item = item.Replace( "{s}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">s</font></color>" );
		item = item.Replace( "{0}", "<font=\"ImperialAssaultSymbols SDF\">0</font>" );
		item = item.Replace( "{1}", "<font=\"ImperialAssaultSymbols SDF\">1</font>" );
		item = item.Replace( "{2}", "<font=\"ImperialAssaultSymbols SDF\">2</font>" );
		item = item.Replace( "{3}", "<font=\"ImperialAssaultSymbols SDF\">3</font>" );
		item = item.Replace( "{4}", "<font=\"ImperialAssaultSymbols SDF\">4</font>" );
		item = item.Replace( "{5}", "<font=\"ImperialAssaultSymbols SDF\">5</font>" );
		item = item.Replace( "{6}", "<font=\"ImperialAssaultSymbols SDF\">6</font>" );

		if ( item.Contains( "{R1}" ) )
		{
			//lighter blue color
			item = item.Replace( "{R1}", "<color=#7FD3FF>" + rebel1 + "</color>" );
		}

		//Saga formatting
		if ( DataStore.gameType == GameType.Saga )
		{
			//trigger value
			Regex regex = new Regex( @"&[\w\s]*&" );
			foreach ( var match in regex.Matches( item ) )
			{
				var t = DataStore.mission.GetTriggerFromName( match.ToString().Replace( "&", "" ) );
				if ( t != null )
				{
					int curValue = GlowEngine.FindUnityObject<TriggerManager>().CurrentTriggerValue( t.GUID );
					item = item.Replace( match.ToString(), curValue.ToString() );
				}
			}

			//threat multiplier
			regex = new Regex( @"\*[\w\s]*\*" );
			foreach ( var match in regex.Matches( item ) )
			{
				int mul = int.Parse( match.ToString().Replace( "*", "" ) );
				item = item.Replace( match.ToString(), (DataStore.sagaSessionData.setupOptions.threatLevel * mul).ToString() );
			}
		}

		//random rebel
		Regex rebelregex = new Regex( @"\{rebel\}", RegexOptions.IgnoreCase );
		foreach ( var match in rebelregex.Matches( item ) )
		{
			//lighter blue color
			item = item.Replace( match.ToString(), "<color=#7FD3FF>" + rebel1 + "</color>" );
		}

		return item;
	}

	DeploymentCard FindRebelSaga()
	{
		//try override to ALL
		var ovrd = DataStore.sagaSessionData.gameVars.GetDeploymentOverride()?.changeTarget;
		//try specific override, which supercedes targeting ALL
		if ( ovrd == null )
			ovrd = DataStore.sagaSessionData.gameVars.GetDeploymentOverride( cardDescriptor.id )?.changeTarget;

		//if there is a targeting override, only use it X% of the time
		if ( ovrd != null && GlowEngine.RandomBool( ovrd.percentChance ) )
		{
			Debug.Log( $"FindRebel()::MODIFYING TARGET" );
			if ( ovrd.targetType == PriorityTargetType.Trait
				|| ovrd.targetType == PriorityTargetType.Rebel )
				return HandleTargetTraits( ovrd );
			else if ( ovrd.targetType == PriorityTargetType.Ally )
				return HandleTargetAlly( ovrd );
			else if ( ovrd.targetType == PriorityTargetType.Hero )
				return HandleTargetHero( ovrd );
			else if ( ovrd.targetType == PriorityTargetType.Other )
				return new DeploymentCard() { name = ovrd.otherTarget };
		}
		//otherwise no targeting override, or RNG failed, try using groups DEFAULT preferred target
		else if ( GlowEngine.RandomBool( 65 ) )
		{
			Debug.Log( "FindRebelSaga()::FINDING REBEL USING DEFAULT PREFERRED TRAITS (65%)" );
			return HandleTargetTraits();
		}

		Debug.Log( "FindRebelSaga()::FINDING FALLBACK REBEL" );
		return FindRebel();
	}

	DeploymentCard HandleTargetHero( ChangeTarget ovrd )
	{
		Debug.Log( "HandleTargetHero()" );
		//find a rebel with no targeting
		DeploymentCard defaultRebel = FindRebel();
		//if finding a specific hero bombs, just find ANY rebel
		defaultRebel = DataStore.deployedHeroes.Where( x => x.id == ovrd.specificHero && !x.heroState.isDefeated ).FirstOr( null ) ?? defaultRebel;

		return defaultRebel;
	}

	DeploymentCard HandleTargetAlly( ChangeTarget ovrd )
	{
		Debug.Log( "HandleTargetAlly()" );
		//find a rebel with no targeting
		DeploymentCard defaultRebel = FindRebel();
		//if finding a specific ally bombs, just find ANY rebel
		defaultRebel = DataStore.deployedHeroes.Where( x => x.id == ovrd.specificAlly && !x.heroState.isDefeated ).FirstOr( null ) ?? defaultRebel;

		return defaultRebel;
	}

	DeploymentCard HandleTargetTraits( ChangeTarget ovrd = null )
	{
		Debug.Log( "HandleTargetTraits()" );
		//find a rebel with no targeting
		DeploymentCard defaultRebel = FindRebel();
		//get the DEFAULT preferred target traits first
		GroupTraits[] preferredTargets = cardDescriptor.preferredTargets;

		//if an override was sent, use it
		if ( ovrd != null && !ovrd.groupPriorityTraits.useDefaultPriority )
			preferredTargets = ovrd.groupPriorityTraits.GetTraitArray();

		var hlist = DataStore.deployedHeroes.GetHealthy().WithTraits( preferredTargets );
		var ulist = DataStore.deployedHeroes.GetUnhealthy().WithTraits( preferredTargets );

		if ( hlist is null )//if no rebels with preferred traits, try ANY healthy rebel
			hlist = DataStore.deployedHeroes.GetHealthy();
		else
			Debug.Log( "PREFFERED::" + string.Join( ", ", preferredTargets ) );
		if ( ulist is null )
			ulist = DataStore.deployedHeroes.GetUnhealthy();

		if ( hlist != null )
		{
			//Debug.Log( "healthy HEROES: " + hlist.Count );
			int[] rnd = GlowEngine.GenerateRandomNumbers( hlist.Count() );
			defaultRebel = hlist[rnd[0]];
		}
		else if ( ulist != null )
		{
			//Debug.Log( "UNhealthy HEROES: " + ulist.Count );
			int[] rnd = GlowEngine.GenerateRandomNumbers( ulist.Count() );
			defaultRebel = ulist[rnd[0]];
		}

		return defaultRebel;
	}

	DeploymentCard FindRebel()
	{
		var hlist = DataStore.deployedHeroes.GetHealthy();
		var ulist = DataStore.deployedHeroes.GetUnhealthy();
		DeploymentCard defaultRebel = null;

		if ( hlist != null )
		{
			//Debug.Log( "healthy HEROES: " + hlist.Count );
			int[] rnd = GlowEngine.GenerateRandomNumbers( hlist.Count() );
			defaultRebel = hlist[rnd[0]];
		}
		else if ( ulist != null )
		{
			//Debug.Log( "UNhealthy HEROES: " + ulist.Count );
			int[] rnd = GlowEngine.GenerateRandomNumbers( ulist.Count() );
			defaultRebel = ulist[rnd[0]];
		}

		return defaultRebel;
	}

	public void OnViewCard()
	{
		spaceListen = false;
		EventSystem.current.SetSelectedGameObject( null );

		CardViewPopup cardViewPopup = GlowEngine.FindUnityObject<CardViewPopup>();
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
			foreach ( Transform tf in instructionContentContainer )
				Destroy( tf.gameObject );
			gameObject.SetActive( false );
		} );
		isActive = false;
		cg.DOFade( 0, .2f );
		transform.GetChild( 1 ).DOScale( .85f, .5f ).SetEase( Ease.OutExpo );
		callback?.Invoke();
		hpTrackerContainer?.UpdateAndHide();
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

	public List<string> GetModifiedInstructions( string ID, List<string> linesOut )
	{
		//all
		var ci = DataStore.sagaSessionData.gameVars.GetDeploymentOverride()?.changeInstructions;
		if ( ci != null )
		{
			List<string> lines = ci.theText.Split( new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
			if ( ci.instructionType == CustomInstructionType.Replace )
				linesOut = lines;
			else if ( ci.instructionType == CustomInstructionType.Top )
				linesOut = lines.Concat( linesOut ).ToList();
			else if ( ci.instructionType == CustomInstructionType.Bottom )
				linesOut = linesOut.Concat( lines ).ToList();
			Debug.Log( $"GetModifiedInstructions()::ALL::MODIFYING WITH {lines.Count} LINES::{ci.instructionType}" );
		}

		//specific
		var dgOvrd = DataStore.sagaSessionData.gameVars.GetDeploymentOverride( ID )?.changeInstructions;
		if ( dgOvrd != null )
		{
			List<string> lines = dgOvrd.theText.Split( new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
			if ( dgOvrd.instructionType == CustomInstructionType.Replace )
				linesOut = lines;
			else if ( dgOvrd.instructionType == CustomInstructionType.Top )
				linesOut = lines.Concat( linesOut ).ToList();
			else if ( dgOvrd.instructionType == CustomInstructionType.Bottom )
				linesOut = linesOut.Concat( lines ).ToList();
			Debug.Log( $"GetModifiedInstructions()::MODIFYING WITH {lines.Count} LINES::{dgOvrd.instructionType}" );
		}

		return linesOut;
	}

	public List<string> GetModifiedRepositioning( string ID, List<string> linesOut )
	{
		//all
		string repo = DataStore.sagaSessionData.gameVars.GetDeploymentOverride()?.repositionInstructions;
		if ( !string.IsNullOrEmpty( repo ) )
		{
			repo = "<color=orange>" + DataStore.uiLanguage.sagaMainApp.repositionTargetUC + ":\n{-} " + repo + "</color>";
			List<string> lines = repo.Split( new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
			lines.Insert( 0, "\n" );
			//place at bottom of instructions
			linesOut = linesOut.Concat( lines ).ToList();
			Debug.Log( $"GetModifiedRepositioning()::ALL::MODIFYING WITH {lines.Count} LINES::{repo}" );
		}

		//specific
		repo = DataStore.sagaSessionData.gameVars.GetDeploymentOverride( ID )?.repositionInstructions;
		if ( !string.IsNullOrEmpty( repo ) )
		{
			repo = "<color=orange>" + DataStore.uiLanguage.sagaMainApp.repositionTargetUC + ":\n{-} " + repo + "</color>";
			List<string> lines = repo.Split( new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
			lines.Insert( 0, "\n" );
			//place at bottom of instructions
			linesOut = linesOut.Concat( lines ).ToList();
			Debug.Log( $"GetModifiedRepositioning()::ALL::MODIFYING WITH {lines.Count} LINES::{repo}" );
		}

		return linesOut;
	}

	public void OnHelpClick()
	{
		spaceListen = false;
		helpPanel.Show( () => spaceListen = true );
	}
}
