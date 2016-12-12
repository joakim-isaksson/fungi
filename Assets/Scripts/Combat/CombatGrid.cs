using UnityEngine;
using Hexagon;
using Utils;
using System.Collections.Generic;

namespace Combat
{
	public class CombatGrid : MonoBehaviour
	{
		[Header("Grid")]
		public int GridWidth;
		public int GridHeight;
		public float ImpassableTerrainProbability;

		[Header("Tiles")]
		public Vector2 TileSize;
		public Vector2 TileOverlap;
		public Vector2 TilePadding;

		[Header("Prefabs")]
		public WeightedPrefab[] PassablePrefabs;
		public WeightedPrefab[] ImpassablePrefabs;
		public WeightedPrefab[] BoarderPrefabs;

		[HideInInspector]
		public Dictionary<Hex, Tile> HexToTile;

		[HideInInspector]
		public List<Tile> Tiles;

		List<List<Tile>> SpawnPoints;

		GameManager Game;

		void Awake()
		{
			Game = GameManager.instance;
		}

		public void Instantiate()
		{
			GenerateGrid(
				CreateWeightedList(PassablePrefabs),
				CreateWeightedList(ImpassablePrefabs),
				CreateWeightedList(BoarderPrefabs)
			);
		}

		WeightedList<WeightedPrefab> CreateWeightedList(WeightedPrefab[] prefabs)
		{
			List<WeightedPrefab> objects = new List<WeightedPrefab>();
			List<float> weights = new List<float>();
			foreach (WeightedPrefab prefab in prefabs)
			{
				objects.Add(prefab);
				weights.Add(prefab.Weight);
			}
			return new WeightedList<WeightedPrefab>(weights, objects);
		}

		void GenerateGrid(WeightedList<WeightedPrefab> passables, WeightedList<WeightedPrefab> impassables, WeightedList<WeightedPrefab> boarders)
		{
			HexToTile = new Dictionary<Hex, Tile>();
			Tiles = new List<Tile>();

			SpawnPoints = new List<List<Tile>>();
			SpawnPoints.Add(new List<Tile>());
			SpawnPoints.Add(new List<Tile>());

			GameObject container = new GameObject("Grid");
			container.transform.parent = transform;

			for (int y = 0; y < GridHeight; ++y)
			{
				for (int x = 0; x < GridWidth; ++x)
				{
					// Determinate what kind of tile we are going to create
					bool impassable = false;
					bool boarder = false;
					bool player1Spawn = false;
					bool player2Spawn = false;
					if (x == 0 || y == 0 || x == GridWidth - 1 || y == GridHeight - 1)
					{
						boarder = true;
						impassable = true;
					}
					else if (x == 1) player1Spawn = true;
					else if (x == GridWidth - 2) player2Spawn = true;
					else if (y != GridHeight - 2 && Random.value < ImpassableTerrainProbability) impassable = true;

					// Calculate position for the tile
					float offSetX = x * (TileSize.x - TileOverlap.x) + x * TilePadding.x;
					float offSetY = -y * (TileSize.y - TileOverlap.y) - y * TilePadding.y;
					if (y % 2 == 0) offSetX -= (TileSize.x + TilePadding.y) / 2;

					// Select prefab for the tile
					GameObject prefab;
					if (boarder) prefab = boarders.GetRandom().Prefab;
					else if (impassable) prefab = impassables.GetRandom().Prefab;
					else prefab = passables.GetRandom().Prefab;

					// Instantiate the new tile and move it to the right world position
					GameObject obj = (GameObject)Instantiate(prefab, container.transform);
					obj.gameObject.transform.Translate(offSetX, offSetY, 0);

					Tile tile = obj.GetComponent<Tile>();
					tile.Position = Hex.FromOddR(x, y);
					HexToTile.Add(tile.Position, tile);
					Tiles.Add(tile);

					if (player1Spawn) SpawnPoints[0].Add(tile);
					else if (player2Spawn) SpawnPoints[1].Add(tile);
				}
			}

			// Center the grid's world position
			float totalWidth = (GridWidth - 1.5f) * (TileSize.x - TileOverlap.x) + (GridWidth - 1.5f) * TilePadding.x;
			float totalHeight = -(GridHeight - 1) * (TileSize.y - TileOverlap.y) - (GridHeight - 1) * TilePadding.y;
			transform.Translate(-totalWidth / 2, -totalHeight / 2, 0);
		}

		public Unit SpawnUnit(UnitInfo info, int playerId)
		{
			Tile spawnPoint = SpawnPoints[playerId][Random.Range(0, SpawnPoints[playerId].Count)];
			SpawnPoints[playerId].Remove(spawnPoint);

			GameObject obj = (GameObject)Instantiate(Game.UnitTypeToPrefab[info.Type], transform);
			Unit unit = obj.GetComponent<Unit>();
			unit.Spawn(spawnPoint, info.Size, playerId, info.AIControlled);

			return unit;
		}

		public void ClearActions()
		{
			foreach (Tile tile in Tiles)
			{
				tile.RemoveAction();
			}
		}

		public void ShowActions(List<CombatAction> actions, bool specialActions)
		{
			actions[0].Agent.Tile.SetActive();
			foreach (CombatAction action in actions)
			{
				if (specialActions && (action.Type != ActionType.Heal && action.Type != ActionType.DrainLife)) continue;
				else if (!specialActions && (action.Type == ActionType.Heal || action.Type == ActionType.DrainLife)) continue;
				else if (action.Type == ActionType.Wait || action.Type == ActionType.Defence) continue;

				Tile tile = HexToTile[action.Position];
				tile.LinkAction(action);
			}
		}
	}
}
