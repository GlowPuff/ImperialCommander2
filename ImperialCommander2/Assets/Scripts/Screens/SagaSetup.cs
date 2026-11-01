using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Saga
{
	public class SagaSetup : MonoBehaviour
	{
		public Camera theCamera;
		//UI TRINKETS
		public GameObject descriptionTextBox;
		public Text difficultyText, adaptiveText;
		public Transform heroContainer;
		public Button adaptiveButton, startMissionButton, viewMissionCardButton, campaignTilesButton, allyButton, difficultyButton, helpButton, heroButton, villainsButton, ignoredButton, threatButton, additionalButton;
		public GameObject miniMugPrefab;
		public Image allyImage, allyBackgroundImage;
		public MWheelHandler threatValue, addtlThreatValue;
		public TextMeshProUGUI versionText, additionalInfoText;
		public MWheelHandler threatLevelWheel, addtlThreatWheel;
		//UI PANELS
		public SagaAddHeroPanel addHeroPanel;
		public SagaModifyGroupsPanel modifyGroupsPanel;
		public MissionCardZoom missionCardZoom;
		public ErrorPanel errorPanel;
		public HelpPanel helpPanel;
		public CanvasGroup uiCanvasGroup;
		//OTHER
		public GameObject warpEffect, rightPanel;
		public Transform thrusterRoot, thrusterLeft, thrusterRight;
		public SagaSetupLanguageController languageController;
		public CanvasGroup faderCG;
		public ColorBlock redBlock;
		public ColorBlock greenBlock;
		public VolumeProfile volume;
		public MissionPicker missionPicker;
		public TextMeshProUGUI warpTitleText;
		public List<DeploymentCard> missionCustomAllies = new List<DeploymentCard>();
		public List<DeploymentCard> missionCustomHeroes = new List<DeploymentCard>();
		public List<DeploymentCard> missionCustomVillains = new List<DeploymentCard>();
		//PLANET (warp effect)
		public Transform planet;
		public SpriteRenderer planetSprite;
		public Sprite[] planetSpritePool;
		//TEXTBOX
		public PopupBase textBoxPopupBase;
		public GameObject textBoxPrefab;
		public bool isDebugMode = false;

		Selectable lastSelected = null;
		Color allyDisabledColor = new Color( 0.5283019f, .0828f, 0 );
		Color allyEnabledColor = new Color( 0, 0.6440244f, 1 );
		Sound sound;
		SagaSetupOptions setupOptions { get; set; }
		bool isFromCampaign = false;
		//these ignored groups are from mission/preset, and player can't toggle them
		List<DeploymentCard> disabledIgnoredGroups = new List<DeploymentCard>();
		//set the fixed ally when a Mission is selected, or grab it from the Campaign's Mission
		[HideInInspector]
		public string fixedAlly = null;

		void LogCallback( string condition, string stackTrace, LogType type )
		{
			//only capture errors, exceptions, asserts
			if ( type != LogType.Warning && type != LogType.Log )
				errorPanel.Show( $"An Error Occurred of Type <color=green>{type}</color>", $"<color=yellow>{condition}</color>\n\n{stackTrace.Replace( "(at", "\n\n(at" )}" );
		}

		private void OnDestroy()
		{
			Application.logMessageReceived -= LogCallback;
		}

		void Awake()
		{
			Debug.Log( "ENTERING SAGA SETUP" );
			if ( isDebugMode )
			{
				//reset app data for testing
				DataStore.InitData();
			}

			//Exception handling for any Unity thrown exception, such as from asset management
			Application.logMessageReceived += LogCallback;

			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

			versionText.text = DataStore.uiLanguage.uiSetup.appMissionFormat + " " + Utils.formatVersion;

			Screen.sleepTimeout = SleepTimeout.NeverSleep;

			float pixelHeightOfCurrentScreen = Screen.height;//.currentResolution.height;
			float pixelWidthOfCurrentScreen = Screen.width;//.currentResolution.width;
			float aspect = pixelWidthOfCurrentScreen / pixelHeightOfCurrentScreen;
			if ( aspect >= 2f )
			{
				thrusterLeft.position = new Vector3( -12f, thrusterLeft.position.y, thrusterLeft.position.z );
				thrusterRight.position = new Vector3( 12f, thrusterRight.position.y, thrusterRight.position.z );
			}
			if ( aspect < 1.7f )//less than 16:9, such as 16:10 and 4:3
			{
				warpTitleText.GetComponent<RectTransform>().SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, 175 );
			}

			//bootstrap the setup screen for debugging
			if ( isDebugMode && Application.isEditor )
				bootstrapDEBUG();

			//set translated UI
			try
			{
				languageController.SetTranslatedUI();
			}
			catch ( Exception e )
			{
				errorPanel.Show( "SetTranslatedUI()", e );
			}

			//apply settings
			if ( volume.TryGet<Bloom>( out var bloom ) )
				bloom.active = PlayerPrefs.GetInt( "bloom2" ) == 1;
			if ( volume.TryGet<Vignette>( out var vig ) )
				vig.active = PlayerPrefs.GetInt( "vignette" ) == 1;

			sound = FindObjectOfType<Sound>();
			//play menu ambient and music
			sound.PlayMusicAndMenuAmbient();

			setupOptions = new SagaSetupOptions();
			DataStore.StartNewSagaSession( setupOptions );
			// TACTICAL AI: a fresh session resets planner state (round order,
			// deck manager). Default OFF: nothing to reset, no-op gate.
			if ( TacticalAI.TacticalAISettings.Enabled )
				TacticalAI.TacticalAIBridge.ResetSession();
			//check if we're loading in from  the campaign manager
			//single shot mission, not from a campaign
			if ( RunningCampaign.sagaCampaignGUID != Guid.Empty )
				SetupCampaignMission();//setupOptions is set here

			ResetSetup( RunningCampaign.sagaCampaignGUID != Guid.Empty );
			UpdateHeroes();

			faderCG.alpha = 0;
			faderCG.DOFade( 1, .5f );

			//M11f: while the tactical badge owns the difficulty slot it is the
			//gamepad/keyboard entry point, not the hidden difficulty button.
			var firstFocus = FirstFocusGameObject();
			if ( firstFocus != null ) EventSystem.current.SetSelectedGameObject( firstFocus );
		}

		void bootstrapDEBUG()
		{
			FileManager.SetupDefaultFolders();
			RunningCampaign.Reset();
			DataStore.InitData();
		}

		// TACTICAL AI (playtest 27.10): runtime mode badge on the mission-card
		// panel. Shown only when the global master switch is ON; click cycles
		// AUTO -> OFF -> S1..S5 -> AUTO. Empty aiMode follows the global mode
		// (concept 4.2.7 fallback semantics).
		// M11f: while the mission actually plays tactically, the legacy
		// difficulty/adaptive toggles are functionless (the planner calibrates on
		// the S-rung) -> hidden via SetActive, and the badge takes their slot.
		// When the badge is deliberately OFF (or the master switch is off) the
		// mission runs the legacy path, the toggles DO have an effect, so they
		// stay visible and the badge keeps its small dev 21 spot below them.
		// All derived from existing settings/sessionValues - no new UI state.
		TMPro.TextMeshProUGUI setupAiBadgeText;
		GameObject setupAiBadgeGo;
		// M12e: the deck line under the badge ("Militaermacht · 4 Karten" +
		// open agenda). Display-only, campaign missions with an Imperial deck.
		TMPro.TextMeshProUGUI setupPersonaLineText;
		GameObject setupPersonaLineGo;
		SagaCampaign setupCampaign;

		/// <summary>True exactly when this mission will run the tactical planner
		/// (master ON and the per-mission token does not force legacy).</summary>
		bool TacticalControlsActive()
		{
			if ( !TacticalAI.TacticalAISettings.Enabled ) return false;
			string mode = setupOptions != null ? setupOptions.aiMode : "";
			return TacticalAI.TacticalAISettings.EffectiveMode( mode ) != "legacy";
		}

		void EnsureAiBadge()
		{
			var show = TacticalAI.TacticalAISettings.Enabled;
			if ( setupAiBadgeText != null )
			{
				setupAiBadgeGo.SetActive( show );
				if ( show )
				{
					ApplyLegacyControls( !TacticalControlsActive() );
					PlaceAiBadge();
					setupAiBadgeText.text = SetupAiLabel( setupOptions.aiMode );
					EnsurePersonaLine();
				}
				return;
			}
			if ( !show || setupOptions == null || difficultyButton == null ) return;

			var go = new GameObject( "tacaiSetupBadge", typeof( RectTransform ) );
			go.transform.SetParent( difficultyButton.transform.parent, false );
			var rt = go.GetComponent<RectTransform>();
			rt.anchorMin = new Vector2( 0f, 1f );
			rt.anchorMax = new Vector2( 0f, 1f );
			rt.pivot = new Vector2( 0f, 1f );
			rt.anchoredPosition = new Vector2( 12f, -160f );
			rt.sizeDelta = new Vector2( 170f, 26f ); // dev 21: ladder names
			var img = go.AddComponent<UnityEngine.UI.Image>();
			img.color = new Color( 0f, .55f, .45f, .9f );
			var btn = go.AddComponent<Button>();
			btn.targetGraphic = img;
			btn.onClick.AddListener( OnSetupAiClicked );

			var textGo = new GameObject( "Text (TMP)", typeof( RectTransform ) );
			textGo.transform.SetParent( go.transform, false );
			var trt = textGo.GetComponent<RectTransform>();
			trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
			trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
			setupAiBadgeText = textGo.AddComponent<TMPro.TextMeshProUGUI>();
			setupAiBadgeText.font = TMPro.TMP_Settings.defaultFontAsset;
			setupAiBadgeText.enableAutoSizing = true; setupAiBadgeText.fontSizeMax = 15; setupAiBadgeText.fontSizeMin = 8;
			setupAiBadgeText.alignment = TMPro.TextAlignmentOptions.Center;
			setupAiBadgeText.color = Color.white;
			setupAiBadgeGo = go;
			// dev 38 (M11f): slot, visibility and Klartext label in one place
			ApplyLegacyControls( !TacticalControlsActive() );
			PlaceAiBadge();
			setupAiBadgeText.text = SetupAiLabel( setupOptions.aiMode );
			EnsurePersonaLine();
		}

		/// <summary>Personas (M1): display-only persona line on the
		/// Einsatzkarte - "Sturmtruppen-Captain — Rang 2" at THIS mission's
		/// threat budget (the rank law is a pure threat function, so the
		/// rank is honest before the mission starts). Replaces the M12e
		/// deck line 1:1 in place and look (Thomas 02.11.: "bei dem Text,
		/// wo jetzt die imperialen Karten angezeigt werden"). Invisible
		/// without a persona: a classic mission shows no empty chrome.
		/// M13i (Thomas screenshot 28.10): placed to the RIGHT of the
		/// difficulty row in the info column (see geometry block), no
		/// longer trailing the badge into the bottom buttons.</summary>
		void EnsurePersonaLine()
		{
			var campaign = setupCampaign ?? RunningCampaign.sagaCampaign;
			if ( campaign == null && RunningCampaign.sagaCampaignGUID != Guid.Empty )
			{
				try { campaign = SagaCampaign.LoadCampaignState( RunningCampaign.sagaCampaignGUID ); }
				catch { campaign = null; }
			}
			int threat = setupOptions != null ? setupOptions.threatLevel : -1;
			var view = TacticalAI.PersonaState.ResolveFor(
				TacticalAI.PersonaState.EffectiveId( campaign ), threat );
			string line = TacticalAI.TacticalAISettingsUnity.PersonaBadgeText( view );
			if ( string.IsNullOrEmpty( line ) )
			{
				if ( setupPersonaLineGo != null ) setupPersonaLineGo.SetActive( false );
				return;
			}

			if ( setupPersonaLineText == null )
			{
				var go = new GameObject( "tacaiPersonaLine", typeof( RectTransform ) );
				go.transform.SetParent( difficultyButton.transform.parent, false );
				var img = go.AddComponent<Image>();
				img.color = new Color( 0f, .35f, .30f, .85f );   // badge family, non-interactive
				img.raycastTarget = false;
				var textGo = new GameObject( "Text (TMP)", typeof( RectTransform ) );
				textGo.transform.SetParent( go.transform, false );
				var trt = textGo.GetComponent<RectTransform>();
				trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
				trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
				setupPersonaLineText = textGo.AddComponent<TMPro.TextMeshProUGUI>();
				setupPersonaLineText.font = TMPro.TMP_Settings.defaultFontAsset;
				setupPersonaLineText.enableAutoSizing = true;
				setupPersonaLineText.fontSizeMax = 12; setupPersonaLineText.fontSizeMin = 7;
				setupPersonaLineText.alignment = TMPro.TextAlignmentOptions.Center;
				setupPersonaLineText.color = new Color( .85f, 1f, .95f, 1f );
				setupPersonaLineGo = go;
			}
			setupPersonaLineGo.SetActive( true );

			var rt = setupPersonaLineGo.GetComponent<RectTransform>();
			// M13i/M13o (Thomas screenshots 28./29.10.): the line sits to the
			// RIGHT of the tactical-AI display (the AI badge, which owns the
			// difficulty slot in tactical mode; legacy difficulty label when
			// the badge is off). Player-10.log showed the old in-place
			// measurement ran BEFORE the first layout pass (rects still 0)
			// and silently fell back to the position BELOW the badge - which
			// is exactly what Thomas kept seeing. The geometry now works in
			// WORLD space (any anchor setup, no point-anchor assumption) and
			// retries through a coroutine until the anchor is actually laid
			// out, so the fallback can no longer stick.
			setupPersonaLineText.text = Utils.ReplaceGlyphs( line );
			if ( !TryPlaceImperialDeckLine( rt ) && impDeckLinePlaceCo == null )
				impDeckLinePlaceCo = StartCoroutine( PlaceImperialDeckLineWhenLaidOut( rt ) );
		}

		Coroutine impDeckLinePlaceCo;

		IEnumerator PlaceImperialDeckLineWhenLaidOut( RectTransform rt )
		{
			// give the canvas a few layout passes (script runs at scene start)
			for ( int i = 0; i < 40 && !TryPlaceImperialDeckLine( rt ); i++ )
				yield return new WaitForEndOfFrame();
			impDeckLinePlaceCo = null;
		}

		/// True once placed on a REAL laid-out rect. Anchor: active AI badge,
		/// else the difficulty label. Right/top edges are converted from world
		/// to this rect's parent space through the root canvas, so stretched
		/// or point anchors both work and no hardcoded pixel column exists.
		bool TryPlaceImperialDeckLine( RectTransform rt )
		{
			RectTransform anchor = null;
			var bGo = setupAiBadgeGo;
			if ( bGo != null && bGo.activeInHierarchy ) anchor = bGo.GetComponent<RectTransform>();
			if ( ( anchor == null || anchor.rect.height <= 0f ) && difficultyText != null )
			{
				var dT = difficultyText.transform as RectTransform;
				if ( dT != null && dT.rect.height > 0f ) anchor = dT;
			}
			if ( anchor == null || anchor.rect.height <= 0f ) return false;
			var parent = rt.parent as RectTransform;
			if ( parent == null ) return false;
			var canvas = rt.GetComponentInParent<Canvas>( true )?.rootCanvas;
			if ( canvas == null ) return false;
			var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
			var tr = anchor;
			var topRightScreen = RectTransformUtility.WorldToScreenPoint( cam,
				tr.TransformPoint( new Vector3( tr.rect.xMax, tr.rect.yMax, 0f ) ) );
			if ( !RectTransformUtility.ScreenPointToLocalPointInRectangle( parent, topRightScreen, cam, out Vector2 lp ) )
				return false;
			float w = 250f;
			float h = Mathf.Max( 44f, tr.rect.height * 2.4f );
			rt.anchorMin = parent.pivot;
			rt.anchorMax = parent.pivot;
			rt.pivot = new Vector2( 0f, 1f );   // top-left, flush right of the anchor's top edge
			rt.sizeDelta = new Vector2( w, h );
			rt.anchoredPosition = lp + new Vector2( 16f, 0f );
			return true;
		}

		/// <summary>M11f: the legacy difficulty/adaptive toggles - shown only when
		/// they have an effect. Their labels are children of the button GameObjects
		/// (red button prefab), so one SetActive covers text + button.</summary>
		void ApplyLegacyControls( bool visible )
		{
			if ( difficultyButton != null && difficultyButton.gameObject.activeSelf != visible )
				difficultyButton.gameObject.SetActive( visible );
			if ( adaptiveButton != null && adaptiveButton.gameObject.activeSelf != visible )
				adaptiveButton.gameObject.SetActive( visible );
		}

		/// <summary>M11f: tactical -> the badge takes the difficulty button's slot
		/// (anchors/position/size copied, works on both layouts); legacy-visible
		/// -> the small dev 21 badge below the difficulty button.</summary>
		void PlaceAiBadge()
		{
			if ( setupAiBadgeGo == null ) return;
			var rt = setupAiBadgeGo.GetComponent<RectTransform>();
			var dRT = difficultyButton != null ? difficultyButton.transform as RectTransform : null;
			if ( TacticalControlsActive() && dRT != null )
			{
				rt.anchorMin = dRT.anchorMin;
				rt.anchorMax = dRT.anchorMax;
				rt.pivot = dRT.pivot;
				rt.anchoredPosition = dRT.anchoredPosition;
				rt.sizeDelta = dRT.sizeDelta;
			}
			else if ( dRT != null )
			{
				rt.anchorMin = new Vector2( 0f, 1f );
				rt.anchorMax = new Vector2( 0f, 1f );
				rt.pivot = new Vector2( 0f, 1f );
				rt.anchoredPosition = new Vector2( dRT.anchoredPosition.x, dRT.anchoredPosition.y - dRT.rect.height - 10f );
				rt.sizeDelta = new Vector2( 170f, 26f );
			}
		}

		string SetupAiLabel( string mode )
		{
			// M11f: Klartext "Tactical AI — Level 3 (Veteran)" etc. (ui.json keys)
			return TacticalAI.TacticalAISettingsUnity.MissionBadgeText( mode );
		}

		/// <summary>Gamepad/keyboard focus entry: the badge owns the difficulty
		/// slot while it is there; otherwise the difficulty button as always.</summary>
		GameObject FirstFocusGameObject()
		{
			if ( difficultyButton != null && !difficultyButton.gameObject.activeInHierarchy
				&& setupAiBadgeGo != null && setupAiBadgeGo.activeInHierarchy )
				return setupAiBadgeGo;
			return difficultyButton != null ? difficultyButton.gameObject : null;
		}

		void OnSetupAiClicked()
		{
			if ( setupOptions == null ) return;
			// AUTO("") -> reference -> overlord -> legacy -> AUTO (same cycle
			// as the campaign-list badge and SagaSetupOptions.ToggleAiMode)
			// dev 21 (Thomas): ladder cycle, shared with the campaign badge
			setupOptions.aiMode = TacticalAI.TacticalAISettings.NextMissionToken( setupOptions.aiMode );
			// M11f: cycling back to OFF must bring the legacy toggles back and
			// move the badge out of their slot again - one refresh covers all.
			EnsureAiBadge();
			//persist for campaign missions (structure is saved with SaveCampaignState)
			if ( RunningCampaign.campaignStructure != null )
				RunningCampaign.campaignStructure.aiMode = setupOptions.aiMode;
			Debug.Log( "SagaSetup::AI badge -> " + ( string.IsNullOrEmpty( setupOptions.aiMode ) ? "AUTO" : setupOptions.aiMode ) );
		}

		/// <summary>
		/// set default mission options, add default ignored groups, add included global imports
		/// </summary>
		public void ResetSetup( bool isCampaign )
		{
			//difficulty
			difficultyText.text = DataStore.uiLanguage.uiSetup.normal;
			//adaptive
			//adaptiveButton.colors = setupOptions.useAdaptiveDifficulty ? greenBlock : redBlock;
			//ColorBlock cb = adaptiveButton.colors;
			//cb.selectedColor = new Color( .6f, .4f, .1f );
			//adaptiveButton.colors = cb;
			adaptiveText.color = setupOptions.useAdaptiveDifficulty ? greenBlock.normalColor : redBlock.normalColor;
			EnsureAiBadge(); // TACTICAL AI panel switch (playtest 27.10)

			//clear hero panel if not loading from campaign
			if ( heroContainer.childCount > 0 )
			{
				for ( int i = 1; i < heroContainer.childCount; i++ )
				{
					Destroy( heroContainer.GetChild( i ).gameObject );
				}
			}
			//threat value
			threatValue.ResetWheeler( setupOptions.threatLevel );
			//additional threat value
			addtlThreatValue.ResetWheeler( setupOptions.addtlThreat );

			//global imports not excluded from Expansions menu
			var included = DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Imperial && !DataStore.IgnoredPrefsImports.Contains( x.customCharacterGUID.ToString() ) );
			DataStore.sagaSessionData.globalImportedCharacters = included.Select( x => x.deploymentCard ).ToList();
			Debug.Log( $"{DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Imperial ).Count()} GLOBALLY IMPORTED IMPERIALS" );
			Debug.Log( $"{DataStore.IgnoredPrefsImports.Count} EXCLUDED IMPORTS (Expansion menu)" );
			Debug.Log( $"{DataStore.sagaSessionData.globalImportedCharacters.Count} ADDED TO SESSION IMPORTS" );

			languageController.UpdateVillainCount( DataStore.sagaSessionData.EarnedVillains.Count );

			if ( !isCampaign )
			{
				//clear ignored groups
				DataStore.sagaSessionData.MissionIgnored.Clear();
				//add default ignored
				//ignore "Other" expansion enemy groups by default, except owned packs
				SetMissionIgnored( DataStore.deploymentCards.Where( x => x.expansion == "Other" && !DataStore.ownedFigurePacks.ContainsCard( x ) ) );
			}
		}

		public void OnCancel()
		{
			sound.FadeOutMusic();
			thrusterRoot.DOMoveZ( -30, .5f );
			if ( !isFromCampaign )
				faderCG.DOFade( 0, .5f ).OnComplete( () => SceneManager.LoadScene( "Title" ) );
			else
				faderCG.DOFade( 0, .5f ).OnComplete( () => SceneManager.LoadScene( "Campaign" ) );
		}

		/// <summary>
		/// Sets DataStore.sagaSessionData.MissionIgnored and updates UI text
		/// </summary>
		void SetMissionIgnored( IEnumerable<DeploymentCard> ignored )
		{
			DataStore.sagaSessionData.MissionIgnored.AddRange( ignored );
			int impCount = DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Imperial ).Count() - DataStore.sagaSessionData.globalImportedCharacters.Count;
			languageController.UpdateIgnoredCount( DataStore.sagaSessionData.MissionIgnored.Count + impCount );
			Debug.Log( $"FOUND {DataStore.sagaSessionData.MissionIgnored.Count} IGNORED GROUPS IN MISSION" );
		}

		/// <summary>
		/// setup the screen when loaded from the campaign manager
		/// </summary>
		void SetupCampaignMission()
		{
			isFromCampaign = true;
			var campaign = SagaCampaign.LoadCampaignState( RunningCampaign.sagaCampaignGUID );
			setupCampaign = campaign;   // M12e: the badge deck line reads the saved deck state
			if ( campaign == null )
			{
				Debug.Log( "SetupCampaignMission()::Could not load the campaign" );
				errorPanel.Show( "SetupCampaignMission()", $"Campaign is null\n'{RunningCampaign.sagaCampaignGUID}'" );
				return;
			}

			campaignTilesButton.gameObject.SetActive( true );

			var structure = RunningCampaign.campaignStructure;
			setupOptions = new SagaSetupOptions()
			{
				threatLevel = structure.threatLevel,
				projectItem = structure.projectItem,
			};

			//only used by FileManager.GetOfficialMissionTranslation() for official campaign missions
			setupOptions.projectItem.expansion = structure.expansionCode;
			//used by SetNextStoryMission() to find the index of the current mission in play (both official and custom campaigns)
			setupOptions.projectItem.missionID = structure.missionID;

			// TACTICAL AI (concept 4.2.7): carry the mission card's planner level
			// into the session. Default OFF: aiMode stays "legacy" and every hook
			// runs its original code path.
			if ( TacticalAI.TacticalAISettings.Enabled && !string.IsNullOrEmpty( structure.aiMode ) )
				setupOptions.aiMode = structure.aiMode;

			//deactivate mission picker
			rightPanel.SetActive( false );
			//add heroes
			foreach ( var item in campaign.campaignHeroes )
			{
				var hero = DataStore.GetHero( item.heroID );
				if ( hero != null )
					DataStore.sagaSessionData.MissionHeroes.Add( hero );
			}
			//add villains
			foreach ( var item in campaign.campaignVillains )
			{
				DataStore.sagaSessionData.EarnedVillains.Add( item );
			}
			//clear ignored groups
			DataStore.sagaSessionData.MissionIgnored.Clear();
			//add default ignored
			//ignore "Other" expansion enemy groups by default, except owned packs
			var ignored = new HashSet<DeploymentCard>();
			DataStore.deploymentCards.Where( x => x.expansion == "Other" && !DataStore.ownedFigurePacks.ContainsCard( x ) ).ToList().ForEach( x => ignored.Add( x ) );

			if ( structure.missionSource == MissionSource.Embedded )
			{
				//get ignored groups from the embedded mission

				//embedded mission GUID is stored into 'hero'
				//imported campaign name is stored into 'bonusText'
				//package GUID is stored into 'expansionText'
				Mission m = FileManager.LoadEmbeddedMission( structure.packageGUID.ToString(), structure.projectItem.missionGUID, out DataStore.sagaSessionData.missionStringified );

				DataStore.mission = m;

				if ( m != null )
				{
					var ign = m.missionProperties.bannedGroups.Select( x => { return DataStore.deploymentCards.GetDeploymentCard( x ); } );
					ign.ToList().ForEach( x => ignored.Add( x ) );
					//add ignored from mission
					disabledIgnoredGroups.AddRange( ign );
					SetMissionIgnored( ignored );
					//set the fixed ally from the mission, if there is one
					if ( m.missionProperties.useFixedAlly == YesNoAll.Yes )
						fixedAlly = m.missionProperties.fixedAlly;
				}
				else
					errorPanel.Show( "SetupCampaignMission()", $"Could not load embedded mission:\n{structure.projectItem.Title}::{structure.projectItem.missionGUID}" );
			}
			//add non-custom (Official) mission specific ignored groups, even if they are owned packs
			else if ( structure.missionSource == MissionSource.Official )
			{
				//get ignored groups from the mission itself
				Mission m = FileManager.LoadMissionFromResource( setupOptions.projectItem.missionID, out string stringified );
				if ( m != null )
				{
					var ign = m.missionProperties.bannedGroups.Select( x => { return DataStore.deploymentCards.GetDeploymentCard( x ); } );

					ign.ToList().ForEach( x => ignored.Add( x ) );
					//add ignored from preset
					disabledIgnoredGroups.AddRange( ign );
					SetMissionIgnored( ignored );

					var card = DataStore.GetMissionCard( structure.projectItem.missionID );
					structure.projectItem.Description = card.descriptionText;

					//set the fixed ally from the mission, if there is one
					if ( m.missionProperties.useFixedAlly == YesNoAll.Yes )
						fixedAlly = m.missionProperties.fixedAlly;
				}

				//Deprecated - Missions are no longer Addressable Assets
				//Leaving it as a reminder of how to use FileManager to load Addressable Assets in the future
				//FileManager.LoadMissionFromAddressable( setupOptions.projectItem.fullPathWithFilename, ( m, s ) =>
				//{
				//	if ( m != null )
				//	{
				//		var ign = m.missionProperties.bannedGroups.Select( x => { return DataStore.deploymentCards.GetDeploymentCard( x ); } );

				//		ign.ToList().ForEach( x => ignored.Add( x ) );
				//		//add ignored from preset
				//		disabledIgnoredGroups.AddRange( ign );
				//		SetMissionIgnored( ignored );
				//	}
				//} );
			}
			else if ( structure.missionSource == MissionSource.Custom )
			{
				//get ignored groups from the mission itself
				Mission m = FileManager.LoadMission( structure.projectItem.fullPathWithFilename );
				if ( m != null )
				{
					var ign = from c in DataStore.deploymentCards join i in m.missionProperties.bannedGroups on c.id equals i select c;
					ign.ToList().ForEach( x => ignored.Add( x ) );
					//add ignored from mission
					disabledIgnoredGroups.AddRange( ign );
					SetMissionIgnored( ignored );
					//set the fixed ally from the mission, if there is one
					if ( m.missionProperties.useFixedAlly == YesNoAll.Yes )
						fixedAlly = m.missionProperties.fixedAlly;
				}
				else
					errorPanel.Show( "SetupCampaignMission()", $"Could not load custom mission:\n{structure.projectItem.fullPathWithFilename}" );
			}

			//set the mission
			missionPicker.selectedMission = setupOptions.projectItem;
			//set mission name
			missionPicker.missionNameText.text = structure.projectItem.Title;
			//set description
			missionPicker.missionDescriptionText.text = structure.projectItem.Description;
			//set additional text
			missionPicker.additionalInfoText.text = structure.projectItem.AdditionalInfo;
			//set threat
			threatValue.ResetWheeler( structure.threatLevel );
			//set text based on custom or built-in mission
			if ( structure.missionSource == MissionSource.Custom )
			{
				setupOptions.projectItem.pickerMode = PickerMode.Custom;
				missionPicker.pickerMode = PickerMode.Custom;
				descriptionTextBox.gameObject.SetActive( true );
				viewMissionCardButton.gameObject.SetActive( false );
			}
			else if ( structure.missionSource == MissionSource.Official )
			{
				setupOptions.projectItem.pickerMode = PickerMode.BuiltIn;
				missionPicker.pickerMode = PickerMode.BuiltIn;
				descriptionTextBox.gameObject.SetActive( false );
				viewMissionCardButton.gameObject.SetActive( true );
			}
			else if ( structure.missionSource == MissionSource.Embedded )//from a custom campaign
			{
				setupOptions.projectItem.pickerMode = PickerMode.Embedded;
				missionPicker.pickerMode = PickerMode.Embedded;
				descriptionTextBox.gameObject.SetActive( false );
				viewMissionCardButton.gameObject.SetActive( true );
			}
		}

		/// <summary>
		/// Fired from the button
		/// </summary>
		public void OnStartMission()
		{
			setupOptions.threatLevel = threatValue.wheelValue;
			setupOptions.addtlThreat = addtlThreatValue.wheelValue;
			setupOptions.projectItem = missionPicker.selectedMission;
			// TACTICAL AI (concept 4.2.7): single missions inherit the global mode
			// preference unless the player picked a level for this mission.
			// Default OFF: no-op, aiMode stays "legacy".
			// Inherit the global preference ONLY when the mission did not choose
			// a mode itself - an explicit per-mission "legacy" must survive
			// (concept 4.2.7 three-state; FINAL-QA audit).
			if ( TacticalAI.TacticalAISettings.Enabled && string.IsNullOrEmpty( setupOptions.aiMode ) )
				setupOptions.aiMode = TacticalAI.TacticalAISettings.Mode;
			DataStore.sagaSessionData.setupOptions = setupOptions;

			missionPicker.isBusy = true;
			uiCanvasGroup.interactable = false;
			threatValue.disableInteractions = true;
			addtlThreatValue.disableInteractions = true;

			//load/validate the mission
			if ( missionPicker.pickerMode == PickerMode.Custom )//custom mission
			{
				DataStore.mission = FileManager.LoadMission( setupOptions.projectItem.fullPathWithFilename, out DataStore.sagaSessionData.missionStringified );
				if ( DataStore.mission != null )
				{
					//set the Mission translation, if there is one
					//var translation = FileManager.GetCustomMissionTranslation( setupOptions.projectItem.fullPathWithFilename );
					////automatically skips if it's null
					//TranslationController.Instance.SetMissionTranslation( translation, DataStore.mission );

					Warp();
				}
			}
			else if ( missionPicker.pickerMode == PickerMode.Embedded )//embedded inside custom campaign package
			{
				//mission is already loaded into DataStore.mission and stringified
				if ( DataStore.mission != null )
				{
					//set the translation, if there is one, and inject it into the Mission data
					//var translation = RunningCampaign.campaignStructure.GetTranslatedMission();
					////automatically skips if it's null
					//TranslationController.Instance.SetMissionTranslation( translation, DataStore.mission );

					Warp();
				}
			}
			else if ( missionPicker.pickerMode == PickerMode.BuiltIn )//official mission
			{
				string stringified = "";
				//load the Mission, set it into the DataStore
				Mission m = FileManager.LoadMissionFromResource( setupOptions.projectItem.missionID, out stringified );
				DataStore.mission = m;

				if ( DataStore.mission != null )
				{
					DataStore.sagaSessionData.missionStringified = stringified;

					//if ( setupOptions.projectItem.hasTranslation )
					//{
					//	//set the translation, if there is one, and inject it into the Mission data
					//	var translation = FileManager.GetOfficialMissionTranslation( setupOptions.projectItem );
					//	//automatically skips if it's null
					//	TranslationController.Instance.SetMissionTranslation( translation, DataStore.mission );
					//}
					//else
					//	Debug.Log( $"OnStartMission()::No Mission translation found for '{DataStore.mission.missionProperties.missionName}'\nPATH: Languages/{DataStore.Language}/Missions/{setupOptions.projectItem.expansion}/{setupOptions.projectItem.fileName}_{DataStore.Language}" );

					Warp();
				}
				else
					errorPanel.Show( "StartMission()", $"Could not load mission:\n'{setupOptions.projectItem.missionID}'" );
			}
		}

		public void AddHero()
		{
			sound.PlaySound( FX.Click );
			addHeroPanel.Show( CharacterType.Hero, () =>
			 {
				 UpdateHeroes();
				 EventSystem.current.SetSelectedGameObject( heroButton.gameObject );
			 } );
		}

		public void AddAlly()
		{
			sound.PlaySound( FX.Click );

			if ( DataStore.sagaSessionData.selectedAlly == null )
			{
				addHeroPanel.Show( CharacterType.Ally, () =>
				{
					UpdateHeroes();
					EventSystem.current.SetSelectedGameObject( allyButton.gameObject );
				} );
			}
			else
			{
				DataStore.sagaSessionData.selectedAlly = null;
				allyImage.gameObject.SetActive( false );
			}
		}

		public void OnIgnored()
		{
			sound.PlaySound( FX.Click );
			modifyGroupsPanel.Show( GroupSelectionMode.Ignored, disabledIgnoredGroups, () =>
			{
				int impCount = DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Imperial ).Count() - DataStore.sagaSessionData.globalImportedCharacters.Count;
				languageController.UpdateIgnoredCount( DataStore.sagaSessionData.MissionIgnored.Count + impCount );
				EventSystem.current.SetSelectedGameObject( ignoredButton.gameObject );
			} );
		}

		public void OnVillains()
		{
			sound.PlaySound( FX.Click );
			modifyGroupsPanel.Show( GroupSelectionMode.Villains, null, () =>
			{
				languageController.UpdateVillainCount( DataStore.sagaSessionData.EarnedVillains.Count );
				EventSystem.current.SetSelectedGameObject( villainsButton.gameObject );
			} );
		}

		public void RemoveHero( DeploymentCard card )
		{
			DataStore.sagaSessionData.MissionHeroes.Remove( card );
			UpdateHeroes();
		}

		void UpdateHeroes()
		{
			for ( int i = 0; i < heroContainer.childCount; i++ )
			{
				Destroy( heroContainer.GetChild( i ).gameObject );
			}
			foreach ( var item in DataStore.sagaSessionData.MissionHeroes )
			{
				var mug = Instantiate( miniMugPrefab, heroContainer );
				mug.GetComponent<MiniMug>().Init( item );
			}
			if ( DataStore.sagaSessionData.MissionHeroes.Count > 0 )
				heroContainer.parent.GetChild( 2 ).gameObject.SetActive( false );
			else
				heroContainer.parent.GetChild( 2 ).gameObject.SetActive( true );

			//ally
			if ( DataStore.sagaSessionData.selectedAlly == null )
				allyImage.gameObject.SetActive( false );
			else
			{
				allyImage.gameObject.SetActive( true );
				allyImage.sprite = Resources.Load<Sprite>( DataStore.sagaSessionData.selectedAlly.mugShotPath );
			}
		}

		/// <summary>
		/// Official mission selected, pi is guaranteed not null
		/// </summary>
		public void OnOfficialMissionSelected( ProjectItem pi )
		{
			//clear ignored groups, banned allies, fixed ally, and custom heroes/allies
			DataStore.sagaSessionData.MissionIgnored.Clear();
			DataStore.sagaSessionData.BannedAllies.Clear();
			missionCustomAllies.Clear();
			missionCustomHeroes.Clear();
			missionCustomVillains.Clear();
			disabledIgnoredGroups.Clear();
			fixedAlly = null;
			//remove any embedded villains from another Mission, otherwise they won't appear
			DataStore.sagaSessionData.EarnedVillains = DataStore.sagaSessionData.EarnedVillains.Where( x => !x.id.Contains( "TC" ) ).ToList();

			var expansion = pi.missionID.Split( ' ' )[0].ToLower();
			var id = pi.missionID.Split( ' ' )[1].ToLower();
			//var presets = DataStore.missionPresets[expansion];
			//var mp = presets.Where( x => x.id.ToLower() == $"{expansion}{id}" ).FirstOr( null );

			//ignore "Other" expansion enemy groups by default, except owned packs
			var ignored = new HashSet<DeploymentCard>( DataStore.deploymentCards.Where( x => x.expansion == "Other" && !DataStore.ownedFigurePacks.ContainsCard( x ) ) );

			//add ignored enemies, banned allies and custom heroes/allies
			Mission m = FileManager.LoadMissionFromString( pi.stringifiedMission );
			if ( m != null )
			{
				//add ignored enemies
				var ign = m.missionProperties.bannedGroups.Select( x => { return DataStore.deploymentCards.GetDeploymentCard( x ); } );
				ign.ToList().ForEach( x => ignored.Add( x ) );
				//add ignored from mission
				disabledIgnoredGroups.AddRange( ign );
				SetMissionIgnored( ignored );

				//set default threat
				threatValue.ResetWheeler( 2 );

				//add custom allies/heroes/villains embedded in Mission
				missionCustomAllies = m.customCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Ally ).Select( x => x.deploymentCard ).ToList();
				missionCustomHeroes = m.customCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Hero ).Select( x => x.deploymentCard ).ToList();
				missionCustomVillains = m.customCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Villain ).Select( x => x.deploymentCard ).ToList();

				//set the fixed ally from the mission, if there is one
				if ( m.missionProperties.useFixedAlly == YesNoAll.Yes )
					fixedAlly = m.missionProperties.fixedAlly;

				//set additional info
				//sanity check that the index is within range of the loaded mission card data
				int idx = Mathf.Clamp( int.Parse( id ) - 1, 0, DataStore.missionCards[m.missionProperties.missionID.Split( ' ' )[0]].Count );
				var mcard = DataStore.missionCards[m.missionProperties.missionID.Split( ' ' )[0]][idx] ?? null;
				if ( !string.IsNullOrEmpty( m.missionProperties.additionalMissionInfo ) )
					additionalInfoText.text = m.missionProperties.additionalMissionInfo;
				else
					additionalInfoText.text = mcard?.bonusText ?? "";

				if ( m.missionProperties.useBannedAlly == YesNoAll.Yes )
					DataStore.sagaSessionData.BannedAllies.Add( m.missionProperties.bannedAlly );
				else if ( m.missionProperties.useBannedAlly == YesNoAll.Multi )
					DataStore.sagaSessionData.BannedAllies.AddRange( m.missionProperties.multipleBannedAllies );
				else if ( m.missionProperties.useBannedAlly == YesNoAll.All )
					DataStore.sagaSessionData.BannedAllies.AddRange( DataStore.allyCards.Select( x => x.id ) );
			}
			else
				errorPanel.Show( "OnMissionSelected()", "Mission is null" );
		}

		/// <summary>
		/// Custom mission selected, pi is guaranteed not null
		/// </summary>
		public void OnCustomMissionSelected( ProjectItem pi )
		{
			//clear ignored groups, banned allies, and custom heroes/allies
			DataStore.sagaSessionData.MissionIgnored.Clear();
			DataStore.sagaSessionData.BannedAllies.Clear();
			missionCustomAllies.Clear();
			missionCustomHeroes.Clear();
			missionCustomVillains.Clear();
			disabledIgnoredGroups.Clear();
			fixedAlly = null;
			//remove any embedded villains from another Mission, otherwise they won't appear
			DataStore.sagaSessionData.EarnedVillains = DataStore.sagaSessionData.EarnedVillains.Where( x => !x.id.Contains( "TC" ) ).ToList();

			//set default values to UI - they don't exist in a custom mission
			threatValue.ResetWheeler( 2 );

			Mission m = FileManager.LoadMissionFromString( pi.stringifiedMission );
			if ( m != null )
			{
				//add custom allies/heroes/villains embedded in Mission
				missionCustomAllies = m.customCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Ally ).Select( x => x.deploymentCard ).ToList();
				missionCustomHeroes = m.customCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Hero ).Select( x => x.deploymentCard ).ToList();
				missionCustomVillains = m.customCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Villain ).Select( x => x.deploymentCard ).ToList();

				//set the fixed ally from the mission, if there is one
				fixedAlly = m.missionProperties.fixedAlly;

				//set additional info
				additionalInfoText.text = m.missionProperties.additionalMissionInfo ?? "";

				//ignore "Other" expansion enemy groups by default, except owned packs
				var ignored = new HashSet<DeploymentCard>( DataStore.deploymentCards.Where( x => x.expansion == "Other" && !DataStore.ownedFigurePacks.ContainsCard( x ) ) );
				//get ignored from mission, even if they are owned packs
				var ign = from c in DataStore.deploymentCards join i in m.missionProperties.bannedGroups on c.id equals i select c;
				ign.ToList().ForEach( x => ignored.Add( x ) );
				//add ignored from mission
				disabledIgnoredGroups.AddRange( ign );
				SetMissionIgnored( ignored );

				//add banned allies
				if ( m.missionProperties.useBannedAlly == YesNoAll.Yes )
					DataStore.sagaSessionData.BannedAllies.Add( m.missionProperties.bannedAlly );
				else if ( m.missionProperties.useBannedAlly == YesNoAll.Multi )
					DataStore.sagaSessionData.BannedAllies.AddRange( m.missionProperties.multipleBannedAllies );
				else if ( m.missionProperties.useBannedAlly == YesNoAll.All )
					DataStore.sagaSessionData.BannedAllies.AddRange( DataStore.allyCards.Select( x => x.id ) );
			}
			else
				errorPanel.Show( "OnMissionSelected()", $"Could not load mission:\n'{pi.fullPathWithFilename}'" );
		}

		public void OnModeChange( PickerMode pmode )
		{
			if ( pmode == PickerMode.Custom )
			{
				descriptionTextBox.gameObject.SetActive( true );
				viewMissionCardButton.gameObject.SetActive( false );
			}
			else
			{
				descriptionTextBox.gameObject.SetActive( false );
				viewMissionCardButton.gameObject.SetActive( true );
			}
		}

		public void OnDifficulty( Button thisButton )
		{
			sound.PlaySound( FX.Click );
			difficultyText.text = setupOptions.ToggleDifficulty();
		}

		public void OnAdaptiveDifficulty( Button thisButton )
		{
			sound.PlaySound( FX.Click );
			setupOptions.useAdaptiveDifficulty = !setupOptions.useAdaptiveDifficulty;
			//thisButton.colors = setupOptions.useAdaptiveDifficulty ? greenBlock : redBlock;
			//ColorBlock cb = thisButton.colors;
			//cb.selectedColor = new Color( .6f, .4f, .1f );
			//thisButton.colors = cb;
			adaptiveText.color = setupOptions.useAdaptiveDifficulty ? greenBlock.normalColor : redBlock.normalColor;
			EnsureAiBadge(); // TACTICAL AI panel switch (playtest 27.10)
		}

		/// <summary>
		/// TACTICAL AI (concept 4.2.7): per-mission 3-state selector for the setup
		/// screen - wire a Button to this handler (editor step) and optionally the
		/// label to aiModeText. While the global switch is OFF this only prints
		/// LEGACY; no planner behaviour is enabled anywhere (Default OFF rule).
		/// </summary>
		public Text aiModeText;

		public void OnAiMode( Button thisButton )
		{
			sound.PlaySound( FX.Click );
			string label = setupOptions.ToggleAiMode();
			if ( aiModeText != null )
				aiModeText.text = label;
			Debug.Log( $"OnAiMode()::aiMode = {setupOptions.aiMode}" );
		}

		public void OnViewMissionCard()
		{
			if ( InputManager.Instance.anyPanelsOpen )
				return;

			if ( missionPicker.pickerMode == PickerMode.BuiltIn )
			{
				sound.PlaySound( FX.Click );

				MissionCard mc;
				string mID = missionPicker.selectedMission.missionID.Replace( " ", "" );
				foreach ( var key in DataStore.missionCards.Keys )
				{
					mc = DataStore.missionCards[key].Where( x => x.id.ToLower() == mID.ToLower() ).FirstOr( null );
					if ( mc != null )
					{
						missionCardZoom.Show( mc );
						break;
					}
				}
			}
			else if ( missionPicker.pickerMode == PickerMode.Embedded )
			{
				sound.PlaySound( FX.Click );

				var campaignStructureMission = RunningCampaign.sagaCampaign.campaignStructure.FirstOrDefault( x => x.missionID == missionPicker.selectedMission.missionID );

				if ( campaignStructureMission != null )
				{
					global::MissionType missionType;
					if ( Enum.IsDefined( typeof( global::MissionType ), campaignStructureMission.missionType.ToString() ) )
						missionType = (global::MissionType)Enum.Parse( typeof( global::MissionType ), campaignStructureMission.missionType.ToString() );
					else
						missionType = global::MissionType.Story;

					MissionCard mc = new MissionCard
					{
						missionType = new global::MissionType[] { missionType },
						descriptionText = missionPicker.selectedMission.Description,
						bonusText = "",
						name = missionPicker.selectedMission.Title,
						tagsText = new string[] { },
						rebelRewardText = "",
						imperialRewardText = "",
						heroText = "",
						villainText = "",
						allyText = "",
						hero = "",
						villain = new string[] { },
						ally = new string[] { },
						page = 0,
						expansionText = RunningCampaign.sagaCampaign.campaignImportedName,
						timePeriod = new int[] { },
						expansion = Expansion.Other
					};

					missionCardZoom.Show( mc );
				}
			}
		}

		public void Warp()
		{
			sound.PlaySound( FX.Click );
			sound.PlaySound( 3 );
			sound.FadeOutMusic();

			if ( PlayerPrefs.GetInt( "skipWarpIntro" ) == 1 )
			{
				faderCG.DOFade( 0, 2.5f ).OnComplete( () =>
				{
					SceneManager.LoadScene( "Saga" );
				} );
				return;
			}

			thrusterRoot.DOMoveZ( -30, 2 );
			warpTitleText.text = setupOptions.projectItem.Title;

			faderCG.interactable = false;
			faderCG.DOFade( 0, .5f ).OnComplete( () =>
			{
				sound.PlaySound( 1 );
				sound.PlaySound( 2 );

				planetSprite.sprite = planetSpritePool[UnityEngine.Random.Range( 0, planetSpritePool.Length )];

				GlowTimer.SetTimer( 1.5f, () => warpEffect.SetActive( true ) );
				GlowTimer.SetTimer( 5, () =>
				{
					DOTween.To( () => theCamera.fieldOfView, x => theCamera.fieldOfView = x, 0, .25f )
					.OnComplete( () =>
					{
						planet.gameObject.SetActive( true );
						warpEffect.SetActive( false );
						theCamera.fieldOfView = 60;

						Sequence sequence = DOTween.Sequence();
						Tween t1 = warpTitleText.DOFade( 1, 2 );
						Tween t2 = warpTitleText.transform.DOMove( warpTitleText.transform.position + warpTitleText.transform.up * 100f, 5 );
						Tween t3 = warpTitleText.DOFade( 0, 2 );
						Tween t4 = planetSprite.DOFade( 0, 2f );
						Tween p = planet.DOMoveZ( 0, .1f ).OnComplete( () =>
						{
							planet.DOMoveZ( -5, 10 ).SetEase( Ease.OutCubic );
						} );
						//play the animation sequence
						sequence
						.Join( t1 )
						.Join( t2 )
						.Join( p )
						.Append( t3 )
						.Join( t4 )
						.OnComplete( () =>
						{
							//load the main game after it finishes
							SceneManager.LoadScene( "Saga" );
						} );
					} );
				} );
			} );
		}

		public void OnHelpClick()
		{
			helpPanel.Show( () => { EventSystem.current.SetSelectedGameObject( helpButton.gameObject ); } );
		}

		public void OnAdditionalinfoClick()
		{
			if ( missionPicker.selectedMission != null
				&& !string.IsNullOrEmpty( additionalInfoText.text ) )
			{
				var go = Instantiate( textBoxPrefab, textBoxPopupBase.transform );
				var tb = go.transform.Find( "TextBox" ).GetComponent<TextBox>();
				textBoxPopupBase.ShowNoZoom();
				tb.Show( additionalInfoText.text, () => textBoxPopupBase.Close() );
			}
		}

		private void Update()
		{
			if ( missionPicker.selectedMission != null
				&& !missionPicker.isBusy )
			{
				allyButton.interactable = true;
				allyBackgroundImage.color = allyEnabledColor;
			}
			else
			{
				allyButton.interactable = false;
				allyBackgroundImage.color = allyDisabledColor;
			}

			if ( DataStore.sagaSessionData.MissionHeroes.Count > 0
				&& missionPicker.selectedMission != null
				&& !missionPicker.isBusy )
			{
				startMissionButton.interactable = true;
			}
			else
			{
				startMissionButton.interactable = false;
			}

			if ( missionPicker.selectedMission != null
				&& !missionPicker.isBusy )
			{
				viewMissionCardButton.interactable = true;
				//set navigation for difficulty and adaptive buttons
				difficultyButton.navigation = new Navigation()
				{
					mode = Navigation.Mode.Explicit,
					selectOnUp = viewMissionCardButton,
					selectOnDown = threatButton
				};
				adaptiveButton.navigation = new Navigation()
				{
					mode = Navigation.Mode.Explicit,
					selectOnUp = viewMissionCardButton,
					selectOnDown = additionalButton,
					selectOnLeft = difficultyButton
				};
			}
			else
			{
				viewMissionCardButton.interactable = false;
				//set navigation for difficulty and adaptive buttons
				difficultyButton.navigation = new Navigation()
				{
					mode = Navigation.Mode.Explicit,
					selectOnUp = null,
					selectOnDown = threatButton,
				};
				adaptiveButton.navigation = new Navigation()
				{
					mode = Navigation.Mode.Explicit,
					selectOnUp = null,
					selectOnDown = additionalButton,
					selectOnLeft = difficultyButton
				};
			}

			//M11f: when the tactical badge sits in the difficulty slot it takes
			//over the difficulty button's explicit navigation edges.
			if ( setupAiBadgeGo != null && setupAiBadgeGo.activeInHierarchy && TacticalControlsActive() )
			{
				var badgeBtn = setupAiBadgeGo.GetComponent<Button>();
				if ( badgeBtn != null )
					badgeBtn.navigation = new Navigation()
					{
						mode = Navigation.Mode.Explicit,
						selectOnUp = viewMissionCardButton.interactable ? viewMissionCardButton : null,
						selectOnDown = threatButton,
					};
			}

			InputFocusCheck();
		}

		private void InputFocusCheck()
		{
			//only process main screen if no panels are active
			if ( InputManager.Instance.anyPanelsOpen )
				return;

			if ( InputManager.Instance.cancelPressed )
			{
				OnCancel();
				return;
			}

			GameObject selected = EventSystem.current.currentSelectedGameObject;

			//set default selected button
			if ( selected == null )
			{
				if ( lastSelected != null )
					lastSelected.Select();
				else
				{
					//M11f: in tactical mode the difficulty button is hidden -
					//the badge holding its slot is the entry point instead.
					var fb = FirstFocusGameObject();
					if ( fb != null ) fb.GetComponent<Selectable>().Select();
				}
				selected = EventSystem.current.currentSelectedGameObject;
				return;
			}

			//handle changing value of threat level and additional threat wheels with controller/keyboard
			if ( InputManager.Instance.increaseValue )
			{
				if ( selected.name == "threatLevelSelectable" )
					threatValue.OnAdd();
				else if ( selected.name == "additionalSelectable" )
					addtlThreatValue.OnAdd();
			}
			else if ( InputManager.Instance.decreaseValue )
			{
				if ( selected.name == "threatLevelSelectable" )
					threatValue.OnSubtract();
				else if ( selected.name == "additionalSelectable" )
					addtlThreatValue.OnSubtract();
			}

			//handle pressing RIGHT NAV
			//avoid a race between built-in Navigation and this custom logic within the same input frame
			if ( InputManager.Instance.navRight )
			{
				//M11f: right from the tactical badge (either layout) goes to the
				//mission picker, like the adaptive button it replaces/hides.
				bool badgeSelected = setupAiBadgeGo != null && setupAiBadgeGo.activeInHierarchy
					&& selected == setupAiBadgeGo;
				if ( selected == difficultyButton.gameObject )
					EventSystem.current.SetSelectedGameObject( adaptiveButton.gameObject );
				else if ( selected == threatButton.gameObject )
					EventSystem.current.SetSelectedGameObject( additionalButton.gameObject );
				else if ( selected == ignoredButton.gameObject )
					EventSystem.current.SetSelectedGameObject( villainsButton.gameObject );
				else if ( badgeSelected
					|| selected == viewMissionCardButton.gameObject
					|| selected == adaptiveButton.gameObject
					|| selected == additionalButton.gameObject
					|| selected == villainsButton.gameObject
					|| selected == allyButton.gameObject
					|| selected == heroButton.gameObject )
				{
					lastSelected = selected.GetComponent<Selectable>();
					missionPicker.SetInputFocus();
				}
			}
		}

		public void OnThreatLevelClick()
		{
			//threatLevelWheel.DoClick();
		}

		public void OnAdditionalThreatClick()
		{
			//addtlThreatWheel.DoClick();
		}
	}
}
