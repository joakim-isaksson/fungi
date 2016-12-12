using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System;
using Hexagon;

namespace Combat
{
	public class SimpleAI
	{
		public delegate void OnAiAction(CombatAction action);

		volatile bool Running;
		CombatAction Result;

		CombatGrid Grid;

		List<CombatAction> Actions;
		List<Unit> Units;

		Random Rng;

		public SimpleAI()
		{
			Rng = new Random();
		}

		public IEnumerator SelectAction(List<CombatAction> actions, List<Unit> units, CombatGrid grid, OnAiAction onAiAction)
		{
			Actions = actions;
			Units = units;
			Grid = grid;
			Running = true;
			Result = null;

			Thread worker = new Thread(new ThreadStart(RunAI));
			worker.Start();
			while (Running) yield return null;

			onAiAction(Result);
		}

		void RunAI()
		{
			Unit agent = Actions[0].Agent;

			List<CombatAction> attack = new List<CombatAction>();
			List<CombatAction> charge = new List<CombatAction>();
			List<CombatAction> drainLife = new List<CombatAction>();
			List<CombatAction> heal = new List<CombatAction>();
			List<CombatAction> run = new List<CombatAction>();
			List<CombatAction> shoot = new List<CombatAction>();
			List<CombatAction> walk = new List<CombatAction>();
			CombatAction defence = null;

			foreach (CombatAction action in Actions)
			{
				switch (action.Type)
				{
					case ActionType.Attack:
						attack.Add(action);
						break;
					case ActionType.Charge:
						charge.Add(action);
						break;
					case ActionType.DrainLife:
						drainLife.Add(action);
						break;
					case ActionType.Heal:
						heal.Add(action);
						break;
					case ActionType.Run:
						run.Add(action);
						break;
					case ActionType.Shoot:
						shoot.Add(action);
						break;
					case ActionType.Walk:
						walk.Add(action);
						break;
					case ActionType.Defence:
						defence = action;
						break;
				}
			}

			if (agent.Walked)
			{
				if (drainLife.Count > 0) Result = SelectDrainLifeAction(drainLife);
				else if (heal.Count > 0) Result = SelectHealAction(heal);
				else if (shoot.Count > 0) Result = SelectShootAction(shoot);
				else if (attack.Count > 0) Result = SelectAttackAction(attack);
				else Result = defence;
			}
			else if (agent.SpecialAbility != SpecialAbilityType.None)
			{
				if (drainLife.Count > 0) Result = SelectDrainLifeAction(drainLife);
				else if (heal.Count > 0) Result = SelectDrainLifeAction(heal);
				else if (attack.Count > 0) Result = SelectAttackAction(attack);
				else if (charge.Count > 0) Result = SelectChargeAction(charge);
				else if (run.Count > 0) Result = SelectMoveAction(run);
				else if (walk.Count > 0) Result = SelectMoveAction(walk);
				else Result = defence;
			}
			else if (agent.CanShoot)
			{
				if (shoot.Count > 0) Result = SelectShootAction(shoot);
				else if (walk.Count > 0) Result = SelectMoveAction(walk);
				else if (attack.Count > 0) Result = SelectAttackAction(attack);
				else if (charge.Count > 0) Result = SelectChargeAction(charge);
				else Result = defence;
			}
			else
			{
				if (attack.Count > 0) Result = SelectAttackAction(attack);
				else if (charge.Count > 0) Result = SelectChargeAction(charge);
				else if (run.Count > 0) Result = SelectMoveAction(run);
				else if (walk.Count > 0) Result = SelectMoveAction(walk);
				else Result = defence;
			}

			Running = false;
		}

