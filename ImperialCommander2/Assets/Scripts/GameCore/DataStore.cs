﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Saga;
using UnityEngine;

public static class DataStore
{
	public static readonly string appVersion = "v.2.2.9";
	public static readonly string[] languageCodeList = { "En", "De", "Es", "Fr", "Pl", "It", "Hu", "No", "Ru", "Nl", "Br" };

	public static Mission mission;
	public static GameType gameType;
	public static Dictionary<string, List<MissionCard>> missionCards;
	public static Dictionary<string, string> translatedExpansionNames;//key = expansion code ie: Core
	/// <summary>
	/// all enemies (INCLUDING imports, EXCLUDING villains)
	/// </summary>
	public static List<DeploymentCard> deploymentCards;
	public static List<DeploymentCard> allyCards;
	public static List<DeploymentCard> villainCards;
	public static List<DeploymentCard> heroCards;
	/// <summary>
	/// ALL enemies in the game, including imports and villains
	/// </summary>
	public static List<DeploymentCard> allEnemyDeploymentCards
	{
		get
		{
			return deploymentCards.Concat( villainCards ).ToList();
		}
	}
	//public static SessionData sessionData;
	public static SagaSession sagaSessionData;
	public static List<Expansion> ownedExpansions;
	public static List<DeploymentCard>
		deploymentHand,
		manualDeploymentList,
		deployedHeroes,//contains deployed heroes AND allies
		deployedEnemies;
	public static List<DeploymentCard> ownedFigurePacks;
	public static List<CardEvent> cardEvents;
	public static List<CardInstruction> activationInstructions;
	public static List<CardInstruction> importedActivationInstructions;
	public static List<BonusEffect> bonusEffects;
	public static List<BonusEffect> importedBonusEffects;
	public static List<DeploymentSound> deploymentSounds;
	public static Dictionary<string, List<MissionPreset>> missionPresets;
	public static ThumbnailData thumbnailData;
	//campaign data
	public static List<CampaignItem> campaignDataItems;
	public static List<CampaignReward> campaignDataRewards;

	/// <summary>
	/// all global imported characters picked up at app startup (not the Mission opt-in global imports)
	/// </summary>
	public static List<CustomToon> globalImportedCharacters;
	public static List<string> IgnoredPrefsImports
	{
		get => PlayerPrefs.GetString( "excludedImports" ).Split( '|' ).Where( x => !string.IsNullOrEmpty( x ) ).ToList();
		set
		{
			Debug.Log( $"IgnoredPrefsImports::OLD COUNT = {IgnoredPrefsImports.Count}" );
			PlayerPrefs.SetString( "excludedImports", string.Join( "|", value ) );
			Debug.Log( $"IgnoredPrefsImports::NEW COUNT = {IgnoredPrefsImports.Count}" );
		}
	}

	public static Vector3[] pipColors = new Vector3[7]
	{
		(0.3301887f).ToVector3(),
		new Vector3(0.6784314f,0,1),
		new Vector3(0,0,0),
		new Vector3(0,0.3294118f,1),
		new Vector3(0,0.735849f,0.1056484f),
		new Vector3(1,0,0),
		new Vector3(1, 202f / 255f, 40f / 255f)
	};
	public static int languageCode;
	public static UILanguage uiLanguage;
	/// <summary>
	/// Returns the 2-letter language code, ie: De
	/// </summary>
	public static string Language { get { return languageCodeList[languageCode]; } }
	/* Things affected by language
	 * UI strings
	 * events, bonus effects, instructions
	 * mission info/rules
	 * card text
	 * */

	private static List<DeploymentCard> villainsToManuallyAdd;

	//manualDeploymentList includes all owned expansion groups plus villains, minus deployment hand, plus both factions, minus reserved, minus starting, minus EARNED villains

