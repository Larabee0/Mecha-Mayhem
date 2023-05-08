using RedButton.Core.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace RedButton.Core
{
    public enum StartScreenState
    {
        Binding,
        MainMenu,
        OptionsMenu,
        ControsMenu,
        SenstivityScreen,
        Credits,
        SetPlayerCount,
        ControllerAssignment,
        ConfirmAssignment,
        MechSelect,
        LevelSelect,
        Closed
    }

    public partial class ControlArbiter : MonoBehaviour
    {
        #region Start Screen

        /// <summary>
        /// Sets up ControlArbiter for starting from main menu
        /// in the future it will support starting from a map scene to speed up testing
        /// </summary>
        private void SetUpForStartScreen()
        {
            startScreenUIActionAsset = GetComponent<InputSystemUIInputModule>().actionsAsset;
            newDevices = new(InputSystem.devices);
            startScreenActionMap = new()
            {
                devices = newDevices.ToArray()
            };
            //startScreenUIActionAsset.devices = newDevices.ToArray();
            startScreenActionMap.UI.Submit.performed += StartScreenAnyButtonPressed;
            startScreenActionMap.UI.StartScreenAux.performed += StartScreenAnyButtonPressed;
            startScreenActionMap.UI.Enable();
        }

        /// <summary>
        /// Input System Callback for grabbing the main player (player one)
        /// All controllers are unlocked and no player input scripts should exist.
        /// </summary>
        /// <param name="obj"></param>
        private void StartScreenAnyButtonPressed(InputAction.CallbackContext obj)
        {
            InputDevice[] devices = ProcessDevice(obj.control.device);
            if (newDevices.IsSupersetOf(devices))
            {
                newDevices.ExceptWith(devices);
                startScreenActionMap.devices = devices;
                startScreenUIActionAsset.devices = devices;
            }
            if (PlayerOne != null)
            {
                Destroy(PlayerOne);
            }
            startScreenActionMap.UI.Disable();
            PlayerOne = InstantiatePlayer(PlayerOneColour, devices, Controller.One);
            PlayerOne.RumbleMotor(0.075f, 1f, RumbleMotor.Both);
            PlayerOne.ControlMap.UI.Cancel.performed += GoBackToStartScreen;
            uiTranslator.SetUIHoverTint(PlayerOneColour);
            startScreenActionMap.UI.Submit.performed -= StartScreenAnyButtonPressed;
            startScreenActionMap.UI.StartScreenAux.performed -= StartScreenAnyButtonPressed;

            PlayerOne.EnableUIonly();
            if (UnityUI)
            {
                startScreenState = StartScreenState.MainMenu;
                uiTranslator.StartMenuUI.ShowMainMenu();
            }
            else
            {
                startScreenState = StartScreenState.SetPlayerCount;
                mainUIController.StartScreenController.ShowPlayerCountPicker();
                EventSystem.current.SetSelectedGameObject(FindObjectOfType<PanelEventHandler>().gameObject);
            }
            //StartCoroutine(DelayedPlayerOneUIEnable(devices));
        }

        private IEnumerator DelayedPlayerOneUIEnable(InputDevice[] devices)
        {
            yield return new WaitForSeconds(10f);
        }

        /// <summary>
        /// Determines whether this is a keyboard or mouse press and returns the 
        /// </summary>
        /// <param name="device">Device to process</param>
        /// <returns>an array containing all required devices for the map</returns>
        private InputDevice[] ProcessDevice(InputDevice device)
        {
            return device switch
            {
                Keyboard or Mouse => new InputDevice[] { Keyboard.current, Mouse.current },
                _ => new InputDevice[] { device }
            };
        }

        /// <summary>
        /// used to maintain player-contorller persistance if the game determines all required controls are still attached.
        /// </summary>
        public void SkipControllerAssignment()
        {
            startScreenState = StartScreenState.ConfirmAssignment;
            startScreenActionMap.UI.Enable();
            if (PlayerOne != null)
            {
                PlayerOne.ControlMap.UI.Cancel.performed -= GoBackToStartScreen;
                PlayerOne.ControlMap.UI.Cancel.performed += GoBackToPlayerCountPickScreen;
            }

            startScreenUIActionAsset.devices = PlayerOne.Devices;
            startScreenActionMap.devices = PlayerOne.Devices;
            PlayerOne.Enable();
        }

        /// <summary>
        /// When the ok button on controller assignmen is press we need to unbind from going back to 
        /// the player count picker screen
        /// </summary>
        public void AcceptControllerAssignment()
        {
            if (PlayerOne != null)
            {
                PlayerOne.ControlMap.UI.Cancel.performed -= GoBackToPlayerCountPickScreen;

                // go back to controll assigment screen with controllers assigned but not accepted.
                PlayerOne.ControlMap.UI.Cancel.performed += GoBackToControllerAssignment;
            }
            startScreenState = StartScreenState.MechSelect;
        }

        /// <summary>
        /// starts controller to player assignment, triggered by the start screen UI.
        /// </summary>
        /// <param name="playersToAssign">all players to assign, in order</param>
        public void StartControllerAssignment(Queue<UnityUITranslationLayer.ControllerAssignHelper> playersToAssign)
        {
            startScreenState = StartScreenState.ControllerAssignment;
            
            startScreenActionMap.devices = newDevices.ToArray();
            startScreenUIActionAsset.devices = newDevices.ToArray();
            startScreenActionMap.UI.Submit.performed += AssignControllerCallback;
            startScreenActionMap.UI.StartScreenAux.performed += AssignControllerCallback;
            //startScreenActionMap.UI.Cancel.performed += GoBackToPlayerCountPickScreen;
            startScreenActionMap.UI.Enable();
            if (PlayerOne != null)
            {
                if (UnityUI)
                {
                    PlayerOne.ControlMap.UI.Cancel.performed -= GoBackToMainMenu;
                }
                PlayerOne.ControlMap.UI.Cancel.performed -= GoBackToStartScreen;
                PlayerOne.ControlMap.UI.Cancel.performed += GoBackToPlayerCountPickScreen;
            }
            StartCoroutine(ControllerAssignmentCoroutine(playersToAssign));
        }

        /// <summary>
        /// coroutine to assign controllers to players, started by StartcontrollerAssignment
        /// </summary>
        /// <param name="playersToAssign">all players to assign, in order</param>
        /// <returns></returns>
        private IEnumerator ControllerAssignmentCoroutine(Queue<UnityUITranslationLayer.ControllerAssignHelper> playersToAssign)
        {
            while (playersToAssign.Count > 0 || playerToAssign != null)
            {
                if (startScreenActionMap.devices.Value.Count != newDevices.Count)
                {
                    startScreenActionMap.devices = newDevices.ToArray();
                    startScreenUIActionAsset.devices = newDevices.ToArray();
                }
                if (playerToAssign == null)
                {
                    playerToAssign = playersToAssign.Dequeue();
                    playerToAssign.Highlight();
                }
                yield return null;
            }
            //startScreenActionMap.UI.Cancel.performed -= GoBackToPlayerCountPickScreen;
            startScreenActionMap.UI.Submit.performed -= AssignControllerCallback;
            startScreenActionMap.UI.StartScreenAux.performed -= AssignControllerCallback;

            startScreenUIActionAsset.devices = PlayerOne.Devices;
            startScreenActionMap.devices = PlayerOne.Devices;
            PlayerOne.EnableUIonly();

            uiTranslator.StartMenuUI.EnableOkBtn();
            startScreenState = StartScreenState.ConfirmAssignment;

        }

        /// <summary>
        /// Input System callback called when any unassigned controller expirences a button press
        /// This assigns that controller to the current "playerToAssign".
        /// </summary>
        /// <param name="obj"></param>
        private void AssignControllerCallback(InputAction.CallbackContext obj)
        {
            InputDevice[] devices = ProcessDevice(obj.control.device);
            if (newDevices.IsSupersetOf(devices))
            {
                newDevices.ExceptWith(devices);
            }
            SpawnAndAssignToPlayer(devices);
            startScreenUIActionAsset.devices = newDevices.ToArray();
            startScreenActionMap.devices = newDevices.ToArray();
            playerToAssign = null;
        }

        private void SpawnAndAssignToPlayer(InputDevice[] devices)
        {
            switch (playerToAssign.playerNum)
            {
                case Controller.One:
                    if (PlayerOne == null)
                    {
                        PlayerOne = Instantiate(playerInputControllerPrefab, transform);
                    }
                    PlayerOne.playerColour = PlayerOneColour;
                    PlayerOne.AssignDevice(devices, Controller.One);
                    playerToAssign.Set(PlayerOne);
                    PlayerOne.RumbleMotor(0.075f, 1f, RumbleMotor.Both);
                    PlayerOne.ControlMap.UI.Cancel.performed += GoBackToPlayerCountPickScreen;
                    PlayerOne.EnableUIonly();
                    break;
                case Controller.Two:
                    if (PlayerTwo == null)
                    {
                        PlayerTwo = Instantiate(playerInputControllerPrefab, transform);
                    }
                    PlayerTwo.playerColour = PlayerTwoColour;
                    PlayerTwo.AssignDevice(devices, Controller.Two);
                    playerToAssign.Set(PlayerTwo);
                    PlayerTwo.RumbleMotor(0.075f, 1f, RumbleMotor.Both);
                    break;
                case Controller.Three:
                    if (PlayerThree == null)
                    {
                        PlayerThree = Instantiate(playerInputControllerPrefab, transform);
                    }
                    PlayerThree.playerColour = PlayerThreeColour;
                    PlayerThree.AssignDevice(devices, Controller.Three);
                    playerToAssign.Set(PlayerThree);
                    PlayerThree.RumbleMotor(0.075f, 1f, RumbleMotor.Both);
                    break;
                case Controller.Four:
                    if (PlayerFour == null)
                    {
                        PlayerFour = Instantiate(playerInputControllerPrefab, transform);
                    }
                    PlayerFour.playerColour = PlayerFourColour;
                    PlayerFour.AssignDevice(devices, Controller.Four);
                    playerToAssign.Set(PlayerFour);
                    PlayerFour.RumbleMotor(0.075f, 1f, RumbleMotor.Both);
                    break;
            }
        }

        #region Backwards
        /// <summary>
        /// Input System callback to return to the start screen and the inital state of the game
        /// on the start screen from the player count picker screen
        /// </summary>
        /// <param name="obj"></param>
        private void GoBackToStartScreen(InputAction.CallbackContext obj)
        {
            // unassign all players
            UnassignPlayers(true);
            newDevices = new HashSet<InputDevice>(InputSystem.devices);
            startScreenActionMap = new()
            {
                devices = newDevices.ToArray()
            };
            startScreenUIActionAsset.devices = newDevices.ToArray();
            startScreenActionMap.UI.Submit.performed += StartScreenAnyButtonPressed;
            startScreenActionMap.UI.StartScreenAux.performed += StartScreenAnyButtonPressed;
            startScreenActionMap.UI.Enable();

            startScreenState = StartScreenState.Binding;
            if (UnityUI)
            {
                uiTranslator.StartMenuUI.ShowBindPanel();
            }
            else
            {
                mainUIController.StartScreenController.ShowMainMenu();
            }
        }

        public void GoForwardFromMainMenu()
        {
            if (PlayerOne != null)
            {
                PlayerOne.ControlMap.UI.Cancel.performed -= GoBackToStartScreen;
                PlayerOne.ControlMap.UI.Cancel.performed += GoBackToMainMenu;
            }
        }

        public void GoForwardToSenstitivty()
        {
            if (PlayerOne != null)
            {
                PlayerOne.ControlMap.UI.Cancel.performed -= GoBackToMainMenu;
                PlayerOne.ControlMap.UI.Cancel.performed += GoBackToOptionsMain;
            }
        }

        public void GoBackToOptionsMain(InputAction.CallbackContext obj)
        {
            if (PlayerOne != null)
            {
                PlayerOne.ControlMap.UI.Cancel.performed -= GoBackToOptionsMain;
                PlayerOne.ControlMap.UI.Cancel.performed += GoBackToMainMenu;
            }

            uiTranslator.StartMenuUI.optionsManager.CloseSensitivty();
        }


        public void GoBackToMainMenu(InputAction.CallbackContext obj)
        {
            if (PlayerOne != null)
            {
                PlayerOne.ControlMap.UI.Cancel.performed -= GoBackToMainMenu;
                PlayerOne.ControlMap.UI.Cancel.performed += GoBackToStartScreen;
            }
            uiTranslator.StartMenuUI.ShowMainMenu();
            startScreenState = StartScreenState.MainMenu;
        }

        public void UnSubFromMainMenuBack()
        {
            if (PlayerOne != null)
            {
                PlayerOne.ControlMap.UI.Cancel.performed -= GoBackToMainMenu;
            }
        }

        /// <summary>
        /// Input System callback to return to the player count screen
        /// from the controller to player assignment screen.
        /// </summary>
        /// <param name="obj"></param>
        public void GoBackToPlayerCountPickScreen(InputAction.CallbackContext obj)
        {
            if(PlayerOne == null)
            {
                return;
            }
            StopAllCoroutines();
            startScreenActionMap.UI.Submit.performed -= AssignControllerCallback;
            startScreenActionMap.UI.StartScreenAux.performed -= AssignControllerCallback;
            startScreenActionMap.UI.Disable();
            startScreenActionMap.devices = PlayerOne.Devices ;
            startScreenUIActionAsset.devices = PlayerOne.Devices ;
            startScreenActionMap.UI.Cancel.performed -= GoBackToPlayerCountPickScreen;

            playerToAssign = null;
            if (PlayerOne != null)
            {
                PlayerOne.ControlMap.UI.Cancel.performed -= GoBackToPlayerCountPickScreen;
            }

            startScreenState = StartScreenState.SetPlayerCount;
            if (UnityUI)
            {
                uiTranslator.ShowStartSreen();
                uiTranslator.StartMenuUI.DisableOkBtn();
                uiTranslator.StartMenuUI.CloseAssignmentInternal();
                uiTranslator.StartMenuUI.OpenPlayerSelect();
            }
            else
            {
                if (PlayerOne != null)
                {
                    PlayerOne.ControlMap.UI.Cancel.performed += GoBackToStartScreen;
                }
                mainUIController.StartScreenController.ShowPlayerCountPicker();
            }

            if (PlayerOne != null)
            {
                PlayerOne.EnableUIonly();
            }
        }

        public void GoBackToControllerAssignment(InputAction.CallbackContext obj)
        {
            startScreenState = StartScreenState.ControllerAssignment;
            if (PlayerOne != null)
            {
                PlayerOne.ControlMap.UI.Cancel.performed += GoBackToPlayerCountPickScreen;
                PlayerOne.ControlMap.UI.Cancel.performed -= GoBackToControllerAssignment;
            }
            if(UnityUI)
            {
                uiTranslator.StartMenuUI.PlayerSelectCallback(playerMode, true);
                uiTranslator.StartMenuUI.mechSelectorManager.gameObject.SetActive(false);
                GiveInputAuthority(0);
            }
            else
            {

                mainUIController.StartScreenController.PlayerSelectCallback(playerMode, true);
            }
            

        }

        public void GoForwardFromMechSelector()
        {
            if (PlayerOne != null)
            {
                PlayerOne.ControlMap.UI.Cancel.performed += GoBackToMechSelector;
                PlayerOne.ControlMap.UI.Cancel.performed -= GoBackToControllerAssignment;
            }
            startScreenState = StartScreenState.LevelSelect;
        }

        public void GoBackToMechSelector(InputAction.CallbackContext obj)
        {
            if (PlayerOne != null)
            {
                PlayerOne.ControlMap.UI.Cancel.performed += GoBackToControllerAssignment;
                PlayerOne.ControlMap.UI.Cancel.performed -= GoBackToMechSelector;
            }

            uiTranslator.StartMenuUI.CLoseLvlSelectInternal();
            uiTranslator.StartMenuUI.mechSelectorManager.OpenSelector();
            startScreenState = StartScreenState.MechSelect;
        }
        #endregion

        /// <summary>
        /// Triggers the total reassignment of player controllers, including main player
        /// </summary>
        public void ResetControllerAssignment()
        {
            UnassignPlayers(true);
            newDevices = new HashSet<InputDevice>(InputSystem.devices);
            if (UnityUI)
            {
                uiTranslator.StartMenuUI.PlayerSelectCallback(playerMode, false);
            }
            else
            {
                mainUIController.StartScreenController.PlayerSelectCallback(playerMode, false);
            }
            
        }

        /// <summary>
        /// Helper method to unassign all player controllers,
        /// this destroyers all player input game objects
        /// </summary>
        /// <param name="playerOne">Whether player one (main player) should be destroyed</param>
        private void UnassignPlayers(bool playerOne = false)
        {
            if (PlayerOne != null && playerOne)
            {
                Destroy(PlayerOne.gameObject);
                PlayerOne = null;
            }
            if (PlayerTwo != null)
            {
                Destroy(PlayerTwo.gameObject);
                PlayerTwo = null;
            }
            if (PlayerThree != null)
            {
                Destroy(PlayerThree.gameObject);
                PlayerThree = null;
            }
            if (PlayerFour != null)
            {
                Destroy(PlayerFour.gameObject);
                PlayerFour = null;
            }
        }
        #endregion
        
        public void ControlArbiterToGameArbiterHandoff()
        {
            if (PlayerOne != null)
            {
                PlayerOne.ControlMap.UI.Cancel.performed -= GoBackToMechSelector;
            }
            uiTranslator.HideAll();
            mainUIController.UIShown = true;
        }

        public void GameArbiterToControlArbiterHandback()
        {
            if (PlayerOne != null)
            {
                PlayerOne.ControlMap.UI.Cancel.performed += GoBackToMechSelector;
            }
            mainUIController.UIShown = false;
            uiTranslator.ShowStartSreen();
            uiTranslator.StartMenuUI.OpenLvlSelect();
        }

    }
}