/// <summary>
/// Edit the DATE if anything is ADDED or MODIFIED
/// 
/// Modified 3/10/2021
/// +GetAspectRatio()
/// return the aspect ratio with 2 digit precison (eg: 1.33)
/// 
/// Modified 2/23/2021
/// Fixed RandomAddTo() for loop
/// Fixed RandomBool()'s use of Random.Range
/// 
/// Modified 4/26
/// +RandomizeArray
/// get an array, return it in random order
/// 
/// Modified 3/25/2020
/// +Average
/// return average Vector3 of Vector3 array
/// 
/// Modified 3/12/2018
/// +FindObjectsOfTypeAll and FindObjectsOfTypeSingle
/// Find objects even if they are disabled
/// 
/// Modified 1/15/2018
/// +ShuffleArray()
/// 
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

[Serializable]
public struct Point
{
	public int X, Y;
	public Point( int px, int py )
	{
		X = px;
		Y = py;
	}

	public static bool operator !=( Point p1, Point p2 )
	{
		return ( p1.X != p2.X || p1.Y != p2.Y );
	}

	public static bool operator ==( Point p1, Point p2 )
	{
		return ( p1.X == p2.X && p1.Y == p2.Y );
	}

	public override bool Equals( object obj )
	{
		return base.Equals( obj );
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string ToString()
	{
		return X.ToString() + "," + Y.ToString();
	}
}

[AddComponentMenu( "GlowPuff/Hooks/UpdateEventHooks" )]
public class UpdateEventHooks : MonoBehaviour
{
	public event System.EventHandler UpdateHook;

	// Update is called once per frame
	void Update()
	{
		if ( this.UpdateHook != null )
			this.UpdateHook( this.gameObject, System.EventArgs.Empty );
	}

	~UpdateEventHooks()
	{
		UpdateHook = null;
	}
}

public static class GlowTimer
{
	public static event System.EventHandler Update
	{
		add
		{
			_updateHook.UpdateHook += value;
		}
		remove
		{
			_updateHook.UpdateHook -= value;
		}
	}

	class TimerObject
	{
		public Action action;
		public float lifespan;

		public TimerObject( float time, Action a )
		{
			lifespan = time;
			action = a;
		}

		public IEnumerator TimerUpdate()
		{
			yield return new WaitForSeconds( lifespan );
			action();
		}
	}

	private static GameObject _gameObject;
	private static UpdateEventHooks _updateHook;

	static GlowTimer()
	{
		_gameObject = new GameObject( "GameLoopEntryObject" );
		_updateHook = _gameObject.AddComponent<UpdateEventHooks>();
		GameObject.DontDestroyOnLoad( _gameObject );
	}

	public static void SetTimer( float timeToPass, Action function )
	{
		TimerObject obj = new TimerObject( timeToPass, function );

		StartCoroutine( obj.TimerUpdate() );
	}

	public static Coroutine StartCoroutine( System.Collections.IEnumerator routine )
	{
		return _updateHook.StartCoroutine( routine );
	}
}

public static class GlowExtensions
{
	/// <summary>
	/// Alpha is from 0-1
	/// </summary>
	public static Color ByteToFloatColor( this Vector4 input )
	{
		float r = input.x / 255f;
		float g = input.y / 255f;
		float b = input.z / 255f;
		return new Color( r, g, b, input.w );
	}

	/// <summary>
	/// Parses an INT out of a string
	/// </summary>
	public static int ParseFast( this string s )
	{
		int r = 0;
		for ( var i = 0; i < s.Length; i++ )
		{
			char letter = s[i];
			r = 10 * r;
			r = r + (int)char.GetNumericValue( letter );
		}
		return r;
	}

	public static Vector2 ToVector2( this Vector3 input )
	{
		return new Vector2( input.x, input.y );
	}

	public static Vector3 ToVector3( this Vector2 input )
	{
		return new Vector3( input.x, input.y, 0 );
	}

