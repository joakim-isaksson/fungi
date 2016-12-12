using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace World
{
	public class DialogManager : MonoBehaviour
	{
		[HideInInspector]
		public static DialogManager instance = null;

		public float TextSpeed = 0.03f;
		public float StartDelay = 1.0f;
		public float SwitchDelay = 0.5f;
		public float EndDelay = 1.5f;

		public Image LeftAvatar;
		public Image RightAvatar;

		public Text DialogText;

		[Header("Sounds")]
		public AudioClip OpenSfx;
		public float OpenSfxDelay = 0.15f;
		public float OpenSfxPitch = 0.5f;
		public AudioClip ScrollingSfx;

		Animator Anim;
		AudioSource Asrc;

		bool Animating;
		bool WaitingForPlayerInput;
		bool Skip;

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

			Anim = GetComponent<Animator>();
			Asrc = GetComponent<AudioSource>();
		}

		void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				if (WaitingForPlayerInput) WaitingForPlayerInput = false;
				else if (Animating) Skip = true;
			}
		}

		public void StartDialog(List<Dialog> dialogs, Action callback)
		{
			StartCoroutine(PlayDialog(0, dialogs, callback));
		}

		IEnumerator PlayDialog(int index, List<Dialog> dialogs, Action callback)
		{
			Dialog dialog = dialogs[index];
			DialogText.text = "";

			if (index == 0)
			{
				Asrc.clip = OpenSfx;
				Asrc.loop = false;
				Asrc.pitch = OpenSfxPitch;
				Asrc.PlayDelayed(OpenSfxDelay);

				if (dialog.LeftSide)
				{
					LeftAvatar.sprite = dialog.Image;
					Anim.SetTrigger("ShowLeft");
				}
				else
				{
					RightAvatar.sprite = dialog.Image;
					Anim.SetTrigger("ShowRight");
				}

				yield return new WaitForSeconds(StartDelay);
			}
			else if (dialog.LeftSide != dialogs[index - 1].LeftSide)
			{
				if (dialog.LeftSide)
				{
					LeftAvatar.sprite = dialog.Image;
					Anim.SetTrigger("ShowLeftAvatar");
				}
				else
				{
					RightAvatar.sprite = dialog.Image;
					Anim.SetTrigger("ShowRightAvatar");
				}

				yield return new WaitForSeconds(SwitchDelay);
			}
			else
			{
				if (dialog.LeftSide) LeftAvatar.sprite = dialog.Image;
				else RightAvatar.sprite = dialog.Image;
			}

			Animating = true;
			StartCoroutine(AnimateText(dialogs[index].Text));
			while (Animating) yield return null;

			WaitingForPlayerInput = true;
			while (WaitingForPlayerInput) yield return null;

			if (++index < dialogs.Count) StartCoroutine(PlayDialog(index, dialogs, callback));
			else
			{
				if (dialog.LeftSide) Anim.SetTrigger("HideLeft");
				else Anim.SetTrigger("HideRight");
				yield return new WaitForSeconds(EndDelay);
				callback();
			}
		}

		IEnumerator AnimateText(string text)
		{
			text = text.Replace("\\n", "\n");
			Asrc.clip = ScrollingSfx;
			Asrc.loop = true;
			Asrc.Play();
			for (int i = 1; i <= text.Length; ++i)
			{
				if (Skip)
				{
					Skip = false;
					DialogText.text = text;
					break;
				}
				DialogText.text = text.Substring(0, i);
				yield return new WaitForSeconds(TextSpeed);
			}
			Asrc.Stop();
			Animating = false;
		}
	}
}
