using UnityEngine;
using UnityEngine.UI;

public class HPTracker : MonoBehaviour
{
	public Image groupColorImage;
	public Text groupID, groupWoundNumber;
	public MWheelHandler healthWheelHandler, indexWheelHandler;

	DeploymentCard card;
	//index = which of the 3 trackers this is
	[System.NonSerialized]
	public int index;

	public void ResetTracker()
	{
		gameObject.SetActive( false );
		healthWheelHandler.ResetWheeler();
		indexWheelHandler.ResetWheeler();
	}

	public void SetValue( DeploymentCard c, int idx, bool setActive )
	{
		gameObject.SetActive( setActive );
		card = c;
		index = idx;
		groupColorImage.color = DataStore.pipColors[c.colorIndex].ToColor();
		healthWheelHandler.ResetWheeler( card.woundTrackerValue[index] );
		indexWheelHandler.ResetWheeler( card.trackerNumbers[index] );
	}

	public void UpdateWoundValue()
	{
		//Debug.Log( wheelHandler.wheelValue );
		if ( card != null )
		{
			card.woundTrackerValue[index] = healthWheelHandler.wheelValue;
			card.trackerNumbers[index] = indexWheelHandler.wheelValue;
		}
	}

	//right click and double click
	public void OnDoubleClicked()
	{
		healthWheelHandler.ResetWheeler();
		if ( card != null )
			card.woundTrackerValue[index] = 0;
	}
}
