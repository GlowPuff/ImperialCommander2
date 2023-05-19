using System.Collections.Generic;
using System.Linq;

namespace Saga
{
	public class LogItem
	{
		public int round;
		public MissionLogType missionLogType;
		public string message;
	}

	public class MissionLogger
	{
		public List<LogItem> logItems = new List<LogItem>();

		public void LogEvent( MissionLogType ltype, string msg )
		{
			//TextBox, InputBox, PromptBox, PlayerSelection, GroupActivation, GroupDeployment, GroupRemoved, GroupDefeated, DeploymentEvent

			if ( string.IsNullOrEmpty( msg?.Trim() ) )
				return;

			string eventText = "";

			string textBoxTitleColor = "#00FFA0";
			string textColor = "white";
			string promptBoxTitleColor = "orange";
			string selectionTitleColor = "#00FFA0";
			string selectionColor = "yellow";
			string removeTitleColor = "red";
			string removeColor = "orange";
			string deployedTitleColor = "#00D486";
			string deployedColor = "#7FD3FF";
			string defeatedTitleColor = "red";
			string defeatedColor = "orange";

			switch ( ltype )
			{
				case MissionLogType.TextBox:
					eventText += $"<b><color={textBoxTitleColor}>{DataStore.uiLanguage.uiLogger.textLabel}:</color></b>\n";
					eventText += $"<color={textColor}>{msg.Trim()}</color>\n\n";
					break;
				case MissionLogType.InputBox:
					eventText += $"<b><color={promptBoxTitleColor}>{DataStore.uiLanguage.uiLogger.inputPromptLabel}:</color></b>\n";
					eventText += $"<color={textColor}>{msg.Trim()}</color>\n\n";
					break;
				case MissionLogType.PromptBox:
					eventText += $"<b><color={promptBoxTitleColor}>{DataStore.uiLanguage.uiLogger.selectionPromptLabel}:</color></b>\n";
					eventText += $"<color={textColor}>{msg.Trim()}</color>\n\n";
					break;
				case MissionLogType.PlayerSelection:
					eventText += $"<b><color={selectionTitleColor}>{DataStore.uiLanguage.uiLogger.selectionLabel}:</color></b> ";
					eventText += $"<color={selectionColor}>{msg.Trim()}</color>\n\n";
					break;
				case MissionLogType.GroupActivation:
					eventText += $"<b><color={removeTitleColor}>{DataStore.uiLanguage.uiLogger.groupActivationLabel}:</color></b> ";
					eventText += $"<color={selectionColor}>{msg.Trim()}</color>\n\n";
					break;
				case MissionLogType.GroupDeployment:
					eventText += $"<b><color={deployedTitleColor}>{DataStore.uiLanguage.uiLogger.groupDeployedLabel}:</color></b> ";
					eventText += $"<color={deployedColor}>{msg.Trim()}</color>\n\n";
					break;
				case MissionLogType.GroupRemoved:
					eventText += $"<b><color={removeTitleColor}>{DataStore.uiLanguage.uiLogger.groupRemovedLabel}:</color></b> ";
					eventText += $"<color={removeColor}>{msg.Trim()}</color>\n\n";
					break;
				case MissionLogType.GroupDefeated:
					eventText += $"<b><color={defeatedTitleColor}>{DataStore.uiLanguage.uiLogger.groupDefeatedLabel}:</color></b> ";
					eventText += $"<color={defeatedColor}>{msg.Trim()}</color>\n\n";
					break;
				case MissionLogType.DeploymentEvent:
					eventText += $"<b><color={promptBoxTitleColor}>{DataStore.uiLanguage.uiLogger.deploymentEventLabel}:</color></b> ";
					eventText += $"<color={selectionColor}>{msg.Trim()}</color>\n\n";
					break;
			}

			logItems.Add( new LogItem()
			{
				round = DataStore.sagaSessionData.gameVars.round,
				message = eventText,
				missionLogType = ltype
			} );
		}

		public List<string> GetLogFromRound( int round )
		{
			return logItems.Where( x => x.round == round ).Select( x => x.message ).ToList();
		}
	}
}
