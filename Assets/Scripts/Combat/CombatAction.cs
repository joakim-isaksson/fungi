using Hexagon;

namespace Combat
{
	public class CombatAction
	{
		public ActionType Type;
		public Unit Agent;
		public Unit Target;
		public Hex Position;

		public CombatAction(ActionType type, Unit agent, Unit target, Hex position)
		{
			Type = type;
			Agent = agent;
			Target = target;
			Position = position;
		}
	}
}