		CombatAction SelectMoveAction(List<CombatAction> actions)
		{
			// Shooters try to get away from enemies
			Unit agent = actions[0].Agent;
			List<CombatAction> bestActions = new List<CombatAction>();
			if (agent.CanShoot)
			{
				int bestDistance = -1;
				foreach (CombatAction action in actions)
				{
					int enemyDistance = 0;
					foreach (Unit unit in Units)
					{
						if (agent.PlayerId == unit.PlayerId) continue;
						enemyDistance += agent.Tile.Position.Distance(unit.Tile.Position);
					}
					if (bestDistance == -1) bestDistance = enemyDistance;

					if (bestDistance == enemyDistance) bestActions.Add(action);
					else if (bestDistance > enemyDistance)
					{
						bestDistance = enemyDistance;
						bestActions.Clear();
						bestActions.Add(action);
					}
				}
			}

			// Melee tryes to get close to the enemies
			else
			{
				List<Hex> blocked = ActionGenerator.GenBlocked(Units, Grid.Tiles);

				int shortestDistance = 100;
				List<Hex> bestPath = new List<Hex>();
				foreach (Unit unit in Units)
				{
					if (agent.PlayerId == unit.PlayerId) continue;
					Hex destination = unit.Tile.Position;
					blocked.Remove(destination);
					List<Hex> path = agent.Tile.Position.PathTo(unit.Tile.Position, blocked);
					if (path == null) continue;
					blocked.Add(destination);
					if (path.Count < shortestDistance)
					{
						shortestDistance = path.Count;
						bestPath = path;
					}
				}

				int bestProgression = -1;
				foreach (CombatAction action in actions)
				{
					for (int i = 0; i < bestPath.Count; ++i)
					{
						if (bestPath[i].Equals(action.Position))
						{
							if (i == bestProgression) bestActions.Add(action);
							else if (i > bestProgression)
							{
								bestProgression = i;
								bestActions.Clear();
								bestActions.Add(action);
							}
						}
					}
				}
			}

			if (bestActions.Count == 0) return actions[Rng.Next(actions.Count)];
			else return bestActions[Rng.Next(bestActions.Count)];
		}

		CombatAction SelectDrainLifeAction(List<CombatAction> actions)
		{
			// prefer actions that can kill a target
			List<CombatAction> bestActions = new List<CombatAction>();
			foreach (CombatAction action in actions)
			{
				if (action.Target.HitPoints < (1 - action.Target.Stats.MagicDefence) * action.Agent.SpecialAbilityStrength) bestActions.Add(action);
			}

			if (bestActions.Count == 0) return actions[Rng.Next(actions.Count)];
			else return bestActions[Rng.Next(bestActions.Count)];
		}

		CombatAction SelectHealAction(List<CombatAction> actions)
		{
			// maximize healing output
			List<CombatAction> bestActions = new List<CombatAction>();
			foreach (CombatAction action in actions)
			{
				int hpLost = action.Target.Stats.Vitality - action.Target.HitPoints;
				if (hpLost >= action.Agent.SpecialAbilityStrength) bestActions.Add(action);
			}

			if (bestActions.Count == 0) return actions[Rng.Next(actions.Count)];
			else return bestActions[Rng.Next(bestActions.Count)];
		}

		CombatAction SelectShootAction(List<CombatAction> actions)
		{
			Unit agent = actions[0].Agent;
			float avgDmg = agent.Stats.ShootMinDmg + ((agent.Stats.ShootMaxDmg - agent.Stats.ShootMinDmg) / 2);
			return SelectDmgAction(actions, avgDmg);
		}

		CombatAction SelectAttackAction(List<CombatAction> actions)
		{
			Unit agent = actions[0].Agent;
			float avgDmg = agent.Stats.AttackMinDmg + ((agent.Stats.AttackMaxDmg - agent.Stats.AttackMinDmg) / 2);
			return SelectDmgAction(actions, avgDmg);
		}

		CombatAction SelectChargeAction(List<CombatAction> actions)
		{
			return SelectAttackAction(actions);
		}

		CombatAction SelectDmgAction(List<CombatAction> actions, float avgDmg)
		{
			// primarily target units that can be killed with one shot
			List<CombatAction> goodActions = new List<CombatAction>();
			foreach (CombatAction action in actions)
			{
				if (action.Target.HitPoints <= avgDmg * (1 - action.Target.Stats.Defence)) goodActions.Add(action);
			}
			if (goodActions.Count == 0) goodActions = actions;

			// secondarily target units with low armor
			float lowestDefence = 0.0f;
			List<CombatAction> bestActions = new List<CombatAction>();
			foreach (CombatAction action in goodActions)
			{
				if (lowestDefence == action.Target.Stats.Defence) bestActions.Add(action);
				else if (lowestDefence > action.Target.Stats.Defence)
				{
					lowestDefence = action.Target.Stats.Defence;
					bestActions.Clear();
					bestActions.Add(action);
				}
			}
			if (bestActions.Count == 0) bestActions = goodActions;

			return bestActions[Rng.Next(bestActions.Count)];
		}
	}
}
