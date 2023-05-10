using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using RedButton.Core.WiimoteSupport;
using RedButton.Core.UI;
using System.Linq;
using UnityEngine.InputSystem.UI;

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


        public static bool Paused { get; private set; }
        public static Controller playerMode;
        public static ControlArbiter Instance { get { return instance; } private set { instance = value; } }

        private static ControlArbiter instance;
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
        public static Color PlayerThreeColour => Instance.playerThreeColour;
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
        public bool UnityUI => unityUI;
        [SerializeField] private UnityUITranslationLayer uiTranslator;
        public UnityUITranslationLayer UITranslator => uiTranslator;

        [Header("Start Screen Settings")]
        [SerializeField] private bool unityUI = false;
        [SerializeField] private bool StartScreen = false;
        public StartScreenState startScreenState = StartScreenState.Binding;
        private DualControllerInput startScreenActionMap;
        private InputActionAsset startScreenUIActionAsset;
        private UnityUITranslationLayer.ControllerAssignHelper playerToAssign;
        [Header("Hot Start Settings")]
        [SerializeField] HotStartControllers hotStartDevices = HotStartControllers.All;
        public Pluse OnPauseMenuQuit;

        private void Awake()
        {
            // we should find a mainUIController which either has us as its parent, has no parent or has no ControlArbiter in its parent(s).
            mainUIController = FindObjectsOfType<MainUIController>().Where(obj => obj.transform.parent == transform || obj.transform == null || obj.GetComponentInParent<ControlArbiter>() == null).FirstOrDefault();
            if(mainUIController == null)
            {
                Debug.LogError("Failed to find usable MainController UI Instance", gameObject);
            }
            else
            {
                mainUIController.transform.parent = transform;
            }
            
            // if an active instance already exists, lets bin ourselves including any UI or playerInput children.
            if (Instance != this &&(Instance != null|| mainUIController == null))
            {
                Destroy(gameObject);
                return;
            }
            StartScreen = mainUIController.StartScene;
            Instance = this;
            
            // if we reach here then we've decided we can go ahead with full start up of the control arbiter instance.
            DontDestroyOnLoad(this);
            InputSystem.onDeviceChange += OnDeviceChanged;

            PollWiimotes();

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
        
        private void Start()
        {
            // if (unityUI)
            // {
            //     uiTranslator.StartMenuUI.PlayerSelectCallback += MainUIController.StartScreenController.PlayerSelectCallback;
            // }

            if (UnityUI)
            {
                uiTranslator.StartMenuUI.optionsManager.LoadFromSettings();
                mainUIController.UIShown = false;
            }
            WiimoteUISetup();
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

        /// <summary>
        /// Goes through every InputController in the scene to ensure no duplicate players and controllers exist.
        /// </summary>
        public void ValidateControllersAndPlayers()
        {
            controllerMap.Clear();
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
                    controllers[i].SetPausingAllowed(false);
                    controllers[i].Disable();
                }
                else
                {
                    AssignToPlayer(controllers, i);
                    
                    if (controllerMap.ContainsKey(controllers[i].DevicePath))
                    {
                        continue;
                    }
                    controllerMap.Add(controllers[i].DevicePath, controllers[i]);
                    controllers[i].Enable();
                    controllers[i].SetWiimotePointerActive(false);
                    controllers[i].SetPausingAllowed(true);
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

        public void LockOutAllPlayers()
        {
            for (int i = 0; i < 4; i++)
            {
                if (this[i] != null)
                {
                    this[i].Disable();
                    this[i].SetPausingAllowed(false);
                }
            }
        }

        private void AssignToPlayer(PlayerInput[] controllers, int i)
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
        }

        public void OverrideUIAssetDevices(PlayerInput player)
        {
            LockOutAllPlayers();
            startScreenUIActionAsset = GetComponent<InputSystemUIInputModule>().actionsAsset;
            startScreenUIActionAsset.devices = player.Devices;
            startScreenActionMap.devices = player.Devices;
            player.EnableUIonly();
            uiTranslator.SetUIHoverTint(player.playerColour);
        }

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
                playerMode = (Controller)i;
            }
            uiTranslator.HideAll();
            startScreenActionMap = new();
            mainUIController.UIShown = true;
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


        #region pausing
        public void PauseGame(PlayerInput pauser)
        {
            Paused = true;
            InputSystem.PauseHaptics();
            LockOutAllPlayers();
            pauser.EnableUIonly();
            pauser.SetPausingAllowed(true);
            uiTranslator.SetUIHoverTint(pauser.playerColour);
            uiTranslator.ShowPauseMenu();
            startScreenUIActionAsset.devices = pauser.Devices;
            startScreenActionMap.devices = pauser.Devices;
            Time.timeScale = 0;
        }

        public void UnPauseGame()
        {
            uiTranslator.HideAll();
            Paused = false;
            Time.timeScale = 1;
            startScreenUIActionAsset.devices = PlayerOne.Devices;
            startScreenActionMap.devices = PlayerOne.Devices;
            
            ValidateControllersAndPlayers();
            InputSystem.ResumeHaptics();
            
        }

        #endregion

        public void GiveInputAuthority(int playerIndex, bool keepPlayerOneUI = false)
        {
            PlayerInput player = this[playerIndex];
            OverrideUIAssetDevices(player);
            if (keepPlayerOneUI)
            {
                PlayerOne.EnableUIonly();
                PlayerOne.SetWiimotePointerActive(false);   
            }
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

        public static Color GetPlayerColour(Controller player)
        {
            return player switch
            {
                Controller.One => PlayerOneColour,
                Controller.Two => PlayerTwoColour,
                Controller.Three => PlayerThreeColour,
                Controller.Four => PlayerFourColour,
                _ => Color.white
            };
        }
    }
}