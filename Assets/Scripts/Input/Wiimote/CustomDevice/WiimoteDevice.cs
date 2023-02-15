using System.Collections.Generic;
using System.ComponentModel;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Haptics;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using WiimoteApi;

/*
 * Built using https://docs.unity3d.com/Packages/com.unity.inputsystem@1.4/manual/Devices.html#creating-custom-devices
 * as a guide
 */

namespace RedButton.Core.WiimoteSupport
{
    /// <summary>
    /// An InputDevice dervied class used to adapat WiimoteAPI.Wiimote class into UnityEngine.InputSystem
    /// This is for the purposes of using existing controller infrastructure, provides a common API for all input devices
    /// and to give a consistant reliable way of interfacing with the Wiimote API
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [InputControlLayout(displayName = "Wiimote", stateType = typeof(WiimoteState))]
    public class WiimoteDevice : InputDevice, IDualMotorRumble, IInputUpdateCallbackReceiver
    {
        // stores the LED state for the 4 LEDS of the wiimote. default is for them to all be on.
        private bool4 ledState = true;

        // stores the current rumble motor state, theoritically this is cached in the wiimote itself, but this will always be
        // what the current state of the rumble motor should be.
        private bool shouldRumble = false;

        // Initilisation variable used to determine when the nunchuck has been required so alllow IR Sensor set up.
        private bool gotNunchuck = false;

        // WiimoteAPI.Wiimote device - the physical wiimote this WiimoteDevice is associated with.
        private Wiimote wiimote;
        public Wiimote Wiimote => wiimote;

        #region Control Properties
        public ButtonControl buttonA { get; private set; }
        public ButtonControl buttonB { get; private set; }
        public ButtonControl buttonC { get; private set; }
        public ButtonControl buttonZ { get; private set; }
        public ButtonControl buttonOne { get; private set; }
        public ButtonControl buttonTwo { get; private set; }
        public ButtonControl buttonPlus { get; private set; }
        public ButtonControl buttonMinus { get; private set; }
        public ButtonControl buttonHome { get; private set; }
        public DpadControl dpad { get; private set; }

        public Vector2Control irPosition { get; protected set; }
        public StickControl nunchuckStick { get; protected set; }

        public ButtonControl this[WiimoteButton button] => button switch
        {
            WiimoteButton.DpadUp => dpad.up,
            WiimoteButton.DpadDown => dpad.down,
            WiimoteButton.DpadLeft => dpad.left,
            WiimoteButton.DpadRight => dpad.right,
            WiimoteButton.A => buttonA,
            WiimoteButton.B => buttonB,
            WiimoteButton.C => buttonC,
            WiimoteButton.Z => buttonZ,
            WiimoteButton.One => buttonOne,
            WiimoteButton.Two => buttonTwo,
            WiimoteButton.Plus => buttonPlus,
            WiimoteButton.Minus => buttonMinus,
            WiimoteButton.Home => buttonHome,
            _ => throw new InvalidEnumArgumentException(nameof(button), (int)button, typeof(GamepadButton)),
        };
        #endregion

        #region Non-Static Methods
        /// <summary>
        /// A way of modifying the IR Camera Sensitivity after the wiimote has been registered.
        /// </summary>
        /// <param name="sensitivity">Sensitivity level of the IR Camera to set</param>
        public void SetIRSensitivity(IRSensitivity sensitivity)
        {
            SetUpIRCamera(wiimote, sensitivity, IRDataType.BASIC);
        }

        /// <summary>
        /// Simple way to set the 4 LEDs on the wiimote
        /// </summary>
        /// <param name="state">Which LEDs are on and off ordered left to right x to w</param>
        public void SetRemoteLEDs(bool4 state)
        {
            ledState = state;
            wiimote?.SendPlayerLED(state.x, state.y, state.z, state.w);
        }

