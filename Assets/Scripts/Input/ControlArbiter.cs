using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UIElements;
using RedButton.Core.UI;

namespace RedButton.Core
{
    public partial class ControlArbiter : MonoBehaviour
    {
        public enum HotStartControllers
        {
            Keyboard,
            Gamepad,
            KeyboardGamepad,
            Wiimote,
            KeyboardWiimote,
            WiimoteGamepad,
            All
        }

        public static Controller playerMode;
        public static bool OverrideDuplicates;
        public static ControlArbiter Instance;
        public static PlayerInput PlayerOne = null;
        public static PlayerInput PlayerTwo = null;
        public static PlayerInput PlayerThree = null;
        public static PlayerInput PlayerFour = null;
        public static PlayerInput KeyboardPlayer = null;
        public PlayerInput this[int i]
        {
            get => i switch
            {
                0 => PlayerOne,
                1 => PlayerTwo,
                2 => PlayerThree,
                3 => PlayerFour,
                _ => null
            };
            private set
            {
                switch (i)
                {
                    case 0:
                        PlayerOne = value;
                        break;
                    case 1:
                        PlayerTwo = value;
                        break;
                    case 2:
                        PlayerThree = value;
                        break;
                    case 3:
                        PlayerFour = value;
                        break;
                }
            }
        }

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

        [SerializeField] private MainUIController mainUIController;
        public MainUIController MainUIController => mainUIController;

        [Header("Start Screen Settings")]
        [SerializeField] private bool StartScreen = false;
        private DualControllerInput startScreenActionMap;
        private InputActionAsset startScreenUIActionAsset;
        private StartScreenUI.ControllerAssignHelper playerToAssign;
        [Header("Hot Start Settings")]
        [SerializeField] HotStartControllers hotStartDevices = HotStartControllers.All;

        private void Awake()
        {
            DontDestroyOnLoad(this);
            InputSystem.onDeviceChange += OnDeviceChanged;

            mainUIController = FindObjectOfType<MainUIController>();
            if (Instance != null && !OverrideDuplicates)
            {
                Debug.LogError("Multiple Control Arbiters in scene! Please remove any duplicates!\nThis may get falsing triggered by switching to a scene with a Control Arbiter in it, set OverrideDuplicate to true before switching to the new scene.");
                return;
            }
            Instance = this;

            PollWiimotes();
            OverrideDuplicates = false;

            if (StartScreen)
            {
                SetUpForStartScreen();
                return;
            }

            Debug.Log("Hot starting ControlArbiter!");

            PlayerOne = null;
            PlayerTwo = null;
            PlayerThree = null;
            PlayerFour = null;
            KeyboardPlayer = null;
            HotStart();
        }

        private void LateUpdate()
        {
            if (toDispose.Count > 0)
            {
                toDispose.Dequeue().reconnectListerner.Dispose();
                if (toDispose.Count == 0)
                {
                    mainUIController.HandleControllerReconnect();
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
        public void ValidateControllersAndPlayers()
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
                mainUIController.FlashControllerConnected(device.displayName);
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
                mainUIController.HandleControllerDisconnect(playerNum, player.playerColour);
            }
        }
        #endregion

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
            if(PlayerOne != null)
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
            if (device is Keyboard || device is Mouse)
            {
                return new InputDevice[] { Keyboard.current, Mouse.current };
            }
            else
            {
                return new InputDevice[] { device };
            }
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
                if(startScreenActionMap.devices.Value.Count != newDevices.Count)
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
                    if(PlayerTwo == null)
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
            startScreenUIActionAsset.devices = newDevices.ToArray();
            startScreenActionMap.devices = newDevices.ToArray();
            playerToAssign = null;
        }

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
            if(PlayerOne != null && playerOne)
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

        #region Hot Start
        private void HotStart()
        {
            List<InputDevice> devices = new();
            switch (hotStartDevices)
            {
                case HotStartControllers.Keyboard:
                    devices.Add(Keyboard.current);
                    break;
                case HotStartControllers.Gamepad:
                    devices.AddRange(Gamepad.all);
                    break;
                case HotStartControllers.KeyboardGamepad:
                    devices.Add(Keyboard.current);
                    devices.AddRange(Gamepad.all);
                    break;
                case HotStartControllers.Wiimote:
                    devices.AddRange(WiimoteDevice.all);
                    break;
                case HotStartControllers.KeyboardWiimote:
                    devices.Add(Keyboard.current);
                    devices.AddRange(WiimoteDevice.all);
                    break;
                case HotStartControllers.WiimoteGamepad:
                    devices.AddRange(WiimoteDevice.all);
                    devices.AddRange(Gamepad.all);
                    break;
                case HotStartControllers.All:
                    devices.Add(Keyboard.current);
                    devices.AddRange(WiimoteDevice.all);
                    devices.AddRange(Gamepad.all);
                    break;
                default:
                    break;
            }

            int runTo = Mathf.Min(devices.Count, 4);
            
            for (int i = 0; i < runTo; i++)
            {
                this[i] = InstantiatePlayer(GetPlayerColour(i), ProcessDevice(devices[i]), (Controller)i);
            }
            ValidateControllersAndPlayers();
        }
        #endregion

        /// <summary>
        /// Method to spawn a player, set their colour and input device
        /// </summary>
        /// <param name="playerColour"></param>
        /// <param name="device"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        private PlayerInput InstantiatePlayer(Color playerColour,InputDevice[] device,Controller controller)
        {
            PlayerInput player = Instantiate(playerInputControllerPrefab, transform);
            player.AssignDevice(device,controller);
            player.playerColour= playerColour;
            return player;
        }

        /// <summary>
        /// index based way of getting player colours
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static Color GetPlayerColour(int i)
        {
            return i switch
            {
                0 => PlayerOneColour,
                1 => PlayerTwoColour,
                2 => PlayerThreeColour,
                3 => PlayerFourColour,
                _ => Color.white
            };
        }
    }
}