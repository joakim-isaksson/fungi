using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Utils;
using System.Collections;
using World;

namespace Combat
{
	public class CombatManager : MonoBehaviour
	{
        public List<Dialog> Dialogs;

        [Header("Buttons")]
		public InfoLevel InfoLevel;
		public Button BtnWait;
		public Button BtnDefend;
		public Button BtnSpecial;
		public Button BtnEscape;
		public Sprite SpecialSprite0;
		public Sprite SpecialSprite1;
		public Sprite SpecialSprite2;
		public GameObject SpecialActionEffect;

		public const int PlayerId = 0;
		public const int EnemyId = 1;

		List<Unit> Units;
		List<Unit> TurnOrder;

		bool WaitingPlayerAction;
		bool SpecialAbilitySelection;

		GameManager Game;
		CombatGrid Grid;
		ScreenFaider Faider;
		Image SpecialActionImage;
		SimpleAI AI;
		AudioManager AudioManager;

        bool dialogRunning;

		void Awake()
		{
			AudioManager = AudioManager.instance;
		}

		public void OnInfoButtonPressed()
		{
			if (InfoLevel == InfoLevel.None) InfoLevel = InfoLevel.Size;
			else if (InfoLevel == InfoLevel.Size) InfoLevel = InfoLevel.SizeAndHealth;
			else if (InfoLevel == InfoLevel.SizeAndHealth) InfoLevel = InfoLevel.All;
			else InfoLevel = InfoLevel.None;
		}

		public void OnEscapeButtonPressed()
		{
			BtnEscape.interactable = false;
			AudioManager.Fade("CombatMusicVol", 0.0f, 1.0f);
			AudioManager.Fade("WorldMapMusicVol", 1.0f, 2.0f);
			Faider.FadeIn(Color.black, 1.0f, delegate { Game.OnCombatEnded(CombatResult.Escape); });
		}

		void Start()
		{
			AI = new SimpleAI();

			Game = GameManager.instance;
			Faider = ScreenFaider.instance;

			SpecialActionImage = BtnSpecial.GetComponent<Image>();
			Grid = GetComponent<CombatGrid>();
			Grid.Instantiate();

			Units = new List<Unit>();
			foreach (UnitInfo info in Game.PlayerUnits)
			{
				Units.Add(Grid.SpawnUnit(info, PlayerId));
			}
			foreach (UnitInfo info in Game.EnemyUnits)
			{
				Units.Add(Grid.SpawnUnit(info, EnemyId));
			}

			BtnEscape.interactable = false;
			DisableActionButtons();

            dialogRunning = true;
            Faider.FadeOut(Color.black, 0.5f, delegate
            {
                if (Dialogs.Count > 0)
                {
                    DialogManager.instance.StartDialog(Dialogs, delegate {
                        dialogRunning = false;
                        BtnEscape.interactable = true;
                        NextRound();
                    });
                }
                else
                {
                    dialogRunning = false;
                    BtnEscape.interactable = true;
                    NextRound();
                }
            });
		}

		void Update()
		{
            if (dialogRunning) return;

			if (Input.GetButtonDown("Cancel"))
			{
				WaitingPlayerAction = false;
				OnEscapeButtonPressed();
			}
			else if (WaitingPlayerAction && Input.GetMouseButtonDown(0))
			{
				CombatAction action = null;
				RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
				if (hit.collider != null)
				{
					action = hit.collider.gameObject.GetComponent<Tile>().LinkedAction;
				}
				if (action != null)
				{
					StartPlayerAction(action);
				}
			}
		}

		void StartPlayerAction(CombatAction action)
		{
			SpecialAbilitySelection = false;
			WaitingPlayerAction = false;
			DisableActionButtons();
			Grid.ClearActions();
			StartAction(action);
		}

		void StartAiAction(CombatAction action)
		{
			StartAction(action);
		}

