using UnityEngine;
using System.Collections;

namespace Utils
{
	public class RandomFlipper : MonoBehaviour
	{
		public float Probability = 0.5f;
		void Start()
		{
			if (Random.value > Probability) transform.localScale = new Vector3(
				-transform.localScale.x,
				transform.localScale.y,
				transform.localScale.z
			);
		}
	}
}

