using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    void Update()
    {
        if (Input.anyKey)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        }
    }
}
