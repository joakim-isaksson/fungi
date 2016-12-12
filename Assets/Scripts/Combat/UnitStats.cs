using UnityEngine;

namespace Combat
{
	[System.Serializable]
	public class UnitStats
	{
		public int Vitality;
		public int AttackMinDmg;
		public int AttackMaxDmg;
		public int ShootMinDmg;
		public int ShootMaxDmg;
		public float DistancePenalty;
		public float Defence;
		public float MagicDefence;
		public int Speed;
		public int Initiative;
	}
}
