using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Saga
{
	//story missions are restricted to the selected expansion
	//side missions can be freely selected

	public class SagaCampaign
	{
		//campaign state
		public int formatVersion;
		//campaignExpansionCode is the CODE, ie: Core
		public string campaignName, campaignExpansionCode, campaignImportedName;
		public string campaignJournal = "";
		//XP is no longer used, XP is stored per Hero in CampaignHero
		public int XP, credits, fame, awards;
		public Guid GUID = Guid.Empty;
		public List<CampaignHero> campaignHeroes = new List<CampaignHero>();
		public List<CampaignStructure> campaignStructure = new List<CampaignStructure>();
		public List<DeploymentCard> campaignVillains = new List<DeploymentCard>();
		public List<DeploymentCard> campaignAllies = new List<DeploymentCard>();
		//store the ID
		public List<string> campaignItems = new List<string>();
		public List<string> campaignRewards = new List<string>();
		public List<string> campaignIgnored = new List<string>();
		//public bool isImported, isCustom;
		public CampaignType campaignType;
		public CampaignPackage campaignPackage;

		// TACTICAL AI (M12b): Imperial class deck of the campaign - null is the
		// VALID "kein Deck" state (older saves load as null; non-tactical
		// campaigns never store AI-owned deck data). ADDITIVE field only:
		// formatVersion STAYS 2 so every older save still loads (and still
		// passes the FileManager/ContinueCampaignPanel format filter).
		public ImperialDeckState imperialDeck;

		// TACTICAL AI (M12d2): Agenda deck of the campaign (RR "Building the
		// Agenda Deck" + "Influence") - null is the VALID 'kein Agenda-State'
		// base state: older saves load as null; non-tactical campaigns never
		// store agenda data. ADDITIVE field only, formatVersion STAYS 2.
		public AgendaState agenda;

		//data sets
		[JsonIgnore]
		public static List<CampaignItem> campaignDataItems;
		[JsonIgnore]
		public static List<CampaignSkill> campaignDataSkills;
		[JsonIgnore]
		public static List<CampaignStructure> campaignDataStructures;
		[JsonIgnore]
		public static List<CampaignReward> campaignDataRewards;

		/// <summary>
		/// use CreateNewCampaign() to create new campaigns
		/// </summary>
		public SagaCampaign()
		{
		}

		public static SagaCampaign CreateNewCampaign( string cname, string expansionCode )
		{
			SagaCampaign c = new SagaCampaign();
			c.formatVersion = 2;
			c.GUID = Guid.NewGuid();
			c.campaignName = cname;
			c.campaignExpansionCode = expansionCode;
			c.campaignType = expansionCode == "Custom" ? CampaignType.Custom : CampaignType.Official;

			c.LoadCampaignData();
			//now campaign structures are set for this campaign's expansion
			c.campaignStructure = campaignDataStructures;

			return c;
		}

		public static SagaCampaign CreateNewImportedCampaign( string cname, CampaignPackage package )
		{
			SagaCampaign c = new SagaCampaign();
			c.formatVersion = 2;
			c.GUID = Guid.NewGuid();
			c.campaignName = cname;
			c.campaignExpansionCode = "Imported";
			c.campaignType = CampaignType.Imported;
			c.campaignPackage = package;
			c.campaignImportedName = package.campaignName;
			c.campaignStructure = package.campaignStructure;

			c.LoadCampaignData();
			return c;
		}

		/// <summary>
		/// For existing campaigns that are loaded, fix the expansion code for "Other" missions which have their "expansionCode" property incorrectly set to the Campaign's expansion code
		/// </summary>
		public void FixExpansionCodes()
		{
			//Doesn't seem to be necessary anymore?
			//official campaigns containing "Other" missions retain their "Other" expansionCode just fine

			//for ( int i = 0; i < campaignStructures.Count; i++ )
			//{
			//	campaignStructure[i].expansionCode = campaignStructures[i].expansionCode;
			//}
		}

		/// <summary>
		/// set the translated card data for lists
		/// onlyOfficial = true when loading state to translate text
		/// custom and imported mission data don't get translated, so skip them on loading state
		/// </summary>
		public void LoadCampaignData( bool onlyOfficial = false )
		{
			try
			{
				//items
				campaignDataItems = LoadAsset<List<CampaignItem>>( $"Languages/{DataStore.Language}/CampaignData/items" );

				//skills
				campaignDataSkills = LoadAsset<List<CampaignSkill>>( $"Languages/{DataStore.languageCodeList[DataStore.languageCode]}/CampaignData/skills" );

				//rewards
				campaignDataRewards = LoadAsset<List<CampaignReward>>( $"Languages/{DataStore.Language}/CampaignData/rewards" );

				//mission structure
				if ( campaignType == CampaignType.Official )//official campaign
				{
					campaignDataStructures = LoadAsset<List<CampaignStructure>>( $"CampaignData/{campaignExpansionCode}" );
					//if the mission ID is already set, make it NOT selectable in the future
					campaignDataStructures = campaignDataStructures.Select( x =>
					{
						x.missionSource = MissionSource.None;
						if ( !string.IsNullOrEmpty( x.missionID ) )
						{
							x.missionSource = MissionSource.Official;
							x.canModify = false;
						}
						return x;
					} ).ToList();
				}
				else if ( !onlyOfficial && campaignType == CampaignType.Custom )
				{
					campaignDataStructures = new List<CampaignStructure>
					{
						new CampaignStructure()
						{
							missionType = MissionType.Introduction,
							missionID = "",
							threatLevel = 1,
							expansionCode = "",
							isAgendaMission = false,
							missionSource= MissionSource.None,
							itemTier = new string[] { "1" }
						}
					};
				}
				else if ( !onlyOfficial && campaignType == CampaignType.Imported )//custom campaign package
				{
					campaignDataStructures = campaignStructure;
					campaignDataStructures = campaignDataStructures.Select( x =>
					{
						Guid guid = Guid.Parse( x.missionID );
						x.canModify = guid == Guid.Empty;
						x.packageGUID = campaignPackage.GUID;
						return x;
					} ).ToList();
				}
			}
			catch ( JsonReaderException e )
			{
				Debug.Log( $"LoadCampaignData() ERROR:\r\nError parsing Campaign data" );
				Debug.Log( e.Message );
				throw new Exception();
			}
		}

		//public static MissionPreset GetMissionPreset( Expansion expansion, string expCode )
		//{
		//	var presets = LoadAsset<List<MissionPreset>>( $"MissionPresets/{expansion}" );
		//	return presets.Where( x => x.id == expCode.ToLower() ).FirstOr( null );
		//}

		/// <summary>
		/// load the actual Campaign state
		/// </summary>
		public static SagaCampaign LoadCampaignState( Guid guid )
		{
			string json = "";
			try
			{
				string path = Path.Combine( FileManager.campaignPath, $"{guid}.json" );
				using ( StreamReader sr = new StreamReader( path ) )
				{
					json = sr.ReadToEnd();
				}
				var state = JsonConvert.DeserializeObject<SagaCampaign>( json );
				//skip loading custom/import structure template, only translate official Mission data
				state.LoadCampaignData( true );

				// TACTICAL AI (AUTO rework dev 2): campaign saves from before the
				// AUTO badge stored "legacy" as an untouched default, which would
				// silently outvote the global AI MODE. Nobody could have chosen
				// legacy deliberately yet (the per-mission switch is new), so
				// normalize it to "" = AUTO (follow the global setting).
				if ( state.campaignStructure != null )
					foreach ( var cs in state.campaignStructure )
						if ( cs.aiMode == "legacy" ) cs.aiMode = "";

				// TACTICAL AI (M12b): load-backfill - a tactical-mode campaign
				// without deck data re-derives the choice from seed + profile
				// (pure, mission-independent; two loads -> bit-identical). In
				// memory ONLY: the file is not rewritten here, the next regular
				// SaveCampaignState persists it. Non-tactical: nothing created.
				if ( state.imperialDeck == null && TacticalAI.TacticalAISettings.Enabled )
				{
					try { state.EnsureImperialDeck(); }
					catch ( Exception e ) { Debug.LogWarning( "SagaCampaign::imperial deck backfill failed " + e.Message ); }
				}

				// TACTICAL AI (M12c): the Imperial Upgrade Stage runs HERE -
				// mission end lands back on this scene (SagaController.
				// EndMission -> Campaign -> this load), the next Einsatzkarte
				// comes later, so 'nach Missionsende-Wertung, vor der
				// naechsten Einsatzkarte' is exactly this point. Purchases
				// are greedy-deterministic (seed + profile + owned + XP
				// account) and in-memory like the M12b backfill: the next
				// SaveCampaignState persists them, a re-load before that
				// replays the identical stage.
				if ( state.imperialDeck != null && TacticalAI.TacticalAISettings.Enabled )
				{
					try { state.RunImperialUpgradeStage(); }
					catch ( Exception e ) { Debug.LogWarning( "SagaCampaign::imperial upgrade stage failed " + e.Message ); }
				}

				// TACTICAL AI (M12d2): agenda deck backfill + the agenda half
				// of the Imperial Upgrade Stage (RR order: "first ... XP ...
				// then influence" -> runs AFTER the class-card stage above).
				// Same in-memory doctrine; the market is ONE draw per stage
				// and fully re-derivable from seed + stage pointer.
				if ( state.agenda == null && TacticalAI.TacticalAISettings.Enabled )
				{
					try { state.EnsureAgendaDeck(); }
					catch ( Exception e ) { Debug.LogWarning( "SagaCampaign::agenda deck backfill failed " + e.Message ); }
				}
				if ( state.agenda != null && TacticalAI.TacticalAISettings.Enabled )
				{
					try { state.RunAgendaUpgradeStage(); }
					catch ( Exception e ) { Debug.LogWarning( "SagaCampaign::agenda upgrade stage failed " + e.Message ); }
				}

				Debug.Log( "***CAMPAIGN LOADED***" );
				return state;
			}
			catch ( Exception e )
			{
				Utils.LogError( "LoadCampaignState()::" + e.Message );
				return null;
			}
		}

		public void SaveCampaignState( List<CampaignStructure> structures = null )
		{
			Debug.Log( "SaveCampaignState()::SAVING CAMPAIGN" );

			// TACTICAL AI (M12b): tactical campaigns persist the KI deck choice
			// (deck + score tuple) with the campaign; non-tactical saves never
			// gain AI-owned deck data (Scope 5: 'kein Deck' is a valid state).
			if ( imperialDeck == null && TacticalAI.TacticalAISettings.Enabled )
			{
				try { EnsureImperialDeck(); }
				catch ( Exception e ) { Debug.LogWarning( "SaveCampaignState()::imperial deck choice failed " + e.Message ); }
			}
			// M12d2: same additive rule for the agenda deck.
			if ( agenda == null && TacticalAI.TacticalAISettings.Enabled )
			{
				try { EnsureAgendaDeck(); }
				catch ( Exception e ) { Debug.LogWarning( "SaveCampaignState()::agenda deck choice failed " + e.Message ); }
			}

			if ( structures != null )
				campaignStructure = structures;

			try
			{
				if ( GUID == Guid.Empty )
					throw new Exception( "Campaign GUID is Empty" );

				string output = JsonConvert.SerializeObject( this, Formatting.Indented );
				string outpath = Path.Combine( FileManager.campaignPath, $"{GUID}.json" );
				using ( var stream = File.CreateText( outpath ) )
				{
					stream.Write( output );
				}

				Debug.Log( "***CAMPAIGN SAVED***" );
			}
			catch ( Exception e )
			{
				Utils.LogError( "SaveCampaignState()::" + e.Message );
			}
		}

		public static T LoadAsset<T>( string assetName )
		{
			try
			{
				string json = Resources.Load<TextAsset>( assetName ).text;
				return JsonConvert.DeserializeObject<T>( json );
			}
			catch ( JsonReaderException e )
			{
				Debug.Log( $"LoadCampaignData() ERROR:\r\nError parsing Campaign data [{assetName}]" );
				Debug.Log( e.Message );
				throw e;
			}
		}

		public CampaignItem GetItemFromID( string id ) => campaignDataItems.FirstOrDefault( x => x.id == id );
		public CampaignSkill GetSkillFromID( string id ) => campaignDataSkills.FirstOrDefault( x => x.id == id );
		public CampaignReward GetRewardFromID( string id ) => campaignDataRewards.FirstOrDefault( x => x.id == id );

		public string GetCampaignInfo()
		{
			if ( campaignType == CampaignType.Imported )//custom campaign package
			{
				//check if there's a translation first
				string instructions = FileManager.LoadEmbeddedCampaignInstructions( campaignPackage.GUID );
				if ( !string.IsNullOrEmpty( instructions ) )
					return instructions;
				else
					return campaignPackage.campaignInstructions;
			}
			else if ( campaignType == CampaignType.Custom )
			{
				return "";
			}
			else
			{
				string text = Resources.Load<TextAsset>( $"Languages/{DataStore.Language}/CampaignData/CampaignInfo/{campaignExpansionCode}Info" ).text;
				return text != null ? text : string.Empty;
			}
		}

		public void SetNextStoryMission( string customMissionID, MissionSource source )
		{
			Debug.Log( $"SetNextStoryMission()::{customMissionID}" );

			//'source' is determined by whether the "next mission" is Custom or Official

			try
			{
				if ( GUID == Guid.Empty )
					throw new Exception( "Campaign GUID is Empty" );
				if ( string.IsNullOrEmpty( customMissionID ) )
					throw new Exception( "Custom Mission ID is Empty" );

				//get the index of the current mission in the structure list
				int idx = campaignStructure.FindIndex( x => x.missionID.ToLower() == DataStore.sagaSessionData.setupOptions.projectItem.missionID.ToLower() );
				if ( idx != -1 )
				{
					//find the next Story/Finale mission AFTER the current Mission in the structure list
					for ( int index = idx + 1; index < campaignStructure.Count; index++ )
					{
						if ( campaignStructure[index].missionType == MissionType.Story
							|| campaignStructure[index].missionType == MissionType.Finale )
						{
							if ( source == MissionSource.Embedded )
							{
								var m = campaignPackage.campaignMissionItems.Where( x => x.customMissionIdentifier.ToLower() == customMissionID.ToLower() ).FirstOr( null );
								if ( m != null )
								{
									Debug.Log( $"campaignStructure with Index={index} changed" );
									campaignStructure[index].missionSource = MissionSource.Embedded;
									campaignStructure[index].missionID = m.missionGUID.ToString();
									campaignStructure[index].projectItem.Title = m.missionName;
									campaignStructure[index].projectItem.missionGUID = m.missionGUID.ToString();

									var currentCampaign = FileManager.importedCampaigns.FirstOrDefault( x => x.campaignName == RunningCampaign.sagaCampaign.campaignImportedName );
									var selectedMission = currentCampaign.campaignMissionItems.FirstOrDefault( x => x.missionGUID == m.missionGUID );
									campaignStructure[index].projectItem.Description = selectedMission.mission.missionProperties.missionDescription;
									campaignStructure[index].projectItem.AdditionalInfo = selectedMission.mission.missionProperties.additionalMissionInfo;

									break;
								}
								else
									Debug.Log( $"WARNING::SetNextStoryMission()::Couldn't find [Embedded] Mission with customMissionIdentifier={customMissionID}" );
							}
							else//official Mission
							{
								var m = DataStore.GetMissionCard( customMissionID );
								if ( m != null )
								{
									Debug.Log( $"campaignStructure with Index={index} changed" );
									campaignStructure[index].missionSource = MissionSource.Official;
									campaignStructure[index].expansionCode = Utils.ParseExpansionName( customMissionID );
									campaignStructure[index].missionID = customMissionID;
									break;
								}
								else
									Debug.Log( $"WARNING::SetNextStoryMission()::Couldn't find [Official] Mission with customMissionIdentifier={customMissionID}" );
							}
						}
						else
							Debug.Log( $"WARNING::SetNextStoryMission()::Couldn't find a STORY Mission that follows this Mission in the Campaign structure: {DataStore.sagaSessionData.setupOptions.projectItem.missionID}" );
					}
				}
				else
				{
					Debug.Log( "WARNING::SetNextStoryMission()::Couldn't find this Mission in the Campaign Mission structure" );
				}
			}
			catch ( Exception e )
			{
				Utils.LogWarning( $"SetNextStoryMission()::ERROR\n{e.Message}" );
			}
		}

		public void ModifyNextMissionThreatLevel( int level )
		{
			Debug.Log( $"ModifyNextMissionThreatLevel()::Modify Threat Level by: {level}" );

			try
			{
				//get the index of the current mission in the structure list
				int idx = campaignStructure.FindIndex( x => x.missionID.ToLower() == DataStore.sagaSessionData.setupOptions.projectItem.missionID.ToLower() );
				if ( idx != -1 )
				{
					//find the next mission (any) AFTER the current Mission in the structure list
					if ( idx + 1 < campaignStructure.Count )
					{
						campaignStructure[idx + 1].threatLevel = Math.Max( 0, campaignStructure[idx + 1].threatLevel + level );
						campaignStructure[idx + 1].threatModifiedByMission = true;
						Debug.Log( $"ModifyNextMissionThreatLevel()::New Threat Level for [Mission index {idx + 1}] Set to [{campaignStructure[idx + 1].threatLevel}]" );
					}
					else
						Debug.Log( "ModifyNextMissionThreatLevel()::There are no Missions after this one in the Campaign Mission structure" );
				}
				else
				{
					Debug.Log( "WARNING::SetNextStoryMission()::Couldn't find this Mission in the Campaign Mission structure" );
				}
			}
			catch ( Exception e )
			{
				Utils.LogWarning( $"ModifyNextMissionThreatLevel()::ERROR\n{e.Message}" );
			}
		}

		// ------------------------------------------------------------ M12b deck

		/// <summary>Campaign profile for the deck choice - the SAME mapping the
		/// canon fixture pins through ImperialCampaignProfile.ProfileFromCampaignSave
		/// (heroes = counted heroIDs, agenda = isAgendaMission OR missionType
		/// Agenda). Enrichment over the pure save data: villain families get the
		/// app's OWN trait table (DataStore.deploymentCards by id, fallback by
		/// name) on top of the printed-name tokens, because saves carry NO
		/// traits ([JsonIgnore]). Resolution runs against static build content,
		/// so it idempotents across loads (determinism rule).</summary>
		public TacticalAI.Core.ImperialCampaignProfile BuildImperialProfile()
		{
			var profile = new TacticalAI.Core.ImperialCampaignProfile();
			if ( campaignHeroes != null )
				foreach ( var h in campaignHeroes )
					if ( h != null && !string.IsNullOrEmpty( h.heroID ) )
						profile.HeroCount++;

			if ( campaignVillains != null )
				foreach ( var v in campaignVillains )
				{
					if ( v == null ) continue;
					var families = new HashSet<string>( StringComparer.Ordinal );
					foreach ( var kv in TacticalAI.Core.ImperialDecks.FamilyCounts( v.name ) )
						families.Add( kv.Key );
					var full = DataStore.deploymentCards?.Find( x => x.id == v.id )
					          ?? DataStore.deploymentCards?.Find( x => x.name == v.name );
					if ( full?.traits != null )
						foreach ( var trait in full.traits )
						{
							var fam = TacticalAI.Core.ImperialDecks.FamilyFromTrait( trait.ToString() );
							if ( fam != null ) families.Add( fam );
						}
					foreach ( var fam in families )
					{
						profile.VillainFamilies.TryGetValue( fam, out int n );
						profile.VillainFamilies[fam] = n + 1;
					}
				}

			if ( campaignStructure != null )
				foreach ( var cs in campaignStructure )
				{
					profile.MissionCount++;
					if ( cs.isAgendaMission )
						profile.AgendaMissionCount++;   // Saga.MissionType has NO Agenda member - the flag IS the signal
				}

			// Collection gate (Noldorion PR #194 review 2.11): the boxes the
			// player activated (DataStore.InitData reads them from PlayerPrefs;
			// Core is always in). An empty list is impossible by app design,
			// but stays NULL-honest: no gate rather than a wrong gate.
			if ( DataStore.ownedExpansions != null && DataStore.ownedExpansions.Count > 0 )
			{
				profile.OwnedExpansions = new HashSet<string>();
				foreach ( var e in DataStore.ownedExpansions )
					profile.OwnedExpansions.Add( e.ToString() );
			}
			return profile;
		}

		/// <summary>Pure deterministic choice (preview): campaign seed + profile,
		/// nothing stored. Non-tactical UIs may show this without ever touching
		/// the campaign store. Null when the deck asset is unavailable.</summary>
		public TacticalAI.Core.ImperialDeckChoice PreviewImperialDeck()
		{
			try
			{
				if ( TacticalAI.Core.ImperialDecks.DocOrNull() == null &&
					 TacticalAI.TacticalAIAssets.ImperialDecksRoot == null )
					return null;
				var seed = TacticalAI.Core.ImperialDecks.DeckSeedFor( GUID.ToString() );
				return TacticalAI.Core.ImperialDecks.Choose( BuildImperialProfile(), seed );
			}
			catch ( Exception e )
			{
				Debug.LogWarning( "SagaCampaign::PreviewImperialDeck failed " + e.Message );
				return null;
			}
		}

		/// <summary>Backfill/creation entry: freeze the deterministic choice
		/// into the store (basic cards acquired, score tuple persisted).
		/// Idempotent; only ever called while the tactical mode is ON.</summary>
		public void EnsureImperialDeck()
		{
			if ( imperialDeck != null ) return;
			var choice = PreviewImperialDeck();
			if ( choice == null ) return;   // asset missing - stay 'kein Deck'
			imperialDeck = ImperialDeckState.FromChoice( choice, GUID.ToString() );
		}

		// TACTICAL AI M12c - Imperial Upgrade Stage (Rules Reference,
		// verbatim: "To purchase Class cards, the Imperial player EXAMINES
		// ALL CARDS in his Class deck and may purchase AS MANY AS HE WISHES
		// by spending XP equal to the cards' costs ... kept faceup ... for
		// the remainder of the campaign."). No market, no chance: the full
		// chosen deck is always visible, so the KI buys greedily by the
		// purchase priority of the M12b profile (TacticalAI.Core.
		// ImperialPurchases) - one buy per step ("1 Karte, wenn XP >= Preis",
		// chain scope 2), and the RR explicitly allows repeating that step
		// in one stage, so RunImperialUpgradeStage() loops while the account
		// can still pay. XP ARRIVES between missions (CM6 bookings from
		// mission outcome branches); purchases RUN on the campaign reload
		// after mission end (LoadCampaignState hook) - i.e. after the
		// mission-end evaluation, before the next Einsatzkarte, exactly as
		// the chain prescribes. Non-tactical campaigns never have a stored
		// deck: every entry point below honestly returns the M12b 'kein
		// Deck' state (Scope 3: the mode UI only ever shows what it allows).

		/// <summary>Read-only view of the KI's NEXT purchase step against the
		/// CURRENT store (account + owned). Never writes, never books - UIs
		/// (M12e) can preview the ranked candidates and the pick without
		/// touching the campaign store. Null: 'kein Deck' state (no stored
		/// deck) or deck asset unavailable.</summary>
		public TacticalAI.Core.ImperialPurchaseChoice PreviewImperialPurchase()
		{
			if ( imperialDeck == null ) return null;
			try
			{
				var table = ImperialDeckTableOrNull();
				if ( table == null ) return null;
				var seed = TacticalAI.Core.ImperialDecks.DeckSeedFor( GUID.ToString() );
				return TacticalAI.Core.ImperialPurchases.Plan( table, BuildImperialProfile(),
					imperialDeck.deckId, OwnedDeckCardIds(), imperialDeck.xpBalance, seed );
			}
			catch ( Exception e )
			{
				Debug.LogWarning( "SagaCampaign::PreviewImperialPurchase failed " + e.Message );
				return null;
			}
		}

		/// <summary>Run the upgrade stage on the stored deck: greedy purchase
		/// steps until the account cannot pay for any remaining card. Each buy
		/// goes through ImperialDeckState.TryBuyDeckCard - the single booking
		/// path (card + spend in one append-only step) - and logs the ui.json
		/// line. In-memory only here (same doctrine as the M12b backfill):
		/// the next regular SaveCampaignState persists purchases; a crash
		/// before that just re-runs the identical stage on the next load.
		/// Returns the choices made (for UI/log), empty in 'kein Deck'.</summary>
		public List<TacticalAI.Core.ImperialPurchaseChoice> RunImperialUpgradeStage()
		{
			var bought = new List<TacticalAI.Core.ImperialPurchaseChoice>();
			if ( imperialDeck == null ) return bought;
			try
			{
				var table = ImperialDeckTableOrNull();
				if ( table == null ) return bought;
				var seed = TacticalAI.Core.ImperialDecks.DeckSeedFor( GUID.ToString() );
				var profile = BuildImperialProfile();
				// era-1 decks hold 8 paid cards; the guard only protects
				// against a future pathological corpus, never against 'no
				// card affordable' (that exits via plan.CardId == null).
				for ( int guard = 0; guard < 100; guard++ )
				{
					var plan = TacticalAI.Core.ImperialPurchases.Plan( table, profile,
						imperialDeck.deckId, OwnedDeckCardIds(), imperialDeck.xpBalance, seed );
					if ( plan == null || plan.CardId == null ) break;
					if ( !imperialDeck.TryBuyDeckCard( plan.CardId, (int)plan.XpCost,
					                                   "buy:" + plan.CardId ) ) break;
					bought.Add( plan );
					Debug.Log( TacticalAI.TacticalAISettingsUnity.ImperialBuyLogText(
						plan.CardName, plan.XpCost ) );
				}
			}
			catch ( Exception e )
			{
				Debug.LogWarning( "SagaCampaign::RunImperialUpgradeStage failed " + e.Message );
			}
			return bought;
		}

		/// <summary>Manual purchase for a HUMAN Imperial player (Scope 3):
		/// same store, same single booking path, same append-only history as
		/// the KI - only the decision source differs. Strictly validated: the
		/// card must be a printed, purchasable (non-basic, XP cost) card of
		/// the STORED deck and not owned yet, and the account must cover the
		/// cost. Without a stored deck ('kein Deck' / non-tactical) nothing
		/// is allowed - returns false. The caller saves via the regular
		/// campaign save path.</summary>
		public bool ManualBuyImperialDeckCard( string cardId )
		{
			if ( imperialDeck == null || string.IsNullOrEmpty( cardId ) ) return false;
			try
			{
				var table = ImperialDeckTableOrNull();
				var deck = table != null ? table.Find( imperialDeck.deckId ) : null;
				if ( deck == null ) return false;
				var card = deck.Cards.FirstOrDefault( x => x.Id == cardId );
				if ( card == null || card.Basic || !card.Xp.HasValue ) return false;
				if ( imperialDeck.cards.Any( c => c.id == cardId ) ) return false;
				if ( !imperialDeck.TryBuyDeckCard( cardId, (int)card.Xp.Value, "buy:" + cardId ) ) return false;
				Debug.Log( TacticalAI.TacticalAISettingsUnity.ImperialBuyLogText( card.Name, card.Xp.Value ) );
				return true;
			}
			catch ( Exception e )
			{
				Debug.LogWarning( "SagaCampaign::ManualBuyImperialDeckCard failed " + e.Message );
				return false;
			}
		}

		/// <summary>Deck table only when the shipped asset is actually
		/// available (touching TacticalAIAssets primes the Core doc cache -
		/// same pattern as PreviewImperialDeck). Null = stay 'kein Deck'.</summary>
		TacticalAI.Core.ImperialClassDeckTable ImperialDeckTableOrNull()
		{
			if ( TacticalAI.Core.ImperialDecks.DocOrNull() == null &&
				 TacticalAI.TacticalAIAssets.ImperialDecksRoot == null )
				return null;
			return TacticalAI.Core.ImperialDecks.Load();
		}

		HashSet<string> OwnedDeckCardIds()
		{
			var owned = new HashSet<string>( StringComparer.Ordinal );
			foreach ( var c in imperialDeck.cards )
				owned.Add( c.id );
			return owned;
		}

		// ------------------------------------------------- M12d2 agenda deck
		// Rules Reference law implemented here (verbatim in
		// TacticalAI/Core/AgendaCards.cs / AgendaMarket.cs headers): setup
		// picks SIX complete 3-card sets (time-period filter is data-driven
		// and vacuous while the app models no campaign time period - see
		// AgendaCards.SetLegal), the deck is shuffled ONCE at setup, and
		// every Imperial Upgrade Stage runs exactly ONE market: "shuffles
		// the Agenda deck and secretly draws four of them", buys anything
		// affordable from the four, returns the rest UNREVEALED. The stage
		// pointer (stagesRun) makes the sequence re-derivable end to end;
		// influence books through the append-only campaign-log ledger
		// (RR "Influence"), income arrives via CM7 bookings like CM6 XP -
		// mission rewards are mission CONTENT, this module never invents
		// income. RR order "first ... XP ... then influence": this runs
		// right after RunImperialUpgradeStage() on the same load.
		// Side/Forced announcements: PRUEFAUFTRAG (2) found NO agenda-shaped
		// trigger (details in the M12d2 closing comment) - the buy becomes
		// the public read-aloud announcement, execution stays with the
		// table. Kept Secret content never leaves the store until the card
		// itself demands resolution (AgendaState.RebelView anti-leak law).

		/// <summary>Read-only view of the deterministic setup choice (6 sets
		/// + deck order) for the CURRENT campaign - the human imperial's
		/// setup PROPOSAL (scope 6: display without compulsion) and the
		/// backfill source. Pure: never writes. Null: asset missing.</summary>
		public TacticalAI.Core.AgendaDeckChoice PreviewAgendaSetup()
		{
			try
			{
				var table = AgendaTableOrNull();
				if ( table == null ) return null;
				var seed = TacticalAI.Core.AgendaCards.AgendaSeedFor( GUID.ToString() );
				return TacticalAI.Core.AgendaCards.ChooseDeck( table, BuildImperialProfile(),
					seed, CampaignTimePeriodOrNull() );
			}
			catch ( Exception e )
			{
				Debug.LogWarning( "SagaCampaign::PreviewAgendaSetup failed " + e.Message );
				return null;
			}
		}

		/// <summary>Backfill/creation entry: freeze the deterministic setup
		/// choice into the store. Idempotent; only ever called while the
		/// tactical mode is ON (same doctrine as EnsureImperialDeck).</summary>
		public void EnsureAgendaDeck()
		{
			if ( agenda != null ) return;
			var choice = PreviewAgendaSetup();
			if ( choice == null ) return;   // asset missing - stay 'kein Agenda-State'
			agenda = AgendaState.FromChoice( choice, GUID.ToString() );
		}

		/// <summary>Run the ONE market of the NEXT upgrade stage against the
		/// stored deck: draw, greedy deterministic buys, return-mix, audit
		/// rows, public log lines. In-memory only (same doctrine as the M12c
		/// stage): the next regular SaveCampaignState persists, a crash
		/// before that replays the identical stage. Returns the played plan
		/// (empty list = 'kein Agenda-State', no asset, or empty deck).</summary>
		public List<TacticalAI.Core.AgendaStagePlan> RunAgendaUpgradeStage()
		{
			var played = new List<TacticalAI.Core.AgendaStagePlan>();
			if ( agenda == null ) return played;
			try
			{
				var table = AgendaTableOrNull();
				if ( table == null || agenda.deck.Count == 0 ) return played;
				var seed = TacticalAI.Core.AgendaCards.AgendaSeedFor( GUID.ToString() );
				int stage = agenda.stagesRun;
				var plan = TacticalAI.Core.AgendaMarket.RunStage( table, BuildImperialProfile(),
					agenda.deck, agenda.influenceBalance, seed, stage );
				if ( plan == null ) return played;
				foreach ( var b in plan.Buys )
					if ( !agenda.TryBuyAgenda( b.Card, b.Type, (int)b.Cost, stage, "buy:" + b.Card ) )
					{
						// cannot happen (the plan priced every buy against this
						// very balance) - stop honestly rather than drift.
						Debug.LogWarning( "SagaCampaign::RunAgendaUpgradeStage booking refused " + b.Card );
						break;
					}
				var boughtIds = new List<string>();
				foreach ( var b in plan.Buys ) boughtIds.Add( b.Card );
				agenda.AddMarketAudit( stage, plan.Market, boughtIds, plan.NotPurchased, plan.SeedHex );
				agenda.ReplaceDeckAfterStage( plan.DeckAfter );
				// Public announcements (ui.json; read-aloud law). Kept Secret
				// buys announce the SPEND only - never the card (AgendaBuyLogText
				// maps that by the RR type; the secret is mechanics, a leak is
				// a bug).
				foreach ( var b in plan.Buys )
					Debug.Log( TacticalAI.TacticalAISettingsUnity.AgendaBuyLogText(
						b.Type, b.Name, b.Cost ) );
				played.Add( plan );
			}
			catch ( Exception e )
			{
				Debug.LogWarning( "SagaCampaign::RunAgendaUpgradeStage failed " + e.Message );
			}
			return played;
		}

		/// <summary>Read-only market preview against the CURRENT store
		/// (imperial audit view - M12e display seam for the HUMAN imperial,
		/// scope 6 'anzeigen ohne Zwang'). Never writes; never feeds a
		/// rebel-facing surface with market content.</summary>
		public TacticalAI.Core.AgendaStagePlan PreviewAgendaStage()
		{
			if ( agenda == null ) return null;
			try
			{
				var table = AgendaTableOrNull();
				if ( table == null || agenda.deck.Count == 0 ) return null;
				var seed = TacticalAI.Core.AgendaCards.AgendaSeedFor( GUID.ToString() );
				return TacticalAI.Core.AgendaMarket.RunStage( table, BuildImperialProfile(),
					agenda.deck, agenda.influenceBalance, seed, agenda.stagesRun );
			}
			catch ( Exception e )
			{
				Debug.LogWarning( "SagaCampaign::PreviewAgendaStage failed " + e.Message );
				return null;
			}
		}

		/// <summary>Rebel-safe purchase ledger for UI/log consumers (anti-
		/// leak law lives in AgendaState.RebelView): public facts only -
		/// RR type and printed influence cost of every buy, card identities
		/// of UNREVEALED Kept Secrets removed.</summary>
		public List<Dictionary<string, object>> AgendaRebelView()
		{
			return agenda == null ? null : agenda.RebelView();
		}

		/// <summary>The campaign's time period number for the RR setup
		/// filter. The app models NO campaign time period (verified in the
		/// M12d2 PRUEFAUFTRAG: MissionCard.timePeriod is per-mission display
		/// data only), so this is honestly null and the filter stays
		/// vacuous. Single seam: a future setup option lands HERE, nowhere
		/// else.</summary>
		long? CampaignTimePeriodOrNull()
		{
			return null;
		}

		/// <summary>Agenda asset table only when the shipped file is really
		/// available (touching TacticalAIAssets primes the Core doc cache -
		/// same pattern as ImperialDeckTableOrNull). Null = stay 'kein
		/// Agenda-State'.</summary>
		TacticalAI.Core.AgendaCardTable AgendaTableOrNull()
		{
			if ( TacticalAI.Core.AgendaCards.DocOrNull() == null &&
				 TacticalAI.TacticalAIAssets.AgendaCardsRoot == null )
				return null;
			return TacticalAI.Core.AgendaCards.Load();
		}
	}
}