	public static Vector2 ToVector2( this Point input )
	{
		return new Vector2( input.X, input.Y );
	}

	public static Vector3 ToVector3( this Point input )
	{
		return new Vector3( input.X, input.Y, 0 );
	}

	public static Vector3 Multiply( this Vector3 input, Vector3 p2 )
	{
		return new Vector3( input.x * p2.x, input.y * p2.y, input.z * p2.z );
	}

	public static Vector3 ToVector3( this float input )
	{
		return new Vector3( input, input, input );
	}

	/// <summary>
	/// Tries to convert supplied string to a supplied Enum type, returns a given default value if it fails
	/// </summary>
	/// <typeparam name="TEnum"></typeparam>
	/// <param name="strEnumValue">String to parse</param>
	/// <param name="defaultValue">Default value to return</param>
	/// <returns></returns>
	public static T ToEnum<T>( this string strEnumValue, T defaultValue )
	{
		//if ( !Enum.IsDefined( typeof( T ), strEnumValue ) )
		//	return defaultValue;
		if ( Enum.GetNames( typeof( T ) ).Any( x => x.ToLower() == strEnumValue.ToLower() ) )
			return (T)Enum.Parse( typeof( T ), strEnumValue, true );
		else
			return defaultValue;
	}

	/// <summary>
	/// Returns TRUE if supplied string is found in supplied Enum, parses string to that Enum type
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="strValue">String to parse</param>
	/// <param name="outValue">Parsed Enum of type T to return</param>
	/// <returns></returns>
	public static bool TryParse<T>( this string strValue, out T outValue )
	{
		if ( !typeof( T ).IsEnum )
			throw new InvalidOperationException( "TryParse requires an enum!" );

		//if ( !Enum.IsDefined( typeof( T ), strValue ) )
		if ( !Enum.GetNames( typeof( T ) ).Any( x => x.ToLower() == strValue.ToLower() ) )
		{
			outValue = default( T );//(T)Activator.CreateInstance( typeof( T ) );//
			return false;
		}
		//value of strValue found in Enum type T, now safely Parse the string to T
		outValue = (T)Enum.Parse( typeof( T ), strValue );
		return true;
	}
}

public class GlowEngine : MonoBehaviour
{
	public float targetAspect;
	public Camera mainCamera;
	//public bool EnforceAspectRatio = true;

	void Start()
	{
		if ( mainCamera != null )
		{
			Debug.Log( "::GlowEngine:: Aspect ratio enforcement: ON" );
			SetupCameraAspectRatio();
		}
		else
			Debug.Log( "::GlowEngine:: Aspect ratio enforcement: OFF" );
	}

	void SetupCameraAspectRatio()
	{
		if ( targetAspect == 0 )
			throw new UnityException( "::targetAspect:: is 0" );

		float windowAspect = (float)Screen.width / (float)Screen.height;
		//Debug.Log( "SCREEN: " + Screen.width + " x " + Screen.height );
		float scaleHeight = windowAspect / targetAspect;
		//Camera camera = GetComponent<Camera>();

		// if scaled height is less than current height, add letterbox
		if ( scaleHeight < 1.0f )
		{
			Rect rect = mainCamera.rect;

			rect.width = 1.0f;
			rect.height = scaleHeight;
			rect.x = 0;
			rect.y = ( 1.0f - scaleHeight ) / 2.0f;

			mainCamera.rect = rect;
		}
		else // add pillarbox
		{
			float scalewidth = 1.0f / scaleHeight;

			Rect rect = mainCamera.rect;

			rect.width = scalewidth;
			rect.height = 1.0f;
			rect.x = ( 1.0f - scalewidth ) / 2.0f;
			rect.y = 0;

			mainCamera.rect = rect;
		}
	}

	public static Vector3 AverageV3( Vector3[] array )
	{
		Vector3 total = array.Aggregate( Vector3.zero, ( acc, next ) =>
		{
			return ( acc += next );
		} );
		total /= array.Length;

		return total;
	}

