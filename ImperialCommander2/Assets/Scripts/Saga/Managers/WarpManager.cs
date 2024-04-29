using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WarpManager : MonoBehaviour
{
	public Camera theCamera;
	public GameObject warpEffect;
	public TextMeshProUGUI warpTitleText;
	public Transform planet;
	public SpriteRenderer planetSprite;
	public Sprite[] planetSpritePool;
	public bool isDebug = false;

	Sound sound;

	void Awake()
	{
		//if skipping warp intro, just load into the mission
		if ( PlayerPrefs.GetInt( "skipWarpIntro" ) == 1 )
		{
			SceneManager.LoadScene( "Saga" );
			return;
		}

		sound = FindObjectOfType<Sound>();
		sound.PlayMusicAndMenuAmbient();

		float pixelHeightOfCurrentScreen = Screen.height;//.currentResolution.height;
		float pixelWidthOfCurrentScreen = Screen.width;//.currentResolution.width;
		float aspect = pixelWidthOfCurrentScreen / pixelHeightOfCurrentScreen;
		if ( aspect < 1.7f )//less than 16:9, such as 16:10 and 4:3
		{
			warpTitleText.GetComponent<RectTransform>().SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, 175 );
		}

		Warp();
	}

	public void Warp()
	{
		if ( DataStore.sagaSessionData != null
			&& DataStore.sagaSessionData.setupOptions.isTutorial )
			warpTitleText.text = DataStore.mission.missionProperties.missionName;
		else if ( DataStore.sagaSessionData != null )
		{
			//get translated mission name
			var card = DataStore.GetMissionCard( DataStore.sagaSessionData.setupOptions.projectItem.missionID );
			if ( card != null )//official mission
				warpTitleText.text = card.name;
			else//custom mission
				warpTitleText.text = DataStore.sagaSessionData.setupOptions.projectItem.Title;
		}
		else
			warpTitleText.text = "";

		if ( isDebug )
			warpTitleText.text = "Debug";

		sound.PlaySound( 1 );
		sound.PlaySound( 2 );

		planetSprite.sprite = planetSpritePool[Random.Range( 0, planetSpritePool.Length )];

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
					if ( !isDebug )
						SceneManager.LoadScene( "Saga" );
				} );
			} );
		} );
	}
}
