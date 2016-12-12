using UnityEngine;
using System.Collections;

namespace Utils
{
	public class VerticalSortingOrder : MonoBehaviour
	{
		public int VerticalOrderMultiplier = -10000;

		SpriteRenderer spriteRenderer;

		void Start()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
		}

		void Update()
		{
			spriteRenderer.sortingOrder = (int)(transform.position.y * VerticalOrderMultiplier);
		}
	}
}