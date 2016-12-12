using UnityEngine;
using Utils;

namespace Combat
{
	public class DecorationSprite : MonoBehaviour
	{
		public float Probability;
		public Vector2 OffsetFrom;
		public Vector2 OffsetTo;
		public Sprite[] Sprites;
		public string SortingLayer;
		public bool UseVerticalSorting;

		public float FlipProb = 0.5f;

		void Awake()
		{
			if (Random.value > Probability) return;

			Sprite sprite = Sprites[Random.Range(0, Sprites.Length)];

			GameObject decoration = new GameObject(sprite.name);
			decoration.transform.parent = transform;
			decoration.transform.Translate(
				Random.Range(OffsetFrom.x, OffsetTo.x),
				Random.Range(OffsetFrom.y, OffsetTo.y),
				0
			);

			SpriteRenderer spriteRenderer = decoration.AddComponent<SpriteRenderer>();
			spriteRenderer.sprite = sprite;
			spriteRenderer.sortingLayerName = SortingLayer;

			if (Random.value > FlipProb) decoration.transform.localScale = new Vector3(
				-decoration.transform.localScale.x,
				decoration.transform.localScale.y,
				decoration.transform.localScale.z
			);

			if (UseVerticalSorting) decoration.AddComponent<VerticalSortingOrder>();
		}
	}
}