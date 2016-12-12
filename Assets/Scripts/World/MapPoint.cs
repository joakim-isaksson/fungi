using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Combat;
using System;

namespace World
{
	public class MapPoint : MonoBehaviour
	{
		public List<Dialog> Dialogs;

		public List<UnitInfo> Troops;
		public GameObject TroopIconPrefab;
		public AudioClip CollectTroopsSfx;

		public List<UnitInfo> EnemyUnits;
		public GameObject EnemyIconPrefab;
		public string SceneName;

		public GameObject HighLightEmpty;
		public GameObject HighLightTroops;
		public GameObject HighLightEnemy;

		public MapPoint[] Neighbours;

		[HideInInspector]
		public bool HighLighted;

		GameObject EnemyIcon;
		GameObject TroopIcon;

		GameManager Game;
		MapManager Map;
		DialogManager Dialog;

		AudioSource Asrc;

		void Awake()
		{
			if (EnemyIconPrefab != null)
			{
				EnemyIcon = (GameObject)Instantiate(EnemyIconPrefab, transform, false);
				EnemyIcon.SetActive(false);
			}
			else if (TroopIconPrefab != null)
			{
				TroopIcon = (GameObject)Instantiate(TroopIconPrefab, transform, false);
				TroopIcon.SetActive(false);
			}

			Asrc = GetComponent<AudioSource>();
		}

		void Start()
		{
			Game = GameManager.instance;
			Map = MapManager.instance;
			Dialog = DialogManager.instance;
		}

		public void OnArrival()
		{
			foreach (MapPoint point in Map.MapPoints)
			{
				point.ClearHighlights();
			}

			if (Dialogs.Count > 0) Dialog.StartDialog(Dialogs, delegate { OnDialogFinished(); });
			else OnDialogFinished();
		}

		public void OnDialogFinished()
		{
			Dialogs.Clear();

			if (EnemyUnits.Count > 0) Game.StartCombat(EnemyUnits, SceneName);
			else if (Troops.Count > 0) CollectTroops();
			else Map.OnMapPointReady(this);
		}

		public void HighLightNeighbours()
		{
			foreach (MapPoint point in Neighbours)
			{
				point.HighLight();
				point.RevealIcons();
			}
		}

		public void ClearIcons()
		{
			if (TroopIcon != null) TroopIcon.SetActive(false);
			else if (EnemyIcon != null) EnemyIcon.SetActive(false);
		}

		void RevealIcons()
		{
			if (TroopIcon != null && Troops.Count > 0) TroopIcon.SetActive(true);
			else if (EnemyIcon != null && EnemyUnits.Count > 0) EnemyIcon.SetActive(true);
		}

		void ClearHighlights()
		{
			HighLighted = false;
			HighLightEmpty.SetActive(false);
			HighLightTroops.SetActive(false);
			HighLightEnemy.SetActive(false);
		}

		void HighLight()
		{
			HighLighted = true;
			if (Troops.Count > 0) HighLightTroops.SetActive(true);
			else if (EnemyUnits.Count > 0) HighLightEnemy.SetActive(true);
			else HighLightEmpty.SetActive(true);
		}

		void CollectTroops()
		{
			foreach (UnitInfo troop in Troops)
			{
				if (troop.Solo) Game.PlayerUnits.Add(troop);
				else
				{
					bool newType = true;
					foreach (UnitInfo unit in Game.PlayerUnits)
					{
						if (unit.Solo) continue;
						else if (unit.Type == troop.Type)
						{
							unit.Size += troop.Size;
							newType = false;
							break;
						}
					}
					if (newType) Game.PlayerUnits.Add(troop);
				}
			}

			Asrc.PlayOneShot(CollectTroopsSfx);

			Troops.Clear();
			ClearIcons();

			Map.OnMapPointReady(this);
		}
	}
}
