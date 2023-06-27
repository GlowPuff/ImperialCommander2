using UnityEngine;
using UnityEngine.UI;

public class ValueAdjuster : MonoBehaviour
{
	public PopupBase popupBase;
	public Text outText;

	MWheelHandler valueAdjusterTarget;

	public void Show( int value, MWheelHandler target )
	{
		popupBase.Show();
		valueAdjusterTarget = target;

		outText.text = value.ToString();
	}

	public void Hide()
	{
		popupBase.Close();
	}

	public void OnAdd()
	{
		valueAdjusterTarget?.OnAdd();
	}

	public void OnSubtract()
	{
		valueAdjusterTarget?.OnSubtract();
	}

	public void SetValue( int value )
	{
		outText.text = value.ToString();
	}
}
