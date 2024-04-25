using System;
using Saga;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DynamicCardPrefab : MonoBehaviour
{
	public GameObject size1, size2, size3, surgeBox, attackRanged, attackMelee, dicePipPrefab, defenseContainer, attackContainer, abilityContainer, modPanel;
	public Image mugshot, faction, mugshotOutline, expansion;
	public Sprite[] factionSprites, expansionSprites;
	public TextMeshProUGUI cardName, traits, keywords, cost, rcost, health, speed, modText;
	public Image cardColor;
	public HPTrackerContainer hpTracker;

	private DeploymentCard card;

	//dynamic card background color = 007CC1

	public void InitCard( DeploymentCard cd, bool hideModifier = false )
	{
		card = cd;

		//handle name
		if ( DataStore.gameType == GameType.Saga )
		{
			//check for override
			var ovrd = DataStore.sagaSessionData?.gameVars.GetDeploymentOverride( cd.id );
			if ( ovrd != null && ovrd.isCustomDeployment )
				card = ovrd.customCard;

			modPanel.gameObject.SetActive( false );
			if ( ovrd != null && ovrd.showMod && !hideModifier )
			{
				modPanel.gameObject.SetActive( true );
				modText.text = Utils.ReplaceGlyphs( ovrd.modification );
			}

			//name, subname
			if ( ovrd != null )
				cardName.text = ovrd.nameOverride;
			else
				cardName.text = card.name;
			if ( !string.IsNullOrEmpty( card.subname ) )
				cardName.text += $"\r\n<size=20><color=\"orange\">{card.subname}</color></size>";

			//reset wound tracker
			if ( hpTracker != null )
				hpTracker.ResetTracker( card );
		}
		else
		{
			cardName.text = card.name;
			if ( !string.IsNullOrEmpty( card.subname ) )
				cardName.text += $"\r\n<size=20><color=\"orange\">{card.subname}</color></size>";
		}

		//surges,traits, abilities, keywords
		ParseSurges();
		ParseTraits();
		ParseAbilities();
		ParseKeywords();

		//expansion
		int exp = (int)Enum.Parse( typeof( Expansion ), card.expansion, true );
		expansion.sprite = expansionSprites[exp];
		if ( card.IsImported && (card.characterType == CharacterType.Hero || card.characterType == CharacterType.Ally) )
			expansion.sprite = expansionSprites[8];

		//dice
		SetDefense();
		SetAttack();

		//faction
		faction.gameObject.SetActive( true );
		if ( card.faction == "Imperial" )
			faction.sprite = factionSprites[0];
		else if ( card.faction == "Mercenary" )
			faction.sprite = factionSprites[1];
		else
			faction.sprite = factionSprites[2];
		if ( card.isCustomEnemyDeployment || card.characterType == CharacterType.Hero || card.characterType == CharacterType.Ally )
			faction.gameObject.SetActive( false );

		//attack type
		attackMelee.SetActive( false );
		attackRanged.SetActive( false );
		if ( card.attackType == AttackType.Melee )
			attackMelee.SetActive( true );
		else if ( card.attackType == AttackType.Ranged )
			attackRanged.SetActive( true );

		//size
		size2.SetActive( false );
		size3.SetActive( false );
		if ( card.size > 1 )
			size2.SetActive( true );
		if ( card.size > 2 )
			size3.SetActive( true );

		//mugshot
		mugshot.sprite = Resources.Load<Sprite>( card.mugShotPath );

		if ( cd.id == "DG070" )//handle custom group
		{
			mugshot.sprite = Resources.Load<Sprite>( $"CardThumbnails/customToken" );
			faction.gameObject.SetActive( false );
		}

		//elite?
		if ( card.isElite )
		{
			cardColor.color = new Color( 1f, 40f / 255f, 0 );
			//mugshotOutline.color = new Color( 1, 40f / 255f, 0 );
		}
		else
		{
			cardColor.color = new Color( 0, 164 / 255f, 1 );
			//mugshotOutline.color = new Color( 0, 164f / 255f, 1 );
		}
		mugshotOutline.color = Utils.String2UnityColor( card.deploymentOutlineColor ?? "LightBlue" );
		//expansion.color = Utils.String2UnityColor( card.deploymentOutlineColor );//mugshotOutline.color;

		///IS COST THE SAME AS THREAT COST FROM OVERRIDE?
		//numbers
		cost.text = card.cost.ToString();
		rcost.text = card.rcost.ToString();
		health.text = card.health.ToString();
		speed.text = card.speed.ToString();

		if ( DataStore.gameType == GameType.Saga )
		{
			var ovrd = DataStore.sagaSessionData?.gameVars.GetDeploymentOverride( cd.id );
			//handle override of NON-custom groups (applies to enemies only)
			if ( ovrd != null && ovrd.useGenericMugshot && !ovrd.isCustomDeployment )
			{
				//mugshot
				if ( card.characterType == CharacterType.Imperial || card.characterType == CharacterType.Villain )
					mugshot.sprite = Resources.Load<Sprite>( "CardThumbnails/genericEnemy" );
				else
					mugshot.sprite = Resources.Load<Sprite>( "CardThumbnails/genericAlly" );
				//and clear out ALL data
				//no faction
				faction.gameObject.SetActive( false );
				//size=1
				size2.SetActive( false );
				size3.SetActive( false );
				//remove elite
				cardColor.color = new Color( 0, 164 / 255f, 1 );
				mugshotOutline.color = new Color( 0, 164f / 255f, 1 );
				//expansion to Core
				expansion.color = mugshotOutline.color;
				//numbers
				cost.text = rcost.text = health.text = speed.text = "0";
				//surges
				foreach ( Transform child in surgeBox.transform )
				{
					Destroy( child.gameObject );
				}
				//traits
				traits.text = "";
				//keywords
				keywords.text = "";
				//abilities
				foreach ( Transform child in abilityContainer.transform )
					Destroy( child.gameObject );
				foreach ( Transform child in defenseContainer.transform )
					Destroy( child.gameObject );
				foreach ( Transform child in attackContainer.transform )
					Destroy( child.gameObject );
			}
		}
	}

	private void ParseSurges()
	{
		foreach ( Transform child in surgeBox.transform )
		{
			Destroy( child.gameObject );
		}

		if ( card.surges == null )
			return;

		for ( int i = 0; i < card.surges.Length; i++ )
		{

			GameObject go = new GameObject( "surge" );
			go.layer = 5;
			go.transform.SetParent( surgeBox.transform );
			go.transform.localScale = Vector3.one;
			go.transform.localEulerAngles = Vector3.zero;
			go.transform.localPosition = Vector3.zero;

			TextMeshProUGUI nt = go.AddComponent<TextMeshProUGUI>();
			nt.color = new Color( 1, 0.7863293f, 0 );
			nt.verticalAlignment = VerticalAlignmentOptions.Middle;
			nt.enableAutoSizing = true;
			nt.margin = new Vector4( 0, 2, 0, 2 );

			//replace glyphs
			string item = Saga.Utils.ReplaceGlyphs( card.surges[i] );

			if ( !string.IsNullOrEmpty( item ) )
				nt.text = $"<color=#7FD3FF>{i + 1})</color> {item}";
		}
	}

	private void ParseTraits()
	{
		traits.text = "";

		if ( card.traits == null || card.traits.Length == 0 )
			return;

		for ( int i = 0; i < card.traits.Length; i++ )
		{
			traits.text += card.traits[i] + ", ";
		}
		traits.text = traits.text.Substring( 0, traits.text.Length - 2 );//remove comma
	}

	private void ParseKeywords()
	{
		keywords.text = "";

		if ( card.keywords == null || card.keywords.Length == 0 )
		{
			keywords.text = DataStore.uiLanguage.uiMainApp.noKeywordsUC;
			return;
		}

		for ( int i = 0; i < card.keywords.Length; i++ )
		{
			keywords.text += Saga.Utils.ReplaceGlyphs( card.keywords[i] ) + "\r\n";
		}
	}

	private void ParseAbilities()
	{
		foreach ( Transform child in abilityContainer.transform )
		{
			Destroy( child.gameObject );
		}

		if ( card.abilities == null || card.abilities.Length == 0 )
		{
			GameObject gon = new GameObject( "ability" );
			gon.layer = 5;
			gon.transform.SetParent( abilityContainer.transform );
			gon.transform.localScale = Vector3.one;
			gon.transform.localEulerAngles = Vector3.zero;
			gon.transform.localPosition = Vector3.zero;

			TextMeshProUGUI nt = gon.AddComponent<TextMeshProUGUI>();
			nt.color = new Color( 0, 1, 0.628047f, 1 );
			nt.text = $"<size=25><b>{DataStore.uiLanguage.uiMainApp.noAbilitiesUC}</b></size>";
		}

		if ( card.abilities != null )
		{
			for ( int i = 0; i < card.abilities.Length; i++ )
			{
				GameObject goa = new GameObject( "ability" );
				goa.layer = 5;
				goa.transform.SetParent( abilityContainer.transform );
				goa.transform.localScale = Vector3.one;
				goa.transform.localEulerAngles = Vector3.zero;
				goa.transform.localPosition = Vector3.zero;

				TextMeshProUGUI ntt = goa.AddComponent<TextMeshProUGUI>();
				ntt.color = new Color( 0.4980392f, 0.8266184f, 1 );

				//replace glyphs
				string aName = Utils.ReplaceGlyphs( card.abilities[i].name );
				string item = Utils.ReplaceGlyphs( card.abilities[i].text );

				if ( !string.IsNullOrEmpty( aName ) )
					ntt.text = $"<size=25><b><color=orange>{aName}:</color></b> ";
				else
					ntt.text = "<size=25>";
				ntt.text += item + "</size>";
			}
		}

		//ignored (title)
		GameObject g = new GameObject( "ability" );
		g.layer = 5;
		g.transform.SetParent( abilityContainer.transform );
		g.transform.localScale = Vector3.one;
		g.transform.localEulerAngles = Vector3.zero;
		g.transform.localPosition = Vector3.zero;

		TextMeshProUGUI tt = g.AddComponent<TextMeshProUGUI>();
		tt.color = new Color( 0, 1, 0.628047f, 1 );//( 137f / 255f, 164f / 255f, 1 );
		tt.text = $"<size=25><b><color=red>{DataStore.uiLanguage.uiMainApp.ignoredAbilitiesUC}:</color></b><br>";

		//ignored abilities
		GameObject go = new GameObject( "ability" );
		go.layer = 5;
		go.transform.SetParent( abilityContainer.transform );
		go.transform.localScale = Vector3.one;
		go.transform.localEulerAngles = Vector3.zero;
		go.transform.localPosition = Vector3.zero;
		if ( card.ignored?.Length == 0 )
		{
			tt.text += DataStore.uiLanguage.uiMainApp.noneUC;
		}
		for ( int i = 0; i < card.ignored?.Length; i++ )
		{
			tt.text += card.ignored[i];
		}
		tt.text += "</size>";

		//scroll to top
		var rt = abilityContainer.GetComponent<RectTransform>();
		abilityContainer.transform.localPosition = new Vector3( abilityContainer.transform.localPosition.x, -rt.rect.size.y, 0 );
	}

	private void SetDefense()
	{
		foreach ( Transform child in defenseContainer.transform )
		{
			Destroy( child.gameObject );
		}

		if ( card.defense == null )
			return;

		for ( int i = 0; i < card.defense.Length; i++ )
		{
			DiceColor dc = (DiceColor)Enum.Parse( typeof( DiceColor ), card.defense[i].ToString(), true );

			var d = Instantiate( dicePipPrefab, defenseContainer.transform );
			d.transform.localScale = Vector3.one;
			d.transform.localEulerAngles = Vector3.zero;
			d.transform.localPosition = Vector3.zero;
			d.GetComponent<DicePip>().image.color = dc == DiceColor.Black ? Color.black : Color.white;
		}
	}

	private void SetAttack()
	{
		foreach ( Transform child in attackContainer.transform )
		{
			Destroy( child.gameObject );
		}

		if ( card.attacks == null )
			return;

		for ( int i = 0; i < card.attacks.Length; i++ )
		{
			DiceColor dc = (DiceColor)Enum.Parse( typeof( DiceColor ), card.attacks[i].ToString(), true );

			var d = Instantiate( dicePipPrefab, attackContainer.transform );
			d.transform.localScale = Vector3.one;
			d.transform.localEulerAngles = Vector3.zero;
			d.transform.localPosition = Vector3.zero;

			switch ( dc )
			{
				case DiceColor.Yellow:
					d.GetComponent<DicePip>().image.color = Color.yellow;
					break;
				case DiceColor.Green:
					d.GetComponent<DicePip>().image.color = Color.green;
					break;
				case DiceColor.Red:
					d.GetComponent<DicePip>().image.color = Color.red;
					break;
				case DiceColor.Blue:
					d.GetComponent<DicePip>().image.color = Color.blue;
					break;
				case DiceColor.Grey:
					d.GetComponent<DicePip>().image.color = Color.grey;
					break;
			}
		}
	}
}