	public static Vector3 RadianToVector2( float radian )
	{
		return new Vector3( Mathf.Cos( radian ), Mathf.Sin( radian ), 0 );
	}

	public static Vector3 DegreeToVector3( float degree )
	{
		return RadianToVector2( degree * Mathf.Deg2Rad );
	}

	/// <summary>
	/// Shuffles an array of objects and returns a new (shuffled) array
	/// </summary>
	public static T[] ShuffleArray<T>( T[] array )
	{
		int[] nums = GenerateRandomNumbers( array.Length );
		T[] temp = new T[array.Length];
		for ( int i = 0; i < array.Length; i++ )
			temp[i] = array[nums[i]];
		return temp;
	}

	/// <summary>
	/// Returns 0-based int[] array filled with "max" random numbers ranging from 0 to max - 1.
	/// </summary>
	public static int[] GenerateRandomNumbers( int max )
	{
		if ( max == 0 )
		{
			int[] znums = new int[1];
			znums[0] = 0;
			return znums;
		}
		int[] nums = new int[max];
		List<int> temp = new List<int>();
		for ( int i = 0; i < max; i++ )
			temp.Add( i );
		for ( int i = 0; i < max; i++ )
		{
			int r = UnityEngine.Random.Range( 0, temp.Count );//Engine.random.Next( temp.Count );
			nums[i] = temp[r];
			temp.RemoveAt( r );
		}
		return nums;
	}

	/// <summary>
	/// Animates a value using a sine wave between a min and max. Increase frequency with frequencyMultiplier
	/// </summary>
	public static float SineAnimation( float low, float high, float frequencyMultiplier = 1 )
	{
		float value = Mathf.Sin( Time.time * frequencyMultiplier );
		return low + ( value - -1 ) * ( high - low ) / ( 1 - -1 );
	}

	/// <summary>
	/// Animates a value using a sine wave between a min and max. Increase frequency with frequencyMultiplier (X). Use offset (Y) to shift sine animation from current time.
	/// </summary>
	/// <param name="multiplier_offset">X=frequency multiplier, Y=time offset for sinewave</param>
	public static float SineAnimation( float low, float high, Vector2 multiplier_offset )
	{
		float value = Mathf.Sin( ( Time.time + multiplier_offset.y ) * multiplier_offset.x );
		return low + ( value - -1 ) * ( high - low ) / ( 1 - -1 );
	}

	/// <summary>
	/// Provide your own elapsed game time
	/// </summary>
	public static float SineAnimation( float low, float high, float frequencyMultiplier, float elapsed )
	{
		float value = Mathf.Sin( elapsed * frequencyMultiplier );
		return low + ( value - -1 ) * ( high - low ) / ( 1 - -1 );
	}

	/// <summary>
	/// Remaps a value between 2 other values. Clamps input value for you.  Second pair of low/high can be reversed.
	/// </summary>
	public static float RemapValue( float value, float low1, float high1, float low2, float high2 )
	{
		value = Mathf.Clamp( value, low1, high1 );
		if ( low2 < high2 )
			return low2 + ( value - low1 ) * ( high2 - low2 ) / ( high1 - low1 );
		else
			return high2 + ( value - high1 ) * ( low2 - high2 ) / ( low1 - high1 );
	}

	/// <summary>
	/// Wraps a value between a specified min and max 
	/// </summary>
	public static float WrapValue( float value, float min, float max )
	{
		if ( value > max )
			return ( value - max ) + min;
		if ( value < min )
			return max - ( min - value );
		return value;
	}

	/// <summary>
	/// Returns in degrees.
	/// </summary>
	public static float AngleBetweenPositions( Vector2 p1, Vector2 p2 )
	{
		return Mathf.Atan2( p2.y - p1.y, p2.x - p1.x ) * Mathf.Rad2Deg;
	}