        /// <summary>
        /// Common interface to set controller rumble motors.
        /// The wiimote only has 1 motor and it binary on or off.
        /// If low or high freq are greater than zero, the ruble motor will run.
        /// </summary>
        /// <param name="lowFrequency"></param>
        /// <param name="highFrequency"></param>
        public void SetMotorSpeeds(float lowFrequency, float highFrequency)
        {
            if (wiimote == null)
            {
                return;
            }
            shouldRumble = false;
            if (lowFrequency > 0.0f)
            {
                shouldRumble = true;
            }
            if (highFrequency > 0.0f)
            {
                shouldRumble = true;
            }
            wiimote.RumbleOn = shouldRumble;
            wiimote.SendStatusInfoRequest();
        }

        /// <summary>
        /// API requirement pause the haptics of the wiimote
        /// </summary>
        public void PauseHaptics()
        {
            if(wiimote == null)
            {
                return;
            }
            wiimote.RumbleOn = false;
            wiimote.SendStatusInfoRequest();
        }

        /// <summary>
        /// API requirement resume the haptics of the wiimote
        /// </summary>
        public void ResumeHaptics()
        {
            if (wiimote == null)
            {
                return;
            }
            wiimote.RumbleOn = shouldRumble;
            wiimote.SendStatusInfoRequest();
        }

        /// <summary>
        /// API requirement reset the haptics of the wiimote (turn them off)
        /// </summary>
        public void ResetHaptics()
        {
            if (wiimote == null)
            {
                return;
            }
            wiimote.RumbleOn = shouldRumble = false;
            wiimote.SendStatusInfoRequest();
        }

        /// <summary>
        /// API requirement, Called once per frame before MonoBehaviour.Update()
        /// This sets the state of the virtual wiimote device.
        /// It also does some device initilisation.
        /// </summary>
        public void OnUpdate()
        {
            // it is possible for a wiimote device to exist breilfly without an actual Wiimote From the WiimoteAPI being associated with it
            // making this safety check is required.
            if (wiimote != null)
            { 
                // read through the data queue from the remote this frame
                UpdateWiimote(wiimote);

                // We can only detect if a nunchuck is present in the default InputDataType remote mode.
                // However, we can only recieve data from the nunchuck in a InputDataType that supports bytes for Extensions.
                // So while we haven't acknowledge we have a nunchuck, we will try and get it.
                if (!gotNunchuck && wiimote.Nunchuck != null)
                {
                    // when we get it we can setup the IR camera and safely go into InputDataType.REPORT_BUTTONS_ACCEL_IR10_EXT6
                    // which allows us 6 bytes for extensions so we can read data from the nunchuck and IR sensor.
                    SetUpIRCamera(wiimote, IRSensitivity.LevelOne, IRDataType.BASIC);
                    gotNunchuck = true;
                }


                // here we create the wiimote's state (buttons, ir position, nunchuck stick position, etc)
                // The order of operations here doesn't matter, the state is created with the IR and Nunchuck Stick position,
                // simply because they are the only vector2 values in the state.
                var state = new WiimoteState()
                {
                    position = UpdateIR(wiimote),
                    nunchuckStick = UpdateNunchuckStick(wiimote)
                };
                //  After we check set the state of all buttons (nunchuck buttons too)
                state = UpdateButtons(wiimote, state);

                // Queue up the state change event for the Input system, this will trigger all relevant the events if the state has changed
                InputSystem.QueueStateEvent(this, state);
            }
        }

        /// <summary>
        /// Sets the wiimote and tries to set the LED state of the remote
        /// </summary>
        /// <param name="remote"></param>
        public void RegisterWiimote(Wiimote remote)
        {
            wiimote = remote;
            UpdateWiimote(wiimote);
            SetRemoteLEDs(ledState);
            Debug.Log("InputSystem has Registered Wiimote!");
        }

        /// <summary>
        /// API requirement, Binds the Control properties to those in the wiimoteState struct
        /// </summary>
        protected override void FinishSetup()
        {
            buttonA = GetChildControl<ButtonControl>("aButton");
            buttonB = GetChildControl<ButtonControl>("bButton");
            buttonC = GetChildControl<ButtonControl>("cButton");
            buttonZ = GetChildControl<ButtonControl>("zButton");
            buttonOne = GetChildControl<ButtonControl>("oneButton");
            buttonTwo = GetChildControl<ButtonControl>("twoButton");
            buttonPlus = GetChildControl<ButtonControl>("plusButton");
            buttonMinus = GetChildControl<ButtonControl>("minusButton");
            buttonHome = GetChildControl<ButtonControl>("homeButton");

            irPosition = GetChildControl<Vector2Control>("position");

            nunchuckStick = GetChildControl<StickControl>("nunchuckStick");

            base.FinishSetup();
        }

