using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

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
        public DualControllerInput ControlMap=>controlMap;

        // the gamepad device assigned to this player, if they are using keyboard, this will be null.
        private Gamepad gamepad;
        private string devicePath;
        public string DevicePath => devicePath;
        public string DeviceName;
        public bool DeviceConnected;

        public InputDevice Device
        {
            get
            {
                if(controlMap== null||!DeviceConnected)
                {
                    return null;
                }

                return controlMap.devices.Value[0];
            }
        }

        // events and information about the status of the fire button.
        public ButtonEventContainer fireOneButton =new();

        public ButtonEventContainer fireTwoButton =new();

        // Joy stick axis events
        public Vector2Axis OnLeftStick;
        public Vector2Axis OnRightStick;

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
        [SerializeField] private bool debugging;
        [SerializeField] private RumbleMotor rumbleMotor;
        [SerializeField, Range(0f, 1f)] private float rumbleRate;
        [SerializeField] private float rumbleDuration = 10f;

        public void AssignDevice(InputDevice device, Controller playerNum, bool keyboard = false)
        {
            StopAllCoroutines();
            if(controlMap != null)
            {
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
                controlMap.Dispose();
            }

            if (keyboard)
            {
                controlMap = new()
                {
                    devices = new InputDevice[] { Keyboard.current, Mouse.current },
                    bindingMask = InputBinding.MaskByGroups("Keyboard", "Mouse")
                };
            }
            else if (device.GetType() == typeof(WiimoteDevice))
            {
                controlMap = new()
                {
                    devices = new[] { device },
                    //bindingMask = InputBinding.MaskByGroup("Gamepad")
                };
            }
            else
            {
                gamepad = (Gamepad)device;
                controlMap = new()
                {
                    devices = new[] { gamepad },
                    //bindingMask = InputBinding.MaskByGroup("Gamepad")
                };
            }

            player = playerNum;
            devicePath = device.path;
            DeviceConnected = true;
            DeviceName = device.displayName;
            // Subscribe to the various stick and button inputs.
            controlMap.MechControls.LeftStick.started += StartLeftStickActivity;
            controlMap.MechControls.RightStick.started += StartRightStickActivity;
            controlMap.MechControls.LeftStick.canceled += StopLeftStick;
            controlMap.MechControls.RightStick.canceled += StopRightStick;

            controlMap.MechControls.Fire1.started += OnFireOneStart;
            controlMap.MechControls.Fire1.canceled += OnFireOneStop;

            controlMap.MechControls.Fire2.started += OnFireTwoStart;
            controlMap.MechControls.Fire2.canceled += OnFireTwoStop;
        }

        /// <summary>
        /// Enable user input for this player
        /// </summary>
        public void Enable()
        {
            controlMap.MechControls.Enable();
            controlMap.UI.Enable();
        }

        public void EnableUIonly()
        {
            controlMap.UI.Enable();
        }

        /// <summary>
        /// Disable user input for this player
        /// </summary>
        public void Disable()
        {
            controlMap.MechControls.Disable();
            controlMap.UI.Disable();
            StopAllCoroutines();
        }

#if UNITY_EDITOR
        private void Update()
        {
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
        }
#endif

        private void OnDestroy()
        {
            Debug.Log("Decommissioning Player Input Script!");
            StopRumbleMotor(global::RumbleMotor.Both);
            Disable();
            ControlMap.Dispose();
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
            if (rightStickProcess != null)
            {
                StopCoroutine(rightStickProcess);
            }

            rightStickProcess = StartCoroutine(RightStickActivity());
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
                OnRightStick?.Invoke(Vector2.zero);
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
                OnRightStick?.Invoke(controlMap.MechControls.RightStick.ReadValue<Vector2>());
                yield return null;
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
            if (gamepad != null)
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
                gamepad.SetMotorSpeeds(lowRumbleCurrent, highRumbleCurrent);
                lowRumbleProcess = null;
            }

            if ((rumbleMotor == global::RumbleMotor.HighFreq || rumbleMotor == global::RumbleMotor.Both) && highRumbleProcess != null)
            {
                StopCoroutine(highRumbleProcess);
                highRumbleCurrent = 0f;
                gamepad.SetMotorSpeeds(lowRumbleCurrent, highRumbleCurrent);
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
            gamepad.SetMotorSpeeds(lowRumbleCurrent, highRumbleCurrent);
            yield return new WaitForSeconds(duration);
            lowRumbleCurrent = 0f;
            yield return new WaitForEndOfFrame();
            gamepad.SetMotorSpeeds(lowRumbleCurrent, highRumbleCurrent);
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
            gamepad.SetMotorSpeeds(lowRumbleCurrent, highRumbleCurrent);
            yield return new WaitForSeconds(duration);
            highRumbleCurrent = 0f;
            yield return new WaitForEndOfFrame();
            gamepad.SetMotorSpeeds(lowRumbleCurrent, highRumbleCurrent);
            highRumbleProcess = null;
        }

        #endregion

        #endregion
    }
}