using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace Utils
{
	public class ScreenFaider : MonoBehaviour
	{
		[HideInInspector]
		public static ScreenFaider instance = null;

		public Image Foreground;

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

		public void SetTo(Color color)
		{
			Foreground.gameObject.SetActive(true);
			Foreground.color = color;
		}

		public void FadeIn(Color color, float seconds, Action callback)
		{
			StartCoroutine(StartFadeIn(color, seconds, callback));
		}

		public void FadeOut(Color color, float seconds, Action callback)
		{
			StartCoroutine(StartFadeOut(color, seconds, callback));
		}

		IEnumerator StartFadeIn(Color color, float seconds, Action callback)
		{
			Foreground.gameObject.SetActive(true);
			Foreground.color = new Color(color.r, color.g, color.b, 0);

			float progress = 0;
			while (Foreground.color.a < 0.99f)
			{
				progress += (Time.deltaTime / seconds);
				Foreground.color = new Color(color.r, color.g, color.b, Mathf.Lerp(0.0f, 1.0f, progress));
				yield return null;
			}
			Foreground.color = new Color(color.r, color.g, color.b, 1.0f);

			callback();
		}

		IEnumerator StartFadeOut(Color color, float seconds, Action callback)
		{
			Foreground.color = new Color(color.r, color.g, color.b, 1);

			float progress = 0;
			while (Foreground.color.a > 0.01f)
			{
				progress += (Time.deltaTime / seconds);
				Foreground.color = new Color(color.r, color.g, color.b, Mathf.Lerp(1.0f, 0.0f, progress));
				yield return null;
			}
			Foreground.color = new Color(color.r, color.g, color.b, 0.0f);
			Foreground.gameObject.SetActive(false);

			callback();
		}
	}
}
