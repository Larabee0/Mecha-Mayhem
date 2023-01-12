using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UIElements;

namespace RedButton.Core
{
    public class ControlArbiter : MonoBehaviour
    {
        public static bool OverrideDuplicates;
        public static ControlArbiter Instance;
        public static PlayerInput PlayerOne = null;
        public static PlayerInput PlayerTwo = null;
        public static PlayerInput PlayerThree = null;
        public static PlayerInput PlayerFour = null;
        public static PlayerInput KeyboardPlayer = null;
        public PlayerInput this[int i] => i switch
        {
            0 => PlayerOne,
            1 => PlayerTwo,
            2 => PlayerThree,
            3 => PlayerFour,
            _ => null
        };

        public static Color PlayerOneColour => Instance.playerOneColour;
        public static Color PlayerTwoColour => Instance.playerTwoColour;
        public static Color PlayerThreeColour => Instance.playerTwoColour;
        public static Color PlayerFourColour => Instance.playerFourColour;

        private readonly Dictionary<string, PlayerInput> controllerMap = new();
        public List<ControllerReconnectHandler> controllerReconnectors = new();
        public HashSet<InputDevice> newDevices = new();
        private readonly Queue<PlayerInput> devicelessMaps = new();

        public Queue<ControllerReconnectHandler> toDispose = new();

        [Header("Settings")]
        [SerializeField] private PlayerInput playerInputControllerPrefab;
        [SerializeField] private Color playerOneColour = new(0.9960785f, 0, 0.9960785f, 1f);
        [SerializeField] private Color playerTwoColour = new(0f, 0.8666667f, 1f, 1f);
        [SerializeField] private Color playerThreeColour = new(0.09803922f, 1f, 0f, 1f);
        [SerializeField] private Color playerFourColour = new(1f, 0.4862745f, 0f, 1f);

        [SerializeField] private MainUIController MainUIController;

        // start Screen
        [SerializeField] private bool StartScreen = false;
        private DualControllerInput startScreenActionMap;
        private InputActionAsset startScreenUIActionAsset;
        private StartScreenUI.ControllerAssignHelper playerToAssign;

        private void Awake()
        {
            InputSystem.onDeviceChange += OnDeviceChanged;

            if (Instance != null && !OverrideDuplicates)
            {
                Debug.LogError("Multiple Control Arbiters in scene! Please remove any duplicates!\nThis may get falsing triggered by switching to a scene with a Control Arbiter in it, set OverrideDuplicate to true before switching to the new scene.");
                return;
            }
            Instance = this;

            if (StartScreen)
            {
                SetUpForStartScreen();
                return;
            }

            MainUIController = FindObjectOfType<MainUIController>();

            OverrideDuplicates = false;
            PlayerOne = null;
            PlayerTwo = null;
            PlayerThree = null;
            PlayerFour = null;
            KeyboardPlayer = null;
            ValidateControllersAndPlayers();
        }

        private void LateUpdate()
        {
            if (toDispose.Count > 0)
            {
                toDispose.Dequeue().reconnectListerner.Dispose();
                if (toDispose.Count == 0)
                {
                    MainUIController.HandleControllerReconnect();
                }
            }
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        /// <summary>
        /// Goes through every InputController in the scene to ensure no duplicate players and controllers exist.
        /// </summary>
        private void ValidateControllersAndPlayers()
        {
            PlayerInput[] controllers = FindObjectsOfType<PlayerInput>();
            if (controllers.Length == 0)
            {
                Debug.LogError("No Player Control scripts exist!");
                return;
            }

            HashSet<Controller> allActiveControllers = new();
            for (int i = 0; i < controllers.Length; i++)
            {
                allActiveControllers.Add(controllers[i].Player);
            }
            if (allActiveControllers.Count != controllers.Length)
            {
                Debug.LogWarning("At least one InputController is trying to use the same device as another.\n Disabling extras");
            }
            allActiveControllers.Clear();
            for (int i = 0; i < controllers.Length; i++)
            {
                if (allActiveControllers.Contains(controllers[i].Player))
                {
                    controllers[i].Disable();
                }
                else
                {
                    switch (controllers[i].Player)
                    {
                        case Controller.One:
                            PlayerOne = controllers[i];
                            PlayerOne.playerColour = playerOneColour;
                            break;
                        case Controller.Two:
                            if (PlayerTwo == null)
                            {
                                PlayerTwo = controllers[i];
                            }
                            PlayerTwo.playerColour = playerTwoColour;
                            break;
                        case Controller.Three:
                            PlayerThree = controllers[i];
                            PlayerThree.playerColour = playerThreeColour;
                            break;
                        case Controller.Four:
                            PlayerFour = controllers[i];
                            PlayerFour.playerColour = playerFourColour;
                            break;
                        case Controller.Keyboard:
                            PlayerTwo = KeyboardPlayer = controllers[i];
                            PlayerTwo.playerColour = playerTwoColour;
                            break;
                    }
                    if (controllerMap.ContainsKey(controllers[i].DevicePath))
                    {
                        continue;
                    }
                    controllerMap.Add(controllers[i].DevicePath, controllers[i]);
                    controllers[i].Enable();
                    allActiveControllers.Add(controllers[i].Player);
                }
            }

            List<PlayerInput> activePlayers = new(4);
            for (int i = 0; i < 4; i++)
            {
                if (this[i] != null)
                {
                    activePlayers.Add(this[i]);
                }
            }

            MainUIController.SetPlayers(activePlayers);
        }

        #region Controller connect/disconnect handlers
        /// <summary>
        /// When a device is plugged in or unplugged, this gets called.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="change"></param>
        private void OnDeviceChanged(InputDevice device, InputDeviceChange change)
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    HandleDeviceConnected(device);
                    break;
                case InputDeviceChange.Disconnected:
                    HandleDeviceLost(device);
                    break;
                case InputDeviceChange.Reconnected:
                    HandleDeviceConnected(device);
                    break;
            }
        }

