using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

[StructLayout(LayoutKind.Explicit, Size = 32)]
public struct WiimoteState : IInputStateTypeInfo
{
    public FourCC format => new('W', 'i', 'i','M');

    [InputControl(name = "dpad",layout = "Dpad",usage = "Hatswitch",displayName = "D-Pad",format ="BIT",sizeInBits = 4, bit = 0)]
    [InputControl(name = "aButton", layout = "Button", bit = (int)WiimoteButton.A, usages =new[] { "PrimaryAction","Submit" }, displayName = "A Button", shortDisplayName = "AWM")]
    [InputControl(name = "bButton", layout = "Button", bit = (int)WiimoteButton.B, usages = new[] { "SecondaryAction","Back", "Cancel" }, displayName = "B Button", shortDisplayName = "BWM")]
    [InputControl(name = "cButton", layout = "Button", bit = (int)WiimoteButton.C, displayName = "Nunchuck C", shortDisplayName = "CNC")]
    [InputControl(name = "zButton", layout = "Button", bit = (int)WiimoteButton.Z, displayName = "Nunchuck Z", shortDisplayName = "ZNC")]
    [InputControl(name = "oneButton", layout = "Button", bit = (int)WiimoteButton.One, displayName = "One Button", shortDisplayName = "OWM")]
    [InputControl(name = "twoButton", layout = "Button", bit = (int)WiimoteButton.Two, displayName = "Two Button", shortDisplayName = "TWM")]
    [InputControl(name = "plusButton", layout = "Button", bit = (int)WiimoteButton.Plus, displayName = "Plus Button", shortDisplayName = "PWM")]
    [InputControl(name = "minusButton", layout = "Button", bit = (int)WiimoteButton.Minus, displayName = "Minus Button", shortDisplayName = "MWM")]
    [InputControl(name = "homeButton", layout = "Button", bit = (int)WiimoteButton.Home,usage = "Menu", displayName = "Home Button", shortDisplayName = "HWM")]
    [FieldOffset(0)]
    public ushort buttons;

    [InputControl(name = "position",usage = "Point", dontReset = false)]
    [FieldOffset(4)]
    public Vector2 position;

    [InputControl(layout = "Stick",usage = "Primary2DMotion",processors = "stickDeadzone",displayName = "Nunchuck Stick",shortDisplayName = "SNC")]
    [FieldOffset(12)]
    public Vector2 nunchuckStick;

    public WiimoteState WithButtons(WiimoteButton button, bool state = true)
    {
        Debug.Assert((int)button < 16, $"Expected button < 16, so we fit into the 16 bit wide bitmask");
        var bit = 1U << (int)button;
        if (state)
            buttons |= (ushort)bit;
        else
            buttons &= (ushort)~bit;
        return this;
    }
}

public enum WiimoteButton
{
    DpadUp,
    DpadDown,
    DpadLeft,
    DpadRight,
    A,
    B,
    C,
    Z,
    One,
    Two,
    Plus,
    Minus,
    Home
}