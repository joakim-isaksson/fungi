using UnityEngine;

namespace Hexagon
{
	public class Layout
	{
		public readonly Orientation Orientation;
		public readonly IntVector2 Size;
		public readonly IntVector2 Origin;

		public Layout(Orientation orientation, IntVector2 size, IntVector2 origin)
		{
			Orientation = orientation;
			Size = size;
			Origin = origin;
		}

		public Vector2 HexToPoint(Hex hex)
		{
			float x = (Orientation.F0 * hex.Q + Orientation.F1 * hex.R) * Size.X;
			float y = (Orientation.F2 * hex.Q + Orientation.F3 * hex.R) * Size.Y;
			return new Vector2(x + Origin.X, y + Origin.Y);
		}

		public FractionalHex PointToHex(Vector2 point)
		{
			Vector2 pt = new Vector2((point.x - Origin.X) / Size.X, (point.y - Origin.Y) / Size.Y);
			float q = Orientation.B0 * pt.x + Orientation.B1 * pt.y;
			float r = Orientation.B2 * pt.x + Orientation.B3 * pt.y;
			return new FractionalHex(q, r, -q - r);
		}

		public Vector2 CornerOffset(int corner)
		{
			float angle = 2.0f * Mathf.PI * (corner + Orientation.StartAngle) / 6.0f;
			return new Vector2(Size.X * Mathf.Cos(angle), Size.Y * Mathf.Sin(angle));
		}

		public Vector2[] Corners(Hex hex)
		{
			Vector2[] corners = new Vector2[6];
			Vector2 center = HexToPoint(hex);
			for (int i = 0; i < corners.Length; ++i)
			{
				Vector2 offset = CornerOffset(i);
				corners[i] = new Vector2(center.x + offset.x, center.y + offset.y);
			}
			return corners;
		}

	}
}
