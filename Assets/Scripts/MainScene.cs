using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScene : MonoBehaviour
{
    private static MainScene Instance;

    public SceneReference[] SceneTrack = null;

    private int currentScene = -1;

    private bool busy = false;

    void Start()
    {
        MainScene.Instance = this;

        MainScene.LoadNextScene();
    }

    public static void LoadNextScene()
    {
        MainScene instance = MainScene.Instance;

        if (instance.busy)
        {
            return;
        }

        instance.StartCoroutine(nameof(loadNextScene));
    }

    IEnumerator loadNextScene()
    {
        this.busy = true;
        if (this.currentScene >= 0)
        {
            yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(this.SceneTrack[this.currentScene]);
        }

        this.currentScene = (this.currentScene + 1) % this.SceneTrack.Length;
        yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(this.SceneTrack[this.currentScene], UnityEngine.SceneManagement.LoadSceneMode.Additive);
        this.busy = false;
    }
}
