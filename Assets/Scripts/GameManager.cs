using UnityEngine;
using System.Collections.Generic;
using Combat;
using World;
using Utils;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
	[HideInInspector]
	public static GameManager instance = null;

	public List<UnitPrefab> UnitPrefabs;

	public List<UnitInfo> StartingUnits;

	[HideInInspector]
	public Dictionary<UnitType, GameObject> UnitTypeToPrefab;

	[HideInInspector]
	public List<UnitInfo> PlayerUnits;

	[HideInInspector]
	public List<UnitInfo> EnemyUnits;

	ScreenFaider Faider;
	MapManager Map;
	AudioManager AudioManager;

	public static void DestroySingleton()
	{
		Destroy(instance.gameObject);
		instance = null;
	}

	void Awake()
	{
		// Singleton
		if (instance == null) instance = this;
		else if (!instance.Equals(this)) Destroy(gameObject);
		DontDestroyOnLoad(gameObject);

		UnitTypeToPrefab = new Dictionary<UnitType, GameObject>();
		foreach (UnitPrefab prefab in UnitPrefabs)
		{
			UnitTypeToPrefab.Add(prefab.Type, prefab.Obj);
		}

		PlayerUnits.AddRange(StartingUnits);
	}

	void Start()
	{
		Faider = ScreenFaider.instance;
		Map = MapManager.instance;
		AudioManager = AudioManager.instance;

		AudioManager.Fade("SfxVol", 1.0f, 1.0f);
		AudioManager.Fade("AtmoVol", 1.0f, 1.0f);
		AudioManager.Fade("WorldMapMusicVol", 1.0f, 2.0f);
		Faider.FadeOut(Color.black, 2.0f, delegate
		{
			Map.OnStartGame();
		});
	}

	public void OnCombatEnded(CombatResult result)
	{
		// The end
		if (result == CombatResult.Win && Map.Hero.Location.Equals(Map.EndingPoint))
		{
			MapManager.DestroySingleton();
			DialogManager.DestroySingleton();
			DestroySingleton();
			SceneManager.LoadScene("Credits", LoadSceneMode.Single);
		}

		// Continue on world map
		else
		{
			SceneManager.LoadScene("WorldMap", LoadSceneMode.Single);
			Map.Show();
			if (result == CombatResult.Win)
			{
				Map.Hero.Location.EnemyUnits.Clear();
				Map.Hero.Location.ClearIcons();

				Faider.FadeOut(Color.black, 0.5f, delegate
				{
					Map.OnMapPointReady(Map.Hero.Location);
				});
			}
			else if (result == CombatResult.Escape) Faider.FadeOut(Color.black, 0.5f, delegate
			{
				StartCoroutine(Map.Hero.Move(Map.Hero.PrevLocation));
			});
			else if (result == CombatResult.Loss) Faider.FadeOut(Color.black, 0.5f, delegate
			{
				StartCoroutine(Map.Hero.Move(Map.Hero.PrevLocation));
			});
		}
	}

	public void StartCombat(List<UnitInfo> enemies, string sceneName)
	{
		AudioManager.Fade("WorldMapMusicVol", 0.0f, 1.0f);
		AudioManager.Fade("CombatMusicVol", 1.0f, 2.0f);

		Faider.FadeIn(Color.black, 1.0f, delegate
		{
			EnemyUnits = enemies;
			Map.Hide();
			SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
		});
	}

	public void GoToMenu()
	{
		AudioManager.Fade("AtmoVol", 1.0f, 1.0f);
		AudioManager.Fade("CombatEndMusicVol", 0.0f, 1.0f);
		AudioManager.Fade("CombatMusicVol", 0.0f, 1.0f);
		AudioManager.Fade("SfxVol", 0.0f, 1.0f);
		AudioManager.Fade("WorldMapMusicVol", 0.0f, 1.0f);

		Faider.FadeIn(Color.black, 1.0f, delegate
		{
			MapManager.DestroySingleton();
			DialogManager.DestroySingleton();
			DestroySingleton();
			SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
		});
	}
}
