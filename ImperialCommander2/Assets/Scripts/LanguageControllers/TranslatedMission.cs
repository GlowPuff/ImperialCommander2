using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Saga;

public interface ITranslatedEventAction
{
	public Guid GUID { get; set; }
	public EventActionType eventActionType { get; set; }
	string eaName { get; set; }
	List<string> Validate( ITranslatedEventAction loadedEA, bool useLooseValidation = false );
}

public class TranslatedEntityProperties
{
	public string entityName { get; set; }
	public Guid GUID { get; set; }
	public string theText { get; set; } = "";
	public List<TranslatedGUIDText> buttonList { get; set; } = new List<TranslatedGUIDText>();
	public TranslatedEntityProperties() { }
}

/// <summary>
/// used for buttons, input prompt items
/// </summary>
public class TranslatedGUIDText
{
	public Guid GUID { get; set; }
	public string theText { get; set; }
}

/// <summary>
/// The main translation container for missions props, events, entities, initial groups
/// </summary>
public sealed class TranslatedMission
{
	public string languageID;
	public TranslatedMissionProperties missionProperties { get; set; }
	public List<TranslatedEvent> events { get; set; } = new List<TranslatedEvent>();
	public List<TranslatedMapEntity> mapEntities { get; set; } = new List<TranslatedMapEntity>();
	public List<TranslatedInitialGroup> initialGroups { get; set; } = new List<TranslatedInitialGroup>();

	/// <summary>
	/// make this class only useable from within CreateTranslation()
	/// </summary>
	private TranslatedMission() { }

	public static TranslatedMission CreateTranslation( Mission mission )
	{
		TranslatedMission translatedMission = new TranslatedMission();
		translatedMission.languageID = mission.languageID;

		//mission properties
		translatedMission.missionProperties = new TranslatedMissionProperties( mission );

		//events
		foreach ( var e in mission.GetAllEvents().Where( x => x.GUID != Guid.Empty ) )
		{
			translatedMission.events.Add( new TranslatedEvent( e ) );
		}

		//entities
		foreach ( var e in mission.mapEntities )
		{
			var me = new TranslatedMapEntity()
			{
				entityName = e.name,
				GUID = e.GUID,
				mainText = e.entityProperties.theText
			};
			foreach ( var item in e.entityProperties.buttonActions )
			{
				me.buttonList.Add( new TranslatedGUIDText() { GUID = item.GUID, theText = item.buttonText } );
			}
			translatedMission.mapEntities.Add( me );
		}

		//initial groups
		foreach ( var item in mission.initialDeploymentGroups )
		{
			var dg = new TranslatedInitialGroup() { customInstructions = item.customText, cardName = item.cardName };
			translatedMission.initialGroups.Add( dg );
		}

		return translatedMission;
	}

}

public class TranslatedMissionProperties
{
	public string missionName { get; set; }
	public string missionDescription { get; set; }
	public string missionInfo { get; set; }
	public string campaignName { get; set; }
	public string startingObjective { get; set; }
	public string repositionOverride { get; set; }
	public string additionalMissionInfo { get; set; }

	public TranslatedMissionProperties()
	{

	}

	public TranslatedMissionProperties( Mission mission )
	{
		missionName = mission.missionProperties.missionName;
		missionDescription = mission.missionProperties.missionDescription;
		missionInfo = mission.missionProperties.missionInfo;
		campaignName = mission.missionProperties.campaignName;
		startingObjective = mission.missionProperties.startingObjective;
		repositionOverride = mission.missionProperties.changeRepositionOverride?.theText;
		additionalMissionInfo = mission.missionProperties.additionalMissionInfo;
	}
}

public class TranslatedInitialGroup
{
	public string cardName;
	public string customInstructions { get; set; }
}

public class TranslatedMapEntity
{
	public string entityName { get; set; }
	public Guid GUID { get; set; }
	public string mainText { get; set; }
	public List<TranslatedGUIDText> buttonList { get; set; } = new List<TranslatedGUIDText>();
}

public class TranslatedEvent
{
	public string eventName { get; set; }
	public Guid GUID { get; set; }
	public string eventText { get; set; }
	[JsonConverter( typeof( TranslatedEventActionConverter ) )]
	public List<ITranslatedEventAction> eventActions { get; set; }

	public TranslatedEvent() { }

