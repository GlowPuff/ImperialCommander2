using UnityEngine;
using UnityEngine.UI;

public class HPTracker : MonoBehaviour
{
	public Image groupColorImage;
	public Text groupID, groupWoundNumber;
	public MWheelHandler wheelHandler;

	DeploymentCard card;
	//index = which of the 3 trackers this is
	public int index;

	public void ResetTracker()
	{
		gameObject.SetActive( false );
		wheelHandler.ResetWheeler();
	}

	public void SetValue( DeploymentCard c, int idx, bool setActive )
	{
		gameObject.SetActive( setActive );
		card = c;
		index = idx;
		groupColorImage.color = DataStore.pipColors[c.colorIndex].ToColor();
		wheelHandler.ResetWheeler( card.woundTrackerValue[index] );
	}

	public void UpdateWoundValue()
	{
		Debug.Log( wheelHandler.wheelValue );
		if ( card != null )
			card.woundTrackerValue[index] = wheelHandler.wheelValue;
	}
}