        /// <summary>
        /// API requirement, used to make this wiimote device instance to the current instance.
        /// </summary>
        public override void MakeCurrent()
        {
            base.MakeCurrent();
            current = this;
        }

        /// <summary>
        /// API requirement, used to add a new wiimote device to the WiimoteDevice.all static array.
        /// </summary>
        protected override void OnAdded()
        {
            base.OnAdded();
            s_AllWiimotes.Add(this);
        }

        /// <summary>
        /// API requirement, used to remove a new wiimote device to the WiimoteDevice.all static array.
        /// </summary>
        protected override void OnRemoved()
        {
            base.OnRemoved();
            s_AllWiimotes.Remove(this);
        }
        #endregion

        #region Static Methods and Properties
        /// <summary>
        /// Sets the button states of all wiimote & nunchuck buttons
        /// </summary>
        /// <param name="remote">Target WiimoteAPI.Wiimote to get the button states of</param>
        /// <param name="state">The current new state of the WiimoteDevice.
        /// It should only include IR and nunchuck stick states at this point</param>
        /// <returns>The WiimoteState now with up to date Wiimote and Nunchuck button states added to it</returns>
        private static WiimoteState UpdateButtons(Wiimote remote, WiimoteState state)
        {
            // the remote we are parsed by be null, guard against that.
            if (remote != null)
            {
                // a remote that does exist can have null ButtonData.
                // If this is the case we'll set all buttons on the remote to released.
                // Otherwise those, we'll set it to the button states of the button Data.
                ButtonData data = remote.Button;
                if (data == null)
                {
                    state.WithButtons(WiimoteButton.A, false);
                    state.WithButtons(WiimoteButton.B, false);

                    state.WithButtons(WiimoteButton.Plus, false);
                    state.WithButtons(WiimoteButton.Minus, false);

                    state.WithButtons(WiimoteButton.One, false);
                    state.WithButtons(WiimoteButton.Two, false);

                    state.WithButtons(WiimoteButton.DpadUp, false);
                    state.WithButtons(WiimoteButton.DpadDown, false);
                    state.WithButtons(WiimoteButton.DpadLeft, false);
                    state.WithButtons(WiimoteButton.DpadRight, false);
                }
                else
                {
                    state.WithButtons(WiimoteButton.A, data.a);
                    state.WithButtons(WiimoteButton.B, data.b);

                    state.WithButtons(WiimoteButton.Plus, data.plus);
                    state.WithButtons(WiimoteButton.Minus, data.minus);

                    state.WithButtons(WiimoteButton.One, data.one);
                    state.WithButtons(WiimoteButton.Two, data.two);

                    state.WithButtons(WiimoteButton.DpadUp, data.d_up);
                    state.WithButtons(WiimoteButton.DpadDown, data.d_down);
                    state.WithButtons(WiimoteButton.DpadLeft, data.d_left);
                    state.WithButtons(WiimoteButton.DpadRight, data.d_right);
                }

                // similarly to above, the nunchuck return null data, so as before we'll set the nunchuck buttons to realeased
                // if that is true.
                NunchuckData nunchuck = remote.Nunchuck;
                if (nunchuck == null)
                {
                    state.WithButtons(WiimoteButton.C, false);
                    state.WithButtons(WiimoteButton.Z, false);
                }
                else
                {
                    state.WithButtons(WiimoteButton.C, nunchuck.c);
                    state.WithButtons(WiimoteButton.Z, nunchuck.z);
                }
            }

            return state;
        }

