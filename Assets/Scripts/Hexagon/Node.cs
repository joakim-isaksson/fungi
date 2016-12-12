using System.Collections.Generic;

namespace Hexagon
{
	public class Node
	{
		public Hex Hex;
		public Node Parent;

		public Node(Hex hex, Node parent)
		{
			Hex = hex;
			Parent = parent;
		}

		public List<Hex> ToList()
		{
			List<Hex> list = new List<Hex>();
			AddTo(list);
			return list;
		}

		void AddTo(List<Hex> list)
		{
			if (Parent != null) Parent.AddTo(list);
			list.Add(Hex);
		}
	}
}