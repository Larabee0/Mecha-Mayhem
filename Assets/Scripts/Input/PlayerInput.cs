using RedButton.Core.WiimoteSupport;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Haptics;
using UnityEngine.InputSystem.XR;
using static UnityEngine.Rendering.DebugUI;

namespace RedButton.Core
{
    public class PlayerInput : MonoBehaviour
    {
        [Header("Information")]
        [SerializeField] private Controller player = 0;
        public Controller Player => player;
        public Color playerColour;
        // input actions asset for this player.
        private DualControllerInput controlMap;
        public DualControllerInput ControlMap => controlMap;
        [SerializeField]private float controllerSensititiivty = 1f;
        public float ControllerSense
        {
            get => controllerSensititiivty;
            set => controllerSensititiivty = Mathf.Clamp(value, 0.5f, 2);
        }
        private bool setWiimotePointer;
        private bool aimAtRightStick = false;
        /// <summary>
        /// should the stick be treated as a direction (false) or position (true)?
        /// </summary>
        public bool AimAtRightStick => aimAtRightStick;

        // the rumble device assigned to this player, if they are using keyboard, this will be null.
        private IDualMotorRumble rumbleDevice;
        private string devicePath;
        public string DevicePath => devicePath;
        public string DeviceName;
        public bool DeviceConnected;

        public InputDevice[] Devices
        {
            get
            {
                if (controlMap == null || !DeviceConnected)
                {
                    return null;
                }

                return controlMap.devices.Value.ToArray();
            }
        }

        // events and information about the status of the fire button.
        public ButtonEventContainer fireOneButton = new();

        public ButtonEventContainer fireTwoButton = new();

        // Joy stick axis events
        public Vector2Axis OnLeftStick;
        public Vector2BoolAxis OnRightStick;

        // current rumble strengths for each rumble motor
        private float lowRumbleCurrent = 0f;
        private float highRumbleCurrent = 0f;

        // coroutines for running and stopping the rumble motors
        private Coroutine lowRumbleProcess;
        private Coroutine highRumbleProcess;

        // coroutines for frame by frame updates of input.
        private Coroutine leftStickProcess;
        private Coroutine rightStickProcess;
        private Coroutine fireOneButtonProcess;
        private Coroutine fireTwoButtonProcess;

        // test settings for manual triggering of rumble motors.
        [Header("Rumble Debugging")]
        public bool controlMapEnabled = false;
        public bool uiOnlyEnabled = false;
        [SerializeField] private bool debugging;
        [SerializeField] private RumbleMotor rumbleMotor;
        [SerializeField, Range(0f, 1f)] private float rumbleRate;
        [SerializeField] private float rumbleDuration = 10f;
        [SerializeField] private Vector2 rightStickInput;

        public void AssignDevice(InputDevice[] devices, Controller playerNum, bool keyboard = false)
        {
            DisposeOfCurrentControlMap();
            setWiimotePointer = false;
            aimAtRightStick = false;
            if (devices.Any(x => x is Keyboard) || devices.Any(x => x is Mouse))
            {
                aimAtRightStick = true;
                rumbleDevice = null;
                DeviceName = "Keyboard & Mouse";
            }

            if (devices[0] is WiimoteDevice wiimote)
            {
                aimAtRightStick = true;
                setWiimotePointer = true;
                rumbleDevice = wiimote;
                DeviceName = "Wiimote";
                switch (playerNum)
                {
                    case Controller.One:
                        wiimote.SetRemoteLEDs(new Unity.Mathematics.bool4(true, false, false, false));
                        break;
                    case Controller.Two:
                        wiimote.SetRemoteLEDs(new Unity.Mathematics.bool4(false, true, false, false));
                        break;
                    case Controller.Three:
                        wiimote.SetRemoteLEDs(new Unity.Mathematics.bool4(false, false, true, false));
                        break;
                    case Controller.Four:
                        wiimote.SetRemoteLEDs(new Unity.Mathematics.bool4(false, false, false, true));
                        break;
                }
            }
            else if (devices[0] is Gamepad gamepad)
            {
                rumbleDevice = gamepad;
                DeviceName = devices[0].displayName;
            }
            EnableWiimotePointer();
            CreateNewControlMap(devices);
            PersistantOptions.instance.OnUserSettingsChangedData += UpdateSensitivity;
            UpdateSensitivity();
            player = playerNum;
            devicePath = devices[0].path;
            DeviceConnected = true;
        }

