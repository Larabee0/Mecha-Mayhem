using System.Collections;
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
using UnityEngine.InputSystem.Utilities;
using WiimoteApi;
//using static WiimoteTesting;
/*
 * Built using https://docs.unity3d.com/Packages/com.unity.inputsystem@1.4/manual/Devices.html#creating-custom-devices
 * as a guide
 */

/// <summary>
/// An InputDevice dervied class used to adapat the WiimoteAPI Wiimote class into UnityEngine.InputSystem.
/// This is for the purpsoes of using existing controller infrastructure 
/// and to give a consistant reliable way of interfacing with the Wiimote API
/// </summary>
#if UNITY_EDITOR
[InitializeOnLoad]
#endif
[InputControlLayout(displayName = "Wiimote",stateType =typeof(WiimoteState))]
public class WiimoteDevice : InputDevice, IDualMotorRumble, IInputUpdateCallbackReceiver
{
    private bool4 ledState = true;
    private bool shouldRumble = false;
    private bool gotNunchuck = false;
    private Wiimote wiimote;
    public Wiimote Wiimote => wiimote;


    public ButtonControl buttonA { get; private set; }
    public ButtonControl buttonB { get;private set; }
    public ButtonControl buttonC { get; private set; }
    public ButtonControl buttonZ { get; private set; }
    public ButtonControl buttonOne { get; private set; }
    public ButtonControl buttonTwo { get; private set; }
    public ButtonControl buttonPlus { get; private set; }
    public ButtonControl buttonMinus { get; private set; }
    public ButtonControl buttonHome { get; private set; }
    public DpadControl dpad { get; private set; }

    public Vector2Control position { get; protected set; }
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

    public void PauseHaptics()
    {
        wiimote.RumbleOn = false;
        wiimote.SendStatusInfoRequest();
    }

    public void ResumeHaptics()
    {
        wiimote.RumbleOn = shouldRumble;
        wiimote.SendStatusInfoRequest();
    }

    public void ResetHaptics()
    {
        wiimote.RumbleOn = shouldRumble = false;
        wiimote.SendStatusInfoRequest();
    }

    public void OnUpdate()
    {
        if (wiimote != null)
        {
            UpdateWiimote(wiimote);
            if (!gotNunchuck && wiimote.Nunchuck != null)
            {
                SetUpIRCamera(wiimote, IRSensitivity.LevelOne, IRDataType.BASIC);
                gotNunchuck = true;
            }
            var state = new WiimoteState()
            {
                position = UpdateIR(wiimote),
                nunchuckStick = UpdateNunchuckStick(wiimote)
            };
            state = UpdateButtons(wiimote, state);
            InputSystem.QueueStateEvent(this, state);
        }
    }

    public void RegisterWiimote(Wiimote remote)
    {
        wiimote = remote;
        UpdateWiimote(wiimote);
        SetRemoteLEDs(ledState);
        Debug.Log("InputSystem has Registered Wiimote!");
    }

    protected override void FinishSetup()
    {

        //press = GetChildControl<ButtonControl>("press");
        buttonA = GetChildControl<ButtonControl>("aButton");
        buttonB = GetChildControl<ButtonControl>("bButton");
        buttonC = GetChildControl<ButtonControl>("cButton");
        buttonZ = GetChildControl<ButtonControl>("zButton");
        buttonOne = GetChildControl<ButtonControl>("oneButton");
        buttonTwo = GetChildControl<ButtonControl>("twoButton");
        buttonPlus = GetChildControl<ButtonControl>("plusButton");
        buttonMinus = GetChildControl<ButtonControl>("minusButton");
        buttonHome = GetChildControl<ButtonControl>("homeButton");

        position = GetChildControl<Vector2Control>("position");

        nunchuckStick = GetChildControl<StickControl>("nunchuckStick");

        base.FinishSetup();
    }

    public override void MakeCurrent()
    {
        base.MakeCurrent();
        current= this;
    }

    protected override void OnAdded()
    {
        base.OnAdded();
        s_AllWiimotes.Add(this);
    }

    protected override void OnRemoved()
    {
        base.OnRemoved();
        s_AllWiimotes.Remove(this);
    }

    private static WiimoteState UpdateButtons(Wiimote remote, WiimoteState state)
    {
        if (remote != null && remote.Button != null)
        {
            ButtonData data = remote.Button;
            if(data != null)
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
            else
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

            NunchuckData nunchuck = remote.Nunchuck;
            if (nunchuck != null)
            {
                state.WithButtons(WiimoteButton.C, nunchuck.c);
                state.WithButtons(WiimoteButton.Z, nunchuck.z);
            }
            else
            {
                state.WithButtons(WiimoteButton.C, false);
                state.WithButtons(WiimoteButton.Z, false);
            }
        }

        return state;
    }

    private static void UpdateWiimote(Wiimote remote)
    {
        int ret;
        do
        {
            ret = remote.ReadWiimoteData();
        } while (ret > 0);
    }

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

    private static byte[] GetSensitivityBlockOne(IRSensitivity sensitivity) => sensitivity switch
    {
        IRSensitivity.LevelOne => new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0x64, 0x00, 0xfe },
        IRSensitivity.LevelTwo => new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0x96, 0x00, 0xb4 },
        IRSensitivity.LevelThree => new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0xaa, 0x00, 0x64 },
        IRSensitivity.LevelFour => new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0xc8, 0x00, 0x36 },
        IRSensitivity.LevelFive => new byte[] { 0x07, 0x00, 0x00, 0x71, 0x01, 0x00, 0x72, 0x00, 0x20 },
        _ => new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0xaa, 0x00, 0x64 },
    };

    private static byte[] GetSensitivityBlockTwo(IRSensitivity sensitivity) => sensitivity switch
    {
        IRSensitivity.LevelOne => new byte[] { 0xfd, 0x05 },
        IRSensitivity.LevelTwo => new byte[] { 0xb3, 0x04 },
        IRSensitivity.LevelThree => new byte[] { 0x63, 0x03 },
        IRSensitivity.LevelFour => new byte[] { 0x35, 0x03 },
        IRSensitivity.LevelFive => new byte[] { 0x1f, 0x03 },
        _ => new byte[] { 0x63, 0x03 },
    };

    private static int SendIRCameraEnabled(Wiimote remote, bool enabled)
    {
        byte[] mask = new byte[] { (byte)(enabled ? 0x04 : 0x00) };

        int first = remote.SendWithType(OutputDataType.IR_CAMERA_ENABLE, mask);
        if (first < 0) { return first; }

        int second = remote.SendWithType(OutputDataType.IR_CAMERA_ENABLE_2, mask);
        if (second < 0) { return second; }

        return first + second; // success
    }

    public static WiimoteDevice current { get; private set; }
    public new static IReadOnlyList<WiimoteDevice> all => s_AllWiimotes;

    private static int s_WiimoteCount;
    private static List<WiimoteDevice> s_AllWiimotes = new();


    static WiimoteDevice() { InputSystem.RegisterLayout<WiimoteDevice>(); }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeInPlayer() { }

}