        /// <summary>
        /// This gets the Nunchuck's stick position, or if not present return float2.zero (neutral).
        /// It also has to process the data from the stick. By default teh API gives data in the range 0,0 to 1,1
        /// 0,0 bottom left, 1,1 top right and 0.5,0.5 being neutral.
        /// For this to correctly with like all other sticks we need to use that data to lerp between
        /// -1 to 1 with -1,-1 being bottom left, and 1,1 top right and 0,0 being neutral.
        /// Note this bakes in some stick deadzone (0.1 off center after processing)
        /// </summary>
        /// <param name="remote">Target WiimoteAPI.Wiimote</param>
        /// <returns>Raw axis of the nunchuck's joystick</returns>
        private static float2 UpdateNunchuckStick(Wiimote remote)
        {
            if (remote != null && remote.current_ext == ExtensionController.NUNCHUCK && remote.Nunchuck != null)
            {
                float[] input = remote.Nunchuck.GetStick01();
                float2 numChuckV2 = new(math.lerp(-1, 1, input[0]), math.lerp(-1, 1, input[1]));

                numChuckV2.x = math.abs(numChuckV2.x) > 0.1f ? numChuckV2.x : 0f;
                numChuckV2.y = math.abs(numChuckV2.y) > 0.1f ? numChuckV2.y : 0f;

                numChuckV2 = math.clamp(numChuckV2, -1, 1);
                return numChuckV2;
            }
            return float2.zero;
        }

        /// <summary>
        /// This converts the IR point position to an equivlent pixel position of
        /// the current screen resolution. This could probably be improved to make the remote point more accurately
        /// </summary>
        /// <param name="remote">Target WiimoteAPI.Wiimote</param>
        /// <returns>Pixel position of the wiimote pointer</returns>
        private static float2 UpdateIR(Wiimote remote)
        {
            if (remote.Ir != null)
            {
                float[] input = remote.Ir.GetPointingPosition();
                float2 IRpos = new(input[0], input[1]);

                float2 wiimotePosition = new()
                {
                    x = math.lerp(0, Screen.width, IRpos.x),
                    y = math.lerp(0, Screen.height, IRpos.y)
                };

                return wiimotePosition;
            }
            return float2.zero;
        }

        /// <summary>
        /// Created based off the WiimoteAPI documentation.
        /// The wiimote class recieves data in a queue, which must be read through to update the class.
        /// This simply reads through all the data currently in the queue until the quite is empty.
        /// </summary>
        /// <param name="remote">Target WiimoteAPI.Wiimote to update the state of</param>
        private static void UpdateWiimote(Wiimote remote)
        {
            int ret;
            do
            {
                ret = remote.ReadWiimoteData();
            } while (ret > 0);
        }

