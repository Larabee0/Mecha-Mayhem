using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace RedButton.Core
{
    [Serializable]
    public struct SceneInfo
    {
        public bool hideInLevelPicker;
        public string name;
        public int buildIndex;
        public string description;
        public Texture2D preview;
    }

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

        private void ActiveSceneChangeCallback(Scene arg0, Scene arg1)
        {
            Debug.Log("Active Scene changed");
            OnActiveSceneChanged?.Invoke();
        }

        public void LoadScene(int index)
        {
            StartCoroutine(LoadMainScene(scenes[index]));
        }

        private IEnumerator LoadMainScene(SceneInfo loadScene)
        {
            if (loadingScreenImages != null && loadingScreenImages.Length > 0)
            {
                LoadScreenImage = loadingScreenImages[UnityEngine.Random.Range(0, loadingScreenImages.Length)];
            }

            loadScreenBackground.style.display = DisplayStyle.Flex;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(loadScene.buildIndex, loadMode);
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