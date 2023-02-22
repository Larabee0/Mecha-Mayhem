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
        [SerializeField] GameObject lvlSelectPanel;

        [Header("Buttons")]
        [SerializeField] GameObject btnPanelBtn;
        [SerializeField] GameObject optionsPanelBtn;
        [SerializeField] GameObject creditsPanelBtn;
        [SerializeField] GameObject playerNumPanelBtn;
        [SerializeField] GameObject lvlSelectPanelBtn;

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


        // modified to work around the lack of controller assignment screen
        public void OpenLvlSelect(int playerNum)
        {

            playerNumPanel.SetActive(false);
            // lvlSelectPanel.SetActive(true);

            Controller playerCount = (Controller)(playerNum- 1);  // will stay
            Debug.Log(playerCount); // will bin
            ControlArbiter.Instance.UnSubFromMainMenuBack();
            ControlArbiter.Instance.startScreenState = StartScreenState.ControllerAssignment;
            PlayerSelectCallback?.Invoke(playerCount); // will stay
            // EventSystem.current.SetSelectedGameObject(lvlSelectPanelBtn);

        }
        // added work around because lack of controller assignment screen
        public void OpenLvlSelectInternal()
        {
            playerNumPanel.SetActive(false);
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
            playerNumPanel.SetActive(true);
            bgPanel.GetComponent<Image>().sprite = bgSprite;
            EventSystem.current.SetSelectedGameObject(playerNumPanelBtn);
        }

        public void CLoseLvlSelectInternal()
        {
            lvlSelectPanel.SetActive(false);
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