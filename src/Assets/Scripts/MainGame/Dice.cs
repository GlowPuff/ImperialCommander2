using System.Collections;
using UnityEngine;

public class Dice : MonoBehaviour
{
	public GameObject[] diceRed;
	public GameObject[] diceGreen;
	public GameObject[] diceBlue;
	public GameObject[] diceYellow;
	public GameObject[] diceBlack;
	public GameObject[] diceWhite;

	[HideInInspector]
	public DiceColor diceColor;

	private GameObject[] dice;
	private float rollTime;

	void Start()
	{
		dice = new GameObject[6];
		rollTime = Random.Range( 1f, 2f );

		switch ( diceColor )
		{
			case DiceColor.Red:
				MakeDice( diceRed );
				break;
			case DiceColor.Green:
				MakeDice( diceGreen );
				break;
			case DiceColor.Blue:
				MakeDice( diceBlue );
				break;
			case DiceColor.Yellow:
				MakeDice( diceYellow );
				break;
			case DiceColor.White:
				MakeDice( diceWhite );
				break;
			case DiceColor.Black:
				MakeDice( diceBlack );
				break;
		}
	}

	private void MakeDice( GameObject[] dcolor )
	{
		//create dice
		for ( int i = 0; i < 6; i++ )
		{
			dice[i] = Instantiate( dcolor[i] );
			dice[i].layer = 5;
			dice[i].transform.SetParent( transform );
			dice[i].transform.localScale = Vector3.one;
			dice[i].transform.localEulerAngles = Vector3.zero;
			dice[i].transform.localPosition = Vector3.zero;
			dice[i].gameObject.SetActive( false );
		}
		//"roll" them
		StartCoroutine( RollRoutine() );
	}

	private IEnumerator RollRoutine()
	{
		float timer = 0;
		//show one random face
		dice[GlowEngine.GenerateRandomNumbers( 6 )[0]].gameObject.SetActive( true );

		while ( rollTime > 0 )
		{
			timer += Time.deltaTime;
			if ( timer > .1f )//change every X milliseconds
			{
				timer = 0;
				//hide them all
				for ( int i = 0; i < 6; i++ )
					dice[i].gameObject.SetActive( false );

				//show one random face
				dice[GlowEngine.GenerateRandomNumbers( 6 )[0]].gameObject.SetActive( true );
				FindObjectOfType<Sound>().PlaySound( FX.Click );
			}

			rollTime -= Time.deltaTime;
			yield return null;
		}

		//hide them all
		for ( int i = 0; i < 6; i++ )
			dice[i].gameObject.SetActive( false );
		//finally pick one for the final dice
		dice[GlowEngine.GenerateRandomNumbers( 6 )[0]].gameObject.SetActive( true );
		FindObjectOfType<Sound>().PlaySound( FX.Click );

		yield return true;
	}
}