	public TranslatedEvent( MissionEvent ev )
	{
		eventName = ev.name;
		GUID = ev.GUID;
		eventText = ev.eventText;
		eventActions = new List<ITranslatedEventAction>();
		foreach ( var item in ev.eventActions )
		{
			switch ( item.eventActionType )
			{
				case EventActionType.M2:
					eventActions.Add( new TranslatedModifyMapEntity( item ) );
					break;
				case EventActionType.D1:
					eventActions.Add( new TranslatedEnemyDeployment( item ) );
					break;
				case EventActionType.G9:
					eventActions.Add( new TranslatedInputPrompt( item ) );
					break;
				case EventActionType.G7:
					eventActions.Add( new TranslatedTextBox( item ) );
					break;
				case EventActionType.G2:
					eventActions.Add( new TranslatedChangeMissionInfo( item ) );
					break;
				case EventActionType.G3:
					eventActions.Add( new TranslatedChangeObjective( item ) );
					break;
				case EventActionType.G6:
					eventActions.Add( new TranslatedQuestionPrompt( item ) );
					break;
				case EventActionType.D2:
					eventActions.Add( new TranslatedAllyDeployment( item ) );
					break;
				case EventActionType.GM1:
					eventActions.Add( new TranslatedChangeGroupInstructions( item ) );
					break;
				case EventActionType.GM4:
					eventActions.Add( new TranslatedChangeRepositionInstructions( item ) );
					break;
				case EventActionType.D6:
					eventActions.Add( new TranslatedCustomEnemyDeployment( item ) );
					break;
				case EventActionType.GM2:
					eventActions.Add(new TranslatedChangeTarget(item));
					break;
			}
		}
	}
}

#region event action models
public class TranslatedModifyMapEntity : ITranslatedEventAction//M2
{
	public List<TranslatedEntityProperties> translatedEntityProperties { get; set; } = new List<TranslatedEntityProperties>();
	public Guid GUID { get; set; }
	public EventActionType eventActionType { get; set; }
	public string eaName { get; set; }

	public TranslatedModifyMapEntity()
	{

	}

	public TranslatedModifyMapEntity( IEventAction ea )
	{
		eaName = ea.displayName;
		GUID = ea.GUID;
		eventActionType = ea.eventActionType;
		foreach ( var item in ((ModifyMapEntity)ea).entitiesToModify )
		{
			var tep = new TranslatedEntityProperties();
			tep.entityName = item.entityProperties.name;
			tep.GUID = item.GUID;
			tep.theText = item.entityProperties.theText;
			foreach ( var btn in item.entityProperties.buttonActions )
			{
				tep.buttonList.Add( new TranslatedGUIDText() { GUID = btn.GUID, theText = btn.buttonText } );
			}
			translatedEntityProperties.Add( tep );
		}
	}

	public List<string> Validate( ITranslatedEventAction loadedEA, bool useLooseValidation = false )
	{
		var problems = new List<string>();

		//for loose validation, we can't check GUID
		if ( !useLooseValidation )
		{
			for ( int entityIdx = 0; entityIdx < (loadedEA as TranslatedModifyMapEntity).translatedEntityProperties.Count; entityIdx++ )
			{
				//check if each TranslatedEntityProperties exists in source
				var validEntity = translatedEntityProperties.Where( x => x.GUID == (loadedEA as TranslatedModifyMapEntity).translatedEntityProperties[entityIdx].GUID ).FirstOr( null );
				if ( validEntity != null )
				{
					//entity exists, set props
					var loadedEntity = (loadedEA as TranslatedModifyMapEntity).translatedEntityProperties[entityIdx];

					//set text
					validEntity.theText = loadedEntity.theText;

					//check buttons
					for ( int buttonIdx = 0; buttonIdx < loadedEntity.buttonList.Count; buttonIdx++ )
					{
						//check if button exists
						var validButton = validEntity.buttonList.Where( x => x.GUID == loadedEntity.buttonList[buttonIdx].GUID ).FirstOr( null );
						if ( validButton != null )
						{
							//button exists, set props
							validButton.theText = loadedEntity.buttonList[buttonIdx].theText;
						}
						else
							problems.Add( $"For loaded Event Action '{loadedEA.eaName}', Entity Button '{loadedEntity.buttonList[buttonIdx].GUID}' doesn't exist in the source data, ignored" );
					}
				}
				else
					problems.Add( $"For loaded Event Action '{loadedEA.eaName}', Entity '{(loadedEA as TranslatedModifyMapEntity).translatedEntityProperties[entityIdx].GUID}' doesn't exist in the source data, ignored" );
			}
		}
		else//loose validation
		{
			//when loading a converted translation, entities in this event action type will have a different GUID than expected, so just iterate each item, copy text and their buttons without matching first, and hope for the best
			for ( int entityIdx = 0; entityIdx < Math.Min( (loadedEA as TranslatedModifyMapEntity).translatedEntityProperties.Count, translatedEntityProperties.Count ); entityIdx++ )
			{
				var loadedEntity = (loadedEA as TranslatedModifyMapEntity).translatedEntityProperties[entityIdx];

				//set text
				translatedEntityProperties[entityIdx].theText = loadedEntity.theText;

				//copy button text over
				for ( int buttonIdx = 0; buttonIdx < Math.Min( loadedEntity.buttonList.Count, translatedEntityProperties[entityIdx].buttonList.Count ); buttonIdx++ )
				{
					translatedEntityProperties[entityIdx].buttonList[buttonIdx].theText = loadedEntity.buttonList[buttonIdx].theText;
				}
			}
		}

		return problems;
	}
}

