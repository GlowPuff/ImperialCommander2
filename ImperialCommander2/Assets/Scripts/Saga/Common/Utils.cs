using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Saga
{
	public static class Utils
	{
		public const string formatVersion = "20";//the EXPECTED mission format

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
					return new Color( .3301887f, .3301887f, .3301887f ) * 1.5f;
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

		public static Guid GUIDOne { get { return Guid.Parse( "11111111-1111-1111-1111-111111111111" ); } }

		public static string ReplaceGlyphs( string item )
		{
			if ( string.IsNullOrEmpty( item ) )
				return "";

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
			item = item.Replace( "{V}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">V</font></color>" );
			item = item.Replace( "{D}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">D</font></color>" );
			item = item.Replace( "{O}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">O</font></color>" );
			item = item.Replace( "{R}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">R</font></color>" );
			item = item.Replace( "{S}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">S</font></color>" );
			item = item.Replace( "{U}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">U</font></color>" );
			item = item.Replace( "{W}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">W</font></color>" );
			item = item.Replace( "{X}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">X</font></color>" );
			item = item.Replace( "{c}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">c</font></color>" );
			item = item.Replace( "{e}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">e</font></color>" );
			item = item.Replace( "{s}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">s</font></color>" );
			item = item.Replace( "{-}", " \u25A0 " );
			item = item.Replace( "{0}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">0</font></color>" );
			item = item.Replace( "{1}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">1</font></color>" );
			item = item.Replace( "{2}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">2</font></color>" );
			item = item.Replace( "{3}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">3</font></color>" );
			item = item.Replace( "{4}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">4</font></color>" );
			item = item.Replace( "{5}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">5</font></color>" );
			item = item.Replace( "{6}", "<color=\"red\"><font=\"ImperialAssaultSymbols SDF\">6</font></color>" );
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
				foreach ( var match in regex.Matches( item ) )
				{
					int mul = int.Parse( match.ToString().Replace( "*", "" ) );
					item = item.Replace( match.ToString(), (DataStore.sagaSessionData.setupOptions.threatLevel * mul).ToString() );
				}

				//random rebel
				regex = new Regex( @"\{rebel\}", RegexOptions.IgnoreCase );
				string rebelName = "";
				foreach ( var match in regex.Matches( item ) )
				{
					var rebel = DataStore.deployedHeroes[GlowEngine.GenerateRandomNumbers( DataStore.deployedHeroes.Count )[0]];
					//look for ally override with a custom name
					var ovrd = DataStore.sagaSessionData.gameVars.GetDeploymentOverride( rebel.id );
					if ( ovrd != null )
						rebelName = ovrd.nameOverride;
					else
						rebelName = rebel.name;

					item = item.Replace( match.ToString(), rebelName );
				}
			}

			return item;
		}

		public static DiceColor[] ParseCustomDice( string[] dice )
		{
			List<DiceColor> diceColors = new List<DiceColor>();
			var regex = new Regex( @"\d\w+", RegexOptions.IgnoreCase );

			foreach ( var diceItem in dice )
			{
				var m = regex.Matches( diceItem );
				foreach ( var match in regex.Matches( diceItem ) )
				{
					int count = int.Parse( match.ToString()[0].ToString() );

					for ( int i = 0; i < count; i++ )
						diceColors.Add( (DiceColor)Enum.Parse( typeof( DiceColor ), match.ToString().Substring( 1 ) ) );
				}
			}

			return diceColors.ToArray();
		}

		/// <summary>
		/// Always returns <see langword="true"/>
		/// </summary>
		public static bool AssetExists( object key )
		{
			return true;
			//			if ( Application.isEditor )// && !Application.isPlaying )
			//			{
			//#if UNITY_EDITOR
			//				return true;
			//				// keys are always asset file paths
			//				//return File.Exists( Path.Combine( Application.dataPath,"SagaMissions", (string)key ) );
			//#endif
			//			}
			//			else if ( Application.isPlaying )
			//{
			//	foreach ( var l in Addressables.ResourceLocators )
			//	{
			//		IList<IResourceLocation> locs;
			//		if ( l.Locate( key, null, out locs ) )
			//			return true;
			//	}
			//	return false;
			//}

			//return false;
		}
	}
}
