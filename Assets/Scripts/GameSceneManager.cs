using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace RedButton.Core
{
    /// <summary>
    /// Struct to hold information about scene
    /// </summary>
    [Serializable]
    public struct SceneInfo
    {
        public bool hideInLevelPicker;
        public string name;
        public int buildIndex;
        public string description;
        public Texture2D preview;
    }

    /// <summary>
    /// Script for mananging loading scenes and displaying information about them such as the preview image.
    /// </summary>
    public class GameSceneManager : MonoBehaviour
    {
        public static GameSceneManager Instance;

        public static bool OverrideDuplicates;

        [SerializeField] private LoadSceneMode loadMode;
        [SerializeField] private SceneInfo[] scenes;
        [Space(100)]
        [SerializeField] private Texture2D[] loadingScreenImages;

        private UIDocument uiDocument;

        private ProgressBar loadingBar;
        private VisualElement loadScreenBackground;
        private VisualElement loadScreenImage;

        public SceneInfo[] Scenes => scenes;
        public Texture2D LoadScreenImage { set => loadScreenImage.style.backgroundImage = value; }

        public Pluse OnActiveSceneChanged;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            uiDocument = GetComponent<UIDocument>();
        }

        private void Start()
        {
            loadScreenBackground = uiDocument.rootVisualElement;
            loadScreenImage = loadScreenBackground.Q("LoadScreenImage");
            loadingBar = loadScreenBackground.Q<ProgressBar>("LoadingBar");
            loadScreenBackground.style.display = DisplayStyle.None;
            SceneManager.activeSceneChanged += ActiveSceneChangeCallback;
        }

        /// <summary>
        /// Event callback when the active scene changes
        /// </summary>
        /// <param name="oldMainScene">previously main scene</param>
        /// <param name="newMainScene">new main scene</param>
        private void ActiveSceneChangeCallback(Scene oldMainScene, Scene newMainScene)
        {
            Debug.Log("Active Scene changed");
            OnActiveSceneChanged?.Invoke();
        }

        /// <summary>
        /// Load the scene from the given index. This is not the build index.
        /// This is the "scenes" array index local to the script.
        /// </summary>
        /// <param name="index">Index of desired scene from the local scenes array</param>
        public void LoadScene(int index)
        {
            StartCoroutine(LoadSceneCoroutine(scenes[index]));
        }

        /// <summary>
        /// Async coroutine to load the given scene, also runs hte load screen UI
        /// It randomly picks an image to show.
        /// </summary>
        /// <param name="sceneToLoad">Desired Scene to load</param>
        /// <returns></returns>
        private IEnumerator LoadSceneCoroutine(SceneInfo sceneToLoad)
        {
            if (loadingScreenImages != null && loadingScreenImages.Length > 0)
            {
                LoadScreenImage = loadingScreenImages[UnityEngine.Random.Range(0, loadingScreenImages.Length)];
            }

            loadScreenBackground.style.display = DisplayStyle.Flex;

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad.buildIndex, loadMode);
            while (!asyncLoad.isDone)
            {
                loadingBar.value = asyncLoad.progress;
                yield return null;
            }

            loadScreenBackground.style.display = DisplayStyle.None;
            loadingBar.value = 0f;
        }
    }
}