		void EndTurn(Unit agent, CombatAction action)
		{
			if (action.Type == ActionType.Heal || action.Type == ActionType.DrainLife) agent.CoolDownLeft = agent.SpecialAbilityCoolDown;
			else if ((action.Type != ActionType.Wait && action.Type != ActionType.Walk) && --agent.CoolDownLeft < 0) agent.CoolDownLeft = 0;

			agent.TurnNumber = 0;
			TurnOrder.Remove(agent);

			bool playerAlive = false;
			bool enemyAlive = false;
			bool heroDied = false;
			bool antiHeroDied = false;
			List<Unit> removeUnits = new List<Unit>();
			foreach (Unit u in Units)
			{
				if (!u.Alive)
				{
					removeUnits.Add(u);
					if (u.Type == UnitType.Healer) heroDied = true;
					else if (u.Type == UnitType.Sorcerer) antiHeroDied = true;
				}
				else if (u.PlayerId == PlayerId) playerAlive = true;
				else if (u.PlayerId == EnemyId) enemyAlive = true;
			}
			foreach (Unit u in removeUnits)
			{
				Units.Remove(u);
				TurnOrder.Remove(u);
			}

			if (heroDied || !playerAlive)
			{
				// PLAYER LOSE
				AudioManager.Fade("CombatMusicVol", 0.0f, 0.1f);
				AudioManager.PlayCombatEndMusic(false);

				DelayedAction(2.0f, delegate
				{
					AudioManager.Fade("WorldMapMusicVol", 1.0f, 2.0f, 1.0f);
					Faider.FadeIn(Color.black, 1.0f, delegate { Game.OnCombatEnded(CombatResult.Loss); });
				});
			}
			else if (antiHeroDied || !enemyAlive)
			{
				// PLAYER WIN
				AudioManager.Fade("CombatMusicVol", 0.0f, 0.1f);
				AudioManager.PlayCombatEndMusic(true);

				DelayedAction(2.0f, delegate
				{
					AudioManager.Fade("WorldMapMusicVol", 1.0f, 2.0f, 1.0f);
					Faider.FadeIn(Color.black, 1.0f, delegate { Game.OnCombatEnded(CombatResult.Win); });
				});
			}
			else
			{
				// Next turn or round
				if (TurnOrder.Count > 0) StartTurn(TurnOrder[0]);
				else NextRound();
			}
		}

		void NextRound()
		{
			Debug.Log("Start New Round");

			SortedDictionary<int, List<Unit>> unitsByInitiative = new SortedDictionary<int, List<Unit>>();
			foreach (Unit unit in Units)
			{
				// Reset unit turn states
				unit.CounterAttackUsed = false;
				unit.Walked = false;

				if (!unitsByInitiative.ContainsKey(unit.Stats.Initiative)) unitsByInitiative.Add(unit.Stats.Initiative, new List<Unit>());
				unitsByInitiative[unit.Stats.Initiative].Add(unit);
			}

			TurnOrder = new List<Unit>();
			foreach (List<Unit> units in unitsByInitiative.Values.Reverse())
			{
				do
				{
					Unit unit = units[Random.Range(0, units.Count)];
					TurnOrder.Add(unit);
					units.Remove(unit);
				} while (units.Count > 0);
			}

			StartTurn(TurnOrder[0]);
		}

		void StartTurn(Unit agent)
		{
			UpdateTurnNumbers();

			agent.Defending = false;

			List<CombatAction> actions = ActionGenerator.GenActions(agent, Units, Grid);

			if (agent.AIControlled)
			{
				Debug.Log("Turn: AI");
				StartCoroutine(AI.SelectAction(actions, Units, Grid, StartAiAction));
			}
			else
			{
				Debug.Log("Turn: Player");
				Grid.ShowActions(actions, false);
				WaitingPlayerAction = true;
				EnableActionButtons(agent, actions);
			}
		}

