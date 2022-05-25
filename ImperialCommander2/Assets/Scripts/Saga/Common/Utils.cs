using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Saga
{
	public static class Utils
	{
		public const string formatVersion = "10";//the EXPECTED mission format

		public static void LogError( string error )
		{
			string basePath = Application.persistentDataPath;
			File.WriteAllText( Path.Combine( basePath, "error_log.txt" ), "ERROR TRACE:\r\n" + error );
			Debug.Log( "ERROR TRACE:\r\n" + error );
		}

		public static Color String2UnityColor( string s )
		{
			switch ( s )
			{
				case "Gray":
					return new Color( .3301887f, .3301887f, .3301887f );
				case "Purple":
					return new Color( .6784314f, 0f, 1f );
				case "Black":
					return Color.black;
				case "Blue":
					return new Color( 0, 0.3294118f, 1 );
				case "Green":
					return new Color( 0, 0.735849f, 0.1056484f );
				case "Red":
					return Color.red;
				case "Yellow":
					return new Color( 1, 202f / 255f, 40f / 255f );
				default:
					Debug.Log( "String2UnityColor()::COLOR NOT FOUND::" + s );
					return Color.white;
			}
		}

		public static string ReplaceGlyphs( string item )
		{
			//symbols
			item = item.Replace( "{H}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">H</font></color>" );
			item = item.Replace( "{C}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">C</font></color>" );
			item = item.Replace( "{J}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">J</font></color>" );
			item = item.Replace( "{K}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">K</font></color>" );
			item = item.Replace( "{A}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">A</font></color>" );
			item = item.Replace( "{Q}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">Q</font></color>" );
			item = item.Replace( "{g}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">g</font></color>" );
			item = item.Replace( "{h}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">h</font></color>" );
			item = item.Replace( "{E}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">E</font></color>" );
			item = item.Replace( "{G}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">G</font></color>" );
			item = item.Replace( "{f}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">f</font></color>" );
			item = item.Replace( "{b}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">b</font></color>" );
			item = item.Replace( "{B}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">B</font></color>" );
			item = item.Replace( "{I}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">I</font></color>" );
			item = item.Replace( "{P}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">P</font></color>" );
			item = item.Replace( "{F}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">F</font></color>" );
			item = item.Replace( "{-}", " \u25A0 " );
			//if ( item.Contains( "{O}" ) )
			//{
			//	item = item.Replace( "{O}", "" );
			//	nt.color = new Color( 1, 0.5586207f, 0, 1 );
			//}

			//Saga formatting
			if ( DataStore.gameType == GameType.Saga )
			{
				//trigger value
				Regex regex = new Regex( @"&[\w\s]*&" );
				var m = regex.Matches( item );
				foreach ( var match in regex.Matches( item ) )
				{
					var t = DataStore.mission.GetTriggerFromName( match.ToString().Replace( "&", "" ) );
					if ( t != null )
					{
						int curValue = GlowEngine.FindUnityObject<TriggerManager>().CurrentTriggerValue( t.GUID );
						item = item.Replace( match.ToString(), curValue.ToString() );
					}
				}

				//threat multiplier
				regex = new Regex( @"\*[\w\s]*\*" );
				m = regex.Matches( item );
				foreach ( var match in regex.Matches( item ) )
				{
					int mul = int.Parse( match.ToString().Replace( "*", "" ) );
					item = item.Replace( match.ToString(), (DataStore.sagaSessionData.gameVars.currentThreat * mul).ToString() );
				}


				//random rebel
				regex = new Regex( @"\{rebel\}", RegexOptions.IgnoreCase );
				m = regex.Matches( item );
				foreach ( var match in regex.Matches( item ) )
				{
					var rebel = DataStore.deployedHeroes[GlowEngine.GenerateRandomNumbers( DataStore.deployedHeroes.Count )[0]];
					item = item.Replace( match.ToString(), rebel.name );
				}
			}

			return item;
		}
	}
}