        /// <summary>
        /// This is boradly copied from the WiimoteAPI.Wiimote class with some key additions.
        /// - The default IRDataType is Basic instead of extended,so the nunchuck works.
        /// - Another argument, IRSensitivity is provided to set the remote's senstivity,
        ///   this supports the same 5 levels of sensitivity the Wii console did.
        /// </summary>
        /// <param name="remote">Target WiimoteAPI.Wiimote</param>
        /// <param name="sensitivity">Desired IR sensitivity Level (1-5)</param>
        /// <param name="type">IR data type (Basic or Extended)</param>
        /// <returns>Returns true if the IR camera was successfully set up</returns>
        private static bool SetUpIRCamera(Wiimote remote, IRSensitivity sensitivity, IRDataType type = IRDataType.BASIC)
        {
            int res;

            res = SendIRCameraEnabled(remote, true);
            if (res < 0) return false;

            res = remote.SendRegisterWriteRequest(RegisterType.CONTROL, 0xb00030, new byte[] { 0x08 });
            if (res < 0) return false;

            int offset = 0xb00000;
            byte[] block = GetSensitivityBlockOne(sensitivity);
            res = remote.SendRegisterWriteRequest(RegisterType.CONTROL, offset, block);
            if (res < 0) return false;

            offset = 0xb0001a;
            block = GetSensitivityBlockTwo(sensitivity);
            res = remote.SendRegisterWriteRequest(RegisterType.CONTROL, offset, block);
            if (res < 0) return false;

            res = remote.SendRegisterWriteRequest(RegisterType.CONTROL, 0xb00033, new byte[] { (byte)type });
            if (res < 0) return false;

            res = remote.SendRegisterWriteRequest(RegisterType.CONTROL, 0xb00030, new byte[] { 0x08 });
            if (res < 0) return false;
            switch (type)
            {
                case IRDataType.BASIC:
                    res = remote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL_IR10_EXT6);
                    break;
                case IRDataType.EXTENDED:
                    res = remote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL_IR12);
                    break;
                case IRDataType.FULL:
                    res = remote.SendDataReportMode(InputDataType.REPORT_INTERLEAVED);
                    break;
            }
            if (res < 0) return false;
            return true;
        }

        /// <summary>
        /// Helper method for SetUpIRCamera.
        /// IR sensivity is set by writing to two register blocks in the wiimote.
        /// This provides the values for Block One, this is a 9 byte value
        /// </summary>
        /// <param name="sensitivity">Desired IR sensitivity Level (1-5)</param>
        /// <returns>Block One Register Values to set for the provided Sensitivity Level</returns>
        private static byte[] GetSensitivityBlockOne(IRSensitivity sensitivity) => sensitivity switch
        {
            IRSensitivity.LevelOne => new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0x64, 0x00, 0xfe },
            IRSensitivity.LevelTwo => new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0x96, 0x00, 0xb4 },
            IRSensitivity.LevelThree => new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0xaa, 0x00, 0x64 },
            IRSensitivity.LevelFour => new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0xc8, 0x00, 0x36 },
            IRSensitivity.LevelFive => new byte[] { 0x07, 0x00, 0x00, 0x71, 0x01, 0x00, 0x72, 0x00, 0x20 },
            _ => new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0xaa, 0x00, 0x64 },
        };

        /// <summary>
        /// IR sensivity is set by writing to two register blocks in the wiimote.
        /// This provides the values for Block Two, this is a 2 byte value
        /// </summary>
        /// <param name="sensitivity">Desired IR sensitivity Level (1-5)</param>
        /// <returns>Block Two Register Values to set for the provided Sensitivity Level</returns>
        private static byte[] GetSensitivityBlockTwo(IRSensitivity sensitivity) => sensitivity switch
        {
            IRSensitivity.LevelOne => new byte[] { 0xfd, 0x05 },
            IRSensitivity.LevelTwo => new byte[] { 0xb3, 0x04 },
            IRSensitivity.LevelThree => new byte[] { 0x63, 0x03 },
            IRSensitivity.LevelFour => new byte[] { 0x35, 0x03 },
            IRSensitivity.LevelFive => new byte[] { 0x1f, 0x03 },
            _ => new byte[] { 0x63, 0x03 },
        };

        /// <summary>
        /// Helper method for SetUpIRCamera
        /// This simply enables/disables the IR camera of a given wiimote.
        /// </summary>
        /// <param name="remote">Target WiimoteAPI.Wiimote</param>
        /// <param name="enabled">Desired IR camera Enabled state</param>
        /// <returns>Success flag of this operations</returns>
        private static int SendIRCameraEnabled(Wiimote remote, bool enabled)
        {
            byte[] mask = new byte[] { (byte)(enabled ? 0x04 : 0x00) };

            int first = remote.SendWithType(OutputDataType.IR_CAMERA_ENABLE, mask);
            if (first < 0) { return first; }

            int second = remote.SendWithType(OutputDataType.IR_CAMERA_ENABLE_2, mask);
            if (second < 0) { return second; }

            return first + second; // success
        }

        /// <summary>
        /// What is interprented to be the "Main" wiimote.
        /// </summary>
        public static WiimoteDevice current { get; private set; }

        /// <summary>
        /// All currently attached virtual wiimote devices.
        /// </summary>
        public new static IReadOnlyList<WiimoteDevice> all => s_AllWiimotes;
        private static List<WiimoteDevice> s_AllWiimotes = new();

        /// <summary>
        /// API requirment to register the wiimote device in the InputAction map editor.
        /// </summary>
        static WiimoteDevice() { InputSystem.RegisterLayout<WiimoteDevice>(); }

        /// <summary>
        /// API requirment to register the wiimote device with the Input System
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeInPlayer() { }
        #endregion
    }
}