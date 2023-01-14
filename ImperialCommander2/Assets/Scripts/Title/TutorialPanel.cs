using Saga;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanel : MonoBehaviour
{
	public PopupBase popupBase;
	public Text startText, cancelText, titleText;
	public TextMeshProUGUI descriptionText, taglineText;

	int tutIndex;

	public void Show( int index )
	{
		startText.text = DataStore.uiLanguage.sagaUISetup.setupStartBtn.ToUpper();
		cancelText.text = DataStore.uiLanguage.uiSetup.cancel.ToUpper();
		tutIndex = index + 1;

		//try to load the mission in selected language
		var json = Resources.Load<TextAsset>( $"SagaTutorials/{DataStore.languageCodeList[DataStore.languageCode]}/TUTORIAL0{tutIndex}" );
		if ( json != null )
			DataStore.mission = FileManager.LoadMissionFromString( json.text );
		else//otherwise fall back to English
		{
			var ENjson = Resources.Load<TextAsset>( $"SagaTutorials/En/TUTORIAL0{tutIndex}" );
			if ( ENjson != null )
				DataStore.mission = FileManager.LoadMissionFromString( ENjson.text );
		}

		if ( DataStore.mission != null )
		{
			titleText.text = DataStore.mission.missionProperties.missionName.ToUpper();
			descriptionText.text = DataStore.mission.missionProperties.missionDescription;
			taglineText.text = DataStore.mission.missionProperties.additionalMissionInfo;
		}

		popupBase.Show();
	}

	public void Close()
	{
		popupBase.Close();
	}

	public void StartTutorial()
	{
		popupBase.Close();

		if ( DataStore.mission == null )
			return;

		//mission is loaded at this point
		var setupOptions = new SagaSetupOptions()
		{
			isTutorial = true,
			tutorialIndex = tutIndex,
			difficulty = Difficulty.Medium,
			threatLevel = 2,
		};
		DataStore.StartNewSagaSession( setupOptions );
		DataStore.sagaSessionData.MissionHeroes.Add( DataStore.heroCards[2] );
		DataStore.sagaSessionData.MissionHeroes.Add( DataStore.heroCards[4] );

		FindObjectOfType<TitleController>().StartTutorial();
	}
}
