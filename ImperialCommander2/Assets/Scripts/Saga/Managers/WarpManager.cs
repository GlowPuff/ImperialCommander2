using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WarpManager : MonoBehaviour
{
	public Camera theCamera;
	public GameObject warpEffect;
	public TextMeshProUGUI titleText;

	Sound sound;

	void Awake()
	{
		sound = FindObjectOfType<Sound>();
		sound.PlayMusicAndMenuAmbient();

		float pixelHeightOfCurrentScreen = Screen.height;//.currentResolution.height;
		float pixelWidthOfCurrentScreen = Screen.width;//.currentResolution.width;
		float aspect = pixelWidthOfCurrentScreen / pixelHeightOfCurrentScreen;
		if ( aspect < 1.7f )//less than 16:9, such as 16:10 and 4:3
		{
			titleText.GetComponent<RectTransform>().SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, 175 );
		}

		Warp();
	}

	public void Warp()
	{
		float timer = 3;

		if ( DataStore.sagaSessionData != null
			&& DataStore.sagaSessionData.setupOptions.isTutorial )
			titleText.text = DataStore.mission.missionProperties.missionName;
		else if ( DataStore.sagaSessionData != null )
		{
			//get translated mission name
			var card = DataStore.GetMissionCard( DataStore.sagaSessionData.setupOptions.projectItem.missionID );
			if ( card != null )//official mission
				titleText.text = card.name;
			else//custom mission
				titleText.text = DataStore.sagaSessionData.setupOptions.projectItem.Title;
		}
		else
			titleText.text = "";

		titleText.transform.DOMove( titleText.transform.position + titleText.transform.up * 100f, 5 );
		titleText.DOFade( 1, 2 );

		sound.PlaySound( 1 );
		sound.PlaySound( 2 );

		GlowTimer.SetTimer( 1.5f, () => warpEffect.SetActive( true ) );
		GlowTimer.SetTimer( 5, () =>
		{
			DOTween.To( () => theCamera.fieldOfView, x => theCamera.fieldOfView = x, 0, .25f )
			.OnComplete( () =>
			{
				//all effects/music finish, load the mission
				GlowTimer.SetTimer( timer, () =>
				{
					SceneManager.LoadScene( "Saga" );
				} );
			} );
		} );
	}
}
