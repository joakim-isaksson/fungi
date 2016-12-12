using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Utils;

namespace World
{
	public class MapManager : MonoBehaviour
	{
		[HideInInspector]
		public static MapManager instance = null;

		public MapPoint StartingPoint;
		public MapPoint EndingPoint;

		[HideInInspector]
		public List<MapPoint> MapPoints;

		[HideInInspector]
		public Hero Hero;

		bool waitingForInput;

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
		}

		void Start()
		{
			MapPoints = new List<MapPoint>();
			foreach (GameObject obj in GameObject.FindGameObjectsWithTag("MapPoint"))
			{
				MapPoints.Add(obj.GetComponent<MapPoint>());
			}

			Hero = GameObject.FindGameObjectWithTag("Player").GetComponent<Hero>();
			Hero.transform.position = StartingPoint.transform.position;

			waitingForInput = true;
		}

		public void OnStartGame()
		{
			Hero.PrevLocation = StartingPoint;
			Hero.Location = StartingPoint;
			StartingPoint.OnArrival();
		}

		void Update()
		{
			if (Input.GetButtonDown("Cancel"))
			{
				waitingForInput = false;
				GameManager.instance.GoToMenu();
			}
			else if (waitingForInput && Input.GetMouseButtonDown(0))
			{
				MapPoint point = null;
				RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
				if (hit.collider != null) point = hit.collider.gameObject.GetComponent<MapPoint>();
				if (point != null && point.HighLighted)
				{
					waitingForInput = false;
					StartCoroutine(Hero.Move(point));
				}
			}
		}

		public void OnMapPointReady(MapPoint point)
		{
			waitingForInput = true;
			point.HighLightNeighbours();
		}

		public void Hide()
		{
			gameObject.SetActive(false);
		}

		public void Show()
		{
			gameObject.SetActive(true);
		}
	}
}
