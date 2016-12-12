using UnityEngine;

namespace World
{
	public class MapUnitScaler : MonoBehaviour
	{
		float VerticalScaleMultiplier = 0.13f;
		float BaseY = -4.0f;
		float MaxDistance = 10.0f;

		Vector3 OriginalScale;

		void Awake()
		{
			OriginalScale = transform.localScale;
		}

		void Update()
		{
			float distance = MaxDistance - Mathf.Abs(BaseY - transform.position.y);
			transform.localScale = new Vector3(
				OriginalScale.x * distance * VerticalScaleMultiplier,
				OriginalScale.y * distance * VerticalScaleMultiplier,
				OriginalScale.z
			);
		}
	}
}