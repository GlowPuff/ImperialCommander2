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
			//TextBox, InputBox, PromptBox, PlayerSelection, GroupActivation, GroupDeployment, GroupRemoved, GroupDefeated

			if ( string.IsNullOrEmpty( msg?.Trim() ) )
				return;

			string eventText = "";

			string textBoxTitleColor = "orange";
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
					eventText += $"<b><color={textBoxTitleColor}>Text:</color></b>\n";
					eventText += $"<color={textColor}>{msg.Trim()}</color>\n\n";
					break;
				case MissionLogType.InputBox:
					eventText += $"<b><color={promptBoxTitleColor}>Input Prompt:</color></b>\n";
					eventText += $"<color={textColor}>{msg.Trim()}</color>\n\n";
					break;
				case MissionLogType.PromptBox:
					eventText += $"<b><color={promptBoxTitleColor}>Selection Prompt:</color></b>\n";
					eventText += $"<color={textColor}>{msg.Trim()}</color>\n\n";
					break;
				case MissionLogType.PlayerSelection:
					eventText += $"<b><color={selectionTitleColor}>Selection:</color></b> ";
					eventText += $"<color={selectionColor}>{msg.Trim()}</color>\n\n";
					break;
				case MissionLogType.GroupActivation:
					eventText += $"<b><color={removeTitleColor}>Group Activation:</color></b> ";
					eventText += $"<color={selectionColor}>{msg.Trim()}</color>\n\n";
					break;
				case MissionLogType.GroupDeployment:
					eventText += $"<b><color={deployedTitleColor}>Group Deployed:</color></b> ";
					eventText += $"<color={deployedColor}>{msg.Trim()}</color>\n\n";
					break;
				case MissionLogType.GroupRemoved:
					eventText += $"<b><color={removeTitleColor}>Group Removed:</color></b> ";
					eventText += $"<color={removeColor}>{msg.Trim()}</color>\n\n";
					break;
				case MissionLogType.GroupDefeated:
					eventText += $"<b><color={defeatedTitleColor}>Group Deployed:</color></b> ";
					eventText += $"<color={defeatedColor}>{msg.Trim()}</color>\n\n";
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
