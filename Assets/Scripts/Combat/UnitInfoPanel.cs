using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Combat
{
	public enum InfoLevel
	{
		None, Size, SizeAndHealth, All
	}

	public class UnitInfoPanel : MonoBehaviour
	{
		public GameObject TurnNumber;
		public GameObject UnitSize;
		public GameObject HealthBar;
		public GameObject HealthBarFg;
		public Sprite DefendingUnitSizeSprite;

		public Text UnitSizeText;
		public Text UnitTurnText;

		Unit Unit;
		CombatManager manager;

		Image UnitSizeBg;
		Sprite DefaultUnitSizeSprite;

		void Start()
		{
			manager = FindObjectOfType<CombatManager>();
			Unit = GetComponentInParent<Unit>();
			UnitSizeBg = UnitSize.GetComponentInChildren<Image>();
			DefaultUnitSizeSprite = UnitSizeBg.sprite;
		}

		void Update()
		{
			if (!Unit.Alive)
			{
				HealthBar.SetActive(false);
				TurnNumber.SetActive(false);
				UnitSize.SetActive(false);
			}
			else
			{
				switch (manager.InfoLevel)
				{
					case InfoLevel.None:
						HealthBar.SetActive(false);
						TurnNumber.SetActive(false);
						UnitSize.SetActive(false);
						break;
					case InfoLevel.Size:
						HealthBar.SetActive(false);
						TurnNumber.SetActive(false);
						UnitSize.SetActive(true);
						UpdateUnitSize();
						break;
					case InfoLevel.SizeAndHealth:
						HealthBar.SetActive(true);
						TurnNumber.SetActive(false);
						UnitSize.SetActive(true);
						UpdateUnitSize();
						UpdateHealthBar();
						break;
					case InfoLevel.All:
						HealthBar.SetActive(true);
						TurnNumber.SetActive(true);
						UnitSize.SetActive(true);
						UpdateUnitSize();
						UpdateHealthBar();
						UpdateTurnNumber();
						break;
				}
			}
		}

		void UpdateHealthBar()
		{
			HealthBarFg.transform.localScale = new Vector3(
				((float)Unit.HitPoints / Unit.Stats.Vitality),
				HealthBarFg.transform.localScale.y,
				HealthBarFg.transform.localScale.z
			);
		}

		void UpdateTurnNumber()
		{
			if (Unit.TurnNumber == 0) UnitTurnText.text = "-";
			else UnitTurnText.text = "" + Unit.TurnNumber;
		}

		void UpdateUnitSize()
		{
			if (Unit.Size == 0) UnitSizeText.text = "-";
			else UnitSizeText.text = "" + Unit.Size;

			if (Unit.Defending) UnitSizeBg.sprite = DefendingUnitSizeSprite;
			else UnitSizeBg.sprite = DefaultUnitSizeSprite;
		}
	}
}
