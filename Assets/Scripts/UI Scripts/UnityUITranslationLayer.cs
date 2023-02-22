using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.Core.UI
{
    public class UnityUITranslationLayer : MonoBehaviour
    {
        [SerializeField] private StartMenuManagerScript startMenuController;
        [SerializeField] private PauseMenuManager pauseMenuController;

        public StartMenuManagerScript StartMenuUI => startMenuController;
        public PauseMenuManager PauseMenuUI => pauseMenuController;

        private void Awake()
        {
            startMenuController = GetComponentInChildren<StartMenuManagerScript>(true);
            pauseMenuController = GetComponentInChildren<PauseMenuManager>(true);
            startMenuController.enabled = false;
            startMenuController.PlayerSelectCallback += PlayerSelectCallback;
        }

        public void ShowStartSreen()
        {
            startMenuController.gameObject.SetActive(true);
            pauseMenuController.gameObject.SetActive(false);
            gameObject.SetActive(true);
        }

        public void HideAll()
        {
            startMenuController.gameObject.SetActive(false);
            pauseMenuController.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        private void PlayerSelectCallback(Controller playerCount, bool existingCheck)
        {
            HideAll();
        }

        private void OnDisable()
        {
            startMenuController.PlayerSelectCallback -= PlayerSelectCallback;
        }
    }
}