using UnityEngine;
using Utils;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
	public string NextScene;
	public string ExitScene;
	public bool QuitOnExit;

	AudioManager AudioManager;
	ScreenFaider Faider;

	bool loading = true;

	void Start()
	{
		AudioManager = AudioManager.instance;
		Faider = ScreenFaider.instance;

		AudioManager.Fade("WorldMapMusicVol", 0.0f, 1.0f);
		AudioManager.Fade("AtmoVol", 1.0f, 1.0f);

		Faider.SetTo(Color.black);
		Faider.FadeOut(Color.black, 1.0f, delegate
		{
			loading = false;
		});
	}

	void Update()
	{
		if (Input.GetButtonDown("Cancel"))
		{
			if (QuitOnExit) Application.Quit();
			else LoadNextScene(ExitScene);
		}
		else if (Input.anyKeyDown) LoadNextScene(NextScene);
	}

	void LoadNextScene(string name)
	{
		if (loading) return;
		loading = true;

		Faider.FadeIn(Color.black, 1.0f, delegate
		{
			SceneManager.LoadScene(name, LoadSceneMode.Single);
		});
	}
}
