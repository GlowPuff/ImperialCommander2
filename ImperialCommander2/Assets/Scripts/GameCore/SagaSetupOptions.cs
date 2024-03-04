namespace Saga
{
	public class SagaSetupOptions
	{
		/*
    difficulty
    adaptive difficulty
    hero selection
    ally selection
    initial threat level / additional threat
    earned villains
    ignored groups
*/
		public Difficulty difficulty;
		public bool useAdaptiveDifficulty;
		public int threatLevel;
		public int addtlThreat;
		public ProjectItem projectItem;
		public bool isTutorial;
		public bool isDebugging;//testing a mission from the command line should not save state
		public int tutorialIndex;

		public SagaSetupOptions()
		{
			Reset();
		}

		public void Reset()
		{
			projectItem = null;
			difficulty = Difficulty.Medium;
			useAdaptiveDifficulty = false;
			threatLevel = 2;
			addtlThreat = 0;
			isTutorial = false;
			isDebugging = false;
			tutorialIndex = 0;
		}

		public string ToggleDifficulty()
		{
			if ( difficulty == Difficulty.Easy )
				difficulty = Difficulty.Medium;
			else if ( difficulty == Difficulty.Medium )
				difficulty = Difficulty.Hard;
			else
				difficulty = Difficulty.Easy;

			if ( difficulty == Difficulty.Easy )
				return DataStore.uiLanguage.uiSetup.easy.ToUpper();
			else if ( difficulty == Difficulty.Medium )
				return DataStore.uiLanguage.uiSetup.normal.ToUpper();
			else
				return DataStore.uiLanguage.uiSetup.hard.ToUpper();
		}
	}
}
