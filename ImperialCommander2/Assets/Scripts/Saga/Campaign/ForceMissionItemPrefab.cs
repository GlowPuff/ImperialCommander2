using System;
using UnityEngine;

namespace Saga
{
	public class ForceMissionItemPrefab : MonoBehaviour
	{
		Action callback;

		public void Init( Action addCallback )
		{
			callback = addCallback;
		}

		public void OnAddClick()
		{
			callback?.Invoke();
		}
	}
}