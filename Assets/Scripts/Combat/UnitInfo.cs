using UnityEngine;

namespace Combat
{
	[System.Serializable]
	public class UnitInfo
	{
		public int Size;
		public UnitType Type;
		public bool AIControlled;
		public bool Solo;
	}
}
