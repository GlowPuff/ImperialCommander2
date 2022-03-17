public class MissionCard
{
	//public List<Card> story, side;
	public string name, id, hero, descriptionText, bonusText, heroText, allyText, villainText, expansionText, rebelRewardText, imperialRewardText;
	public MissionType[] missionType;
	public string[] ally, villain, tags, tagsText;
	public int page, influenceCost;
	public int[] timePeriod;
	public Expansion expansion;
}

//public struct Card
//{
//	public string name, id;
//}

/*
 		"id": "Core1",
		"name": "A New Threat",
		"type": ["Story"],
		"hero": "",
		"ally": [],
		"villain": [],
		"tags": [],
		"page": 24,
		"timePeriod": [],
		"influenceCost": "",
		"descriptionText": "<i>Rebel forces discover that the distress beacon was a distraction orchestrated by General Weiss, a rising star in the Imperial military.\nHis efforts have made him a person of interest to Rebel High Command. Operatives are searching the galaxy for information on this General's plans...</i>",
		"bonusText": "",
		"heroText": "",
		"allyText": "",
		"villainText": "",
		"tagsText": [],
		"expansionText": "Core Game",
		"rebelRewardText": "",
		"imperialRewardText": ""

DATA
		"id": "Core1",
		"missionType": ["Story"],
		"hero": "",
		"ally": [],
		"villain": [],
		"tags": [],
		"page": 24,
		"timePeriod": [],
		"influenceCost": 0,

TRANSLATION
		"id": "Core1",
		"name": "A New Threat",
		"descriptionText": "<i>Rebel forces discover that the distress beacon was a distraction orchestrated by General Weiss, a rising star in the Imperial military.\nHis efforts have made him a person of interest to Rebel High Command. Operatives are searching the galaxy for information on this General's plans...</i>",
		"bonusText": "",
		"heroText": "",
		"allyText": "",
		"villainText": "",
		"tagsText": [],
		"expansionText": "Core Game",
		"rebelRewardText": "",
		"imperialRewardText": ""

*/