        private void UpdateSensitivity()
        {
            ControllerSense = PersistantOptions.instance.userSettings.GetPlayerSense(player);
        }

        private void DisposeOfCurrentControlMap()
        {
            StopAllCoroutines();
            if (controlMap != null)
            {
                PersistantOptions.instance.OnUserSettingsChangedData -= UpdateSensitivity;
                // if we had a control map, un subscribe from it Un-Subscribe to the various stick and button inputs.
                // then dispose of it.
                controlMap.MechControls.LeftStick.started -= StartLeftStickActivity;
                controlMap.MechControls.RightStick.started -= StartRightStickActivity;
                controlMap.MechControls.LeftStick.canceled -= StopLeftStick;
                controlMap.MechControls.RightStick.canceled -= StopRightStick;

                controlMap.MechControls.Fire1.started -= OnFireOneStart;
                controlMap.MechControls.Fire1.canceled -= OnFireOneStop;

                controlMap.MechControls.Fire2.started -= OnFireTwoStart;
                controlMap.MechControls.Fire2.canceled -= OnFireTwoStop;
                controlMap.UI.PauseGame.performed -= PauseTesterCallback;
                controlMap.Dispose();
            }
        }

        private void CreateNewControlMap(InputDevice[] devices)
        {
            controlMap = new()
            {
                devices = devices,
            };

            // Subscribe to the various stick and button inputs.
            controlMap.MechControls.LeftStick.started += StartLeftStickActivity;
            controlMap.MechControls.RightStick.started += StartRightStickActivity;
            controlMap.MechControls.LeftStick.canceled += StopLeftStick;
            controlMap.MechControls.RightStick.canceled += StopRightStick;

            controlMap.MechControls.Fire1.started += OnFireOneStart;
            controlMap.MechControls.Fire1.canceled += OnFireOneStop;

            controlMap.MechControls.Fire2.started += OnFireTwoStart;
            controlMap.MechControls.Fire2.canceled += OnFireTwoStop;

            controlMap.UI.PauseGame.performed += PauseTesterCallback;
        }

        private void PauseTesterCallback(InputAction.CallbackContext obj)
        {
            Debug.LogFormat("Recived Pause input from {0}", DeviceName);
        }

        /// <summary>
        /// Enable user input for this player
        /// </summary>
        public void Enable()
        {
            uiOnlyEnabled = false;
            controlMapEnabled = true;
            controlMap.MechControls.Enable();
            controlMap.UI.Enable();
        }

        public void EnableUIonly()
        {
            controlMapEnabled = false;
            uiOnlyEnabled = true;
            controlMap.UI.Enable();
            controlMap.MechControls.Disable();
        }

        public void SetPausingAllowed(bool allowed = false)
        {
            if(allowed)
            {
                controlMap.UI.PauseGame.performed += PauseCallback;
                return;
            }
            controlMap.UI.PauseGame.performed -= PauseCallback;
        }

        private void PauseCallback(InputAction.CallbackContext obj)
        {
            if (ControlArbiter.Paused)
            {
                ControlArbiter.Instance.UnPauseGame();
                return;
            }
            
            ControlArbiter.Instance.PauseGame(this);
        }

        /// <summary>
        /// Disable user input for this player
        /// </summary>
        public void Disable()
        {
            controlMapEnabled= uiOnlyEnabled = false;
            controlMap.MechControls.Disable();
            controlMap.UI.Disable();
            StopAllCoroutines();
        }

        private void Update()
        {
            EnableWiimotePointer();
            if (debugging)
            {
                if (Input.GetKeyUp(KeyCode.Space) && !Input.GetKey(KeyCode.LeftShift))
                {
                    RumbleMotor(rumbleDuration, rumbleRate, rumbleMotor);
                }
                if (Input.GetKeyUp(KeyCode.Space) && Input.GetKey(KeyCode.LeftShift))
                {
                    StopRumbleMotor(rumbleMotor);
                }
            }
            if (AimAtRightStick && controlMap != null && controlMap.MechControls.enabled)
            {
                RightStickLogic();
            }
            if(AimAtRightStick && controlMap != null && controlMap.UI.enabled)
            {
                SetWiimotePointer(controlMap.UI.Point.ReadValue<Vector2>());
            }
        }
        private void OnDestroy()
        {
            Debug.Log("Decommissioning Player Input Script!");
            Disable();
            DisposeOfCurrentControlMap();
            StopRumbleMotor(global::RumbleMotor.Both);
        }

