using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

    [SerializeField] private float delaySecondsToChangeScene;
    public void ChangeScene(string sceneName)
    {
        StartCoroutine(loadAsynchronously(sceneName));
    }

    IEnumerator loadAsynchronously(string sceneName)
    {
        yield return new WaitForSeconds(delaySecondsToChangeScene);
        SceneManager.LoadScene(sceneName);

    }
}
