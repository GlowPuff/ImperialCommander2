using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DynamicCardPrefab : MonoBehaviour
{
	public GameObject size1, size2, size3, abilityBox, surgeBox, attackRanged, attackMelee, dicePipPrefab, defenseContainer, attackContainer, abilityContainer;
	public Image mugshot, faction, mugshotOutline, expansion;
	public Sprite[] factionSprites, expansionSprites;
	public TextMeshProUGUI cardName, traits, keywords, cost, rcost, health, speed;
	public Image cardColor;

	private CardDescriptor card;

	public void InitCard( CardDescriptor cd )
	{
		card = cd;

		//name, subname
		cardName.text = card.name;
		if ( !string.IsNullOrEmpty( card.subname ) )
			cardName.text += $"\r\n<size=20><color=\"orange\">{card.subname}</color></size>";

		//surges,traits, abilities, keywords
		ParseSurges();
		ParseTraits();
		ParseAbilities();
		ParseKeywords();

		//expansion
		int exp = (int)Enum.Parse( typeof( Expansion ), card.expansion, true );
		expansion.sprite = expansionSprites[exp];

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
		if ( DataStore.deploymentCards.cards.Any( x => x.id == card.id ) )
			mugshot.sprite = Resources.Load<Sprite>( $"Cards/Enemies/{cd.expansion}/{cd.id.Replace( "DG", "M" )}" );
		else if ( DataStore.villainCards.cards.Any( x => x.id == cd.id ) )
			mugshot.sprite = Resources.Load<Sprite>( $"Cards/Villains/{cd.id.Replace( "DG", "M" )}" );
		else if ( DataStore.allyCards.cards.Any( x => x.id == cd.id ) )
			mugshot.sprite = Resources.Load<Sprite>( $"Cards/Allies/{cd.id.Replace( "A", "M" )}" );
		else if ( DataStore.heroCards.cards.Any( x => x.id == cd.id ) )
			mugshot.sprite = Resources.Load<Sprite>( $"Cards/Heroes/{cd.id}" );
		else if ( cd.id == "DG070" )//handle custom group
		{
			mugshot.sprite = Resources.Load<Sprite>( $"Cards/Enemies/Other/M070" );
			faction.gameObject.SetActive( false );
		}

		//elite?
		if ( card.isElite )
		{
			cardColor.color = new Color( 1f, 40f / 255f, 0 );
			mugshotOutline.color = new Color( 1, 40f / 255f, 0 );
		}
		else
		{
			cardColor.color = new Color( 0, 164 / 255f, 1 );
			mugshotOutline.color = new Color( 0, 164f / 255f, 1 );
		}
		expansion.color = mugshotOutline.color;

		//numbers
		cost.text = card.cost.ToString();
		rcost.text = card.rcost.ToString();
		health.text = card.health.ToString();
		speed.text = card.speed.ToString();
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
			nt.color = new Color( 1, 201f / 255f, 0 );
			nt.verticalAlignment = VerticalAlignmentOptions.Middle;
			nt.enableAutoSizing = true;
			nt.margin = new Vector4( 0, 2, 0, 2 );

			//replace glyphs
			string item = ReplaceGlyphs( card.surges[i] );

			nt.text = $"<color=#00A4FF>{i + 1 })</color> {item}";
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
			keywords.text += ReplaceGlyphs( card.keywords[i] ) + "\r\n";
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
			nt.color = new Color( 137f / 255f, 164f / 255f, 1 );
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
				ntt.color = new Color( 137f / 255f, 164f / 255f, 1 );

				//replace glyphs
				string aName = ReplaceGlyphs( card.abilities[i].name );
				string item = ReplaceGlyphs( card.abilities[i].text );

				ntt.text = $"<size=25><b><color=orange>{aName}:</color></b> ";
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
		tt.color = new Color( 137f / 255f, 164f / 255f, 1 );
		tt.text = $"<size=25><b><color=red>{DataStore.uiLanguage.uiMainApp.ignoredAbilitiesUC}:</color></b><br>";

		//ignored abilities
		GameObject go = new GameObject( "ability" );
		go.layer = 5;
		go.transform.SetParent( abilityContainer.transform );
		go.transform.localScale = Vector3.one;
		go.transform.localEulerAngles = Vector3.zero;
		go.transform.localPosition = Vector3.zero;
		if ( card.ignored.Length == 0 )
		{
			tt.text += DataStore.uiLanguage.uiMainApp.noneUC;
		}
		for ( int i = 0; i < card.ignored.Length; i++ )
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

		return item;
	}
}