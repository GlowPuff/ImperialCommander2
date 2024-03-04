using DG.Tweening;
using UnityEngine;

namespace Saga
{

	public class BGEffect : MonoBehaviour
	{
		public Transform effect1;

		private void Start()
		{
			effect1.DOScaleX( 1f, 1.5f ).SetEase( Ease.InOutBounce ).SetLoops( -1, LoopType.Yoyo );
		}
	}
}