	/// <summary>
	/// Creates all card lists, load app settings, mission presets and translations, saga mode.
	/// Called when app starts.
	/// Add Custom Characters BEFORE InitData.
	/// </summary>
	public static void InitData()
	{
		Debug.Log( "Datastore::InitData()" );

		gameType = GameType.Saga;
		string[] expansions = Enum.GetNames( typeof( Expansion ) );

		missionCards = new Dictionary<string, List<MissionCard>>();
		deploymentHand = new List<DeploymentCard>();
		manualDeploymentList = new List<DeploymentCard>();
		deployedHeroes = new List<DeploymentCard>();
		deployedEnemies = new List<DeploymentCard>();
		villainsToManuallyAdd = new List<DeploymentCard>();
		deploymentSounds = new List<DeploymentSound>();
		missionPresets = new Dictionary<string, List<MissionPreset>>();
		globalImportedCharacters = new List<CustomToon>();
		ownedFigurePacks = new List<DeploymentCard>();
		mission = null;

		cardEvents = new List<CardEvent>();
		activationInstructions = new List<CardInstruction>();
		importedActivationInstructions = new List<CardInstruction>();
		bonusEffects = new List<BonusEffect>();
		importedBonusEffects = new List<BonusEffect>();

		//load deployment sound lookup
		deploymentSounds = LoadDeploymentSounds();
		//load mission presets
		LoadMissionPresets();
		//load global imported characters saved on device
		globalImportedCharacters = FileManager.LoadGlobalImportedCharacters();
		//validate IgnoredPrefsImports, filter out groups that no longer exist on the device
		Debug.Log( $"FOUND {IgnoredPrefsImports.Count} EXCLUDED IMPORTS STORED IN PlayerPrefs" );
		var valid = globalImportedCharacters.Where( x => IgnoredPrefsImports.Contains( x.deploymentCard.customCharacterGUID.ToString() ) ).Select( x => x.deploymentCard.customCharacterGUID.ToString() ).ToList();
		if ( IgnoredPrefsImports.Count() > 0 )
			Debug.Log( $"{valid.Count()} / {IgnoredPrefsImports.Count} OF THESE EXIST ON THIS DEVICE" );
		if ( valid.Count() != IgnoredPrefsImports.Count() )
		{
			Debug.Log( $"REMOVED {IgnoredPrefsImports.Count() - valid.Count()} INVALID IMPERIALS FROM PlayerPrefs" );
		}
		IgnoredPrefsImports = valid.ToList();

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
		ownedExpansions = new List<Expansion>
		{
			//always add core
			Expansion.Core
		};
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
		Debug.Log( "OWNED EXPANSIONS: " + String.Join( ", ", ownedExpansions ) );
		//figure packs
		for ( int i = 62; i <= 69; i++ )
		{
			if ( PlayerPrefs.GetInt( $"figurepack{i}", 0 ) == 1 )
				ownedFigurePacks.Add( GetEnemy( $"DG0{i}" ) );
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
			thumbnailData = LoadThumbnailData();
			//ui
			uiLanguage = LoadUILanguage();
			uiLanguage.uiDeploymentGroups = LoadDeploymentCardTranslations();
			LoadMissionCardTranslations();
			//help overlays
			uiLanguage.uiHelpOverlay = LoadHelpOverlays();
			//campaign data (items, rewards)
			campaignDataItems = FileManager.LoadAssetFromResource<List<CampaignItem>>( $"Languages/{DataStore.Language}/CampaignData/items" );
			campaignDataRewards = FileManager.LoadAssetFromResource<List<CampaignReward>>( $"Languages/{DataStore.Language}/CampaignData/rewards" );

			//assign translations to card data
			SetCardTranslations( deploymentCards );
			SetCardTranslations( allyCards );
			SetCardTranslations( villainCards );
			SetCardTranslations( heroCards );

			Debug.Log( "Loaded Language: " + languageCodeList[languageCode] );
		}
		catch ( Exception e )
		{
			Utils.LogTranslationError( $"LoadTranslatedData() ERROR::Error parsing data\n{e.Message}" );
			Debug.Log( $"LoadTranslatedData() ERROR:\r\nError parsing data" );
			Debug.Log( e );
			//default to English so app loads correctly next time
			languageCode = 0;
			PlayerPrefs.SetInt( "language", 0 );
			PlayerPrefs.Save();
		}
	}

	/// <summary>
	/// new game session, called from SagaController upon NEW GAME, always called AFTER InitData
	/// </summary>
	public static void StartNewSagaSession( SagaSetupOptions opts )
	{
		sagaSessionData = new SagaSession( opts );
		gameType = GameType.Saga;
	}

	static List<DeploymentCard> LoadCards( string asset )
	{
		try
		{
			string json = Resources.Load<TextAsset>( $"CardData/{asset}" ).text;
			var obj = JsonConvert.DeserializeObject<List<DeploymentCard>>( json );
			//set default thumbnail path and outline color
			foreach ( var item in obj )
			{
				item.mugShotPath = $"CardThumbnails/Stock{item.characterType}{item.id.GetDigits()}";
				item.deploymentOutlineColor = "LightBlue";
				if ( item.isElite || item.characterType == CharacterType.Villain )
					item.deploymentOutlineColor = "Red";//default is already Blue
				if ( item.characterType == CharacterType.Ally )
					item.deploymentOutlineColor = "Green";
			}

			return obj;
		}
		catch ( JsonException e )
		{
			Utils.LogError( $"LoadCards()::Error parsing {asset}.json\n{e.Message}" );
			throw e;
		}
	}

	static List<CardEvent> LoadEvents()
	{
		try
		{
			string json = Resources.Load<TextAsset>( "Languages/" + languageCodeList[languageCode] + "/events" ).text;
			return JsonConvert.DeserializeObject<EventList>( json ).events;
		}
		catch ( JsonReaderException e )
		{
			Utils.LogTranslationError( $"LoadTranslatedData()::Error parsing Events\n{e.Message}" );
			Utils.LogError( $"LoadTranslatedData()::Error parsing Events\n{e.Message}" );
			throw e;
		}
	}