		void StartAction(CombatAction action)
		{
			Debug.Log("Action: " + System.Enum.GetName(typeof(ActionType), action.Type));

			switch (action.Type)
			{
				case ActionType.Walk:
					List<Tile> walkPath = ActionGenerator.GenPath(action.Agent, action.Position, Units, Grid);
					StartCoroutine(action.Agent.Move(walkPath, false, delegate { StartTurn(action.Agent); }));
					break;
				case ActionType.Run:
					List<Tile> runPath = ActionGenerator.GenPath(action.Agent, action.Position, Units, Grid);
					StartCoroutine(action.Agent.Move(runPath, true, delegate { EndTurn(action.Agent, action); }));
					break;
				case ActionType.Attack:
					StartCoroutine(action.Agent.Attack(action.Target, delegate { EndTurn(action.Agent, action); }, false));
					break;
				case ActionType.Shoot:
					StartCoroutine(action.Agent.Shoot(action.Target, delegate { EndTurn(action.Agent, action); }));
					break;
				case ActionType.Charge:
					List<Tile> chargePath = ActionGenerator.GenPath(action.Agent, action.Target.Tile.Position, Units, Grid);
					chargePath.RemoveAt(chargePath.Count - 1);
					StartCoroutine(action.Agent.Move(chargePath, false, delegate
					{
						StartCoroutine(action.Agent.Attack(action.Target, delegate { EndTurn(action.Agent, action); }, false));
					}));
					break;
				case ActionType.Defence:
					StartCoroutine(action.Agent.Defence(delegate { EndTurn(action.Agent, action); }));
					break;
				case ActionType.Wait:
					TurnOrder.Add(action.Agent);
					StartCoroutine(action.Agent.Wait(delegate { EndTurn(action.Agent, action); }));
					break;
				case ActionType.Heal:
					StartCoroutine(action.Agent.Heal(action.Target, delegate { EndTurn(action.Agent, action); }));
					break;
				case ActionType.DrainLife:
					StartCoroutine(action.Agent.DrainLife(action.Target, delegate { EndTurn(action.Agent, action); }));
					break;
			}
		}

		void UpdateTurnNumbers()
		{
			int turnNumber = 1;
			foreach (Unit unit in TurnOrder)
			{
				unit.TurnNumber = turnNumber++;
			}
		}

		void DisableActionButtons()
		{
			BtnWait.interactable = false;
			BtnWait.onClick.RemoveAllListeners();
			BtnDefend.interactable = false;
			BtnDefend.onClick.RemoveAllListeners();
			BtnSpecial.interactable = false;
			BtnSpecial.onClick.RemoveAllListeners();
			BtnSpecial.gameObject.SetActive(false);
			SpecialActionEffect.SetActive(false);
		}

		void EnableActionButtons(Unit agent, List<CombatAction> actions)
		{
			bool specialAbilityAvailable = false;
			foreach (CombatAction action in actions)
			{
				CombatAction refAction = action;
				switch (action.Type)
				{
					case ActionType.Wait:
						BtnWait.interactable = true;
						BtnWait.onClick.AddListener(delegate { StartPlayerAction(refAction); });
						break;
					case ActionType.Defence:
						BtnDefend.interactable = true;
						BtnDefend.onClick.AddListener(delegate { StartPlayerAction(refAction); });
						break;
					case ActionType.Heal:
					case ActionType.DrainLife:
						specialAbilityAvailable = true;
						SpecialActionEffect.SetActive(true);

						// Replace the particles to correct position (should be using UI particles instead)
						SpecialActionEffect.transform.position = Camera.main.ScreenToWorldPoint(BtnSpecial.transform.position);
						SpecialActionEffect.transform.position = new Vector3(
							SpecialActionEffect.transform.position.x - 0.5f,
							SpecialActionEffect.transform.position.y + 0.5f, -5.0f
						);
						break;

				}
			}

			if (agent.SpecialAbility != SpecialAbilityType.None)
			{
				BtnSpecial.gameObject.SetActive(true);
				if (agent.CoolDownLeft == 0) SpecialActionImage.sprite = SpecialSprite0;
				else if (agent.CoolDownLeft == 1) SpecialActionImage.sprite = SpecialSprite1;
				else SpecialActionImage.sprite = SpecialSprite2;

				if (specialAbilityAvailable)
				{
					BtnSpecial.interactable = true;
					BtnSpecial.onClick.AddListener(delegate { SpecialAbilityButtonPressed(actions); });
				}
			}
		}

		void SpecialAbilityButtonPressed(List<CombatAction> actions)
		{
			SpecialAbilitySelection = !SpecialAbilitySelection;
			Grid.ClearActions();
			Grid.ShowActions(actions, SpecialAbilitySelection);
		}

		void DelayedAction(float delay, System.Action callback)
		{
			StartCoroutine(StartDelayedAction(delay, callback));
		}

		IEnumerator StartDelayedAction(float delay, System.Action callback)
		{
			yield return new WaitForSeconds(delay);

			callback();
		}
	}
}