	/// <summary>
	/// Converts an angle (degrees) to a Vector2
	/// </summary>
	public static Vector2 AngleToVector( float angle )
	{
		return new Vector2( Mathf.Sin( angle * Mathf.Deg2Rad ), -Mathf.Cos( angle * Mathf.Deg2Rad ) );
	}

	/// <summary>
	/// returns degrees
	/// </summary>
	public static float VectorToAngle( Vector2 vector )
	{
		return Mathf.Atan2( vector.x, -vector.y ) * Mathf.Rad2Deg;
	}

	/// <summary>
	/// Animates return value (from 0 to maxValue) along half a sine curve for a "bounce" effect
	/// </summary>
	/// <param name="scalar">0 to 1 to modulate along curve</param>
	/// <param name="maxValue">The MAX value to return.</param>
	/// <returns></returns>
	public static float Bounce( float scalar, float maxValue )
	{
		return Mathf.Sin( RemapValue( scalar, 0, 1, 0, Mathf.PI ) ) * maxValue;
	}

	/// <summary>
	/// Extracts each individual digit from a number and returns them as an array of int[]
	/// </summary>
	public static int[] NumberArrayFromInt( int num )
	{
		Stack<int> numq = new Stack<int>();
		do
		{
			numq.Push( num % 10 );
			num /= 10;
		} while ( num > 0 );
		int[] rval = new int[numq.Count];
		int c = numq.Count;
		for ( int i = 0; i < c; i++ )
			rval[i] = numq.Pop();
		return rval;
	}

	/// <summary>
	/// Indicates whether a value is within a certain tolerance of another value
	/// </summary>
	public static bool WithinTolerance( float value1, float value2, float tolerance )
	{
		return Mathf.Abs( value1 - value2 ) <= tolerance;
	}

	/// <summary>
	/// Indicates whether a value is within a certain tolerance of another value.
	/// Correctly handles + and - numbers
	/// </summary>
	public static bool WithinToleranceV2( float value1, float value2, float tolerance )
	{
		if ( value1 >= 0 && value2 >= 0 )
			return Mathf.Abs( value1 - value2 ) <= Mathf.Abs( tolerance );
		else if ( value1 < 0 && value2 < 0 )
			return Mathf.Abs( Mathf.Abs( value1 ) + value2 ) <= Mathf.Abs( tolerance );
		else if ( value1 < 0 && value2 >= 0 )
			return Mathf.Abs( value1 + value2 ) <= Mathf.Abs( tolerance );
		else if ( value1 >= 0 && value2 < 0 )
			return Mathf.Abs( value1 + value2 ) <= Mathf.Abs( tolerance );
		else//never happen
			return Mathf.Abs( value1 - value2 ) <= Mathf.Abs( tolerance );
	}

	/// <summary>
	/// Calculates the speed in pixels per second
	/// </summary>
	public static float CalculateSpeed( Vector2 p1, Vector2 p2, TimeSpan time )
	{
		return Vector2.Distance( p1, p2 ) / (float)time.TotalSeconds;
	}

	/// <summary>
	/// Calculates the duration (in seconds) given a distance and a speed (in pixels per seconds)
	/// </summary>
	public static float CalculateDuration( Vector2 p1, Vector2 p2, float speed )
	{
		return (float)TimeSpan.FromSeconds( Vector2.Distance( p1, p2 ) / speed ).TotalSeconds;
	}

	/// <summary>
	/// Generates specified number of random floats, all of which add up to specified value
	/// </summary>
	public static float[] RandomAddTo( int numberOfValues, float addUpTo )
	{
		float[] randoms = new float[numberOfValues];
		float sum = 0;
		for ( int i = 0; i < numberOfValues; i++ )
		{
			randoms[i] = UnityEngine.Random.Range( 1f, 100f );//Tools.RandomFloat( 1f, 100f );
			sum += randoms[i];
		}
		for ( int i = 0; i < numberOfValues; i++ )//i < 6; i++ )
		{
			randoms[i] /= sum;
			randoms[i] *= addUpTo;
		}
		return randoms;
	}