	static List<CardInstruction> LoadInstructions()
	{
		try
		{
			string json = Resources.Load<TextAsset>( "Languages/" + languageCodeList[languageCode] + "/instructions" ).text;
			return JsonConvert.DeserializeObject<List<CardInstruction>>( json );
		}
		catch ( JsonReaderException e )
		{
			Utils.LogTranslationError( $"LoadTranslatedData() ERROR:\r\nError parsing Instructions\n{e.Message}" );
			Utils.LogError( $"LoadTranslatedData() ERROR:\r\nError parsing Instructions\n{e.Message}" );
			throw e;
		}
	}

	static List<BonusEffect> LoadBonusEffects()
	{
		try
		{
			string json = Resources.Load<TextAsset>( "Languages/" + languageCodeList[languageCode] + "/bonuseffects" ).text;
			return JsonConvert.DeserializeObject<List<BonusEffect>>( json );
		}
		catch ( JsonReaderException e )
		{
			Utils.LogTranslationError( $"LoadTranslatedData() ERROR:\r\nError parsing Bonus Effects\n{e.Message}" );
			Utils.LogError( $"LoadTranslatedData() ERROR:\r\nError parsing Bonus Effects\n{e.Message}" );
			throw e;
		}
	}

	static List<DeploymentSound> LoadDeploymentSounds()
	{
		string json = Resources.Load<TextAsset>( "sounds" ).text;
		return JsonConvert.DeserializeObject<List<DeploymentSound>>( json );
	}

	static void LoadMissionPresets()
	{
		//Core, Twin, Hoth, Bespin, Jabba, Empire, Lothal, Other
		string[] e = Enum.GetNames( typeof( Expansion ) );
		for ( int i = 0; i < e.Length; i++ )
		{
			string json = Resources.Load<TextAsset>( $"MissionPresets/{e[i]}" ).text;
			missionPresets.Add( e[i].ToLower(), JsonConvert.DeserializeObject<List<MissionPreset>>( json ) );
		}
	}

	static UILanguage LoadUILanguage()
	{
		try
		{
			string englishSource = Resources.Load<TextAsset>( "Languages/En/ui" ).text;
			string translationSource = Resources.Load<TextAsset>( "Languages/" + languageCodeList[languageCode] + "/ui" ).text;

			//inject the translation, which may have omissions (which are skipped), into the English source
			//this way, SOMETHING (English) will be shown, instead of empty strings
			var englishObject = JsonConvert.DeserializeObject<UILanguage>( englishSource );
			var translationObject = JsonConvert.DeserializeObject<UILanguage>( translationSource );
			var expectedProps = typeof( UILanguage ).GetFields();

			foreach ( var prop in expectedProps )
			{
				var englishValue = prop.GetValue( englishObject );//uiTitle, etc
				var transValue = prop.GetValue( translationObject );
				try
				{
					foreach ( var field in englishValue.GetType().GetFields() )//FieldInfo
					{
						if ( field.Name == "missionTypeStrings" )
							continue;

						object value = field.GetValue( transValue );
						//check for non-null and non-empty string values
						if ( value != null
							&& !(value is string && string.IsNullOrEmpty( (string)value )) )
						{
							field.SetValue( englishValue, value );
						}
						else
							Utils.LogTranslationError( $"LoadUILanguage()::Found missing UI translation [{Language}]: {field.Name}" );
					}
				}
				catch ( Exception ) {/* Debug.Log( $"{prop.Name}, {e.Message}" );*/ }
			}

			return englishObject;
		}
		catch ( JsonReaderException e )
		{
			Utils.LogTranslationError( $"LoadTranslatedData() ERROR:\r\nError parsing UI Language\n{e.Message}" );
			Utils.LogError( $"LoadTranslatedData() ERROR:\r\nError parsing UI Language\n{e.Message}" );
			throw e;
		}
	}

	static UIDeploymentGroups LoadDeploymentCardTranslations()
	{
		string asset = "";
		try
		{
			string enemies = Resources.Load<TextAsset>( $"Languages/{languageCodeList[languageCode]}/DeploymentGroups/enemies" ).text;
			string allies = Resources.Load<TextAsset>( $"Languages/{languageCodeList[languageCode]}/DeploymentGroups/allies" ).text;
			string villains = Resources.Load<TextAsset>( $"Languages/{languageCodeList[languageCode]}/DeploymentGroups/villains" ).text;
			string heroes = Resources.Load<TextAsset>( $"Languages/{languageCodeList[languageCode]}/DeploymentGroups/heroes" ).text;

			asset = "enemies";
			List<CardLanguage> enemyCards = JsonConvert.DeserializeObject<List<CardLanguage>>( enemies );
			asset = "allies";
			List<CardLanguage> allyCards = JsonConvert.DeserializeObject<List<CardLanguage>>( allies );
			asset = "villains";
			List<CardLanguage> villainCards = JsonConvert.DeserializeObject<List<CardLanguage>>( villains );
			asset = "heroes";
			List<CardLanguage> heroCards = JsonConvert.DeserializeObject<List<CardLanguage>>( heroes );

			return new UIDeploymentGroups() { allyCards = allyCards, villainCards = villainCards, heroCards = heroCards, enemyCards = enemyCards };
		}
		catch ( JsonReaderException e )
		{
			Utils.LogTranslationError( $"LoadCardTranslations({asset})::Error parsing Card Languages\n{e.Message}" );
			Utils.LogError( $"LoadCardTranslations({asset})::Error parsing Card Languages\n{e.Message}" );
			throw e;
		}
	}

