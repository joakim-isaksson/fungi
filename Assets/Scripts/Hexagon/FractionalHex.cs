using UnityEngine;

namespace Hexagon
{
	public class FractionalHex
	{
		public readonly float Q;
		public readonly float R;
		public readonly float S;

		public FractionalHex(float q, float r, float s)
		{
			Q = q;
			R = r;
			S = s;
		}

		public Hex Round()
		{
			int q = (int)Mathf.Round(Q);
			int r = (int)Mathf.Round(R);
			int s = (int)Mathf.Round(S);

			float qDiff = Mathf.Abs(q - Q);
			float rDiff = Mathf.Abs(r - R);
			float sDiff = Mathf.Abs(s - S);

			if (qDiff > rDiff && qDiff > sDiff) q = -r - s;
			else if (rDiff > sDiff) r = -q - s;
			else s = -q - r;

			return new Hex(q, r, s);
		}
	}
}
