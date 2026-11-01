using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Saga
{
	public class CampaignManager : MonoBehaviour
	{
		[HideInInspector]
		public SagaCampaign sagaCampaign;

		public Fader fader;
		public VolumeProfile volume;
		public GameObject leftPanel, rightPanel, switchButton;
		//prefabs
		public GameObject forceMissionItemPrefab, listItemPrefab, missionItemPrefab, customAddMissionBarPrefab;
		//UI
		public CampaignJournal campaignJournal;
		public AddCampaignItemPopup addCampaignItemPopup;
		public ModifyCustomPropsPopup modifyCustomPropsPopup;
		public TMP_InputField campaignNameInputField;
		public Transform villainContainer, allyContainer, itemContainer, structureContainer, rewardContainer;
		public CampaignHeroPrefab[] heroPrefabs;
		public TextMeshProUGUI xpText, creditsText, fameText, awardsText;
		public MWheelHandler xpWheel, creditsWheel, fameWheel, awardsWheel;
		public Text campaignExpansion;
		public CanvasGroup topButtonsGroup, rightPanelGroup;
		public ValueAdjuster valueAdjuster;
		public Image expansionLogo;
		public Sprite[] expansionSprites;
		public HelpPanel helpPanel;
		public GameObject campaignHelpMockup;
		public ErrorPanel errorPanel;
		public Button campaignInfoButton;
		//translatable UI
		public CampaignLanguageController languageController;

		public bool isDebugMode = false;

		[HideInInspector]
		public Sound sound;
		int view = 1;//0=left, 1=right
		float aspect;

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

		private void Awake()
		{
			//Exception handling for any Unity thrown exception, such as from asset management
			Application.logMessageReceived += LogCallback;
		}

		void Start()
		{
			Debug.Log( "ENTERING CAMPAIGN MANAGER" );

			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

			Screen.sleepTimeout = SleepTimeout.NeverSleep;

			float pixelHeightOfCurrentScreen = Screen.height;//.currentResolution.height;
			float pixelWidthOfCurrentScreen = Screen.width;//.currentResolution.width;
			aspect = pixelWidthOfCurrentScreen / pixelHeightOfCurrentScreen;
			// M13c2 (Thomas 30.10.): the base game's small-screen single-view
			// mode (mission list hidden, 'switch view button' shown whenever
			// aspect < 1.7) is RETIRED for this screen: the mission list and
			// the hero information are ALWAYS shown together, the switch
			// button never appears. Campaign-manager dual layout exactly as
			// the game showed it on wide screens before; the IMP bar button
			// (Thomas: keep as is) is untouched.
			if ( switchButton != null ) switchButton.SetActive( false );
			leftPanel.SetActive( true );
			campaignHelpMockup.SetActive( true );

			FileManager.SetupDefaultFolders();

			//re-initialize all card data, otherwise deployed cards and other items carry over when coming back from a campaign game
			DataStore.InitData();

			//BOOTSTRAP CAMPAIGN
			bootstrapCampaign( !isDebugMode );//true = production build

			//sagaCampaign is set at this point

			fader.UnFade( 2 );

			//apply settings
			sound = FindObjectOfType<Sound>();
			sound.PlayMusicAndMenuAmbient();

			//set translated UI
			languageController.SetTranslatedUI();

			if ( volume.TryGet<Bloom>( out var bloom ) )
				bloom.active = PlayerPrefs.GetInt( "bloom2" ) == 1;
			if ( volume.TryGet<Vignette>( out var vig ) )
				vig.active = PlayerPrefs.GetInt( "vignette" ) == 1;

			//popuplate UI with loaded campaign data
			InitUI();
		}

		void bootstrapCampaign( bool isProduction )
		{
			Debug.Log( $"***BOOTSTRAP (Campaign Manager) PRODUCTION={isProduction}***" );

			//campaign is already setup from Title screen
			if ( RunningCampaign.sagaCampaignGUID != null && RunningCampaign.sagaCampaignGUID != Guid.Empty )
			{
				sagaCampaign = SagaCampaign.LoadCampaignState( RunningCampaign.sagaCampaignGUID );
				//sagaCampaign.FixExpansionCodes();
			}
			else//error or debugging, setup new test campaign
			{
				RunningCampaign.expansionCode = "Core";
				sagaCampaign = SagaCampaign.CreateNewCampaign( "Error/Debug - Not Found", RunningCampaign.expansionCode );
			}

			//set the campaign into the persistent RunningCampaign structure
			RunningCampaign.sagaCampaign = sagaCampaign;
		}

		/// <summary>
		/// fill in name and hero/item/villain/ally lists from campaign data
		/// </summary>
		private void InitUI()
		{
			if ( sagaCampaign.campaignType == CampaignType.Custom )
				campaignInfoButton.gameObject.SetActive( false );

			campaignNameInputField.text = sagaCampaign.campaignName;
			//use translated expansion name
			if ( sagaCampaign.campaignType == CampaignType.Official )
			{
				campaignExpansion.text = DataStore.translatedExpansionNames[sagaCampaign.campaignExpansionCode];
				//expansion icon
				expansionLogo.gameObject.SetActive( true );
				expansionLogo.sprite = expansionSprites[(int)Enum.Parse( typeof( Expansion ), sagaCampaign.campaignExpansionCode )];
			}
			else if ( sagaCampaign.campaignType == CampaignType.Custom )
			{
				campaignExpansion.text = DataStore.uiLanguage.uiCampaign.customCampaign;
				//expansion icon
				expansionLogo.gameObject.SetActive( true );
				expansionLogo.sprite = expansionSprites[8];//rebel icon
			}
			else if ( sagaCampaign.campaignType == CampaignType.Imported )
			{
				var packages = FileManager.GetCampaignPackageList( false );
				var p = packages.Where( x => x.GUID == sagaCampaign.campaignPackage.GUID ).FirstOr( null );
				if ( p != null )
				{
					//expansion icon
					Texture2D tex = new Texture2D( 2, 2 );
					if ( tex.LoadImage( p.iconBytesBuffer ) )
					{
						expansionLogo.gameObject.SetActive( true );
						Sprite iconSprite = Sprite.Create( tex, new Rect( 0, 0, tex.width, tex.height ), new Vector2( 0, 0 ), 100f );
						expansionLogo.sprite = iconSprite;
					}

					//checking if campaign/missions names should be updated to current language
					var currentLanguageIntro = p.campaignStructure.Where( x => x.missionType == MissionType.Introduction ).FirstOr( null );
					var loadedIntro = sagaCampaign.campaignStructure.Where( x => x.missionType == MissionType.Introduction ).FirstOr( null );
					if ( currentLanguageIntro != null && loadedIntro != null && currentLanguageIntro.projectItem.Title != loadedIntro.projectItem.Title )
					{
						sagaCampaign.campaignImportedName = p.campaignName;

						foreach ( var item in sagaCampaign.campaignStructure )
						{
							var currentLanguageItem = p.campaignMissionItems.Where( x => x.missionGUID.ToString() == item.missionID ).FirstOr( null );
							if ( currentLanguageItem != null )
							{
								item.projectItem.Title = currentLanguageItem.missionName;
								item.projectItem.Description = currentLanguageItem.mission.missionProperties.missionDescription;
								item.projectItem.AdditionalInfo = currentLanguageItem.mission.missionProperties.additionalMissionInfo;
							}
						}

						foreach ( var item in sagaCampaign.campaignPackage.campaignMissionItems )
						{
							var currentLanguageItem = p.campaignMissionItems.Where( x => x.missionGUID == item.missionGUID ).FirstOr( null );
							if ( currentLanguageItem != null )
							{
								item.missionName = currentLanguageItem.missionName;
							}
						}
					}
					else
					{
						var introMissionItem = p.campaignMissionItems.Where( x => x.missionGUID.ToString() == loadedIntro.missionID ).FirstOr( null );
						if ( introMissionItem != null )
						{
							loadedIntro.projectItem.Description = introMissionItem.mission.missionProperties.missionDescription;
							loadedIntro.projectItem.AdditionalInfo = introMissionItem.mission.missionProperties.additionalMissionInfo;
						}
					}
				}
				else
					Utils.LogError( $"CampaignManager::InitUI()::The original Campaign Package is either missing or was manipulated, and no longer exists in its original form.\nCampaign Name: [{sagaCampaign.campaignImportedName}], GUID= {sagaCampaign.campaignPackage.GUID}" );

				campaignExpansion.text = sagaCampaign.campaignImportedName;
			}

			creditsWheel.ResetWheeler( sagaCampaign.credits );
			xpWheel.ResetWheeler( sagaCampaign.XP );
			fameWheel.ResetWheeler( sagaCampaign.fame );
			awardsWheel.ResetWheeler( sagaCampaign.awards );

			//items
			Dictionary<string, string> items = new Dictionary<string, string>();
			foreach ( var campaignItem in sagaCampaign.campaignItems )
			{
				if ( SagaCampaign.campaignDataItems.Any( x => x.id == campaignItem ) )
					items.Add( campaignItem, SagaCampaign.campaignDataItems.First( x => x.id == campaignItem ).name );
			}
			foreach ( var item in items.OrderBy( x => x.Value ) )
				AddItemToUI( sagaCampaign.GetItemFromID( item.Key ) );
			//M13k: Imperial class-deck info behind a bar button + scrollable
			//popup (the old inline section crowded the overview and overlapped
			//the rebel items list - Thomas playtest 29.10.)
			AddPersonaButtonToBar();
			//allies
			foreach ( var ally in sagaCampaign.campaignAllies.OrderBy( x => x.name ) )
				AddAllyToUI( ally );
			//villains
			foreach ( var villain in sagaCampaign.campaignVillains.OrderBy( x => x.name ) )
				AddVillainToUI( villain );
			//heroes
			int c = sagaCampaign.campaignHeroes.Count;
			for ( int i = 0; i < c; i++ )
			{
				heroPrefabs[i].AddHeroToUI( sagaCampaign.campaignHeroes[i] );
				heroPrefabs[i].mWheelHandler.valueAdjuster = valueAdjuster;
			}
			//rewards
			Dictionary<string, string> rewards = new Dictionary<string, string>();
			foreach ( var campaignReward in sagaCampaign.campaignRewards )
			{
				if ( SagaCampaign.campaignDataRewards.Any( x => x.id == campaignReward ) )
					rewards.Add( campaignReward, SagaCampaign.campaignDataRewards.First( x => x.id == campaignReward ).name );
			}
			foreach ( var reward in rewards.OrderBy( x => x.Value ) )
				AddRewardToUI( sagaCampaign.GetRewardFromID( reward.Key ) );
			//campaign structure
			foreach ( Transform item in structureContainer )
				Destroy( item.gameObject );
			//if it's a custom campaign, add the custom add mission bar
			if ( sagaCampaign.campaignType == CampaignType.Custom )
			{
				var cgo = Instantiate( customAddMissionBarPrefab, structureContainer );
			}
			//add campaign structure
			foreach ( var item in sagaCampaign.campaignStructure )
			{
				var go = Instantiate( missionItemPrefab, structureContainer );
				go.GetComponent<MissionItemPrefab>().Init( item, valueAdjuster, sagaCampaign );
			}
			//add forced mission bar
			if ( sagaCampaign.campaignExpansionCode != "Custom" && sagaCampaign.campaignExpansionCode != "Imported" )
			{
				//add 1 force mission item
				var fgo = Instantiate( forceMissionItemPrefab, structureContainer );
				fgo.GetComponent<ForceMissionItemPrefab>().Init( OnAddForcedMissionClick );
			}
		}

		#region UI callbacks from "AddCampaignItemPopup" window
		void AddAllyToUI( DeploymentCard a )
		{
			if ( a == null )
			{
				Utils.LogError( $"AddAllyToUI()::card is null" );
				return;
			}

			var index = sagaCampaign.campaignAllies.OrderBy( x => x.name ).ToList().FindIndex( x => x.name == a.name );

			var go = Instantiate( listItemPrefab, allyContainer );

			if ( index < allyContainer.childCount )
				go.transform.SetSiblingIndex( index );

			go.GetComponent<CampaignListItemPrefab>().InitAlly( a.name, ( n ) =>
			{
				sagaCampaign.campaignAllies.Remove( a );
				Destroy( go );
			} );
		}

		void AddVillainToUI( DeploymentCard v )
		{
			if ( v == null )
			{
				Utils.LogError( $"AddVillainToUI()::card is null" );
				return;
			}

			var index = sagaCampaign.campaignVillains.OrderBy( x => x.name ).ToList().FindIndex( x => x.name == v.name );

			var go = Instantiate( listItemPrefab, villainContainer );

			if ( index < villainContainer.childCount )
				go.transform.SetSiblingIndex( index );

			go.GetComponent<CampaignListItemPrefab>().InitVillain( v.name, ( n ) =>
			{
				sagaCampaign.campaignVillains.Remove( v );
				Destroy( go );
			} );
		}

		void AddItemToUI( CampaignItem item, bool showCoinIcon = false )
		{
			if ( item == null )
			{
				Utils.LogWarning( $"AddItemToUI()::item is null" );
				return;
			}

			List<string> items = new List<string>();
			foreach ( var campaignItem in sagaCampaign.campaignItems )
			{
				if ( SagaCampaign.campaignDataItems.Any( x => x.id == campaignItem ) )
					items.Add( SagaCampaign.campaignDataItems.First( x => x.id == campaignItem ).name );
			}

			var index = items.OrderBy( x => x ).ToList().FindIndex( x => x == item.name );

			var go = Instantiate( listItemPrefab, itemContainer );

			if ( index < itemContainer.childCount )
				go.transform.SetSiblingIndex( index );

			go.GetComponent<CampaignListItemPrefab>().InitGeneralItem( item.name, ( n ) =>
			{
				//add item cost back to credits
				var sc = FindObjectOfType<CampaignManager>();
				sc.UpdateCreditsUI( RunningCampaign.sagaCampaign.credits + item.cost );
				sc.sound.PlaySound( 1 );

				//remove item from hero items as well
				foreach ( var hero in sagaCampaign.campaignHeroes )
				{
					if ( hero.campaignItems.Any( x => x.id == item.id ) )
					{
						hero.campaignItems.Remove( hero.campaignItems.Where( x => x.id == item.id ).First() );

						int c = sagaCampaign.campaignHeroes.Count;
						for ( int i = 0; i < c; i++ )
						{
							if ( sagaCampaign.campaignHeroes[i].heroID == hero.heroID )
								heroPrefabs[i].AddHeroToUI( sagaCampaign.campaignHeroes[i] );
						}
					}
				}

				sagaCampaign.campaignItems.Remove( item.id );
				Destroy( go );
			} );
		}

		/// <summary>
		/// M13k (Thomas playtest 29.10.): the Imperial class-deck/agenda info
		/// moves off the crowded overview into a scrollable popup behind a new
		/// button inserted in the top-right bar, LEFT of the yellow "?" help
		/// button (clone of that button: same visual language, zero prefab/
		/// scene edits - the M12e runtime-build doctrine). Cloned onClick is
		/// replaced with a fresh ButtonClickedEvent so the inherited "show
		/// help" persistent call cannot fire; recolor + "IMP" overlay mark it
		/// apart. Hidden entirely when the feature or a deck is absent
		/// (classic campaigns render exactly as before).
		/// </summary>
		void AddPersonaButtonToBar()
		{
			try
			{
				// Personas (M1): the IMP button became the PERSONA control -
				// it shows the live persona line, a click cycles the choice.
				// Visible whenever the tactical AI plays (also at 'none':
				// the button is HOW you pick one).
				if ( !TacticalAI.TacticalAISettings.Enabled ) return;
				// M13o (Thomas Player-10.log): at this call the bar may not
				// be ACTIVE yet (GameObject.Find only sees activeInHierarchy)
				// - the run failed with "help button not found" and the
				// button never appeared. Locate + clone through a retry
				// coroutine that waits for the bar (and its first layout).
				if ( personaButtonCo == null ) personaButtonCo = StartCoroutine( TryAddPersonaButtonCo() );
			}
			catch ( Exception e )
			{
				Utils.LogWarning( "AddPersonaButtonToBar skipped: " + e.Message );
			}
		}

		Coroutine personaButtonCo;
		TMPro.TextMeshProUGUI personaButtonLabel;   // live persona line

		GameObject FindPersonaBarAnchor()
		{
			// M13b (Player-13/14/15): the scene names scanned before are
			// the SAGA GAME help names - the Campaign Manager's "?" is the
			// screen's own serialized campaignInfoButton reference, which
			// was never consulted here (why the hunt timed out every run).
			// Reference first; name scans only as fallback.
			var own = campaignInfoButton != null && campaignInfoButton.gameObject.scene.IsValid()
				? campaignInfoButton.gameObject : null;
			if ( own != null ) return own;
			var go = GameObject.Find( "Help Activation Button" ) ?? GameObject.Find( "info Button" );
			if ( go != null ) return go;
			// bar members can still be hidden at this moment: scan every
			// loaded scene Transform by NAME (M13aa lesson: these objects
			// can carry a name but NO Button component, so a Button-typed
			// scan finds nothing)
			foreach ( var t in Resources.FindObjectsOfTypeAll<Transform>() )
			{
				if ( t == null || !t.gameObject.scene.IsValid() ) continue;
				var n = t.name;
				if ( n == "Help Activation Button" || n == "info Button" ) return t.gameObject;
			}
			return null;
		}

		IEnumerator TryAddPersonaButtonCo()
		{
			GameObject helpGO = null;
			for ( int i = 0; i < 240 && helpGO == null; i++ )
			{
				helpGO = FindPersonaBarAnchor();
				if ( helpGO == null ) yield return new WaitForEndOfFrame();
			}
			var btn0 = helpGO != null ? helpGO.GetComponent<Button>() : null;
			if ( helpGO == null )
			{
				// M13b: abort ONLY on a missing anchor object - help
				// objects legitimately lack a Button component (M13aa),
				// the clone gets one added instead.
				Utils.LogWarning( "AddPersonaButtonToBar skipped: bar anchor not found after 4s" );
				personaButtonCo = null;
				yield break;
			}
			if ( btn0 == null ) Debug.Log( "AddPersonaButtonToBar::anchor has no Button component - clone gets a fresh one" );
			yield return AddPersonaButtonClone( helpGO );
			personaButtonCo = null;
		}

		IEnumerator AddPersonaButtonClone( GameObject helpGO )
		{
			var bar = helpGO.transform.parent;
			var clone = Instantiate( helpGO, bar );
			clone.name = "tacaiPersonaButton";
			clone.SetActive( true );
			// full detour from the clone's inherited help behaviour:
			// fresh event object, persistent calls cannot survive it
			// (M13b: help anchors can lack the component outright)
			var btn = clone.GetComponent<Button>() ?? clone.gameObject.AddComponent<Button>();
			btn.onClick = new Button.ButtonClickedEvent();
			btn.onClick.AddListener( OnPersonaButtonClicked );
			// visual difference: teal (the tactical-AI accent) + own label
			var img = clone.GetComponent<Image>();
			if ( img != null ) img.color = new Color( 0.40f, 0.85f, 0.75f, 1f );
			for ( int i = clone.transform.childCount - 1; i >= 0; i-- )
				Destroy( clone.transform.GetChild( i ).gameObject );
			var label = new GameObject( "label", typeof( RectTransform ) ).AddComponent<TMPro.TextMeshProUGUI>();
			label.transform.SetParent( clone.transform, false );
			var lrt = label.GetComponent<RectTransform>();
			lrt.anchorMin = Vector2.zero;
			lrt.anchorMax = Vector2.one;
			lrt.offsetMin = Vector2.zero;
			lrt.offsetMax = Vector2.one * -1f;
			label.font = TMPro.TMP_Settings.defaultFontAsset;
			label.fontSize = 20;
			label.fontStyle = TMPro.FontStyles.Bold;
			label.alignment = TMPro.TextAlignmentOptions.Center;
			label.color = new Color( 0.02f, 0.10f, 0.09f, 1f );
			label.enableWordWrapping = false;
			label.raycastTarget = false;
			label.text = PersonaButtonLabel();
			personaButtonLabel = label;
			// sit it left of the "?": a bar layout group re-flows it, a
			// manually placed bar needs the measured offset - so wait for
			// the anchor's REAL rect (M13o: no more zero-rect placement)
			var h = helpGO.transform as RectTransform;
			var c = clone.transform as RectTransform;
			bool barHasLayout = bar.GetComponent<UnityEngine.UI.LayoutGroup>() != null;
			if ( barHasLayout )
				c.SetSiblingIndex( h.GetSiblingIndex() );
			else
			{
				for ( int i = 0; i < 40 && h.rect.width <= 0f; i++ )
					yield return new WaitForEndOfFrame();
				c.anchorMin = h.anchorMin;
				c.anchorMax = h.anchorMax;
				c.pivot = h.pivot;
				c.sizeDelta = h.sizeDelta;
				c.anchoredPosition = h.anchoredPosition - new Vector2( h.rect.width + 10f, 0f );
			}
			Debug.Log( "AddPersonaButtonToBar::persona=" + TacticalAI.PersonaState.DescribeSelection( sagaCampaign ) );
		}

		GameObject personaPopup;   // at most one open persona popup

		/// <summary>Button click = cycle the campaign persona (the old IMP
		/// popup stays reachable through the long list? No: click CYCLES -
		/// one control, live label, the popup opens from the same click via
		/// the details toggle below). Every change is saved immediately,
		/// like every other campaign field.</summary>
		void OnPersonaButtonClicked()
		{
			try
			{
				var snd = UnityEngine.Object.FindObjectOfType<Sound>();
				if ( snd != null ) snd.PlaySound( FX.Click );
			}
			catch { }
			var next = TacticalAI.PersonaState.Cycle( sagaCampaign );
			if ( personaButtonLabel != null ) personaButtonLabel.text = PersonaButtonLabel();
			// an open popup is live: rebuild it on the new choice
			if ( personaPopup != null ) ShowPersonaPopup();
			Debug.Log( "PersonaButton::cycle -> " + next );
		}

		/// <summary>Short live label: the persona in effect, no long words
		/// (the bar button is small; full text lives in the popup).</summary>
		string PersonaButtonLabel()
		{
			try
			{
				var tl = DataStore.uiLanguage != null ? DataStore.uiLanguage.uiTacticalAI : null;
				string head = tl != null && !string.IsNullOrEmpty( tl.personaButton )
					? tl.personaButton : "PERSONA";
				var id = TacticalAI.PersonaState.EffectiveId( sagaCampaign );
				if ( string.IsNullOrEmpty( id ) || id == TacticalAI.PersonaState.ExplicitNone )
					return head + ": " + ( tl != null ? tl.personaNone : "none" );
				var name = TacticalAI.PersonaState.DisplayName( id );
				return head + ": " + name;
			}
			catch { return "PERSONA"; }
		}

		/// <summary>All persona info (choice, rank per mission at ITS own
		/// threat, motto + signature of the active persona) in one scrollable
		/// text popup; click beside the box or the close button dismisses it.
		/// Replaces the M12e class-deck popup 1:1 in look and place.</summary>
		void ShowPersonaPopup()
		{
			try
			{
				if ( personaPopup != null ) Destroy( personaPopup );
				if ( sagaCampaign == null ) return;
				var canvas = GameObject.Find( "Canvas" )?.transform;
				if ( canvas == null ) return;
				var tl = DataStore.uiLanguage != null ? DataStore.uiLanguage.uiTacticalAI : null;

				// full-screen dim catch layer: any click outside closes
				personaPopup = new GameObject( "tacaiPersonaPopup", typeof( RectTransform ) );
				personaPopup.transform.SetParent( canvas, false );
				var pr = personaPopup.GetComponent<RectTransform>();
				pr.anchorMin = Vector2.zero;
				pr.anchorMax = Vector2.one;
				pr.offsetMin = Vector2.zero;
				pr.offsetMax = Vector2.zero;
				var dim = personaPopup.AddComponent<Image>();
				dim.color = new Color( 0f, 0f, 0f, 0.55f );
				var dimBtn = personaPopup.AddComponent<Button>();
				dimBtn.transition = Selectable.Transition.None;
				dimBtn.onClick.AddListener( () => { if ( personaPopup != null ) Destroy( personaPopup ); } );

				var tl2 = DataStore.uiLanguage;
				// centred box
				var box = new GameObject( "box", typeof( RectTransform ) );
				box.transform.SetParent( personaPopup.transform, false );
				var brt = box.GetComponent<RectTransform>();
				brt.anchorMin = new Vector2( .5f, .5f );
				brt.anchorMax = new Vector2( .5f, .5f );
				brt.pivot = new Vector2( .5f, .5f );
				brt.sizeDelta = new Vector2( 760f, 560f );
				var bg = box.AddComponent<Image>();
				bg.color = new Color( 0.02f, 0.10f, 0.09f, 0.97f );

				// header (same localized line as the old section used)
				var headerGO = new GameObject( "header", typeof( RectTransform ) ).AddComponent<TMPro.TextMeshProUGUI>();
				headerGO.transform.SetParent( box.transform, false );
				var hrt = headerGO.GetComponent<RectTransform>();
				hrt.anchorMin = new Vector2( 0f, 1f );
				hrt.anchorMax = new Vector2( 1f, 1f );
				hrt.pivot = new Vector2( .5f, 1f );
				hrt.anchoredPosition = new Vector2( 0f, -10f );
				hrt.sizeDelta = new Vector2( -20f, 34f );
				headerGO.font = TMPro.TMP_Settings.defaultFontAsset;
				headerGO.fontSize = 20;
				headerGO.fontStyle = TMPro.FontStyles.Bold;
				headerGO.alignment = TMPro.TextAlignmentOptions.Center;
				headerGO.color = Color.white;
				headerGO.text = Utils.ReplaceGlyphs(
					tl != null && !string.IsNullOrEmpty( tl.personaPopupHeader ) ? tl.personaPopupHeader : "IMPERIAL PERSONA" );

				// scroll window
				var scrollGO = new GameObject( "scroll", typeof( RectTransform ) ).AddComponent<ScrollRect>();
				scrollGO.transform.SetParent( box.transform, false );
				var srt = scrollGO.GetComponent<RectTransform>();
				srt.anchorMin = new Vector2( 0f, 0f );
				srt.anchorMax = new Vector2( 1f, 1f );
				srt.offsetMin = new Vector2( 12f, 56f );
				srt.offsetMax = new Vector2( -12f, -50f );
				var viewport = new GameObject( "viewport", typeof( RectTransform ) );
				viewport.transform.SetParent( scrollGO.transform, false );
				var vrt = viewport.GetComponent<RectTransform>();
				vrt.anchorMin = Vector2.zero;
				vrt.anchorMax = Vector2.one;
				vrt.offsetMin = Vector2.zero;
				vrt.offsetMax = Vector2.zero;
				viewport.AddComponent<UnityEngine.UI.RectMask2D>();
				var contentGO = new GameObject( "content", typeof( RectTransform ) ).AddComponent<TMPro.TextMeshProUGUI>();
				contentGO.transform.SetParent( viewport.transform, false );
				var crt = contentGO.GetComponent<RectTransform>();
				crt.anchorMin = new Vector2( 0f, 1f );
				crt.anchorMax = new Vector2( 1f, 1f );
				crt.pivot = new Vector2( 0f, 1f );
				crt.anchoredPosition = Vector2.zero;
				contentGO.font = TMPro.TMP_Settings.defaultFontAsset;
				contentGO.fontSize = 12;
				contentGO.color = Color.white;
				contentGO.enableWordWrapping = true;
				contentGO.richText = true;
				contentGO.alignment = TMPro.TextAlignmentOptions.TopLeft;
				contentGO.text = Utils.ReplaceGlyphs( BuildPersonaPopupText() );
				contentGO.rectTransform.sizeDelta = new Vector2( 0f, contentGO.GetPreferredValues( contentGO.text, 700f, 0f ).y );
				crt.sizeDelta = new Vector2( 0f, Mathf.Max( crt.sizeDelta.y, contentGO.GetPreferredValues( contentGO.text, 700f, 0f ).y ) );
				scrollGO.horizontal = false;
				scrollGO.vertical = true;
				scrollGO.movementType = ScrollRect.MovementType.Clamped;
				scrollGO.viewport = vrt;
				scrollGO.content = crt;

				// close button (localized)
				var closeGO = new GameObject( "close", typeof( RectTransform ) );
				closeGO.transform.SetParent( box.transform, false );
				var clrt = closeGO.GetComponent<RectTransform>();
				clrt.anchorMin = new Vector2( .5f, 0f );
				clrt.anchorMax = new Vector2( .5f, 0f );
				clrt.pivot = new Vector2( .5f, 0f );
				clrt.anchoredPosition = new Vector2( 0f, 10f );
				clrt.sizeDelta = new Vector2( 180f, 36f );
				var cimg = closeGO.AddComponent<Image>();
				cimg.color = new Color( 1f, .62f, .2f, 1f );
				var cbtn = closeGO.AddComponent<Button>();
				cbtn.onClick.AddListener( () => { if ( personaPopup != null ) Destroy( personaPopup ); } );
				var closeLabel = new GameObject( "label", typeof( RectTransform ) ).AddComponent<TMPro.TextMeshProUGUI>();
				closeLabel.transform.SetParent( closeGO.transform, false );
				var cl2 = closeLabel.GetComponent<RectTransform>();
				cl2.anchorMin = Vector2.zero;
				cl2.anchorMax = Vector2.one;
				cl2.offsetMin = Vector2.zero;
				cl2.offsetMax = Vector2.zero;
				closeLabel.font = TMPro.TMP_Settings.defaultFontAsset;
				closeLabel.fontSize = 14;
				closeLabel.fontStyle = TMPro.FontStyles.Bold;
				closeLabel.alignment = TMPro.TextAlignmentOptions.Center;
				closeLabel.color = Color.white;
				closeLabel.raycastTarget = false;
				closeLabel.text = Utils.ReplaceGlyphs(
					tl2?.uiMainApp?.close != null ? tl2.uiMainApp.close.ToUpper() : "CLOSE" ).Replace( "\r", "" ).Replace( "\n", "" );

				personaPopup.transform.SetAsLastSibling();   // on top of every panel
				Debug.Log( "ShowPersonaPopup::persona=" + TacticalAI.PersonaState.DescribeSelection( sagaCampaign ) );
			}
			catch ( Exception e )
			{
				if ( personaPopup != null ) Destroy( personaPopup );
				Utils.LogWarning( "ShowPersonaPopup skipped: " + e.Message );
			}
		}

		/// <summary>Persona popup body (rich text): the current choice with
		/// motto, the persona signature (what it does to its troops), then ONE
		/// row per campaign mission - mission name, its threat budget and the
		/// rank the persona WILL play there (rank law: pure threat function,
		/// so it can be shown before the mission is even started). All
		/// ui.json vocabulary; printed-name doctrine does not apply (personas
		/// are app chrome, fully localized).</summary>
		string BuildPersonaPopupText()
		{
			var tl = DataStore.uiLanguage != null ? DataStore.uiLanguage.uiTacticalAI : null;
			var sb = new System.Text.StringBuilder();
			sb.Append( "<color=white><b>" ).Append(
				TacticalAI.PersonaState.DescribeSelection( sagaCampaign ) ).Append( "</b></color>\n" );

			var id = TacticalAI.PersonaState.EffectiveId( sagaCampaign );
			bool none = string.IsNullOrEmpty( id ) || id == TacticalAI.PersonaState.ExplicitNone;
			if ( none )
			{
				// 'none' explained once - the honest zero-state sentence
				sb.Append( "<color=#C9D6D3><i>" ).Append( PersonaChrome(
					tl != null ? tl.personaPlainNote : null,
					"Standard folgen = keine KI-Persona: die Imperiale Seite spielt sachneutral." ) )
				  .Append( "</i></color>\n" );
			}
			var def = none ? null : TacticalAI.PersonaState.Def( id );
			if ( def != null )
			{
				var motto = PersonaKeyText( def.MottoKey );
				if ( !string.IsNullOrEmpty( motto ) )
					sb.Append( "<color=#8FE3C8><i>" ).Append( motto ).Append( "</i></color>\n\n" );
				var sig = PersonaKeyText( PersonaSigKey( def ) );
				if ( !string.IsNullOrEmpty( sig ) )
					sb.Append( "<color=#C9D6D3>" ).Append( sig ).Append( "</color>\n\n" );
			}

			sb.Append( "<color=white><b>" ).Append( PersonaChrome(
				tl != null ? tl.personaRankColumn : null, "Rang je Mission" ) )
			  .Append( "</b></color>\n" );
			var structs = sagaCampaign.campaignStructure ?? new List<CampaignStructure>();
			foreach ( var cs in structs )
			{
				if ( cs == null ) continue;
				var v = TacticalAI.PersonaState.ResolveFor( id, cs.threatLevel );
				var name = PersonaMissionName( cs );
				string rankTxt = v.Rank > 0
					? RankChip( tl, v.Rank )
					: ( tl != null ? tl.personaNone : "none" );
				sb.Append( "<color=#C9D6D3>" ).Append( name ).Append( "</color>  " )
				  .Append( "<color=#8FE3C8>" ).Append( cs.threatLevel ).Append( "</color>  " )
				  .Append( "<color=orange><b>" ).Append( rankTxt ).Append( "</b></color>\n" );
			}
			if ( def == null )
				sb.Append( "\n<color=#C9D6D3><i>" ).Append( PersonaChrome(
					tl != null ? tl.personaCycleNote : null,
					"Klick auf die PERSONA-Schaltflaeche waehlt die Persona (KEINE = sachneutral)." ) )
				  .Append( "</i></color>" );
			return sb.ToString();
		}

		/// <summary>Popup chrome: ui.json text with the German dev fallback
		/// (same pattern as the settings dialog's Pick()).</summary>
		static string PersonaChrome( string localized, string fallback )
		{
			return string.IsNullOrEmpty( localized ) ? fallback : localized;
		}

		/// <summary>Mission display name for a structure row (the SAME
		/// translated source the mission list rows use - missionCards per
		/// expansion; custom/imported missions carry projectItem.Title).</summary>
		string PersonaMissionName( CampaignStructure cs )
		{
			try
			{
				if ( cs.projectItem != null && !string.IsNullOrEmpty( cs.projectItem.Title ) )
					return cs.projectItem.Title;
				if ( DataStore.missionCards != null && cs.expansionCode != null
					&& DataStore.missionCards.ContainsKey( cs.expansionCode ) )
				{
					var mc = DataStore.missionCards[cs.expansionCode]
						.FirstOrDefault( x => x.id == cs.missionID );
					if ( mc != null && !string.IsNullOrEmpty( mc.name ) ) return mc.name;
				}
			}
			catch { }
			return cs.missionID;
		}

		static string RankChip( UITacticalAI tl, int rank )
		{
			var fmt = tl != null && !string.IsNullOrEmpty( tl.personaRank ) ? tl.personaRank : "rank {0}";
			return fmt.Replace( "{0}", rank.ToString( System.Globalization.CultureInfo.InvariantCulture ) );
		}

		/// <summary>ui.json field lookup by key (motto/signaturText keys are
		/// asset data - the asset stays the validator, reflection like
		/// PersonaState.Localized).</summary>
		static string PersonaKeyText( string key )
		{
			if ( string.IsNullOrEmpty( key ) ) return null;
			var tl = DataStore.uiLanguage != null ? DataStore.uiLanguage.uiTacticalAI : null;
			if ( tl == null ) return null;
			var fi = typeof( UITacticalAI ).GetField( key );
			return fi == null ? null : fi.GetValue( tl ) as string;
		}

		/// <summary>The signature key of a persona def = the signaturText
		/// effect key its highest rank carries (Core.Personas keeps it as an
		/// effect row so the planner and the UI read the SAME key).</summary>
		static string PersonaSigKey( TacticalAI.Core.PersonaDef def )
		{
			if ( def == null || def.Ranks == null ) return null;
			foreach ( var r in def.Ranks )
				foreach ( var e in r.Effects ?? new List<TacticalAI.Core.PersonaEffect>() )
					if ( e.Kind == "signaturText" && !string.IsNullOrEmpty( e.Key ) ) return e.Key;
			return null;
		}

		void AddRewardToUI( CampaignReward item )
		{
			if ( item == null )
			{
				Utils.LogWarning( $"AddRewardToUI()::item is null" );
				return;
			}
			List<string> rewards = new List<string>();
			foreach ( var campaignReward in sagaCampaign.campaignRewards )
			{
				if ( SagaCampaign.campaignDataRewards.Any( x => x.id == campaignReward ) )
					rewards.Add( SagaCampaign.campaignDataRewards.First( x => x.id == campaignReward ).name );
			}

			var index = rewards.OrderBy( x => x ).ToList().FindIndex( x => x == item.name );

			var go = Instantiate( listItemPrefab, rewardContainer );

			if ( index < rewardContainer.childCount )
				go.transform.SetSiblingIndex( index );

			go.GetComponent<CampaignListItemPrefab>().InitGeneralItem( item.name, ( n ) =>
			{
				sagaCampaign.campaignRewards.Remove( item.id );
				Destroy( go );
			} );
		}

		public void OnXPChanged()
		{
			sagaCampaign.XP = xpWheel.wheelValue;
		}

		public void OnCreditsChanged()
		{
			sagaCampaign.credits = creditsWheel.wheelValue;
		}

		public void OnFameChanged()
		{
			sagaCampaign.fame = fameWheel.wheelValue;
		}

		public void OnAwardsChanged()
		{
			sagaCampaign.awards = awardsWheel.wheelValue;
		}
		#endregion

		#region ADD BUTTONS
		public void OnAddAlly()
		{
			addCampaignItemPopup.AddAlly( ( a ) =>
			{
				if ( !sagaCampaign.campaignAllies.Contains( a ) )
				{
					sagaCampaign.campaignAllies.Add( a );
					AddAllyToUI( a );
				}
			} );
		}

		public void OnAddVillain()
		{
			addCampaignItemPopup.AddVillain( ( v ) =>
			{
				if ( !sagaCampaign.campaignVillains.Contains( v ) )
				{
					sagaCampaign.campaignVillains.Add( v );
					AddVillainToUI( v );
				}
			} );
		}

		public void OnAddItem()
		{
			addCampaignItemPopup.AddItem( ( item ) =>
			{
				if ( !sagaCampaign.campaignItems.Contains( item.id ) )
				{
					sagaCampaign.campaignItems.Add( item.id );
					AddItemToUI( item );
				}
			}, false );
		}

		public void OnAddReward()
		{
			addCampaignItemPopup.AddReward( ( item ) =>
			{
				if ( !sagaCampaign.campaignRewards.Contains( item.id ) )
				{
					sagaCampaign.campaignRewards.Add( item.id );
					AddRewardToUI( item );
				}
			} );
		}
		#endregion

		public void AddHeroToCampaign( CampaignHero hero )
		{
			sagaCampaign.campaignHeroes.Add( hero );
		}

		public void RemoveHeroFromCampaign( CampaignHero hero )
		{
			sagaCampaign.campaignHeroes.Remove( hero );
		}

		public void OnAddForcedMissionClick()
		{
			addCampaignItemPopup.AddMission( sagaCampaign.campaignType, sagaCampaign.campaignExpansionCode, MissionType.Forced, AddForcedMission );
		}

		public void OnMissionNameClick( MissionType missionType, Action<MissionCard> callback )
		{
			addCampaignItemPopup.AddMission( sagaCampaign.campaignType, sagaCampaign.campaignExpansionCode, missionType, callback );
		}

		public void OnAddCustomMission( MissionType missionType )
		{
			modifyCustomPropsPopup.Show( new CampaignModify()
			{
				missionType = missionType,
				threatValue = 1,
				itemTierArray = new string[] { "1" },
				missionID = "",
				expansionCode = ""
			}, AddCustomMission );
		}

		public void OnModifyCustomMission( MissionItemPrefab missionItem )
		{
			CampaignModify modifier = new CampaignModify()
			{
				missionType = missionItem.campaignStructure.missionType,
				threatValue = missionItem.campaignStructure.threatLevel,
				itemTierArray = missionItem.campaignStructure.itemTier,
				agendaToggle = missionItem.campaignStructure.isAgendaMission,
				missionID = missionItem.campaignStructure.missionID,
				expansionCode = missionItem.campaignStructure.expansionCode
			};

			modifyCustomPropsPopup.Show( modifier, ( mod ) =>
			{
				missionItem.OnModify( mod );
				//go back to normal campaign list mode
				FindObjectOfType<CustomAddMissionBarPrefab>().DeactivateModifyMode();
			} );
		}

		void AddCustomMission( CampaignModify modifier )
		{
			var cs = new CampaignStructure()
			{
				missionType = modifier.missionType,
				missionID = modifier.missionID,
				threatLevel = modifier.threatValue,
				itemTier = modifier.itemTierArray,
				expansionCode = modifier.expansionCode,
				isAgendaMission = modifier.agendaToggle,
				missionSource = MissionSource.Custom
			};
			//add the newly added mission
			var go = Instantiate( missionItemPrefab, structureContainer );
			go.GetComponent<MissionItemPrefab>().Init( cs, valueAdjuster, sagaCampaign );
		}

		void AddForcedMission( MissionCard card )
		{
			//custom missions:
			//full path to custom mission is stored in 'hero'
			//additional info is stored in 'bonusText'

			string path = "";
			if ( card.id == "Custom" )
				path = card.hero;
			else
				path = $"{card.id.ToUpper()}-{card.name}";

			var cs = new CampaignStructure()
			{
				missionType = MissionType.Forced,
				missionID = card.id,
				threatLevel = 2,//default because of removal of preset dependence
				isForced = true,
				expansionCode = card.expansion.ToString(),
				missionSource = card.id == "Custom" ? MissionSource.Custom : MissionSource.Official,
				projectItem = new ProjectItem()
				{
					missionID = card.id,
					Description = card.descriptionText,
					AdditionalInfo = card.bonusText,
					fullPathWithFilename = path,
					Title = DataStore.missionCards[card.expansion.ToString()].Where( x => x.id == card.id ).FirstOr( new MissionCard() { name = card.name } )?.name
				}
			};
			//remove the "add forced mission" prefab
			foreach ( Transform item in structureContainer )
			{
				if ( item.GetComponent<ForceMissionItemPrefab>() != null )
					Destroy( item.gameObject );
			}
			//add the newly added mission
			var go = Instantiate( missionItemPrefab, structureContainer );
			go.GetComponent<MissionItemPrefab>().Init( cs, valueAdjuster, sagaCampaign );
			//add the "forced mission" prefab back to bottom of list
			var fgo = Instantiate( forceMissionItemPrefab, structureContainer );
			fgo.GetComponent<ForceMissionItemPrefab>().Init( OnAddForcedMissionClick );
		}

		public void RemoveMission( Guid GUID )
		{
			foreach ( Transform item in structureContainer )
			{
				var pf = item.GetComponent<MissionItemPrefab>();
				if ( pf != null )
				{
					if ( pf.campaignStructure.missionSource == MissionSource.Custom )
						FindObjectOfType<CustomAddMissionBarPrefab>()?.DeactivateModifyMode();
					if ( pf.campaignStructure.structureGUID == GUID )
						Destroy( item.gameObject );
				}
			}
		}

		public void ToggleDisableUI( bool disable )
		{
			topButtonsGroup.interactable = rightPanelGroup.interactable = disable;
			foreach ( Transform item in structureContainer )
			{
				var prefab = item.GetComponent<MissionItemPrefab>();
				if ( prefab != null )
				{
					prefab.ToggleModifyMode( !disable );
				}
			}
		}

		public void OnSaveCampaign()
		{
			var list = new List<CampaignStructure>();
			foreach ( Transform item in structureContainer )
			{
				var cs = item.GetComponent<MissionItemPrefab>();
				if ( cs != null )
					list.Add( cs.campaignStructure );
			}

			sagaCampaign.SaveCampaignState( list );
		}

		public void OnInfoClicked()
		{
			GlowEngine.FindUnityObject<CampaignMessagePopup>().Show( DataStore.uiLanguage.uiCampaign.campaignSetup, Utils.ReplaceGlyphs( sagaCampaign.GetCampaignInfo() ), 700 );
		}

		public void OnJournalClicked()
		{
			campaignJournal.Show( sagaCampaign.campaignJournal, ( text ) => sagaCampaign.campaignJournal = text );
		}

		public void OnSwitchViewClicked()
		{
			view = view == 0 ? 1 : 0;
			leftPanel.SetActive( view == 0 );
			rightPanel.SetActive( view == 1 );
			campaignHelpMockup.SetActive( view == 0 );
		}

		public void OnExitCampaignScreen()
		{
			//auto-save
			OnSaveCampaign();

			RunningCampaign.Reset();

			fader.Fade();
			float foo = 1;
			DOTween.To( () => foo, x => foo = x, 0, .5f ).OnComplete( () =>
			 SceneManager.LoadScene( "Title" ) );
		}

		public void OnEndEditCampaignName()
		{
			sagaCampaign.campaignName = campaignNameInputField.text;
		}

		public void StartMission( CampaignStructure campaignStructure )
		{
			//auto-save
			OnSaveCampaign();

			RunningCampaign.campaignStructure = campaignStructure;
			RunningCampaign.sagaCampaignGUID = sagaCampaign.GUID;

			fader.Fade();
			float foo = 1;
			DOTween.To( () => foo, x => foo = x, 0, .5f ).OnComplete( () =>
			 SceneManager.LoadScene( "SagaSetup" ) );
		}

		public void OnHelpClick()
		{
			helpPanel.Show();
		}

		public void UpdateCreditsUI( int newValue )
		{
			sagaCampaign.credits = newValue;
			creditsWheel.ResetWheeler( sagaCampaign.credits );
		}
	}
}