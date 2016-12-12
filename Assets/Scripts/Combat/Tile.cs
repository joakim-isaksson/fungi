using UnityEngine;
using Hexagon;

namespace Combat
{
	public class Tile : MonoBehaviour
	{
		[Header("Type")]
		public bool Impassable;
		public bool Boarder;

		[Header("Sprites")]
		public Sprite Default;
		public Sprite Active;
		public Sprite Walk;
		public Sprite Run;
		public Sprite Charge;
		public Sprite Attack;
		public Sprite Shoot;
		public Sprite Heal;
		public Sprite DrainLife;

		[HideInInspector]
		public Hex Position;

		[HideInInspector]
		public CombatAction LinkedAction;

		SpriteRenderer spriteRenderer;

		static int order;

		void Start()
		{
			if (Boarder) return;
			spriteRenderer = GetComponent<SpriteRenderer>();
			spriteRenderer.sortingOrder = order++;
			spriteRenderer.sprite = Default;
		}

		public void RemoveAction()
		{
			if (Boarder) return;
			spriteRenderer.sprite = Default;
			LinkedAction = null;
		}

		public void SetActive()
		{
			spriteRenderer.sprite = Active;
		}

		public void LinkAction(CombatAction action)
		{
			switch (action.Type)
			{
				case ActionType.Walk:
					spriteRenderer.sprite = Walk;
					LinkedAction = action;
					break;
				case ActionType.Run:
					spriteRenderer.sprite = Run;
					LinkedAction = action;
					break;
				case ActionType.Attack:
					spriteRenderer.sprite = Attack;
					LinkedAction = action;
					break;
				case ActionType.Shoot:
					spriteRenderer.sprite = Shoot;
					LinkedAction = action;
					break;
				case ActionType.Charge:
					spriteRenderer.sprite = Charge;
					LinkedAction = action;
					break;
				case ActionType.Heal:
					spriteRenderer.sprite = Heal;
					LinkedAction = action;
					break;
				case ActionType.DrainLife:
					spriteRenderer.sprite = DrainLife;
					LinkedAction = action;
					break;
			}
		}
	}
}