public class TranslatedEnemyDeployment : ITranslatedEventAction//D1
{
	//customText in this model is "enemyGroupData.customText" in the EnemyDeployment model
	public string enemyName { get; set; }
	public string customText { get; set; }
	public string modification { get; set; }
	public string repositionInstructions { get; set; }
	public Guid GUID { get; set; }
	public EventActionType eventActionType { get; set; }
	public string evName { get; set; }
	public string eaName { get; set; }

	public TranslatedEnemyDeployment()
	{

	}

	public TranslatedEnemyDeployment( IEventAction ea )
	{
		eaName = ea.displayName;
		GUID = ea.GUID;
		eventActionType = ea.eventActionType;
		enemyName = ((EnemyDeployment)ea).enemyName;
		customText = ((EnemyDeployment)ea).enemyGroupData.customText;
		modification = ((EnemyDeployment)ea).modification;
		repositionInstructions = ((EnemyDeployment)ea).repositionInstructions;
	}

	public List<string> Validate( ITranslatedEventAction loadedEA, bool useLooseValidation = false )
	{
		var problems = new List<string>();

		var props = typeof( TranslatedEnemyDeployment ).GetProperties();
		foreach ( var prop in props )
		{
			var transProp = typeof( TranslatedEnemyDeployment ).GetProperty( prop.Name ).GetValue( loadedEA );
			if ( transProp is string )
				typeof( TranslatedEnemyDeployment ).GetProperty( prop.Name ).SetValue( this, transProp );
		}

		return problems;
	}
}

public class TranslatedInputPrompt : ITranslatedEventAction//G9
{
	public string mainText { get; set; }
	public string failText { get; set; }
	public List<TranslatedGUIDText> inputList { get; set; } = new List<TranslatedGUIDText>();
	public Guid GUID { get; set; }
	public EventActionType eventActionType { get; set; }
	public string eaName { get; set; }

	public TranslatedInputPrompt()
	{

	}

	public TranslatedInputPrompt( IEventAction ea )
	{
		eaName = ea.displayName;
		GUID = ea.GUID;
		eventActionType = ea.eventActionType;
		mainText = ((InputPrompt)ea).theText;
		failText = ((InputPrompt)ea).failText;
		foreach ( var item in ((InputPrompt)ea).inputList )
		{
			inputList.Add( new TranslatedGUIDText() { GUID = item.GUID, theText = item.theText } );
		}
	}

	public List<string> Validate( ITranslatedEventAction loadedEA, bool useLooseValidation = false )
	{
		var problems = new List<string>();

		mainText = (loadedEA as TranslatedInputPrompt).mainText;
		failText = (loadedEA as TranslatedInputPrompt).failText;

		if ( !useLooseValidation )
		{
			for ( int i = 0; i < (loadedEA as TranslatedInputPrompt).inputList.Count; i++ )
			{
				//check if each inputList TranslatedGUIDText exists in source
				var validInput = inputList.Where( x => x.GUID == (loadedEA as TranslatedInputPrompt).inputList[i].GUID ).FirstOr( null );
				if ( validInput != null )
				{
					//input exists, set props
					var loadedInput = (loadedEA as TranslatedInputPrompt).inputList[i];
					validInput.theText = loadedInput.theText;
				}
				else
					problems.Add( $"For loaded Event Action '{loadedEA.eaName}', Input Item '{(loadedEA as TranslatedInputPrompt).inputList[i].GUID}' doesn't exist in the source data, ignored" );
			}
		}
		else//loose validation
		{
			//when loading a converted translation, input items in this event action type will have a different GUID, so just iterate each item, copy text without matching first, and hope for the best
			for ( int i = 0; i < Math.Min( (loadedEA as TranslatedInputPrompt).inputList.Count, inputList.Count ); i++ )
			{
				inputList[i].theText = (loadedEA as TranslatedInputPrompt).inputList[i].theText;
			}
		}

		return problems;
	}
}

