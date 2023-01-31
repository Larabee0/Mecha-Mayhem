using UnityEngine;

public static class ControllerExtensions
{
    public static Controller Opposite(this Controller controller)
    {
        return (byte)controller < 2 ? (controller + 2) : (controller - 2);
    }

    public static Controller Previous(this Controller controller)
    {
        return controller == Controller.One ? Controller.Four : (controller-1);
    }

    public static Controller Next(this Controller controller)
    {
        return controller == Controller.Four ? Controller.One : (controller + 1);
    }

}


public delegate void Vector2Axis(Vector2 axis);
public delegate void Vector2BoolAxis(Vector2 axis,bool position = false);
public delegate void Pluse();
public delegate void FloatPassThrough(float value);

public class ButtonEventContainer
{
    public Pluse OnButtonPressed;
    public Pluse OnButtonReleased;
    public Pluse OnButtonHeld;
    public bool buttonDown = false;
}


public enum Controller : byte
{
    One,
    Two,
    Three,
    Four,
    Keyboard
}

public enum RumbleMotor : byte
{
    LowFreq,
    HighFreq,
    Both
}

public enum ControlBinding : byte
{
    Fire1,
    Fire2
}

public enum ControlBehaviour : byte
{
    OnPress,
    OnRelease,
    OnHeld
}
