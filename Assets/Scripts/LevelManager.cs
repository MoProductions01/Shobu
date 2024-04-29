using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class just handles going from one scene to another. 
/// </summary>
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