        #region Buttons
        private void OnFireOneStart(InputAction.CallbackContext context)
        {
            fireOneButton.buttonDown = true;
            fireOneButton.OnButtonPressed?.Invoke();
            if (fireOneButtonProcess != null)
            {
                StopCoroutine(fireOneButtonProcess);
            }
            fireOneButtonProcess = StartCoroutine(OnFireOneHeld());
        }

        private void OnFireOneStop(InputAction.CallbackContext context)
        {
            if (fireOneButtonProcess != null)
            {
                StopCoroutine(fireOneButtonProcess);
                fireOneButtonProcess = null;
            }
            fireOneButton.buttonDown = false;
            fireOneButton.OnButtonReleased?.Invoke();
        }

        private IEnumerator OnFireOneHeld()
        {
            while (true)
            {
                fireOneButton.OnButtonHeld?.Invoke();
                yield return null;
            }
        }


        private void OnFireTwoStart(InputAction.CallbackContext context)
        {
            fireTwoButton.buttonDown = true;
            fireTwoButton.OnButtonPressed?.Invoke();
            if (fireTwoButtonProcess != null)
            {
                StopCoroutine(fireTwoButtonProcess);
            }
            fireTwoButtonProcess = StartCoroutine(OnFireTwoHeld());
        }

        private void OnFireTwoStop(InputAction.CallbackContext context)
        {

            if (fireTwoButtonProcess != null)
            {
                StopCoroutine(fireTwoButtonProcess);
                fireTwoButtonProcess = null;
            }
            fireTwoButton.buttonDown = false;
            fireTwoButton.OnButtonReleased?.Invoke();
        }

        private IEnumerator OnFireTwoHeld()
        {
            while (true)
            {
                fireTwoButton.OnButtonHeld?.Invoke();
                yield return null;
            }
        }
        #endregion

        #region Sticks
        private void StartLeftStickActivity(InputAction.CallbackContext context)
        {
            if (leftStickProcess != null)
            {
                StopCoroutine(leftStickProcess);
            }

            leftStickProcess = StartCoroutine(LeftStickActivity());
        }

        private void StartRightStickActivity(InputAction.CallbackContext context)
        {
            if (!aimAtRightStick)
            {
                if (rightStickProcess != null)
                {
                    StopCoroutine(rightStickProcess);
                }

                rightStickProcess = StartCoroutine(RightStickActivity());
            }
        }

        private void StopLeftStick(InputAction.CallbackContext context)
        {

            if (leftStickProcess != null)
            {
                StopCoroutine(leftStickProcess);
                OnLeftStick?.Invoke(Vector2.zero);
            }
        }

        private void StopRightStick(InputAction.CallbackContext context)
        {
            if (rightStickProcess != null)
            {
                StopCoroutine(rightStickProcess);
                Vector2 value = AimAtRightStick ? controlMap.MechControls.RightStick.ReadValue<Vector2>() : Vector2.zero;
                OnRightStick?.Invoke(value, AimAtRightStick);
                if (setWiimotePointer)
                {
                    SetWiimotePointer(value);
                }
            }
        }


        private IEnumerator LeftStickActivity()
        {
            while (true)
            {
                OnLeftStick?.Invoke(controlMap.MechControls.LeftStick.ReadValue<Vector2>());
                yield return null;
            }
        }

        private IEnumerator RightStickActivity()
        {
            while (true)
            {
                RightStickLogic();
                yield return null;
            }
        }

        private void RightStickLogic()
        {
            Vector2 value = controlMap.MechControls.RightStick.ReadValue<Vector2>();
            rightStickInput = value;
            OnRightStick?.Invoke(value, AimAtRightStick);
            if (setWiimotePointer)
            {
                SetWiimotePointer(value);
            }
        }

        private void SetWiimotePointer(Vector2 value)
        {
            switch (player)
            {
                case Controller.One:
                    ControlArbiter.Wiimote1PointerPos = value;
                    break;
                case Controller.Two:
                    ControlArbiter.Wiimote2PointerPos = value;
                    break;
                case Controller.Three:
                    ControlArbiter.Wiimote3PointerPos = value;
                    break;
                case Controller.Four:
                    ControlArbiter.Wiimote4PointerPos = value;
                    break;

            }
        }
        private void EnableWiimotePointer()
        {
            switch (player)
            {
                case Controller.One:
                    ControlArbiter.Wiimote1PointerEnable = setWiimotePointer;
                    break;
                case Controller.Two:
                    ControlArbiter.Wiimote2PointerEnable = setWiimotePointer;
                    break;
                case Controller.Three:
                    ControlArbiter.Wiimote3PointerEnable = setWiimotePointer;
                    break;
                case Controller.Four:
                    ControlArbiter.Wiimote4PointerEnable = setWiimotePointer;
                    break;

            }
        }
        #endregion

