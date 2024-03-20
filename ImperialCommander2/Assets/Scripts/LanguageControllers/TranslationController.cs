using System;
using System.Linq;
using UnityEngine;

namespace Saga
{
	public class TranslationController
	{
		private static TranslationController _instance;

		public static TranslationController Instance
		{
			get
			{
				if ( _instance is null )
					_instance = new TranslationController();

				return _instance;
			}
		}

		/// <summary>
		/// Injects the translation into the actual Mission data by matching GUIDs
		/// </summary>
		public void SetMissionTranslation( TranslatedMission translation, Mission mission )
		{
			//inject it into the Mission
			if ( mission != null && translation != null )
			{
				InjectTranslationIntoMission( translation, mission );
				Debug.Log( $"SetMissionTranslation::Mission translation set: {translation.languageID}" );
			}
			else
				Debug.Log( "SetMissionTranslation()::WARNING::mission or translation is null" );
		}

		private void InjectTranslationIntoMission( TranslatedMission translation, Mission mission )
		{
			try
			{
				if ( translation != null )
				{
					Debug.Log( "InjectTranslation()::Injecting translation into loaded Mission..." );

					//mission properties
					mission.missionProperties.startingObjective = translation.missionProperties.startingObjective;
					mission.missionProperties.missionInfo = translation.missionProperties.missionInfo;
					mission.missionProperties.additionalMissionInfo = translation.missionProperties.additionalMissionInfo;
					mission.missionProperties.missionInfo = translation.missionProperties.missionInfo;
					if ( mission.missionProperties.changeRepositionOverride != null )
						mission.missionProperties.changeRepositionOverride.theText = translation.missionProperties.repositionOverride;

					//initial groups
					for ( int groupIdx = 0; groupIdx < mission.initialDeploymentGroups.Count; groupIdx++ )
					{
						if ( translation.initialGroups.Any( x => x.cardName == mission.initialDeploymentGroups[groupIdx].cardName ) )
						{
							mission.initialDeploymentGroups[groupIdx].customText = translation.initialGroups.First( x => x.cardName == mission.initialDeploymentGroups[groupIdx].cardName ).customInstructions;
						}
					}

					//entities
					translation.mapEntities.ForEach( translatedEntity =>
					{
						var missionEntity = mission.mapEntities.Where( x => x.GUID == translatedEntity.GUID ).FirstOr( null );
						if ( missionEntity != null )
						{
							//main text
							missionEntity.entityProperties.theText = translatedEntity.mainText;
							//buttons
							missionEntity.entityProperties.buttonActions.ForEach( buttonAction =>
							{
								var translatedButton = translatedEntity.buttonList.Where( x => x.GUID == buttonAction.GUID ).FirstOr( null );
								if ( translatedButton != null )
								{
									buttonAction.buttonText = translatedButton.theText;
								}
							} );
						}
					} );

					//events
					translation.events.ForEach( translatedEvent =>
					{
						//find matching Event in the Mission
						var missionEvent = mission.GetAllEvents().Where( x => x.GUID == translatedEvent.GUID ).FirstOr( null );

						if ( missionEvent != null )
						{
							//main text
							missionEvent.eventText = translatedEvent.eventText;
							//now check loaded event actions
							foreach ( var translatedEA in translatedEvent.eventActions )
							{
								var missionEA = missionEvent.eventActions.Where( x => x.GUID == translatedEA.GUID ).FirstOr( null );
								if ( missionEA != null )
								{
									switch ( missionEA.eventActionType )
									{
										case EventActionType.M2:
											ModifyMapEntity mme = missionEA as ModifyMapEntity;
											TranslatedModifyMapEntity tmme = translatedEA as TranslatedModifyMapEntity;

											foreach ( var tmod in tmme.translatedEntityProperties )
											{
												var missionmme = mme.entitiesToModify.Where( x => x.GUID == tmod.GUID ).FirstOr( null );
												if ( missionmme != null )
												{
													//text
													missionmme.entityProperties.theText = tmod.theText;
													//buttons
													foreach ( var tbtn in tmod.buttonList )
													{
														var missionmmebtn = missionmme.entityProperties.buttonActions.Where( x => x.GUID == tbtn.GUID ).FirstOr( null );
														if ( missionmmebtn != null )
														{
															missionmmebtn.buttonText = tbtn.theText;
														}
													}
												}
											}
											break;

										case EventActionType.D1:
											EnemyDeployment enemyDeployment = missionEA as EnemyDeployment;
											TranslatedEnemyDeployment translatedEnemyDeployment = translatedEA as TranslatedEnemyDeployment;

											enemyDeployment.enemyName = translatedEnemyDeployment.enemyName;
											enemyDeployment.enemyGroupData.customText = translatedEnemyDeployment.customText;
											enemyDeployment.modification = translatedEnemyDeployment.modification;
											enemyDeployment.repositionInstructions = translatedEnemyDeployment.repositionInstructions;
											break;

										case EventActionType.G9:
											InputPrompt inputPrompt = missionEA as InputPrompt;
											TranslatedInputPrompt translatedInputPrompt = translatedEA as TranslatedInputPrompt;

											inputPrompt.theText = translatedInputPrompt.mainText;
											inputPrompt.failText = translatedInputPrompt.failText;
											foreach ( var tItem in translatedInputPrompt.inputList )
											{
												var mItem = inputPrompt.inputList.Where( x => x.GUID == tItem.GUID ).FirstOr( null );
												if ( mItem != null )
												{
													mItem.theText = tItem.theText;
												}
											}
											break;

										case EventActionType.G7:
											ShowTextBox mtb = missionEA as ShowTextBox;
											TranslatedTextBox ttb = translatedEA as TranslatedTextBox;

											mtb.theText = ttb.tbText;
											break;

										case EventActionType.G2:
											ChangeMissionInfo changeMissionInfo = missionEA as ChangeMissionInfo;
											TranslatedChangeMissionInfo translatedChangeMissionInfo = translatedEA as TranslatedChangeMissionInfo;

											changeMissionInfo.theText = translatedChangeMissionInfo.theText;
											break;

										case EventActionType.G3:
											ChangeObjective changeObjective = missionEA as ChangeObjective;
											TranslatedChangeObjective translatedChangeObjective = translatedEA as TranslatedChangeObjective;

											changeObjective.theText = translatedChangeObjective.shortText;
											changeObjective.longText = translatedChangeObjective.longText;
											break;

										case EventActionType.G6:
											QuestionPrompt questionPrompt = missionEA as QuestionPrompt;
											TranslatedQuestionPrompt translatedQuestionPrompt = translatedEA as TranslatedQuestionPrompt;

											questionPrompt.theText = translatedQuestionPrompt.mainText;
											foreach ( var tItem in translatedQuestionPrompt.buttonList )
											{
												var mBtn = questionPrompt.buttonList.Where( x => x.GUID == tItem.GUID ).FirstOr( null );
												if ( mBtn != null )
												{
													mBtn.buttonText = tItem.theText;
												}
											}
											break;

										case EventActionType.D2:
											AllyDeployment allyDeployment = missionEA as AllyDeployment;
											TranslatedAllyDeployment translatedAllyDeployment = translatedEA as TranslatedAllyDeployment;

											allyDeployment.allyName = translatedAllyDeployment.customName;
											break;

										case EventActionType.GM1:
											ChangeInstructions changeInstructions = missionEA as ChangeInstructions;
											TranslatedChangeGroupInstructions translatedChangeGroupInstructions = translatedEA as TranslatedChangeGroupInstructions;

											changeInstructions.theText = translatedChangeGroupInstructions.newInstructions;
											break;

										case EventActionType.GM4:
											ChangeReposition changeReposition = missionEA as ChangeReposition;
											TranslatedChangeRepositionInstructions translatedChangeRepositionInstructions = translatedEA as TranslatedChangeRepositionInstructions;

											changeReposition.theText = translatedChangeRepositionInstructions.repositionText;
											break;
									}
								}
							}
						}
					} );

					Debug.Log( "InjectTranslation()::DONE Injecting translation into loaded Mission" );
				}
				else
					Debug.Log( "InjectTranslation()::translatedMission is null, no translation for this Mission" );
			}
			catch ( Exception e )
			{
				Utils.LogWarning( $"InjectTranslationIntoMission()::Error injecting translation:\n{e.Message}" );
			}
		}
	}
}
