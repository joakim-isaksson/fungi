using System.Collections.Generic;
using Hexagon;

namespace Combat
{
	public class ActionGenerator
	{
		public static List<CombatAction> GenActions(Unit agent, List<Unit> units, CombatGrid grid)
		{
			List<CombatAction> actions = new List<CombatAction>();
			List<Hex> blocked = GenBlocked(units, grid.Tiles);

			// Defence
			actions.Add(new CombatAction(ActionType.Defence, agent, null, null));

			// Wait
			actions.Add(new CombatAction(ActionType.Wait, agent, null, null));

			// Move
			List<CombatAction> moveActions;
			if (agent.Walked) moveActions = GenMoveActions(agent, 1, blocked);
			else moveActions = GenMoveActions(agent, agent.Stats.Speed, blocked);
			actions.AddRange(moveActions);

			// Attack
			List<CombatAction> attackActions = GenAttackActions(agent, units);
			actions.AddRange(attackActions);

			// Shoot
			List<CombatAction> shootActions = GenShootActions(agent, units, attackActions);
			actions.AddRange(shootActions);

			// Charge
			actions.AddRange(GenChargeActions(agent, units, moveActions, attackActions, shootActions));

			// Heal
			actions.AddRange(GenHealActions(agent, units));

			// Drain Life
			actions.AddRange(GenDrainLifeActions(agent, units));

			return actions;
		}

		public static List<Tile> GenPath(Unit agent, Hex destination, List<Unit> units, CombatGrid grid)
		{
			List<Tile> tilePath = new List<Tile>();
			List<Hex> blocked = GenBlocked(units, grid.Tiles);
			blocked.Remove(destination);
			List<Hex> path = agent.Tile.Position.PathTo(destination, blocked);
			foreach (Hex hex in path)
			{
				tilePath.Add(grid.HexToTile[hex]);
			}
			return tilePath;
		}

		public static List<Hex> GenBlocked(List<Unit> units, List<Tile> tiles)
		{
			List<Hex> blocked = new List<Hex>();
			foreach (Tile tile in tiles)
			{
				if (tile.Impassable) blocked.Add(tile.Position);
			}
			foreach (Unit unit in units)
			{
				blocked.Add(unit.Tile.Position);
			}
			return blocked;
		}

		static List<CombatAction> GenMoveActions(Unit agent, int maxSteps, List<Hex> blocked)
		{
			List<CombatAction> moveActions = new List<CombatAction>();

			List<Hex> visited = new List<Hex>();
			List<List<Hex>> fringes = new List<List<Hex>>();
			fringes.Add(new List<Hex>());
			fringes[0].Add(agent.Tile.Position);
			visited.Add(agent.Tile.Position);

			for (int step = 1; step <= maxSteps; ++step)
			{
				fringes.Add(new List<Hex>());
				foreach (Hex fringe in fringes[step - 1])
				{
					foreach (Hex candidate in fringe.Neighbors())
					{
						if (!blocked.Contains(candidate) && !visited.Contains(candidate))
						{
							if (step == maxSteps) moveActions.Add(new CombatAction(ActionType.Run, agent, null, candidate));
							else moveActions.Add(new CombatAction(ActionType.Walk, agent, null, candidate));
							fringes[step].Add(candidate);
							visited.Add(candidate);
						}
					}
				}
			}

			return moveActions;
		}

		static List<CombatAction> GenAttackActions(Unit agent, List<Unit> units)
		{
			List<CombatAction> attackActions = new List<CombatAction>();
			foreach (Hex pos in agent.Tile.Position.Neighbors())
			{
				foreach (Unit target in units)
				{
					if (target.PlayerId != agent.PlayerId && target.Tile.Position.Equals(pos))
					{
						attackActions.Add(new CombatAction(ActionType.Attack, agent, target, target.Tile.Position));
					}
				}
			}
			return attackActions;
		}

		static List<CombatAction> GenShootActions(Unit agent, List<Unit> units, List<CombatAction> attackActions)
		{
			List<CombatAction> shootActions = new List<CombatAction>();

			if (!agent.CanShoot || attackActions.Count > 0) return shootActions;

			foreach (Unit target in units)
			{
				if (target.PlayerId != agent.PlayerId)
				{
					shootActions.Add(new CombatAction(ActionType.Shoot, agent, target, target.Tile.Position));
				}
			}

			return shootActions;
		}

		static List<CombatAction> GenChargeActions(Unit agent, List<Unit> units, List<CombatAction> moveActions, List<CombatAction> attackActions, List<CombatAction> shootActions)
		{
			List<CombatAction> chargeActions = new List<CombatAction>();
			foreach (Unit target in units)
			{
				if (target.PlayerId == agent.PlayerId) continue;

				bool alreadyUnderAttack = false;
				foreach (CombatAction attack in attackActions)
				{
					if (attack.Target.Equals(target))
					{
						alreadyUnderAttack = true;
						break;
					}
				}
				if (alreadyUnderAttack) continue;

				foreach (CombatAction attack in shootActions)
				{
					if (attack.Target.Equals(target))
					{
						alreadyUnderAttack = true;
						break;
					}
				}
				if (alreadyUnderAttack) continue;

				bool chargePositionFound = false;
				foreach (Hex hex in target.Tile.Position.Neighbors())
				{
					foreach (CombatAction move in moveActions)
					{
						if (move.Type == ActionType.Run) continue;

						if (move.Position.Equals(hex))
						{
							chargeActions.Add(new CombatAction(ActionType.Charge, agent, target, target.Tile.Position));
							chargePositionFound = true;
							break;
						}
					}
					if (chargePositionFound) break;
				}
			}

			return chargeActions;
		}

		static List<CombatAction> GenHealActions(Unit agent, List<Unit> units)
		{
			List<CombatAction> actions = new List<CombatAction>();

			if (!agent.SpecialAbility.Equals(SpecialAbilityType.Heal) || agent.CoolDownLeft != 0) return actions;

			foreach (Unit target in units)
			{
				if (target.PlayerId == agent.PlayerId && target.HitPoints < target.Stats.Vitality)
				{
					actions.Add(new CombatAction(ActionType.Heal, agent, target, target.Tile.Position));
				}
			}

			return actions;
		}

		static List<CombatAction> GenDrainLifeActions(Unit agent, List<Unit> units)
		{
			List<CombatAction> actions = new List<CombatAction>();

			if (!agent.SpecialAbility.Equals(SpecialAbilityType.DrainLife) || agent.CoolDownLeft != 0) return actions;

			foreach (Unit target in units)
			{
				if (target.PlayerId != agent.PlayerId)
				{
					actions.Add(new CombatAction(ActionType.DrainLife, agent, target, target.Tile.Position));
				}
			}

			return actions;
		}
	}
}
