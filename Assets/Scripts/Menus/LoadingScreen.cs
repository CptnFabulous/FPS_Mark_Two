using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

// Coroutine must be invoked on an object that exists before the loading screen or new scene is loaded, and continues to exist afterwards.

public class SceneLoader : MonoBehaviour
{
    public static readonly string loadingSceneName = "Loading Screen";
    public static readonly string mainMenuSceneName = "Main Menu";

    static SceneLoader _instance;

    public static SceneLoader instance
    {
        get
        {
            if (_instance == null)
            {
                // Spawn in a SceneLoader, and ensure it isn't destroyed when a scene is unloaded
                _instance = new GameObject("Scene Loader").AddComponent<SceneLoader>();
                DontDestroyOnLoad(_instance);
            }

            return _instance;
        }
    }

    public bool loadInProgress { get; private set; }

    public void LoadScene(string sceneToLoad)
    {
        Debug.Log($"{this}: loading scene '{sceneToLoad}'");
        StartCoroutine(LoadSceneAsync(sceneToLoad));
    }
    public void ReturnToMainMenu() => LoadScene(mainMenuSceneName);

    IEnumerator LoadSceneAsync(string newScene)
    {
        loadInProgress = true;

        // Load the loading screen
        AsyncOperation loadLoadingScreen = SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Single);
        yield return new WaitUntil(() => loadLoadingScreen.isDone);

        // Load the level, wait for the continue prompt and activate it
        LoadingScreen loadingScreen = FindObjectOfType<LoadingScreen>();
        yield return loadingScreen.LoadSequence(newScene, LoadSceneMode.Additive);

        // Unload the loading screen scene
        AsyncOperation unloadLoadingScreen = SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(loadingSceneName));
        yield return new WaitUntil(() => unloadLoadingScreen.isDone);

        loadInProgress = false;
    }
}




public class LoadingScreen : MonoBehaviour
{
    [SerializeField] CanvasGroup visualElements;
    [SerializeField] TextMeshProUGUI levelName;
    [SerializeField] Animator animator;
    [SerializeField] UnityEvent<float> progressEffects;
    [SerializeField] Button continueButton;
    [SerializeField] string continueInput;
    [SerializeField] int animatorLayerIndex = 0;

    public bool loadInProgress { get; private set; }
    public bool mayContinue { get; private set; }

    /// <summary>
    /// Enables or disables a menu window in a way that doesn't interfere with the functioning of any child menus.
    /// </summary>
    /// <param name="active"></param>
    public bool menuIsActive
    {
        get
        {
            return gameObject.activeSelf && visualElements.interactable && visualElements.blocksRaycasts && visualElements.alpha > 0;
        }
        private set
        {
            gameObject.SetActive(true);
            visualElements.interactable = value; // Objects in hidden menus are disabled so they aren't picked up by the event system
            visualElements.blocksRaycasts = value; // Objects in hidden menus are disabled so they don't block the player from clicking buttons in the current menu
            visualElements.alpha = value ? 1 : 0; // Alpha is adjusted to show visibility. If I disable the gameobject or canvas component it will hide children as well
        }
    }

    private void Awake()
    {
        continueButton.onClick.AddListener(() => mayContinue = true);
    }
    
    public IEnumerator LoadSequence(string sceneName, LoadSceneMode sceneMode)
    {
        Debug.Log("Load started");
        loadInProgress = true;
        mayContinue = false;
        continueButton.enabled = false;

        levelName.text = sceneName;

        // Disable player input

        // Play animation to bring in loading screen
        // Unload previous level
        // Load new level

        menuIsActive = true;

        yield return YieldOnAnimation("Enter");

        Debug.Log("Entry animation complete, loading new scene");
        Time.timeScale = 0f;

        // Run AsyncOperation to load the desired scene
        AsyncOperation newSceneLoad = SceneManager.LoadSceneAsync(sceneName, sceneMode);
        newSceneLoad.allowSceneActivation = false;

        // While scene loads, run effects based on progress
        while (newSceneLoad.progress < 0.9f)
        {
            Debug.Log("Transition to loading screen, " + newSceneLoad.progress);
            progressEffects.Invoke(newSceneLoad.progress / 0.9f);
            yield return null;
        }

        Debug.Log("Scene loaded, waiting for player confirmation");
        // Hide load bar, enable button
        continueButton.enabled = true;
        yield return YieldOnAnimation("Finished");

        // Wait until the necessary trigger is invoked, by pressing a button
        yield return new WaitUntil(() => mayContinue);

        Debug.Log("Exiting load screen");
        // Allow new scene to activate
        newSceneLoad.allowSceneActivation = true;
        // Restore timescale
        Time.timeScale = 1;
        // Play animation to exit the load screen
        yield return YieldOnAnimation("Exit");

        // Restore player input


        // Indicate that loading is complete
        menuIsActive = false;
        loadInProgress = false;

        Debug.Log("Loading finished");
    }

    IEnumerator YieldOnAnimation(string triggerName)
    {
        if (animator == null) yield break;
        bool valueRecognised = MiscFunctions.TrySetAnimatorTrigger(animator, triggerName);
        if (!valueRecognised) yield break;
        yield return new WaitWhile(AnimationInProgress);
    }
    bool AnimationInProgress() => animator.GetCurrentAnimatorStateInfo(animatorLayerIndex).normalizedTime < 1;
}