	/// <summary>
	/// loads translations for ALL expansions
	/// </summary>
	static void LoadMissionCardTranslations()
	{
		string asset = "";
		translatedExpansionNames = new Dictionary<string, string>();
		try
		{
			for ( int i = 0; i < Enum.GetNames( typeof( Expansion ) ).Length; i++ )
			{
				asset = ((Expansion)i).ToString();
				string missions = Resources.Load<TextAsset>( $"Languages/{languageCodeList[languageCode]}/MissionCardText/{(Expansion)i}" ).text;
				var cards = JsonConvert.DeserializeObject<List<MissionCard>>( missions );
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

				if ( ((Expansion)i) != Expansion.Other )
					translatedExpansionNames.Add( ((Expansion)i).ToString(), missionCards[((Expansion)i).ToString()][0].expansionText );
				else
					translatedExpansionNames.Add( "Other", uiLanguage.uiCampaign.otherUC );
			}
		}
		catch ( JsonReaderException e )
		{
			Utils.LogTranslationError( $"LoadMissionCardTranslations({asset})::Error parsing Card Languages\n{e.Message}" );
			Utils.LogError( $"LoadMissionCardTranslations({asset})::Error parsing Card Languages\n{e.Message}" );
			throw e;
		}
	}

	public static void SetCardTranslations( List<DeploymentCard> toCards )
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
					//sanity check, will fail for imported characters (normal behavior when loading state)
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
					{
						Utils.LogTranslationError( $"SetCardTranslations()::langcard is null::{toCards[i].name}/{toCards[i].id}" );
						Debug.Log( $"SetCardTranslations()::langcard is null::{toCards[i].name}/{toCards[i].id}" );
						//Utils.LogError( $"SetCardTranslations()::langcard is null::{toCards[i].name}/{toCards[i].id}" );
					}
				}
			}
		}
		catch ( Exception e )
		{
			Utils.LogError( $"SetCardTranslations()::Error parsing card data\n{e.Message}" );
		}
	}

	static UIHelpOverlay LoadHelpOverlays()
	{
		try
		{
			//load the English backup
			string backupOverlay = Resources.Load<TextAsset>( $"Languages/EN/help" ).text;
			List<HelpOverlayPanel> backupList = JsonConvert.DeserializeObject<List<HelpOverlayPanel>>( backupOverlay );
			Debug.Log( $"Loaded English backup UI Help Overlay panels" );
			uiLanguage.uiHelpOverlayBackup = new UIHelpOverlay() { helpOverlayPanels = backupList.ToArray() };

			//try to load the translated version
			string helpOverlay = Resources.Load<TextAsset>( $"Languages/{languageCodeList[languageCode]}/help" ).text;
			List<HelpOverlayPanel> list = JsonConvert.DeserializeObject<List<HelpOverlayPanel>>( helpOverlay );
			Debug.Log( $"Found Help Overlays for {list.Count} Panels" );
			return new UIHelpOverlay() { helpOverlayPanels = list.ToArray() };
		}
		catch ( JsonException e )
		{
			Utils.LogTranslationError( $"LoadHelpOverlays()::Error parsing help.json\n{e.Message}" );
			Utils.LogError( $"LoadHelpOverlays()::Error parsing help.json\n{e.Message}" );
			throw e;
		}
		catch ( Exception e )
		{
			Utils.LogTranslationError( $"LoadHelpOverlays()::Error loading help.json\n{e.Message}" );
			Utils.LogError( $"LoadHelpOverlays()::Error loading help.json\n{e.Message}" );
			return uiLanguage.uiHelpOverlayBackup;//fallback to English if translation is missing or broken
		}
	}

	public static ThumbnailData LoadThumbnailData()
	{
		try
		{
			string json = Resources.Load<TextAsset>( "CardData/thumbnails" ).text;
			return JsonConvert.DeserializeObject<ThumbnailData>( json );
		}
		catch ( JsonReaderException e )
		{
			Utils.LogError( $"LoadTranslatedData()::Error parsing Bonus Effects\n{e.Message}" );
			throw e;
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

	/// <summary>
	/// When a Mission starts, adds Mission-embedded custom characters and associated data
	/// </summary>
	public static void AddEmbeddedImportsToPools( bool skipCard = false )
	{
		if ( mission != null )
		{
			foreach ( var item in mission.customCharacters )
			{
				if ( !skipCard )
				{
					Debug.Log( $"AddEmbeddedImportsToPools()::Adding embedded card: {item.cardName}::{item.cardID}" );
					//add non-villains
					if ( item.deploymentCard.characterType == CharacterType.Imperial )
					{
						item.deploymentCard.customCharacterGUID = item.customCharacterGUID;
						deploymentCards.Add( item.deploymentCard );
					}
					//add villains
					else if ( item.deploymentCard.characterType == CharacterType.Villain )
					{
						item.deploymentCard.customCharacterGUID = item.customCharacterGUID;
						villainCards.Add( item.deploymentCard );
					}
					//add allies and rebels
					else if ( item.deploymentCard.characterType == CharacterType.Ally || item.deploymentCard.characterType == CharacterType.Rebel )
					{
						item.deploymentCard.customCharacterGUID = item.customCharacterGUID;
						allyCards.Add( item.deploymentCard );
					}
					else if ( item.deploymentCard.characterType == CharacterType.Hero )
					{
						item.deploymentCard.customCharacterGUID = item.customCharacterGUID;
						heroCards.Add( item.deploymentCard );
					}
				}

				Debug.Log( $"AddEmbeddedImportsToPools()::Adding instructions/effects for {item.cardName}/{item.cardID}" );
				//activation instructions
				importedActivationInstructions.Add( item.cardInstruction );
				//bonus effects
				importedBonusEffects.Add( item.bonusEffect );
			}
		}
		else
			Debug.Log( "AddEmbeddedImportsToPools()::Mission is NULL, skipping" );
	}

	/// <summary>
	/// When a Mission starts, adds globally imported characters and associated data from current sesssion, skipCard upon restoring state
	/// </summary>
	public static void AddGlobalImportsToPools( bool skipCard = false )
	{
		//when restoring the game, skip adding card to pool since pool state is already restored (skipCard)
		//instructions and bonus effects for global imports are already added to their respective pools when the toons are imported (FileManager)
		foreach ( var item in sagaSessionData.globalImportedCharacters )
		{
			if ( !skipCard )
			{
				//only need to add Imperials because heroes/allies/villains are already added to their own special Lists
				//rebel types aren't needed because they aren't used as GLOBAL imports, only EMBEDDED inside Missions
				if ( item.characterType == CharacterType.Imperial )
				{
					Debug.Log( $"AddGlobalImportsToPools()::Adding embedded card: {item.name}::{item.customCharacterGUID}" );
					//item.deploymentCard.customCharacterGUID = item.customCharacterGUID;
					deploymentCards.Add( item );
				}
			}
		}
	}

	public static void CreateManualDeployment()
	{
		Debug.Log( "CreateManualDeployment()" );
		//filter owned expansions
		var available = deploymentCards
			.OwnedPlusOther()
			.ToList();
		//add all villains
		available = available.Concat( villainCards ).ToList();
		//filter out reserved/starting/earned villains
		available = available
			.MinusInDeploymentHand()
			.MinusReserved()
			.MinusStarting()
			.MinusEarnedVillains();

		//add any earned villains that didn't make it into the dep hand
		available.AddRange( villainsToManuallyAdd );
		//foreach ( var cd in villainsToManuallyAdd )
		//{
		//	Debug.Log( "TO ADD: " + cd.name );
		//}

		available.Sort( ( x, y ) =>
		 {
			 if ( double.Parse( x.id.GetDigits() ) == double.Parse( y.id.GetDigits() ) )
				 return 0;
			 else
				 return double.Parse( x.id.GetDigits() ) < double.Parse( y.id.GetDigits() ) ? -1 : 1;
		 } );

		manualDeploymentList = available.ToList();
		Debug.Log( $"MANUAL GROUP SIZE: {manualDeploymentList.Count} CARDS" );
	}

	public static void SortManualDeployList()
	{
		manualDeploymentList.Sort( ( x, y ) =>
		{
			if ( double.Parse( x.id.GetDigits() ) == double.Parse( y.id.GetDigits() ) )
				return 0;
			else
				return double.Parse( x.id.GetDigits() ) < double.Parse( y.id.GetDigits() ) ? -1 : 1;
		} );
	}

	public static void CreateDeploymentHand( List<DeploymentCard> EarnedVillains, int threatLevel )
	{
		var available = deploymentCards
			.OwnedPlusOther()
			.FilterByFaction()
			.MinusIgnored()
			.MinusStarting()
			.MinusReserved()
			.ToList();
		//Debug.Log( $"OF {deploymentCards.cards.Count} CARDS, USING {available.Count()}" );

		//add earned villains that are NOT reserved
		available = available.Concat( EarnedVillains.MinusReserved() ).ToList();
		//Debug.Log( $"ADD VILLAINS FILTERED TO {available.Count()} CARDS" );

		//filter available list by threat level and tier
		if ( threatLevel <= 3 )
			available = GetCardsByTier( available.ToList(), 2, 2, 0 );
		else if ( threatLevel == 4 )
			available = GetCardsByTier( available.ToList(), 1, 2, 1 );
		else if ( threatLevel >= 5 )
			available = GetCardsByTier( available.ToList(), 1, 2, 2 );

		//if there are any non-reserved earned villains and none were added to Hand above, "help" add one (50% chance)
		if ( EarnedVillains.MinusReserved().Count > 0
			&& !available.Any( x => EarnedVillains.MinusReserved().ContainsCard( x ) )
			&& GlowEngine.RandomBool() )
		{
			int[] rv = GlowEngine.GenerateRandomNumbers( EarnedVillains.Count );
			var v = EarnedVillains[rv[0]];
			available = available.Concat( new List<DeploymentCard>() { v } ).ToList();
			//add any remaining non-reserved earned villains back into manual deploy list
			foreach ( var cd in EarnedVillains.MinusReserved() )
			{
				if ( !available.ContainsCard( cd ) )
					villainsToManuallyAdd.Add( cd );
			}
			//Debug.Log( $"ADDED A VILLAIN (50%): {v.name}" );
		}
		else
		{
			//if no non-reserved earned villain was added to Hand or by 50% chance above, add it to manual deployment list
			foreach ( var cd in EarnedVillains.MinusReserved() )
			{
				if ( !available.ContainsCard( cd ) )
				{
					//Debug.Log( "VILLAIN *NOT* ADDED TO DH: " + cd.name );
					villainsToManuallyAdd.Add( cd );
				}
			}
		}

		Debug.Log( $"DEPLOYMENT HAND SIZE: {available.Count()} CARDS" );
		for ( int i = 0; i < available.Count(); i++ )
		{
			Debug.Log( $"DEPLOYMENT HAND::{available.ElementAt( i ).name}::{available.ElementAt( i ).id}" );
		}
		//finally, create the Hand from the filtered list
		deploymentHand = available.ToList();
	}

	/// <summary>
	/// Classic mode
	/// </summary>
	public static bool LoadState()
	{
		string json = "";
		try
		{
			//deployment hand
			string path = Path.Combine( FileManager.classicSessionPath, "deploymenthand.json" );
			using ( StreamReader sr = new StreamReader( path ) )
			{
				json = sr.ReadToEnd();
			}
			deploymentHand = JsonConvert.DeserializeObject<List<DeploymentCard>>( json );

			//manual deployment deck
			path = Path.Combine( FileManager.classicSessionPath, "manualdeployment.json" );
			using ( StreamReader sr = new StreamReader( path ) )
			{
				json = sr.ReadToEnd();
			}
			manualDeploymentList = JsonConvert.DeserializeObject<List<DeploymentCard>>( json );

			//deployed enemies
			path = Path.Combine( FileManager.classicSessionPath, "deployedenemies.json" );
			using ( StreamReader sr = new StreamReader( path ) )
			{
				json = sr.ReadToEnd();
			}
			deployedEnemies = JsonConvert.DeserializeObject<List<DeploymentCard>>( json );

			//deployed heroes
			path = Path.Combine( FileManager.classicSessionPath, "heroesallies.json" );
			using ( StreamReader sr = new StreamReader( path ) )
			{
				json = sr.ReadToEnd();
			}
			deployedHeroes = JsonConvert.DeserializeObject<List<DeploymentCard>>( json );

			//remaining events
			path = Path.Combine( FileManager.classicSessionPath, "events.json" );
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
			cardEvents = (from ev in LoadEvents() join ev2 in cardEvents on ev.eventID equals ev2.eventID select ev).ToList();

			return true;
		}
		catch ( Exception e )
		{
			Utils.LogError( "LoadState()::" + e.Message );
			return false;
		}
	}

	/// <summary>
	/// Randomly gets the requested number of cards according to tier
	/// </summary>
	static List<DeploymentCard> GetCardsByTier( List<DeploymentCard> haystack, int t1, int t2, int t3 )
	{
		List<DeploymentCard> retval = new List<DeploymentCard>();
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
	/// Includes globally imported heroes at app startup
	/// </summary>
	public static DeploymentCard GetHero( string id )
	{
		return heroCards.Concat( globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Hero ).Select( x => x.deploymentCard ) ).FirstOrDefault( x => x.id == id ) ?? null;
	}

	/// <summary>
	/// Includes globally imported heroes (not opt-in)
	/// </summary>
	//public static DeploymentCard GetAlly( string id )
	//{
	//	return allyCards.Concat( globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Ally ).Select( x => x.deploymentCard ) ).First( x => x.id == id ) ?? null;
	//}

	/// <summary>
	/// Get a normal or villain from the id
	/// </summary>
	public static DeploymentCard GetEnemy( string id )
	{
		//also search hero and villain global imports
		var imports = globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Imperial || x.deploymentCard.characterType == CharacterType.Villain ).Select( x => x.deploymentCard );

		if ( villainCards.Concat( imports ).Any( x => x.id == id ) )
			return villainCards.Where( x => x.id == id ).FirstOr( null );
		else if ( deploymentCards.Concat( imports ).Any( x => x.id == id ) )
			return deploymentCards.Concat( imports ).Where( x => x.id == id ).FirstOr( null );
		else
			return null;
	}

	/// <summary>
	/// CAN be in dep hand, minus deployed, minus reserved, minus ignored
	/// </summary>
	public static DeploymentCard GetNonEliteVersion( DeploymentCard elite )
	{
		//starting groups already deployed, no need to filter
		//1) filter to NON elites only
		//2) the elite version of the card (passed into this method) will have the NON elite NAME in its name property
		var valid = deploymentCards
			.Where( x => !x.isElite )
			.Where( x => elite.name.ToLowerInvariant().Contains( x.name.ToLowerInvariant() ) ).ToList()
			.MinusDeployed()
			.MinusReserved()
			.MinusIgnored();
		return valid.FirstOr( null );
	}

	/// <summary>
	/// CAN be in dep hand, minus deployed, minus reserved, minus ignored
	/// </summary>
	public static DeploymentCard GetEliteVersion( DeploymentCard cd )
	{
		//starting groups already deployed, no need to filter
		//1) filter to elites only
		//2) the elite version of the card will have the NAME in its name property
		var valid = deploymentCards
			.Where( x => x.isElite )
			.Where( x => x.name.ToLowerInvariant().Contains( cd.name.ToLowerInvariant() ) ).ToList()
			.MinusDeployed()
			.MinusReserved()
			.MinusIgnored();
		return valid.FirstOr( null );
	}

	/// <summary>
	/// Calculate and return a valid reinforcement, optionally applying a -1 rcost modifier for Onslaught, if Saga game also checks override if it CAN reinforce
	/// </summary>
	public static DeploymentCard GetReinforcement( int currentThreat, bool isOnslaught = false )
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
			x.rcost >= 0 &&
			x.currentSize < x.size &&
			Math.Max( 1, x.rcost - costModifier ) <= currentThreat ).ToList();

		if ( gameType == GameType.Saga )
		{
			//check for "canReinforce" override and remove those cards if they can't
			for ( int i = valid.Count - 1; i >= 0; i-- )
			{
				var ovrd = sagaSessionData.gameVars.GetDeploymentOverride( valid[i].id );
				if ( ovrd != null && !ovrd.canReinforce )
				{
					Debug.Log( "GetReinforcement()::SKIPPING CANNOT REINFORCE::" + ovrd.ID + "::" + ovrd.nameOverride );
					valid.RemoveAt( i );
				}
			}
		}

		if ( valid.Count > 0 )
		{
			int[] rnd = GlowEngine.GenerateRandomNumbers( valid.Count );
			//Debug.Log( "GET: " + valid[rnd[0]].currentSize );
			return valid[rnd[0]];
		}

		return null;
	}

	/// <summary>
	/// Calculate and return a deployable group from the hand using "fuzzy" deployment, DOES NOT remove it from deployment hand, if Saga game also checks override if it CAN redeploy
	/// </summary>
	public static DeploymentCard GetFuzzyDeployable( int currentThreat, bool isOnslaught = false )
	{
		/*
		 If the app chooses to deploy a Tier III (=expensive) group, but does not have enough threat by up to 3 points, it still deploys the unit and reduces threat to 0. This way, the deployment of expensive units does not hinge on a tiny amount of missing threat, but doesn’t simply make them cheaper. Example: The app chooses to deploy an AT-ST (threat cost 14). It can deploy even if there is only 11, 12, or 13 threat left
		*/

		List<DeploymentCard> tier1Group = new List<DeploymentCard>();
		List<DeploymentCard> tier2Group = new List<DeploymentCard>();
		DeploymentCard tier3Group = null;
		List<DeploymentCard> tier23Group = new List<DeploymentCard>();
		DeploymentCard validEnemy = null;
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
			x.cost <= currentThreat ) )
		{
			tier1Group = deploymentHand.Where( x => x.tier == 1 && x.cost <= currentThreat ).ToList();
		}
		//check for "canRedeploy" override and remove those cards if they can't
		//this might be redundant now that exhausting an enemy will not add it back to the hand anyways
		//if ( gameType == GameType.Saga && tier1Group.Count > 0 )
		//{
		//	for ( int i = tier1Group.Count - 1; i >= 0; i-- )
		//	{
		//		var ovrd = sagaSessionData.gameVars.GetDeploymentOverride( tier1Group[i].id );
		//		if ( ovrd != null && tier1Group[i].hasDeployed && !ovrd.canRedeploy )
		//			tier1Group.RemoveAt( i );
		//	}
		//}

		//get tier 2 affordable groups
		if ( deploymentHand.Any( x =>
			x.tier == 2 &&
			x.cost - t2modifier <= currentThreat ) )
		{
			tier2Group = deploymentHand.Where( x =>
			x.tier == 2 &&
			x.cost - t2modifier <= currentThreat )
			.ToList();
		}
		//check for "canRedeploy" override and remove those cards if they can't
		//if ( gameType == GameType.Saga && tier2Group.Count > 0 )
		//{
		//	for ( int i = tier2Group.Count - 1; i >= 0; i-- )
		//	{
		//		var ovrd = sagaSessionData.gameVars.GetDeploymentOverride( tier2Group[i].id );
		//		if ( ovrd != null && tier2Group[i].hasDeployed && !ovrd.canRedeploy )
		//			tier2Group.RemoveAt( i );
		//	}
		//}

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
				x.cost - t3modifier <= currentThreat + 3 &&
				!deployedEnemies.ContainsCard( x )
		) )
		{
			var t3 = deploymentHand.Where( x =>
				x.tier == 3 &&
				x.cost - t3modifier <= currentThreat + 3 &&
				!deployedEnemies.ContainsCard( x )
			).ToList();

			//check for "canRedeploy" override and remove those cards if they can't
			//if ( gameType == GameType.Saga && t3.Count > 0 )
			//{
			//	for ( int i = t3.Count - 1; i >= 0; i-- )
			//	{
			//		var ovrd = sagaSessionData.gameVars.GetDeploymentOverride( t3[i].id );
			//		if ( ovrd != null && t3[i].hasDeployed && !ovrd.canRedeploy )
			//			t3.RemoveAt( i );
			//	}
			//}

			if ( t3.Count > 0 )
			{
				rnd = GlowEngine.GenerateRandomNumbers( t3.Count );
				tier3Group = t3[rnd[0]];
			}
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

	/// <summary>
	/// 'id' is case insensitive and can optionally contain a space between the expansion and #, such as 'core 1'
	/// </summary>
	public static MissionCard GetMissionCard( string id )
	{
		if ( id != null && id != "Custom" )
		{
			foreach ( var item in missionCards.Keys )
			{
				foreach ( var card in missionCards[item] )
				{
					if ( card.id.ToLower() == id.ToLower().Replace( " ", "" ) )
						return card;
				}
			}
		}

		return null;
	}

	public static CardInstruction GetActivationInstruction( string id )
	{
		var list = activationInstructions.Concat( importedActivationInstructions );
		return list.Where( x => x.instID.ToLower() == id.ToLower() ).FirstOr( null );
	}

	public static BonusEffect GetBonusEffect( string id )
	{
		var list = bonusEffects.Concat( importedBonusEffects );
		return list.Where( x => x.bonusID.ToLower() == id.ToLower() ).FirstOr( null );
	}

	public static void SetDefaultPlayerPrefs()
	{
		if ( !PlayerPrefs.HasKey( "music" ) )
			PlayerPrefs.SetInt( "music", 1 );
		if ( !PlayerPrefs.HasKey( "sound" ) )
			PlayerPrefs.SetInt( "sound", 1 );
		if ( !PlayerPrefs.HasKey( "bloom2" ) )
			PlayerPrefs.SetInt( "bloom2", 0 );
		if ( !PlayerPrefs.HasKey( "vignette" ) )
			PlayerPrefs.SetInt( "vignette", 1 );
		if ( !PlayerPrefs.HasKey( "language" ) )
			PlayerPrefs.SetInt( "language", 0 );
		if ( !PlayerPrefs.HasKey( "ambient" ) )
			PlayerPrefs.SetInt( "ambient", 1 );
		if ( !PlayerPrefs.HasKey( "zoombuttons" ) )
			PlayerPrefs.SetInt( "zoombuttons", 0 );
		if ( !PlayerPrefs.HasKey( "viewToggle" ) )
			PlayerPrefs.SetInt( "viewToggle", 0 );
		if ( !PlayerPrefs.HasKey( "closeWindowToggle" ) )
		{
			//default off (0) for Android
#if UNITY_ANDROID
			PlayerPrefs.SetInt( "closeWindowToggle", 0 );
#else
			PlayerPrefs.SetInt( "closeWindowToggle", 1 );
#endif
		}
		if ( !PlayerPrefs.HasKey( "excludedImports" ) )
			PlayerPrefs.SetString( "excludedImports", "" );
		if ( !PlayerPrefs.HasKey( "musicVolume" ) )//1-10
			PlayerPrefs.SetInt( "musicVolume", 5 );
		if ( !PlayerPrefs.HasKey( "ambientVolume" ) )//1-10
			PlayerPrefs.SetInt( "ambientVolume", 5 );
		if ( !PlayerPrefs.HasKey( "soundVolume" ) )//1-10
			PlayerPrefs.SetInt( "soundVolume", 7 );
		if ( !PlayerPrefs.HasKey( "roundLimitToggle" ) )
			PlayerPrefs.SetInt( "roundLimitToggle", 1 );
		if ( !PlayerPrefs.HasKey( "useFullScreen" ) )
			PlayerPrefs.SetInt( "useFullScreen", 1 );//1=true, 0=false
		if ( !PlayerPrefs.HasKey( "skipWarpIntro" ) )
			PlayerPrefs.SetInt( "skipWarpIntro", 0 );//1=true, 0=false
		if ( !PlayerPrefs.HasKey( "defaultRegularEnemyColor1" ) )
			PlayerPrefs.SetInt( "defaultRegularEnemyColor1", 0 );//0=grey
		if ( !PlayerPrefs.HasKey( "defaultRegularEnemyColor2" ) )
			PlayerPrefs.SetInt( "defaultRegularEnemyColor2", 0 );//0=grey
		if ( !PlayerPrefs.HasKey( "defaultEliteEnemyColor1" ) )
			PlayerPrefs.SetInt( "defaultEliteEnemyColor1", 0 );//0=grey
		if ( !PlayerPrefs.HasKey( "defaultEliteEnemyColor2" ) )
			PlayerPrefs.SetInt( "defaultEliteEnemyColor2", 0 );//0=grey
		if ( !PlayerPrefs.HasKey( "defaultVillainColor" ) )
			PlayerPrefs.SetInt( "defaultVillainColor", 0 );//0=grey
	}
}