        /// <summary>
        /// Backend to handle a new controller being plugged in.
        /// </summary>
        /// <param name="device"></param>
        private void HandleDeviceConnected(InputDevice device)
        {
            newDevices.Add(device);
            if (devicelessMaps.Count > 0)
            {
                controllerReconnectors.Add(new(devicelessMaps.Dequeue(), this));
                MainUIController.FlashControllerConnected(device.displayName);
            }
        }

        /// <summary>
        /// Backend to handle a controller being disconnected.
        /// </summary>
        /// <param name="device"></param>
        private void HandleDeviceLost(InputDevice device)
        {
            if (newDevices.Contains(device))
            {
                newDevices.Remove(device);
            }
            string path = device.path;
            if (controllerMap.ContainsKey(path) && controllerMap[path].DeviceConnected)
            {
                PlayerInput player = controllerMap[path];
                player.Disable();
                player.DeviceConnected = false;
                devicelessMaps.Enqueue(player);
                int playerNum = Mathf.Clamp(((int)player.Player) + 1, 1, 4);
                MainUIController.HandleControllerDisconnect(playerNum, player.playerColour);
            }
        }
        #endregion

        #region Start Screen
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

        private void StartScreenAnyButtonPressed(InputAction.CallbackContext obj)
        {
            if (newDevices.Contains(obj.control.device))
            {
                newDevices.Remove(obj.control.device);
                startScreenActionMap.devices = new[] { obj.control.device };
                startScreenUIActionAsset.devices = new[] { obj.control.device };
            }

            PlayerOne = Instantiate(playerInputControllerPrefab, transform);
            PlayerOne.playerColour = PlayerOneColour;
            PlayerOne.AssignDevice(obj.control.device, Controller.One);

            startScreenActionMap.UI.Submit.performed -= StartScreenAnyButtonPressed;
            startScreenActionMap.UI.Disable();

            MainUIController.StartScreenController.ProgressToPlayerCountPikcer();
            EventSystem.current.SetSelectedGameObject(FindObjectOfType<PanelEventHandler>().gameObject);
        }

        public void StartControllerAssignment(Queue<StartScreenUI.ControllerAssignHelper> playersToAssign)
        {
            startScreenActionMap.devices = newDevices.ToArray();
            startScreenUIActionAsset.devices = newDevices.ToArray();
            startScreenActionMap.UI.Submit.performed += AssignControllerCallback;
            startScreenActionMap.UI.Enable();
            StartCoroutine(ControllerAssignmentCoroutine(playersToAssign));
        }

        private IEnumerator ControllerAssignmentCoroutine(Queue<StartScreenUI.ControllerAssignHelper> playersToAssign)
        {
            while (playersToAssign.Count > 0 || playerToAssign != null)
            {
                if (playerToAssign == null)
                {
                    playerToAssign = playersToAssign.Dequeue();
                    playerToAssign.Highlight();
                }
                yield return null;
            }

            startScreenActionMap.Dispose();
            PlayerOne.Enable();
            // next screen
        }

        private void AssignControllerCallback(InputAction.CallbackContext obj)
        {
            if (newDevices.Contains(obj.control.device))
            {
                newDevices.Remove(obj.control.device);
            }
            switch (playerToAssign.playerNum)
            {
                case 2:
                    if(PlayerTwo == null)
                    {
                        PlayerTwo = Instantiate(playerInputControllerPrefab, transform);
                    }
                    PlayerTwo.playerColour = PlayerTwoColour;
                    PlayerTwo.AssignDevice(obj.control.device, Controller.Two);
                    // PlayerTwo.Disable();
                    playerToAssign.Set(PlayerTwo);
                    break;
                case 3:
                    if (PlayerTwo == null)
                    {
                        PlayerThree = Instantiate(playerInputControllerPrefab, transform);
                    }
                    PlayerThree.playerColour = PlayerThreeColour;
                    PlayerThree.AssignDevice(obj.control.device, Controller.Three);
                    // PlayerThree.Disable();
                    playerToAssign.Set(PlayerThree);
                    break;
                case 4:
                    if (PlayerTwo == null)
                    {
                        PlayerFour = Instantiate(playerInputControllerPrefab, transform);
                    }
                    PlayerFour.playerColour = PlayerFourColour;
                    PlayerFour.AssignDevice(obj.control.device, Controller.Four);
                    // PlayerFour.Disable();
                    playerToAssign.Set(PlayerFour);
                    break;
            }
            startScreenUIActionAsset.devices = newDevices.ToArray();
            startScreenActionMap.devices = newDevices.ToArray();
            playerToAssign = null;
        }

        #endregion
    }
}