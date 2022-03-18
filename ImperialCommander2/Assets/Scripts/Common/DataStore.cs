using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public static class DataStore
{
	public static readonly string appVersion = "v.1.0.22";
	public static readonly string[] languageCodeList = { "En", "De", "Es", "Fr", "Pl", "It" };

	public static Dictionary<string, List<MissionCard>> missionCards;
	/// <summary>
	/// all enemies (excluding villains)
	/// </summary>
	public static DeploymentCards deploymentCards;
	public static DeploymentCards allyCards;
	public static DeploymentCards villainCards;
	public static DeploymentCards heroCards;
	public static SessionData sessionData;
	public static List<Expansion> ownedExpansions;
	public static List<CardDescriptor>
		deploymentHand,
		manualDeploymentList,
		deployedHeroes,//contains deployed heroes AND allies
		deployedEnemies;
	public static List<CardEvent> cardEvents;
	public static List<CardInstruction> activationInstructions;
	public static List<BonusEffect> bonusEffects;
	public static List<DeploymentSound> deploymentSounds;
	public static Dictionary<string, List<MissionPreset>> missionPresets;
	public static Vector3[] pipColors = new Vector3[6]
	{
		(0.3301887f).ToVector3(),
		new Vector3(0.6784314f,0,1),
		new Vector3(0,0,0),
		new Vector3(0,0.3294118f,1),
		new Vector3(0,0.735849f,0.1056484f),
		new Vector3(1,0,0)
	};
	public static int languageCode;
	public static UILanguage uiLanguage;
	/* Things affected by language
	 * UI strings
	 * events, bonus effects, instructions
	 * mission info/rules
	 * card text
	 * */

	private static List<CardDescriptor> villainsToManuallyAdd;

	//manualDeploymentList includes all owned expansion groups plus villains, minus deployment hand, plus both factions, minus reserved, minus starting, minus EARNED villains

	/// <summary>
	/// Creates all the card lists, load app settings
	/// </summary>
	public static void InitData()
	{
		string[] expansions = Enum.GetNames( typeof( Expansion ) );

		missionCards = new Dictionary<string, List<MissionCard>>();
		deploymentHand = new List<CardDescriptor>();
		manualDeploymentList = new List<CardDescriptor>();
		deployedHeroes = new List<CardDescriptor>();
		deployedEnemies = new List<CardDescriptor>();
		villainsToManuallyAdd = new List<CardDescriptor>();
		deploymentSounds = new List<DeploymentSound>();
		missionPresets = new Dictionary<string, List<MissionPreset>>();

		cardEvents = new List<CardEvent>();
		activationInstructions = new List<CardInstruction>();
		bonusEffects = new List<BonusEffect>();

		//load deployment sound lookup
		deploymentSounds = LoadDeploymentSounds();
		//load mission presets
		LoadMissionPresets();

		//setup language
		//default language playerprefs key should be set by now, but just in case...
		if ( PlayerPrefs.HasKey( "language" ) )
			languageCode = PlayerPrefs.GetInt( "language" );
		else
		{
			PlayerPrefs.SetInt( "language", 0 );
			PlayerPrefs.Save();
			languageCode = 0;
		}

		//cards, events, activation instructions, bonus effects, ui
		LoadTranslatedData();

		//load settings from local storage
		ownedExpansions = new List<Expansion>();
		//always add core
		ownedExpansions.Add( Expansion.Core );
		for ( int i = 0; i < expansions.Length; i++ )
		{
			//skip core, already added by default
			if ( expansions[i] != "Core" && PlayerPrefs.HasKey( expansions[i] ) )
			{
				if ( PlayerPrefs.GetString( expansions[i] ) == "true" )
					ownedExpansions.Add( (Expansion)Enum.Parse( typeof( Expansion ), expansions[i] ) );
			}
			else
			{
				PlayerPrefs.SetString( expansions[i], "false" );
				PlayerPrefs.Save();
			}
		}
	}

	public static void LoadTranslatedData()
	{
		try
		{
			string[] expansions = Enum.GetNames( typeof( Expansion ) );
			TextAsset json;
			missionCards = new Dictionary<string, List<MissionCard>>();
			//load mission card DATA
			foreach ( string expansion in expansions )
			{
				json = Resources.Load<TextAsset>( $"MissionData/{expansion}" );
				if ( json != null )
				{
					var cards = JsonConvert.DeserializeObject<List<MissionCard>>( json.text );
					missionCards.Add( expansion, cards );
				}
			}

			//load card DATA
			deploymentCards = LoadCards( "enemies" );
			allyCards = LoadCards( "allies" );
			villainCards = LoadCards( "villains" );
			heroCards = LoadCards( "heroes" );

			//events, activation instructions, bonus effects
			cardEvents = LoadEvents();
			activationInstructions = LoadInstructions();
			bonusEffects = LoadBonusEffects();
			//ui
			uiLanguage = LoadUILanguage();
			uiLanguage.uiDeploymentGroups = LoadDeploymentCardTranslations();
			LoadMissionCardTranslations();

			//assign translations to card data
			SetCardTranslations( deploymentCards.cards );
			SetCardTranslations( allyCards.cards );
			SetCardTranslations( villainCards.cards );
			SetCardTranslations( heroCards.cards );

			Debug.Log( "Loaded Language: " + languageCodeList[languageCode] );
		}
		catch ( Exception e )
		{
			Debug.Log( $"LoadTranslatedData() ERROR:\r\nError parsing data" );
			Debug.Log( e );
			//default to English so app loads correctly next time
			languageCode = 0;
			PlayerPrefs.SetInt( "language", 0 );
			PlayerPrefs.Save();
		}
	}

	/// <summary>
	/// Creates new SessionData, always called AFTER InitData
	/// </summary>
	public static void StartNewSession()
	{
		sessionData = new SessionData();
	}

	static DeploymentCards LoadCards( string asset )
	{
		try
		{
			TextAsset json = Resources.Load<TextAsset>( $"CardData/{asset}" );
			return JsonConvert.DeserializeObject<DeploymentCards>( json.text );
		}
		catch ( JsonException e )
		{
			Debug.Log( $"LoadCards() ERROR:\r\nError parsing {asset}.json" );
			Debug.Log( e.Message );
			LogError( e.Message );
			throw new Exception();
		}
	}

	static List<CardEvent> LoadEvents()
	{
		try
		{
			TextAsset json = Resources.Load<TextAsset>( "Languages/" + languageCodeList[languageCode] + "/events" );
			return JsonConvert.DeserializeObject<EventList>( json.text ).events;
		}
		catch ( JsonReaderException e )
		{
			Debug.Log( $"LoadTranslatedData() ERROR:\r\nError parsing Events" );
			Debug.Log( e.Message );
			LogError( e.Message );
			throw new Exception();
		}
	}

	static List<CardInstruction> LoadInstructions()
	{
		try
		{
			TextAsset json = Resources.Load<TextAsset>( "Languages/" + languageCodeList[languageCode] + "/instructions" );
			return JsonConvert.DeserializeObject<List<CardInstruction>>( json.text );
		}
		catch ( JsonReaderException e )
		{
			Debug.Log( $"LoadTranslatedData() ERROR:\r\nError parsing Instructions" );
			Debug.Log( e.Message );
			LogError( e.Message );
			throw new Exception();
		}
	}

	static List<BonusEffect> LoadBonusEffects()
	{
		try
		{
			TextAsset json = Resources.Load<TextAsset>( "Languages/" + languageCodeList[languageCode] + "/bonuseffects" );
			return JsonConvert.DeserializeObject<List<BonusEffect>>( json.text );
		}
		catch ( JsonReaderException e )
		{
			Debug.Log( $"LoadTranslatedData() ERROR:\r\nError parsing Bonus Effects" );
			Debug.Log( e.Message );
			LogError( e.Message );
			throw new Exception();
		}
	}

	static List<DeploymentSound> LoadDeploymentSounds()
	{
		TextAsset json = Resources.Load<TextAsset>( "sounds" );
		return JsonConvert.DeserializeObject<List<DeploymentSound>>( json.text );
	}

	static void LoadMissionPresets()
	{
		//Core, Twin, Hoth, Bespin, Jabba, Empire, Lothal, Other
		string[] e = Enum.GetNames( typeof( Expansion ) );
		for ( int i = 0; i < e.Length; i++ )
		{
			TextAsset json = Resources.Load<TextAsset>( $"MissionPresets/{e[i]}" );
			missionPresets.Add( e[i].ToLower(), JsonConvert.DeserializeObject<List<MissionPreset>>( json.text ) );
		}
	}

	static UILanguage LoadUILanguage()
	{
		try
		{
			TextAsset json = Resources.Load<TextAsset>( "Languages/" + languageCodeList[languageCode] + "/ui" );
			return JsonConvert.DeserializeObject<UILanguage>( json.text );
		}
		catch ( JsonReaderException e )
		{
			Debug.Log( $"LoadTranslatedData() ERROR:\r\nError parsing UI Language" );
			Debug.Log( e.Message );
			LogError( e.Message );
			throw new Exception();
		}
	}

	static UIDeploymentGroups LoadDeploymentCardTranslations()
	{
		string asset = "";
		try
		{
			TextAsset enemies = Resources.Load<TextAsset>( $"Languages/{languageCodeList[languageCode]}/DeploymentGroups/enemies" );
			TextAsset allies = Resources.Load<TextAsset>( $"Languages/{languageCodeList[languageCode]}/DeploymentGroups/allies" );
			TextAsset villains = Resources.Load<TextAsset>( $"Languages/{languageCodeList[languageCode]}/DeploymentGroups/villains" );
			TextAsset heroes = Resources.Load<TextAsset>( $"Languages/{languageCodeList[languageCode]}/DeploymentGroups/heroes" );

			asset = "enemies";
			List<CardLanguage> enemyCards = JsonConvert.DeserializeObject<List<CardLanguage>>( enemies.text );
			asset = "allies";
			List<CardLanguage> allyCards = JsonConvert.DeserializeObject<List<CardLanguage>>( allies.text );
			asset = "villains";
			List<CardLanguage> villainCards = JsonConvert.DeserializeObject<List<CardLanguage>>( villains.text );
			asset = "heroes";
			List<CardLanguage> heroCards = JsonConvert.DeserializeObject<List<CardLanguage>>( heroes.text );

			return new UIDeploymentGroups() { allyCards = allyCards, villainCards = villainCards, heroCards = heroCards, enemyCards = enemyCards };
		}
		catch ( JsonReaderException e )
		{
			Debug.Log( $"LoadCardTranslations({asset}) ERROR:\r\nError parsing Card Languages" );
			Debug.Log( e.Message );
			LogError( e.Message );
			throw new Exception();
		}
	}

	/// <summary>
	/// loads translations for ALL expansions
	/// </summary>
	static void LoadMissionCardTranslations()
	{
		string asset = "";
		try
		{
			for ( int i = 0; i < Enum.GetNames( typeof( Expansion ) ).Length; i++ )
			{
				asset = ((Expansion)i).ToString();
				TextAsset missions = Resources.Load<TextAsset>( $"Languages/{languageCodeList[languageCode]}/MissionCardText/{(Expansion)i}" );
				var cards = JsonConvert.DeserializeObject<List<MissionCard>>( missions.text );
				//set translation data ONLY
				for ( int e = 0; e < cards.Count; e++ )
				{
					missionCards[((Expansion)i).ToString()][e].expansion = (Expansion)i;
					missionCards[((Expansion)i).ToString()][e].name = cards[e].name;
					missionCards[((Expansion)i).ToString()][e].descriptionText = cards[e].descriptionText;
					missionCards[((Expansion)i).ToString()][e].bonusText = cards[e].bonusText;
					missionCards[((Expansion)i).ToString()][e].heroText = cards[e].heroText;
					missionCards[((Expansion)i).ToString()][e].allyText = cards[e].allyText;
					missionCards[((Expansion)i).ToString()][e].villainText = cards[e].villainText;
					missionCards[((Expansion)i).ToString()][e].tagsText = cards[e].tagsText;
					missionCards[((Expansion)i).ToString()][e].expansionText = cards[e].expansionText;
					missionCards[((Expansion)i).ToString()][e].rebelRewardText = cards[e].rebelRewardText;
					missionCards[((Expansion)i).ToString()][e].imperialRewardText = cards[e].imperialRewardText;
				}
			}
		}
		catch ( JsonReaderException e )
		{
			Debug.Log( $"LoadMissionCardTranslations({asset}) ERROR:\r\nError parsing Card Languages" );
			Debug.Log( e.Message );
			LogError( e.Message );
			throw new Exception();
		}
	}

	public static void SetCardTranslations( List<CardDescriptor> toCards )
	{
		try
		{
			var langCards = uiLanguage.uiDeploymentGroups.enemyCards;
			langCards = langCards.Concat( uiLanguage.uiDeploymentGroups.heroCards ).ToList();
			langCards = langCards.Concat( uiLanguage.uiDeploymentGroups.villainCards ).ToList();
			langCards = langCards.Concat( uiLanguage.uiDeploymentGroups.allyCards ).ToList();

			for ( int i = 0; i < toCards.Count; i++ )
			{
				//don't try to load card data TO a dummy hero, since no data exists
				if ( !toCards[i].isDummy )
				{
					var langcard = langCards.Where( x => x.id == toCards[i].id ).FirstOr( null );
					//sanity check
					if ( langcard != null )
					{
						toCards[i].name = langcard.name;
						toCards[i].subname = langcard.subname;
						toCards[i].ignored = langcard.ignored;
						toCards[i].traits = langcard.traits;
						toCards[i].surges = langcard.surges;
						toCards[i].keywords = langcard.keywords;
						toCards[i].abilities = langcard.abilities;
					}
					else
						throw new Exception( "'langcard' is null" );
				}
			}
		}
		catch ( Exception e )
		{
			Debug.Log( $"SetCardTranslations() ERROR:\r\nError parsing card data" );
			Debug.Log( e );
			LogError( e.Message );
			throw new Exception();
		}
	}

	public static void AddExpansion( string exp )
	{
		PlayerPrefs.SetString( exp, "true" );
		PlayerPrefs.Save();
		Expansion xp = (Expansion)Enum.Parse( typeof( Expansion ), exp );
		if ( !ownedExpansions.Contains( xp ) )
			ownedExpansions.Add( xp );
	}

	public static void RemoveExpansions( string exp )
	{
		PlayerPrefs.SetString( exp, "false" );
		PlayerPrefs.Save();
		Expansion xp = (Expansion)Enum.Parse( typeof( Expansion ), exp );
		if ( ownedExpansions.Contains( xp ) )
			ownedExpansions.Remove( xp );
	}

	public static void CreateManualDeployment()
	{
		//filter owned expansions
		var available = deploymentCards.cards
			.OwnedPlusOther()
			.ToList();
		//add all villains
		available = available.Concat( villainCards.cards ).ToList();
		//filter out reserved/starting/earned villains
		available = available
			.MinusInDeploymentHand()
			.MinusReserved()
			.MinusStarting()
			.MinusEarnedVillains();

		//add any earned villains that didn't make it into the dep hand
		available.AddRange( villainsToManuallyAdd );
		foreach ( var cd in villainsToManuallyAdd )
		{
			//Debug.Log( "TO ADD: " + cd.name );
		}

		available.Sort( ( x, y ) =>
		 {
			 if ( int.Parse( x.id.Replace( "DG", "" ) ) == int.Parse( y.id.Replace( "DG", "" ) ) )
				 return 0;
			 else
				 return int.Parse( x.id.Replace( "DG", "" ) ) < int.Parse( y.id.Replace( "DG", "" ) ) ? -1 : 1;
		 } );

		manualDeploymentList = available.ToList();
		Debug.Log( $"MANUAL GROUP SIZE: {manualDeploymentList.Count} CARDS" );
	}

	public static void SortManualDeployList()
	{
		manualDeploymentList.Sort( ( x, y ) =>
		{
			if ( int.Parse( x.id.Replace( "DG", "" ) ) == int.Parse( y.id.Replace( "DG", "" ) ) )
				return 0;
			else
				return int.Parse( x.id.Replace( "DG", "" ) ) < int.Parse( y.id.Replace( "DG", "" ) ) ? -1 : 1;
		} );
	}

	public static void CreateDeploymentHand()
	{
		var available = deploymentCards.cards
			.OwnedPlusOther()
			.FilterByFaction()
			.MinusIgnored()
			.MinusStarting()
			.MinusReserved()
			.ToList();
		//Debug.Log( $"OF {deploymentCards.cards.Count} CARDS, USING {available.Count()}" );

		//add earned villains
		available = available.Concat( sessionData.EarnedVillains ).ToList();
		//Debug.Log( $"ADD VILLAINS FILTERED TO {available.Count()} CARDS" );

		if ( sessionData.threatLevel <= 3 )
			available = GetCardsByTier( available.ToList(), 2, 2, 0 );
		else if ( sessionData.threatLevel == 4 )
			available = GetCardsByTier( available.ToList(), 1, 2, 1 );
		else if ( sessionData.threatLevel >= 5 )
			available = GetCardsByTier( available.ToList(), 1, 2, 2 );

		//if there are any villains and none were added, "help" add one (50% chance)
		if ( sessionData.EarnedVillains.Count > 0
			&& !available.Any( x => sessionData.EarnedVillains.Contains( x ) )
			&& GlowEngine.RandomBool() )
		{
			int[] rv = GlowEngine.GenerateRandomNumbers( sessionData.EarnedVillains.Count );
			var v = sessionData.EarnedVillains[rv[0]];
			available = available.Concat( new List<CardDescriptor>() { v } ).ToList();
			//add any remaining earned villains back into manual deploy list
			foreach ( var cd in sessionData.EarnedVillains )
			{
				if ( !available.Contains( cd ) )
					villainsToManuallyAdd.Add( cd );
			}
			//Debug.Log( $"ADDED A VILLAIN (50%): {v.name}" );
		}
		else
		{
			//if villain wasn't already added to DH, AND it didn't get helped into hand, add it to manual deployment list
			foreach ( var cd in sessionData.EarnedVillains )
			{
				if ( !available.Contains( cd ) )
				{
					//Debug.Log( "VILLAIN *NOT* ADDED TO DH: " + cd.name );
					villainsToManuallyAdd.Add( cd );
				}
			}
		}

		Debug.Log( $"DEPLOYMENT HAND SIZE: {available.Count()} CARDS" );
		//for ( int i = 0; i < available.Count(); i++ )
		//{
		//	Debug.Log( available.ElementAt( i ).name );
		//}
		deploymentHand = available.ToList();
	}

	public static bool LoadState()
	{
		string basePath = Path.Combine( Application.persistentDataPath, "Session" );

		string json = "";
		try
		{
			//deployment hand
			string path = Path.Combine( basePath, "deploymenthand.json" );
			using ( StreamReader sr = new StreamReader( path ) )
			{
				json = sr.ReadToEnd();
			}
			deploymentHand = JsonConvert.DeserializeObject<List<CardDescriptor>>( json );

			//manual deployment deck
			path = Path.Combine( basePath, "manualdeployment.json" );
			using ( StreamReader sr = new StreamReader( path ) )
			{
				json = sr.ReadToEnd();
			}
			manualDeploymentList = JsonConvert.DeserializeObject<List<CardDescriptor>>( json );

			//deployed enemies
			path = Path.Combine( basePath, "deployedenemies.json" );
			using ( StreamReader sr = new StreamReader( path ) )
			{
				json = sr.ReadToEnd();
			}
			deployedEnemies = JsonConvert.DeserializeObject<List<CardDescriptor>>( json );

			//deployed heroes
			path = Path.Combine( basePath, "heroesallies.json" );
			using ( StreamReader sr = new StreamReader( path ) )
			{
				json = sr.ReadToEnd();
			}
			deployedHeroes = JsonConvert.DeserializeObject<List<CardDescriptor>>( json );

			//remaining events
			path = Path.Combine( basePath, "events.json" );
			using ( StreamReader sr = new StreamReader( path ) )
			{
				json = sr.ReadToEnd();
			}
			cardEvents = JsonConvert.DeserializeObject<List<CardEvent>>( json );

			//set card text translations
			SetCardTranslations( deploymentHand );
			SetCardTranslations( manualDeploymentList );
			SetCardTranslations( deployedEnemies );
			SetCardTranslations( deployedHeroes );

			return true;
		}
		catch ( Exception e )
		{
			Debug.Log( "***ERROR*** LoadState:: " + e.Message );
			File.WriteAllText( Path.Combine( basePath, "error_log.txt" ), "RESTORE STATE TRACE:\r\n" + e.Message );
			return false;
		}
	}

	public static void LogError( string error )
	{
		string basePath = Application.persistentDataPath;
		File.WriteAllText( Path.Combine( basePath, "error_log.txt" ), "ERROR TRACE:\r\n" + error );
	}

	/// <summary>
	/// Randomly gets the requested number of cards according to tier
	/// </summary>
	static List<CardDescriptor> GetCardsByTier( List<CardDescriptor> haystack, int t1, int t2, int t3 )
	{
		List<CardDescriptor> retval = new List<CardDescriptor>();
		;
		if ( t1 > 0 )
		{
			var g = haystack.Where( x => x.tier == 1 ).ToList();
			int[] rands = GlowEngine.GenerateRandomNumbers( g.Count() );
			for ( int i = 0; i < Math.Min( g.Count(), t1 ); i++ )
				retval.Add( g[rands[i]] );
		}
		if ( t2 > 0 )
		{
			var g = haystack.Where( x => x.tier == 2 ).ToList();
			int[] rands = GlowEngine.GenerateRandomNumbers( g.Count() );
			for ( int i = 0; i < Math.Min( g.Count(), t2 ); i++ )
				retval.Add( g[rands[i]] );
		}
		if ( t3 > 0 )
		{
			var g = haystack.Where( x => x.tier == 3 ).ToList();
			int[] rands = GlowEngine.GenerateRandomNumbers( g.Count() );
			for ( int i = 0; i < Math.Min( g.Count(), t3 ); i++ )
				retval.Add( g[rands[i]] );
		}

		return retval;
	}

	/// <summary>
	/// Get a normal or villain from the id
	/// </summary>
	public static CardDescriptor GetEnemy( string id )
	{
		if ( villainCards.cards.Any( x => x.id == id ) )
			return villainCards.cards.Where( x => x.id == id ).First();
		else if ( deploymentCards.cards.Any( x => x.id == id ) )
			return deploymentCards.cards.Where( x => x.id == id ).First();
		else
			return null;
	}

	/// <summary>
	/// CAN be in dep hand, minus deployed, minus reserved, minus ignored
	/// </summary>
	public static CardDescriptor GetNonEliteVersion( CardDescriptor elite )
	{
		//starting groups already deployed, no need to filter
		//1) filter to NON elites only
		//2) the elite version of the card (passed into this method) will have the NON elite NAME in its name property
		var valid = deploymentCards.cards
			.Where( x => !x.isElite )
			.Where( x => elite.name.Contains( x.name ) ).ToList()
			.MinusDeployed()
			.MinusReserved()
			.MinusIgnored();
		return valid.FirstOr( null );
	}

	/// <summary>
	/// CAN be in dep hand, minus deployed, minus reserved, minus ignored
	/// </summary>
	public static CardDescriptor GetEliteVersion( CardDescriptor cd )
	{
		//starting groups already deployed, no need to filter
		//1) filter to elites only
		//2) the elite version of the card will have the NAME in its name property
		var valid = deploymentCards.cards
			.Where( x => x.isElite )
			.Where( x => x.name.Contains( cd.name ) ).ToList()
			.MinusDeployed()
			.MinusReserved()
			.MinusIgnored();
		return valid.FirstOr( null );
	}

	/// <summary>
	/// Calculate and return a valid reinforcement, optionally applying a -1 rcost modifier for Onslaught
	/// </summary>
	public static CardDescriptor GetReinforcement( bool isOnslaught = false )
	{
		//up to 2 groups reinforce, this method handles ONE
		//get deployed groups that CAN reinforce
		//	-reinforce cost > 0
		//	- current size < max size
		//	-reinforce cost <= current threat
		int costModifier = 0;
		if ( isOnslaught )
			costModifier = 1;

		var valid = deployedEnemies.Where( x =>
			x.rcost > 0 &&
			x.currentSize < x.size &&
			Math.Max( 1, x.rcost - costModifier ) <= sessionData.gameVars.currentThreat ).ToList();
		if ( valid.Count > 0 )
		{
			int[] rnd = GlowEngine.GenerateRandomNumbers( valid.Count );
			//Debug.Log( "GET: " + valid[rnd[0]].currentSize );
			return valid[rnd[0]];
		}

		return null;
	}

	/// <summary>
	/// Calculate and return a deployable group using "fuzzy" deployment, DOES NOT remove it from deployment hand
	/// </summary>
	public static CardDescriptor GetFuzzyDeployable( bool isOnslaught = false )
	{
		/*
		 If the app chooses to deploy a Tier III (=expensive) group, but does not have enough threat by up to 3 points, it still deploys the unit and reduces threat to 0. This way, the deployment of expensive units does not hinge on a tiny amount of missing threat, but doesn’t simply make them cheaper. Example: The app chooses to deploy an AT-ST (threat cost 14). It can deploy even if there is only 11, 12, or 13 threat left
		*/

		List<CardDescriptor> tier1Group = new List<CardDescriptor>();
		List<CardDescriptor> tier2Group = new List<CardDescriptor>();
		CardDescriptor tier3Group = null;
		List<CardDescriptor> tier23Group = new List<CardDescriptor>();
		CardDescriptor validEnemy = null;
		int[] rnd;
		int t2modifier = 0;
		int t3modifier = 0;
		if ( isOnslaught )
		{
			t2modifier = 1;
			t3modifier = 2;
		}

		//get tier 1 affordable groups
		if ( deploymentHand.Any( x =>
			x.tier == 1 &&
			x.cost <= sessionData
			.gameVars.currentThreat ) )
		{
			tier1Group = deploymentHand.Where( x => x.tier == 1 && x.cost <= sessionData.gameVars.currentThreat ).ToList();
		}
		//get tier 2 affordable groups
		if ( deploymentHand.Any( x =>
			x.tier == 2 &&
			x.cost - t2modifier <= sessionData.gameVars.currentThreat ) )
		{
			tier2Group = deploymentHand.Where( x =>
			x.tier == 2 &&
			x.cost - t2modifier <= sessionData.gameVars.currentThreat )
			.ToList();
		}

		//concatenate the tier 1 and tier 2 groups
		tier23Group = tier1Group.Concat( tier2Group ).ToList();
		//filter list - minus deployed
		tier23Group = tier23Group.MinusDeployed();
		//now get ONE of them randomly IF there are any
		if ( tier23Group.Count > 0 )
		{
			rnd = GlowEngine.GenerateRandomNumbers( tier23Group.Count );
			validEnemy = tier23Group[rnd[0]];
		}

		//get a random tier 3 group from deployment hand with cost up to 3 over current threat and NOT DEPLOYED, if one exists
		if ( deploymentHand.Any( x =>
				x.tier == 3 &&
				x.cost - t3modifier <= sessionData.gameVars.currentThreat + 3 &&
				!deployedEnemies.Contains( x )
		) )
		{
			var t3 = deploymentHand.Where( x =>
				x.tier == 3 &&
				x.cost - t3modifier <= sessionData.gameVars.currentThreat + 3 &&
				!deployedEnemies.Contains( x )
			).ToList();
			rnd = GlowEngine.GenerateRandomNumbers( t3.Count );
			tier3Group = t3[rnd[0]];
		}

		//if there are valid tier 3 AND tier 1/2 groups, there is a 50/50 chance of either being returned
		if ( validEnemy != null && tier3Group != null )
		{
			Debug.Log( "ELITE DEPLOYMENT COIN FLIP" );
			if ( GlowEngine.RandomBool() )
			{
				return validEnemy;
			}
			else
			{
				return tier3Group;
			}
		}
		//otherwise try to return the tier 3 group, if any picked
		else if ( validEnemy == null && tier3Group != null )
			return tier3Group;

		//finally try to return the tier1/2 group, even if it's null
		return validEnemy;
	}
}
