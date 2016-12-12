using UnityEngine;

namespace Hexagon
{
	public class Orientation
	{
		public static readonly Orientation Pointy = new Orientation(
			Mathf.Sqrt(3.0f), Mathf.Sqrt(3.0f) / 2.0f, 0.0f, 3.0f / 2.0f,
			Mathf.Sqrt(3.0f) / 3.0f, -1.0f / 3.0f, 0.0f, 2.0f / 3.0f,
			0.5f
		);

		public static readonly Orientation Flat = new Orientation(
			3.0f / 2.0f, 0.0f, Mathf.Sqrt(3.0f) / 2.0f, Mathf.Sqrt(3.0f),
			2.0f / 3.0f, 0.0f, -1.0f / 3.0f, Mathf.Sqrt(3.0f) / 3.0f,
			0.0f
		);

		public readonly float F0;
		public readonly float F1;
		public readonly float F2;
		public readonly float F3;
		public readonly float B0;
		public readonly float B1;
		public readonly float B2;
		public readonly float B3;
		public readonly float StartAngle;

		Orientation(float f0, float f1, float f2, float f3, float b0, float b1, float b2, float b3, float startAngle)
		{
			F0 = f0;
			F1 = f1;
			F2 = f2;
			F3 = f3;
			B0 = b0;
			B1 = b1;
			B2 = b2;
			B3 = b3;
			StartAngle = startAngle;
		}
	}
}
