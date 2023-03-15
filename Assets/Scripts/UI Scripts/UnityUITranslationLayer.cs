using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RedButton.Core.UI
{
    public class UnityUITranslationLayer : MonoBehaviour
    {
        [SerializeField] private StartMenuManagerScript startMenuController;
        [SerializeField] private PauseMenuManager pauseMenuController;
        [SerializeField] private EndScreenManager endScreenController;
        [SerializeField] private Button[] allButtons;
        public StartMenuManagerScript StartMenuUI => startMenuController;
        public PauseMenuManager PauseMenuUI => pauseMenuController;
        public EndScreenManager EndScreenUI => endScreenController;

        private void Awake()
        {
            allButtons = GetComponentsInChildren<Button>(true);
            startMenuController = GetComponentInChildren<StartMenuManagerScript>(true);
            pauseMenuController = GetComponentInChildren<PauseMenuManager>(true);
            startMenuController.enabled = false;
            //startMenuController.PlayerSelectCallback += PlayerSelectCallback;
        }

        public void SetUIHoverTint(Color colour)
        {
            for (int i = 0; i < allButtons.Length; i++)
            {
                ColorBlock block = allButtons[i].colors;
                block.selectedColor = colour;
                allButtons[i].colors = block;
            }
        }

        public void ShowStartSreen()
        {
            startMenuController.gameObject.SetActive(true);
            pauseMenuController.gameObject.SetActive(false);
            gameObject.SetActive(true);
        }

        public void ShowPauseMenu()
        {
            startMenuController.gameObject.SetActive(false);
            pauseMenuController.Open();
            gameObject.SetActive(true);
        }

        public void HideAll()
        {
            startMenuController.gameObject.SetActive(false);
            pauseMenuController.gameObject.SetActive(false);
            endScreenController.gameObject.SetActive(false);
        }

        //private void PlayerSelectCallback(Controller playerCount, bool existingCheck)
        //{
        //    HideAll();
        //}
        //
        //private void OnDisable()
        //{
        //    startMenuController.PlayerSelectCallback -= PlayerSelectCallback;
        //}

        public class ControllerAssignHelper
        {
            public Controller playerNum;
            public Text lable;
            public Image image;

            public ControllerAssignHelper(Controller playerNum, Text lable, Image image)
            {
                this.playerNum = playerNum;
                this.lable = lable;
                this.image = image;
                lable.text = "";
                SetShown(false);
            }

            public void Highlight()
            {
                image.color = Color.white;
                lable.text = "Press Any Button";
            }

            public void Set(PlayerInput player)
            {
                image.color = player.playerColour;
                lable.text = string.Format("{0}", player.DeviceName);
            }

            public void SetShown(bool hidden)
            {
                lable.transform.parent.gameObject.SetActive(hidden);
                image.color = Color.gray;
                lable.text = "";
            }

            public static string FirstLine(Controller playerNum)
            {
                return playerNum switch
                {
                    Controller.One => "Player One\n",
                    Controller.Two => "Player Two\n",
                    Controller.Three => "Player Three\n",
                    Controller.Four => "Player Four\n",
                    _ => "Invalid Player\n"
                };
            }
        }
    }
}