public class TranslatedTextBox : ITranslatedEventAction//G7
{
	public string tbText { get; set; }
	public Guid GUID { get; set; }
	public EventActionType eventActionType { get; set; }
	public string eaName { get; set; }

	public TranslatedTextBox()
	{

	}

	public TranslatedTextBox( IEventAction ea )
	{
		eaName = ea.displayName;
		GUID = ea.GUID;
		eventActionType = ea.eventActionType;
		tbText = ((ShowTextBox)ea).theText;
	}

	public List<string> Validate( ITranslatedEventAction loadedEA, bool useLooseValidation = false )
	{
		var problems = new List<string>();

		tbText = (loadedEA as TranslatedTextBox).tbText;

		return problems;
	}
}

public class TranslatedChangeMissionInfo : ITranslatedEventAction//G2
{
	public string theText { get; set; }
	public Guid GUID { get; set; }
	public EventActionType eventActionType { get; set; }
	public string eaName { get; set; }

	public TranslatedChangeMissionInfo()
	{

	}

	public TranslatedChangeMissionInfo( IEventAction ea )
	{
		eaName = ea.displayName;
		GUID = ea.GUID;
		eventActionType = ea.eventActionType;
		theText = ((ChangeMissionInfo)ea).theText;
	}

	public List<string> Validate( ITranslatedEventAction loadedEA, bool useLooseValidation = false )
	{
		var problems = new List<string>();

		theText = (loadedEA as TranslatedChangeMissionInfo).theText;

		return problems;
	}
}

public class TranslatedChangeObjective : ITranslatedEventAction//G3
{
	public string shortText { get; set; }
	public string longText { get; set; }
	public Guid GUID { get; set; }
	public EventActionType eventActionType { get; set; }
	public string eaName { get; set; }

	public TranslatedChangeObjective()
	{

	}

	public TranslatedChangeObjective( IEventAction ea )
	{
		eaName = ea.displayName;
		GUID = ea.GUID;
		eventActionType = ea.eventActionType;
		shortText = ((ChangeObjective)ea).theText;
		longText = ((ChangeObjective)ea).longText;
	}

	public List<string> Validate( ITranslatedEventAction loadedEA, bool useLooseValidation = false )
	{
		var problems = new List<string>();

		shortText = (loadedEA as TranslatedChangeObjective).shortText;
		longText = (loadedEA as TranslatedChangeObjective).longText;

		return problems;
	}
}

public class TranslatedQuestionPrompt : ITranslatedEventAction//G6
{
	public string mainText { get; set; }
	public List<TranslatedGUIDText> buttonList { get; set; } = new List<TranslatedGUIDText>();
	public Guid GUID { get; set; }
	public EventActionType eventActionType { get; set; }
	public string eaName { get; set; }

	public TranslatedQuestionPrompt()
	{

	}

	public TranslatedQuestionPrompt( IEventAction ea )
	{
		eaName = ea.displayName;
		GUID = ea.GUID;
		eventActionType = ea.eventActionType;
		mainText = ((QuestionPrompt)ea).theText;
		foreach ( var item in ((QuestionPrompt)ea).buttonList )
		{
			buttonList.Add( new TranslatedGUIDText() { GUID = item.GUID, theText = item.buttonText } );
		}
	}

	public List<string> Validate( ITranslatedEventAction loadedEA, bool useLooseValidation = false )
	{
		var problems = new List<string>();

		mainText = (loadedEA as TranslatedQuestionPrompt).mainText;

		if ( !useLooseValidation )
		{
			//check buttons
			for ( int buttonIdx = 0; buttonIdx < (loadedEA as TranslatedQuestionPrompt).buttonList.Count; buttonIdx++ )
			{
				//check if button exists
				var validButton = buttonList.Where( x => x.GUID == (loadedEA as TranslatedQuestionPrompt).buttonList[buttonIdx].GUID ).FirstOr( null );
				if ( validButton != null )
				{
					//button exists, set props
					validButton.theText = (loadedEA as TranslatedQuestionPrompt).buttonList[buttonIdx].theText;
				}
				else
					problems.Add( $"For loaded Event Action '{loadedEA.eaName}', Button '{(loadedEA as TranslatedQuestionPrompt).buttonList[buttonIdx].GUID}' doesn't exist in the source data, ignored" );
			}
		}
		else//loose validation
		{
			//when loading a converted translation, entities in this event action type will have a different GUID than expected, so just iterate each item, copy text and their buttons without matching first, and hope for the best
			for ( int buttonIdx = 0; buttonIdx < Math.Min( (loadedEA as TranslatedQuestionPrompt).buttonList.Count, buttonList.Count ); buttonIdx++ )
			{
				buttonList[buttonIdx].theText = (loadedEA as TranslatedQuestionPrompt).buttonList[buttonIdx].theText;
			}
		}

		return problems;
	}
}

