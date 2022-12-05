using UnityEngine;

public class HPTrackerContainer : MonoBehaviour
{
	public HPTracker[] trackers;
	public CanvasGroup cg;

	public void Reset( DeploymentCard card )
	{
		//show it
		gameObject.SetActive( true );

		for ( int i = 0; i < trackers.Length; i++ )
		{
			//hide trackers and set them to 0
			trackers[i].ResetTracker();
		}

		//show trackers for the # of enemies in the group
		for ( int i = 0; i < card.size; i++ )
		{
			trackers[i].SetValue( card, i );
		}
	}

	public void Hide()
	{
		gameObject.SetActive( false );
		for ( int i = 0; i < 3; i++ )
			trackers[i].UpdateWoundValue();

		for ( int i = 0; i < trackers.Length; i++ )
			trackers[i].ResetTracker();
	}
}
