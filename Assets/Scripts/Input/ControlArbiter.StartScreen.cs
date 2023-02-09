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
            startScreenUIActionAsset.devices = newDevices.ToArray();
            startScreenActionMap.UI.Submit.performed += StartScreenAnyButtonPressed;
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
            PlayerOne = InstantiatePlayer(PlayerOneColour, devices, Controller.One);
            PlayerOne.RumbleMotor(0.075f, 1f, RumbleMotor.Both);
            PlayerOne.ControlMap.UI.Cancel.performed += GoBackToStartScreen;
            PlayerOne.EnableUIonly();
            startScreenActionMap.UI.Submit.performed -= StartScreenAnyButtonPressed;
            startScreenActionMap.UI.Disable();

            mainUIController.StartScreenController.ShowPlayerCountPicker();
            EventSystem.current.SetSelectedGameObject(FindObjectOfType<PanelEventHandler>().gameObject);
        }

        private InputDevice[] ProcessDevice(InputDevice device)
        {
            return device switch
            {
                Keyboard or Mouse => new InputDevice[] { Keyboard.current, Mouse.current },
                _ => new InputDevice[] { device }
            };
        }

        /// <summary>
        /// starts controller to player assignment, triggered by the start screen UI.
        /// </summary>
        /// <param name="playersToAssign">all players to assign, in order</param>
        public void StartControllerAssignment(Queue<StartScreenUI.ControllerAssignHelper> playersToAssign)
        {
            startScreenActionMap.devices = newDevices.ToArray();
            startScreenUIActionAsset.devices = newDevices.ToArray();
            startScreenActionMap.UI.Submit.performed += AssignControllerCallback;
            startScreenActionMap.UI.Cancel.performed += GoBackToPlayerCountPickScreen;
            startScreenActionMap.UI.Enable();
            if (PlayerOne != null)
            {
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
        private IEnumerator ControllerAssignmentCoroutine(Queue<StartScreenUI.ControllerAssignHelper> playersToAssign)
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
            startScreenActionMap.UI.Cancel.performed -= GoBackToPlayerCountPickScreen;
            startScreenActionMap.UI.Submit.performed -= AssignControllerCallback;
            startScreenUIActionAsset.devices = PlayerOne.Device;
            startScreenActionMap.devices = PlayerOne.Device;
            PlayerOne.Enable();
            mainUIController.StartScreenController.ShowAssignmentButtonPanel();
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
                    if (PlayerTwo == null)
                    {
                        PlayerThree = Instantiate(playerInputControllerPrefab, transform);
                    }
                    PlayerThree.playerColour = PlayerThreeColour;
                    PlayerThree.AssignDevice(devices, Controller.Three);
                    playerToAssign.Set(PlayerThree);
                    PlayerThree.RumbleMotor(0.075f, 1f, RumbleMotor.Both);
                    break;
                case Controller.Four:
                    if (PlayerTwo == null)
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
            startScreenActionMap.UI.Enable();
            mainUIController.StartScreenController.ShowMainMenu();
        }

        /// <summary>
        /// Input System callback to return to the player count screen
        /// from the controller to player assignment screen.
        /// </summary>
        /// <param name="obj"></param>
        private void GoBackToPlayerCountPickScreen(InputAction.CallbackContext obj)
        {
            StopAllCoroutines();
            startScreenActionMap.UI.Submit.performed -= AssignControllerCallback;
            mainUIController.StartScreenController.ShowPlayerCountPicker();
            startScreenActionMap.UI.Disable();
            startScreenActionMap.devices = new[] { obj.control.device };
            startScreenUIActionAsset.devices = new[] { obj.control.device };
            startScreenActionMap.UI.Cancel.performed -= GoBackToPlayerCountPickScreen;

            playerToAssign = null;
            if (PlayerOne != null)
            {
                PlayerOne.ControlMap.UI.Cancel.performed -= GoBackToPlayerCountPickScreen;
                PlayerOne.Disable();
            }

            mainUIController.StartScreenController.ShowPlayerCountPicker();
        }
        #endregion

        /// <summary>
        /// Triggers the total reassignment of player controllers, including main player
        /// </summary>
        public void ResetControllerAssignment()
        {
            UnassignPlayers(true);
            newDevices = new HashSet<InputDevice>(InputSystem.devices);
            mainUIController.StartScreenController.PlayerSelectCallback(playerMode, false);
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
    }
}