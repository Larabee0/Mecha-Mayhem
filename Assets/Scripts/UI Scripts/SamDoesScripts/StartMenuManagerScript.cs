using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace RedButton.Core.UI
{
    public class StartMenuManagerScript : MonoBehaviour
    {
        public delegate void PlayerCountSelect(Controller playerCount, bool existingCheck = true);

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

        public PlayerCountSelect PlayerSelectCallback;

        public void ShowBindPanel()
        {
            bindPanel.SetActive(true);
            btnPanel.SetActive(false);
        }


        private void Update()
        {
            if (bindPanel.activeSelf)
            {
                if (Input.anyKeyDown)
                {
                    ShowMainMenu();
                }
            }
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
            EventSystem.current.SetSelectedGameObject(btnPanelBtn);
            //controller binding goes here
        }



        public void OpenPlayerSelect()
        {
            ControlArbiter.Instance.startScreenState = StartScreenState.SetPlayerCount;
            btnPanel.SetActive(false);
            playerNumPanel.SetActive(true);
            creditsPanel.SetActive(false);
            EventSystem.current.SetSelectedGameObject(playerNumPanelBtn);
            ControlArbiter.Instance.GoForwardFromMainMenu();
        }

        public void ClosePlayerSelect()
        {
            ControlArbiter.Instance.GoBackToControllerAssignment(new UnityEngine.InputSystem.InputAction.CallbackContext());
            return;
            btnPanel.SetActive(true);
            playerNumPanel.SetActive(false);
            EventSystem.current.SetSelectedGameObject(btnPanelBtn);
        }

        public void OpenAssignment(int playerNum)
        {
            Debug.Log(playerNum); // will bin
            Controller playerCount = (Controller)(playerNum- 1);  // will stay
            ControlArbiter.Instance.UnSubFromMainMenuBack();
            ControlArbiter.Instance.startScreenState = StartScreenState.ControllerAssignment;
            PlayerSelectCallback?.Invoke(playerCount); // will stay
            PlayerAssignPanel.SetActive(true);
            playerNumPanel.SetActive(false);
            EventSystem.current.SetSelectedGameObject(returnBtn);
        }

        public void ChangeAssignment()
        {
            p1Bg.color = Color.white; //Should be player 1 colour
            p2Bg.color = Color.white;
            p3Bg.color = Color.gray;
            p4Bg.color = Color.gray;
            DisableOkBtn();
        }

        public void EnableOkBtn()
        {
            okBtn.SetActive(true);
            changeBtn.SetActive(true);
            EventSystem.current.SetSelectedGameObject(okBtn);
        }

        public void DisableOkBtn()
        {
            okBtn.SetActive(false);
            changeBtn.SetActive(false);
            EventSystem.current.SetSelectedGameObject(returnBtn);
        }

        public void CloseAssignment()
        {
            PlayerAssignPanel.SetActive(false);
            playerNumPanel.SetActive(true);
            EventSystem.current.SetSelectedGameObject(playerNumPanelBtn);
        }



        // modified to work around the lack of controller assignment screen
        public void OpenLvlSelect()
        {
            lvlSelectPanel.SetActive(true);
            PlayerAssignPanel.SetActive(false);
            EventSystem.current.SetSelectedGameObject(lvlSelectPanelBtn);
        }
        // added work around because lack of controller assignment screen
        public void OpenLvlSelectInternal()
        {
            PlayerAssignPanel.SetActive(false);
            lvlSelectPanel.SetActive(true);
            EventSystem.current.SetSelectedGameObject(lvlSelectPanelBtn);
        }

        public void LvlBtnClick(int lvl)
        {
            Debug.Log("Selected level is " + lvl);
        }

        // this needs to go back to the controller assignment screen, which does not exist
        public void CloseLvlSelect()
        {
            ControlArbiter.Instance.GoBackToControllerAssignment(new UnityEngine.InputSystem.InputAction.CallbackContext());
            return;
            PlayerAssignPanel.SetActive(true);
            bgPanel.GetComponent<Image>().sprite = bgSprite;
            EventSystem.current.SetSelectedGameObject(playerNumPanelBtn);
        }

        public void CLoseLvlSelectInternal()
        {
            lvlSelectPanel.SetActive(false);
            PlayerAssignPanel.SetActive(true);
        }

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
            //ControlArbiter.Instance.GoForwardFromMainMenu();
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
    }
}