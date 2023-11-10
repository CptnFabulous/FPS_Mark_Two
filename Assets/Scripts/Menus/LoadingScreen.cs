using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

public class LoadingScreen : MonoBehaviour
{
    public Canvas loadingScreenInfo;
    public Text sceneTitle;
    public UnityEvent<float> progressEffects;
    public Canvas continueScreen;
    public string continueInput;



    public static readonly string loadingSceneName = "Loading Screen";
    public static readonly string mainMenuSceneName = "Main Menu";

    public static void LoadScene(string sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad);
        // StartCoroutine(LoadSequence(newScene));
    }

    public static void ReturnToMainMenu()
    {
        LoadScene(mainMenuSceneName);
    }

    static IEnumerator LoadSequence(string newScene)
    {
        AsyncOperation screenTransition = SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Single);
        // yield return new WaitUntil(() => loadScreenLoad.isDone);
        while (screenTransition.progress < 0.9f)
        {
            Debug.Log("Transition to loading screen, " + screenTransition.progress);
            yield return null;
        }

        Debug.Log("Loading scene fully loaded");

        AsyncOperation newSceneLoad = SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
        newSceneLoad.allowSceneActivation = false;
        // yield return new WaitUntil(() => sequence.isDone);
        while (newSceneLoad.progress < 0.9f)
        {
            Debug.Log("Loading new scene, " + newSceneLoad.progress);
            //screen.progressEffects.Invoke(newSceneLoad.progress);
            yield return null;
        }

        yield return new WaitUntil(() => Input.GetButtonDown("Jump"));
        // Unload load screen scene and activate new scene
        SceneManager.UnloadSceneAsync(loadingSceneName);
        newSceneLoad.allowSceneActivation = true;
    }


    /*
    public static IEnumerator LoadSequence(string newScene)
    {
        // Unloads current scene and loads the loading screen
        AsyncOperation loadScreenLoad = SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Single);
        Debug.Log("Pot roast");
        yield return new WaitUntil(() => loadScreenLoad.isDone);
        Debug.Log("Blah");
        // Use FindObjectOfType to get a load scene management script in the loading screen scene and run load scene stuff from it
        LoadingScreen screen = FindObjectOfType<LoadingScreen>();
        screen.loadingScreenInfo.gameObject.SetActive(false);
        screen.continueScreen.gameObject.SetActive(true);
        screen.sceneTitle.text = newScene;
        Debug.Log("Screen loaded");
        // Starts loading of new scene asynchronously, setting a bool so it doesn't activate immediately
        AsyncOperation sequence = SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
        sequence.allowSceneActivation = false;
        // Initiate a loop within the IEnumerator while waiting for the sequence to load
        while (sequence.isDone == false)
        {
            Debug.Log("Loop");
            // In this loop, do things while waiting for the load sequence to complete
            screen.progressEffects.Invoke(sequence.progress);
            yield return null;
        }

        Debug.Log("Complete");
        // Change load screen graphics to show new scene is ready to activate
        screen.loadingScreenInfo.gameObject.SetActive(false);
        screen.continueScreen.gameObject.SetActive(true);

        Debug.Log("Waiting on input");
        // Wait until the player presses the button to continue
        yield return new WaitUntil(() => Input.GetButtonDown(screen.continueInput));
        // Unload load screen scene and activate new scene
        SceneManager.UnloadSceneAsync(loadingSceneName);
        sequence.allowSceneActivation = true;
    }
    */

}