	/// <summary>
	/// percentChance defaults to 50, out of 100
	/// </summary>
	public static bool RandomBool( float percentChance = 50 )
	{
		percentChance = Mathf.Clamp( percentChance, 0, 100 );
		return UnityEngine.Random.Range( 1f, 100f ) <= percentChance;
	}

	public static T[] RandomizeArray<T>( T[] array )
	{
		int[] na = GenerateRandomNumbers( array.Length );
		T[] a = new T[array.Length];
		for ( int i = 0; i < array.Length; i++ )
			a[i] = array[na[i]];

		return a;
	}

	/// <summary>
	/// Returns random Y values along a sine wave given the TOTAL TIME (GT). randoms[] must be an array of 6
	/// </summary>
	public static float RandomSineWave( float[] randoms, float totalTime )
	{
		//float totalTime = Time.time;
		if ( randoms.Length < 6 )
			throw new ArgumentException( "randoms[] must be an array of 6" );
		return (float)Math.Min( 1, ( randoms[0] * Math.Sin( randoms[1] * totalTime * 10f ) +
			 randoms[2] * Math.Sin( randoms[3] * totalTime * 10f ) +
			 randoms[4] * Math.Sin( randoms[5] * totalTime * 10f ) ) );
	}

	/// <summary>
	/// Finds objects even if they are not active
	/// </summary>
	public static List<T> FindObjectsOfTypeAll<T>()
	{
		List<T> results = new List<T>();
		for ( int i = 0; i < SceneManager.sceneCount; i++ )
		{
			var s = SceneManager.GetSceneAt( i );
			if ( s.isLoaded )
			{
				var allGameObjects = s.GetRootGameObjects();
				for ( int j = 0; j < allGameObjects.Length; j++ )
				{
					var go = allGameObjects[j];
					results.AddRange( go.GetComponentsInChildren<T>( true ) );
				}
			}
		}
		return results;
	}

	/// <summary>
	/// Finds a single object even if it is not active
	/// </summary>
	public static T FindObjectsOfTypeSingle<T>()
	{
		List<T> results = new List<T>();
		for ( int i = 0; i < SceneManager.sceneCount; i++ )
		{
			var s = SceneManager.GetSceneAt( i );
			if ( s.isLoaded )
			{
				var allGameObjects = s.GetRootGameObjects();
				for ( int j = 0; j < allGameObjects.Length; j++ )
				{
					var go = allGameObjects[j];
					results.AddRange( go.GetComponentsInChildren<T>( true ) );
				}
			}
		}
		return results[0];
	}

	public static float GetAspectRatio()
	{
		//float aspect = (float)Screen.height / (float)Screen.width; // Portrait
		//landscape
		float aspect = (float)Screen.width / (float)Screen.height;
		aspect = (float)Math.Round( aspect, 2 );
		//Debug.Log( "Aspect Ratio:" + aspect );
		//if ( aspect >= 1.87 )
		//{
		//	Debug.Log( "19.5:9" ); // iPhone X
		//}
		//else if ( aspect >= 1.74 )  // 16:9
		//{
		//	Debug.Log( "16:9" );
		//}
		//else if ( aspect > 1.6 )// 5:3
		//	Debug.Log( "5:3" );
		//else if ( Math.Abs( aspect - 1.6 ) < Mathf.Epsilon )// 16:10
		//	Debug.Log( "16:10" );
		//else if ( aspect >= 1.5 )// 3:2
		//	Debug.Log( "3:2" );
		//else
		//{ // 4:3
		//	Debug.Log( "4:3 or other" );
		//}

		//string _r = aspect.ToString( "F2" );
		//string ratio = _r.Substring( 0, 4 );
		//print( "RATIO:" + ratio );


		return aspect;
	}
}