public class TranslatedAllyDeployment : ITranslatedEventAction//D2
{
	public string customName { get; set; }
	public Guid GUID { get; set; }
	public EventActionType eventActionType { get; set; }
	public string eaName { get; set; }

	public TranslatedAllyDeployment()
	{

	}

	public TranslatedAllyDeployment( IEventAction ea )
	{
		eaName = ea.displayName;
		GUID = ea.GUID;
		eventActionType = ea.eventActionType;
		customName = ((AllyDeployment)ea).allyName;

	}

	public List<string> Validate( ITranslatedEventAction loadedEA, bool useLooseValidation = false )
	{
		var problems = new List<string>();

		customName = (loadedEA as TranslatedAllyDeployment).customName;

		return problems;
	}
}

public class TranslatedChangeGroupInstructions : ITranslatedEventAction//GM1
{
	public string newInstructions { get; set; }
	public Guid GUID { get; set; }
	public EventActionType eventActionType { get; set; }
	public string eaName { get; set; }

	public TranslatedChangeGroupInstructions()
	{

	}

	public TranslatedChangeGroupInstructions( IEventAction ea )
	{
		eaName = ea.displayName;
		GUID = ea.GUID;
		eventActionType = ea.eventActionType;
		newInstructions = ((ChangeInstructions)ea).theText;
	}

	public List<string> Validate( ITranslatedEventAction loadedEA, bool useLooseValidation = false )
	{
		var problems = new List<string>();

		newInstructions = (loadedEA as TranslatedChangeGroupInstructions).newInstructions;

		return problems;
	}
}

public class TranslatedChangeRepositionInstructions : ITranslatedEventAction//GM4
{
	public string repositionText { get; set; }
	public Guid GUID { get; set; }
	public EventActionType eventActionType { get; set; }
	public string eaName { get; set; }

	public TranslatedChangeRepositionInstructions()
	{

	}

	public TranslatedChangeRepositionInstructions( IEventAction ea )
	{
		eaName = ea.displayName;
		GUID = ea.GUID;
		eventActionType = ea.eventActionType;
		repositionText = ((ChangeReposition)ea).theText;
	}

	public List<string> Validate( ITranslatedEventAction loadedEA, bool useLooseValidation = false )
	{
		var problems = new List<string>();

		repositionText = (loadedEA as TranslatedChangeRepositionInstructions).repositionText;

		return problems;
	}
}

public class TranslatedChangeTarget : ITranslatedEventAction//GM2
{
	public string otherTarget { get; set; }
	public Guid GUID { get; set; }
	public EventActionType eventActionType { get; set; }
	public string eaName { get; set; }

	public TranslatedChangeTarget()
	{

	}

	public TranslatedChangeTarget(IEventAction ea)
	{
		eaName = ea.displayName;
		GUID = ea.GUID;
		eventActionType = ea.eventActionType;
		otherTarget = ((ChangeReposition)ea).theText;
	}

	public List<string> Validate(ITranslatedEventAction loadedEA, bool useLooseValidation = false)
	{
		var problems = new List<string>();

		otherTarget = (loadedEA as TranslatedChangeTarget).otherTarget;

		return problems;
	}
}

public class TranslatedCustomEnemyDeployment : ITranslatedEventAction//D6
{
	public Guid GUID { get; set; }
	public EventActionType eventActionType { get; set; }
	public string eaName { get; set; }

	//customText is custom instructions
	public string repositionInstructions { get; set; }
	public string surges { get; set; }
	public string bonuses { get; set; }
	public string keywords { get; set; }
	public string abilities { get; set; }
	public string customText { get; set; }
	public string cardName { get; set; }

	public TranslatedCustomEnemyDeployment()
	{

	}
	public TranslatedCustomEnemyDeployment( IEventAction ea )
	{

	}

	public List<string> Validate( ITranslatedEventAction loadedEA, bool useLooseValidation = false )
	{
		var problems = new List<string>();

		var props = typeof( TranslatedEnemyDeployment ).GetProperties();
		foreach ( var prop in props )
		{
			var transProp = typeof( TranslatedEnemyDeployment ).GetProperty( prop.Name ).GetValue( loadedEA );
			if ( transProp is string )
				typeof( TranslatedEnemyDeployment ).GetProperty( prop.Name ).SetValue( this, transProp );
		}

		return problems;
	}
}
#endregion