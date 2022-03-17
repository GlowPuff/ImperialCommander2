using UnityEngine;

public class DiceClick : MonoBehaviour
{
	public void OnPointerClick()
	{
		Destroy( transform.parent.gameObject );
	}
}
