using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace RedButton.Core.UI
{
    public class StartMenuManagerScript : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] GameObject bgPanel;
        [SerializeField] GameObject bindPanel;
        [SerializeField] GameObject btnPanel;
        [SerializeField] GameObject optionsPanel;
        [SerializeField] GameObject creditsPanel;
        [SerializeField] GameObject willCreditsPanel;
        [SerializeField] GameObject playerNumPanel;
        [SerializeField] GameObject PlayerAssignPanel;
        [SerializeField] GameObject lvlSelectPanel;

        [Header("Buttons")]
        [SerializeField] GameObject btnPanelBtn;
        [SerializeField] GameObject optionsPanelBtn;
        [SerializeField] GameObject creditsPanelBtn;
        [SerializeField] GameObject playerNumPanelBtn;
        [SerializeField] GameObject returnBtn;
        [SerializeField] GameObject okBtn;
        [SerializeField] GameObject changeBtn;
        [SerializeField] GameObject lvlSelectPanelBtn;

        [Header("Assignment Stuff")]
        [SerializeField] Image p1Bg;
        [SerializeField] Text p1ControllerTxt;
        [SerializeField] Image p2Bg;
        [SerializeField] Text p2ControllerTxt;
        [SerializeField] Image p3Bg;
        [SerializeField] Text p3ControllerTxt;
        [SerializeField] Image p4Bg;
        [SerializeField] Text p4ControllerTxt;

        [SerializeField] Sprite bgSprite;
        [SerializeField] Sprite willSprite;

        [Header("Mech Selector")]
        public MechSelectorManager mechSelectorManager;
        
        private UnityUITranslationLayer.ControllerAssignHelper PlayerOneAssign;
        private UnityUITranslationLayer.ControllerAssignHelper PlayerTwoAssign;
        private UnityUITranslationLayer.ControllerAssignHelper PlayerThreeAssign;
        private UnityUITranslationLayer.ControllerAssignHelper PlayerFourAssign;

        private void Awake()
        {
            PlayerOneAssign = new(Controller.One, p1ControllerTxt, p1Bg);
            PlayerTwoAssign = new(Controller.Two, p2ControllerTxt, p2Bg);
            PlayerThreeAssign = new(Controller.Three, p3ControllerTxt, p3Bg);
            PlayerFourAssign = new(Controller.Four, p4ControllerTxt, p4Bg);
        }

        #region MainMenu & binding
        public void ShowBindPanel()
        {
            bindPanel.SetActive(true);
            btnPanel.SetActive(false);
        }

        public void ShowMainMenu()
        {
            bindPanel.SetActive(false);
            btnPanel.SetActive(true);
            playerNumPanel.SetActive(false);
            optionsPanel.SetActive(false);
            creditsPanel.SetActive(false);
            willCreditsPanel.SetActive(false);
            PlayerAssignPanel.SetActive(false);
            lvlSelectPanel.SetActive(false);
            PlayerAssignPanel.SetActive(false);
            DisableOkBtn();

            EventSystem.current.SetSelectedGameObject(btnPanelBtn);
        }

        #region MainMenu Other Buttons
        public void OpenOptions()
        {
            ControlArbiter.Instance.startScreenState = StartScreenState.OptionsMenu;
            btnPanel.SetActive(false);
            optionsPanel.SetActive(true);
            EventSystem.current.SetSelectedGameObject(optionsPanelBtn);
            ControlArbiter.Instance.GoForwardFromMainMenu();
        }
        public void CloseOptions()
        {
            btnPanel.SetActive(true);
            optionsPanel.SetActive(false);
            EventSystem.current.SetSelectedGameObject(btnPanelBtn);
        }

        public void OpenCredits()
        {
            ControlArbiter.Instance.startScreenState = StartScreenState.Credits;
            btnPanel.SetActive(false);
            creditsPanel.SetActive(true);
            EventSystem.current.SetSelectedGameObject(creditsPanelBtn);
            ControlArbiter.Instance.GoForwardFromMainMenu();
        }
        public void CloseCredits()
        {
            btnPanel.SetActive(true);
            creditsPanel.SetActive(false);
            EventSystem.current.SetSelectedGameObject(btnPanelBtn);
        }

        //Will Credits
        public void OpenWillCredits()
        {
            bgPanel.GetComponent<Image>().sprite = willSprite;
            btnPanel.SetActive(false);
            willCreditsPanel.SetActive(true);
        }
        public void CloseWillCredits()
        {
            bgPanel.GetComponent<Image>().sprite = bgSprite;
            btnPanel.SetActive(true);
            willCreditsPanel.SetActive(false);
        }



        public void Quit()
        {
            Debug.Log("Quitting");
            Application.Quit();
        }
        #endregion
        #endregion

        #region Player Selector
        public void OpenPlayerSelect()
        {
            ControlArbiter.Instance.startScreenState = StartScreenState.SetPlayerCount;
            btnPanel.SetActive(false);
            playerNumPanel.SetActive(true);
            creditsPanel.SetActive(false);
            EventSystem.current.SetSelectedGameObject(playerNumPanelBtn);
            ControlArbiter.Instance.GoForwardFromMainMenu();

            PlayerOneAssign.SetShown(false);
            PlayerTwoAssign.SetShown(false);
            PlayerThreeAssign.SetShown(false);
            PlayerFourAssign.SetShown(false);
        }

        public void ClosePlayerSelect()
        {
            ControlArbiter.Instance.GoBackToMainMenu(new());
        }

        #region Controller Assignment
        public void OpenAssignment(int playerNum)
        {
            Controller playerCount = (Controller)(playerNum- 1);
            ControlArbiter.Instance.UnSubFromMainMenuBack();
            ControlArbiter.Instance.startScreenState = StartScreenState.ControllerAssignment;
            PlayerSelectCallback(playerCount);
        }

        public void PlayerSelectCallback(Controller playerCount,bool existingCheck = true)
        {
            playerNumPanel.SetActive(false);
            DisableOkBtn();
            ControlArbiter.playerMode = playerCount;
            if(existingCheck && TryLoadAssignmentScreenWithCurrent())
            {
                return;
            }

            PlayerOneAssign.SetShown(true);
            PlayerTwoAssign.SetShown(false);
            PlayerThreeAssign.SetShown(false);
            PlayerFourAssign.SetShown(false);
            UIAssignPlayerOne();

            Queue<UnityUITranslationLayer.ControllerAssignHelper> players = new();
            if(ControlArbiter.PlayerOne == null)
            {
                players.Enqueue(PlayerOneAssign);
            }

            switch (ControlArbiter.playerMode)
            {
                case Controller.Two:
                    players.Enqueue(PlayerTwoAssign);
                    PlayerTwoAssign.SetShown(true);
                    break;
                case Controller.Three:
                    players.Enqueue(PlayerTwoAssign);
                    players.Enqueue(PlayerThreeAssign);
                    PlayerTwoAssign.SetShown(true);
                    PlayerThreeAssign.SetShown(true);
                    break;
                case Controller.Four:
                    players.Enqueue(PlayerTwoAssign);
                    players.Enqueue(PlayerThreeAssign);
                    players.Enqueue(PlayerFourAssign);
                    PlayerTwoAssign.SetShown(true);
                    PlayerThreeAssign.SetShown(true);
                    PlayerFourAssign.SetShown(true);
                    break;
            }


            PlayerAssignPanel.SetActive(true);
            playerNumPanel.SetActive(false);

            EventSystem.current.SetSelectedGameObject(returnBtn);


            ControlArbiter.Instance.StartControllerAssignment(players);
        }

        public bool TryLoadAssignmentScreenWithCurrent()
        {
            PlayerOneAssign.SetShown(false);
            PlayerTwoAssign.SetShown(false);
            PlayerThreeAssign.SetShown(false);
            PlayerFourAssign.SetShown(false);

            bool allExpectPresent = ControlArbiter.playerMode switch
            {
                Controller.Two => UIAssignPlayerOne() && UIAssignPlayerTwo(),
                Controller.Three => UIAssignPlayerOne() && UIAssignPlayerTwo() && UIAssignPlayerThree(),
                Controller.Four => UIAssignPlayerOne() && UIAssignPlayerTwo() && UIAssignPlayerThree() && UIAssignPlayerFour(),
                _ => UIAssignPlayerOne(),
            };

            PlayerAssignPanel.SetActive(true);
            playerNumPanel.SetActive(false);

            if (allExpectPresent)
            {
                ControlArbiter.Instance.SkipControllerAssignment();
                EnableOkBtn();
                return true;
            }

            return false;
        }

        private bool UIAssignPlayerOne()
        {
            if (ControlArbiter.PlayerOne != null)
            {
                UIAssignPlayer(ControlArbiter.PlayerOne, PlayerOneAssign);
                return true;
            }
            return false;
        }

        private bool UIAssignPlayerTwo()
        {
            if (ControlArbiter.PlayerTwo != null)
            {
                UIAssignPlayer(ControlArbiter.PlayerTwo, PlayerTwoAssign);
                return true;
            }
            return false;
        }

        private bool UIAssignPlayerThree()
        {
            if (ControlArbiter.PlayerThree != null)
            {
                UIAssignPlayer(ControlArbiter.PlayerThree, PlayerThreeAssign);
                return true;
            }
            return false;
        }

        private bool UIAssignPlayerFour()
        {
            if (ControlArbiter.PlayerFour != null)
            {
                UIAssignPlayer(ControlArbiter.PlayerFour, PlayerFourAssign);
                return true;
            }
            return false;
        }


        public void UIAssignPlayer(PlayerInput playerInput, UnityUITranslationLayer.ControllerAssignHelper controllerAssignHelper)
        {
            controllerAssignHelper.SetShown(true);
            controllerAssignHelper.Set(playerInput);
        }

        public void ChangeAssignment()
        {
            p1Bg.color = Color.white; //Should be player 1 colour
            p2Bg.color = Color.white;
            p3Bg.color = Color.gray;
            p4Bg.color = Color.gray;
            ControlArbiter.Instance.ResetControllerAssignment();
        }

        public void EnableOkBtn()
        {
            okBtn.SetActive(true);
            changeBtn.SetActive(true);
            returnBtn.SetActive(true);
            EventSystem.current.SetSelectedGameObject(okBtn);
        }

        public void DisableOkBtn()
        {
            okBtn.SetActive(false);
            changeBtn.SetActive(false);
            returnBtn.SetActive(false);
            EventSystem.current.SetSelectedGameObject(returnBtn);
        }

        public void CloseAssignmentInternal()
        {
            PlayerAssignPanel.SetActive(false);
            playerNumPanel.SetActive(true);
            EventSystem.current.SetSelectedGameObject(playerNumPanelBtn);
        }

        public void CloseAssignment()
        {
            ControlArbiter.Instance.GoBackToPlayerCountPickScreen(new());
        }
        #endregion
        #endregion

        #region Mech selector
        public void OpenMechSelector()
        {
            PlayerAssignPanel.SetActive(false);
            ControlArbiter.Instance.AcceptControllerAssignment();
            mechSelectorManager.OpenSelector();
        }
        #endregion

        #region Level selector
        public void OpenLvlSelect()
        {
            mechSelectorManager.gameObject.SetActive(false);
            lvlSelectPanel.SetActive(true);
            PlayerAssignPanel.SetActive(false);
            EventSystem.current.SetSelectedGameObject(lvlSelectPanelBtn);
        }

        public void LvlBtnClick(int lvl)
        {
            Debug.Log("Selected level is " + lvl);

            ControlArbiter.Instance.ControlArbiterToGameArbiterHandoff();
            GameSceneManager.Instance.LoadScene(lvl);
        }

        // this needs to go back to the controller assignment screen, which does not exist
        public void CloseLvlSelect()
        {
            ControlArbiter.Instance.GoBackToMechSelector(new());
        }

        public void CLoseLvlSelectInternal()
        {
            bgPanel.GetComponent<Image>().sprite = bgSprite;
            lvlSelectPanel.SetActive(false);
        }
        #endregion

    }
}