        #region RumbleMotors
        /// <summary>
        /// Provides a way of starting the rumble motors for this player's controller.
        /// </summary>
        /// <param name="duration"> How long the motors will run. </param>
        /// <param name="strength"> How strong the rumble will be. </param>
        /// <param name="rumbleMotor"> Which motor will be run, low, high or both rumble frequencies.</param>
        public void RumbleMotor(float duration, float strength, RumbleMotor rumbleMotor)
        {
            if (rumbleDevice != null)
            {
                switch (rumbleMotor)
                {
                    case global::RumbleMotor.LowFreq:
                    case global::RumbleMotor.Both:
                        if (lowRumbleProcess != null)
                        {
                            StopRumbleMotor(global::RumbleMotor.LowFreq);
                        }
                        lowRumbleProcess = StartCoroutine(LowRumbleMotor(duration, strength));
                        break;
                }

                switch (rumbleMotor)
                {
                    case global::RumbleMotor.HighFreq:
                    case global::RumbleMotor.Both:
                        if (highRumbleProcess != null)
                        {
                            StopRumbleMotor(global::RumbleMotor.HighFreq);
                        }
                        highRumbleProcess = StartCoroutine(HighRumbleMotor(duration, strength));
                        break;
                }
            }
        }

        /// <summary>
        /// Provides a way to force stop the rumble motors for this player's controller
        /// </summary>
        /// <param name="rumbleMotor"> Specific motor to stop, low, high or both rumble frequencies.</param>
        private void StopRumbleMotor(RumbleMotor rumbleMotor)
        {
            if ((rumbleMotor == global::RumbleMotor.LowFreq || rumbleMotor == global::RumbleMotor.Both) && lowRumbleProcess != null)
            {
                StopCoroutine(lowRumbleProcess);
                lowRumbleCurrent = 0f;
                rumbleDevice.SetMotorSpeeds(lowRumbleCurrent, highRumbleCurrent);
                lowRumbleProcess = null;
            }

            if ((rumbleMotor == global::RumbleMotor.HighFreq || rumbleMotor == global::RumbleMotor.Both) && highRumbleProcess != null)
            {
                StopCoroutine(highRumbleProcess);
                highRumbleCurrent = 0f;
                rumbleDevice.SetMotorSpeeds(lowRumbleCurrent, highRumbleCurrent);
                highRumbleProcess = null;
            }
        }

        #region Rumble Coroutines
        /// <summary>
        /// Coroutine for running the low frequency rumble motor.
        /// </summary>
        /// <param name="duration">How long to run the motor for.</param>
        /// <param name="strength">How strong it will rumble.</param>
        /// <returns></returns>
        private IEnumerator LowRumbleMotor(float duration, float rate)
        {
            lowRumbleCurrent = Mathf.Clamp01(rate);
            yield return new WaitForEndOfFrame();
            rumbleDevice.SetMotorSpeeds(lowRumbleCurrent, highRumbleCurrent);
            yield return new WaitForSeconds(duration);
            lowRumbleCurrent = 0f;
            yield return new WaitForEndOfFrame();
            rumbleDevice.SetMotorSpeeds(lowRumbleCurrent, highRumbleCurrent);
            lowRumbleProcess = null;
        }

        /// <summary>
        /// Coroutine for running the high frequency rumble motor.
        /// </summary>
        /// <param name="duration">How long to run the motor for.</param>
        /// <param name="strength">How strong it will rumble.</param>
        /// <returns></returns>
        private IEnumerator HighRumbleMotor(float duration, float strength)
        {
            highRumbleCurrent = Mathf.Clamp01(strength);
            yield return new WaitForEndOfFrame();
            rumbleDevice.SetMotorSpeeds(lowRumbleCurrent, highRumbleCurrent);
            yield return new WaitForSeconds(duration);
            highRumbleCurrent = 0f;
            yield return new WaitForEndOfFrame();
            rumbleDevice.SetMotorSpeeds(lowRumbleCurrent, highRumbleCurrent);
            highRumbleProcess = null;
        }

        #endregion

        #endregion
    }
}
