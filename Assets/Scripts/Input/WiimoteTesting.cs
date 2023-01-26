using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.tvOS;
using WiimoteApi;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem.UI;

public class WiimoteTesting : MonoBehaviour
{
    public enum IRSensitivity
    {
        LevelOne,
        LevelTwo,
        LevelThree,
        LevelFour,
        LevelFive
    }

    [SerializeField] private VirtualMouseInput wiiVirtualMouse;
    private Wiimote remote;
    public IRSensitivity irSensitivity = IRSensitivity.LevelOne;
    public bool gotNunchuck;
    public int frames = 0;

    public float debugRumbleMotorTime = 0.5f;

    private Vector2 nunChuckStick;
    private Vector2 pointPosition;
    public Vector2 NunChuckStick =>nunChuckStick;
    public Vector2 PointPosition => pointPosition;


    public bool ButtonA;
    public bool ButtonB;
    public bool ButtonC;
    public bool ButtonZ;

    public bool ButtonPlus;
    public bool ButtonMinus;
    public bool ButtonOne;
    public bool ButtonTwo;

    public bool DpadUp;
    public bool DpadDown;
    public bool DpadLeft;
    public bool DpadRight;

    RectTransform rectTransform;
    // Start is called before the first frame update
    void Awake()
    {
        wiiVirtualMouse = GetComponent<VirtualMouseInput>();
        gotNunchuck = false;
        // WiimoteManager.Debug_Messages = false;
        Debug.LogFormat("Finding wiimotes {0}", WiimoteManager.FindWiimotes());
        if(WiimoteManager.Wiimotes== null || WiimoteManager.Wiimotes.Count == 0)
        {
            enabled = false;
            return;
        }
        remote = WiimoteManager.Wiimotes[0];
        remote.SendPlayerLED(Random.value > 0.5f, Random.value > 0.5f, Random.value > 0.5f, Random.value > 0.5f);

        // remote.SetupIRCamera(IRDataType.BASIC);
        // SetUpIRCamera(remote, irSensitivity, IRDataType.BASIC);
        // remote.SendStatusInfoRequest();
        UpdateWiimote(remote);
    }

    private void Start()
    {
        UpdateWiimote(remote);
        rectTransform =  GetComponent<Image>().rectTransform;
        wiiVirtualMouse.cursorTransform = rectTransform;
        
    }

    private void Update()
    {
        if(wiiVirtualMouse != null)
        {
            if(wiiVirtualMouse.virtualMouse.delta.EvaluateMagnitude() > 0)
            {
                Debug.Log(wiiVirtualMouse.virtualMouse.delta);
            }
            else
            {
                Debug.Log("no delta");
            }
        }
        UpdateWiimote(remote);
        if (!gotNunchuck && remote.Nunchuck != null)
        {
            Debug.LogFormat("Extension {0}", remote.current_ext);
            remote.SendPlayerLED(true, true, true, true);
            gotNunchuck = true;
            SetUpIRCamera(remote, irSensitivity, IRDataType.BASIC);
        }
        else if (!gotNunchuck)
        {
            frames++;
        }
        UpdateWiimoteButtons(remote);
        UpdateNunchuckStick(remote);
        UpdateIR(remote);
    }

    private void UpdateWiimote(Wiimote remote)
    {
        int ret;
        do
        {
            ret = remote.ReadWiimoteData();
        } while (ret > 0);
    }

    private void UpdateWiimoteButtons(Wiimote remote)
    {
        ButtonData data = remote.Button;
        ButtonA = data.a;
        ButtonB = data.b;

        ButtonPlus = data.plus;
        ButtonMinus = data.minus;

        ButtonOne = data.one;
        ButtonTwo = data.two;

        DpadUp = data.d_up;
        DpadDown = data.d_down;
        DpadLeft = data.d_left;
        DpadRight = data.d_right;

        if (remote.Nunchuck != null)
        {
            NunchuckData nunchuck = remote.Nunchuck;
            ButtonC = nunchuck.c;
            ButtonZ = nunchuck.z;
        }
    }

    private void UpdateNunchuckStick(Wiimote remote)
    {
        if (gotNunchuck && remote.Nunchuck != null)
        {
            float[] input = remote.Nunchuck.GetStick01();
            float2 numChuckV2 = new(math.lerp(-1, 1, input[0]), math.lerp(-1, 1, input[1]));

            numChuckV2.x = math.abs(numChuckV2.x) > 0.1f ? numChuckV2.x : 0f;
            numChuckV2.y = math.abs(numChuckV2.y) > 0.1f ? numChuckV2.y : 0f;

            numChuckV2 = math.clamp(numChuckV2, -1, 1);
            nunChuckStick = numChuckV2;
        }
    }

    private void UpdateIR(Wiimote remote)
    {
        if (remote.Ir != null)
        {
            float[] input = remote.Ir.GetPointingPosition();
            float2 IRpos = new(input[0], input[1]);

            // float2 IRPixelPos = new()
            // {
            //     x = math.lerp(0, 1024, IRpos.x),
            //     y = IRpos.y
            // };

            // float2 sixteenByNineCorrection = IRPixelPos;// + new float2(171, 0);

            // float2 sixteenByNineLinear = new()
            // {
            //     x = math.unlerp(-171, 1366, sixteenByNineCorrection.x),
            //     y = IRpos.y
            // };

            float2 wiimotePosition = new()
            {
                x = math.lerp(0, Screen.width, IRpos.x),
                y = math.lerp(0, Screen.height, IRpos.y)
            };

            // float2 mouse = new (Input.mousePosition.x, Input.mousePosition.y);
            pointPosition = wiimotePosition;
            rectTransform.position = new float3(wiimotePosition, 0);
            // Debug.LogFormat("mouse pos {0}, wii pos {1} ",mouse, sixteenByNineLinear);

        }
    }

    private bool SetUpIRCamera(Wiimote remote, IRSensitivity sensitivity,IRDataType type = IRDataType.BASIC)
    {
        int res;
        // 1. Enable IR Camera (Send 0x04 to Output Report 0x13)
        // 2. Enable IR Camera 2 (Send 0x04 to Output Report 0x1a)
        res = SendIRCameraEnabled(remote, true);
        if (res < 0) return false;
        // 3. Write 0x08 to register 0xb00030
        res = remote.SendRegisterWriteRequest(RegisterType.CONTROL, 0xb00030, new byte[] { 0x08 });
        if(res < 0) return false;
        // 4. Write Sensitivity Block 1 to registers at 0xb00000
        // Wii sensitivity level 3:
        // 02 00 00 71 01 00 aa 00 64
        // High Sensitivity:
        // 00 00 00 00 00 00 90 00 41
        int offset = 0xb00000;
        byte[] block = GetSensitivityBlockOne(sensitivity);
        res = remote.SendRegisterWriteRequest(RegisterType.CONTROL, offset, block);
        if (res < 0) return false;
        // 5. Write Sensitivity Block 2 to registers at 0xb0001a
        // Wii sensitivity level 3: 
        // 63 03
        // High Sensitivity:
        // 40 00
        offset = 0xb0001a;
        block = GetSensitivityBlockTwo(sensitivity);
        res = remote.SendRegisterWriteRequest(RegisterType.CONTROL, offset, block);
        if (res < 0) return false;
        // 6. Write Mode Number to register 0xb00033
        res = remote.SendRegisterWriteRequest(RegisterType.CONTROL, 0xb00033, new byte[] { (byte)type });
        if (res < 0) return false;
        // 7. Write 0x08 to register 0xb00030 (again)
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

    private byte[] GetSensitivityBlockOne(IRSensitivity sensitivity) => sensitivity switch
    {
        IRSensitivity.LevelOne => new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0x64, 0x00, 0xfe },
        IRSensitivity.LevelTwo => new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0x96, 0x00, 0xb4 },
        IRSensitivity.LevelThree => new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0xaa, 0x00, 0x64 },
        IRSensitivity.LevelFour => new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0xc8, 0x00, 0x36 },
        IRSensitivity.LevelFive => new byte[] { 0x07, 0x00, 0x00, 0x71, 0x01, 0x00, 0x72, 0x00, 0x20 },
        _ => new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0xaa, 0x00, 0x64 },
    };

    private byte[] GetSensitivityBlockTwo(IRSensitivity sensitivity) => sensitivity switch
    {
        IRSensitivity.LevelOne => new byte[] { 0xfd, 0x05 },
        IRSensitivity.LevelTwo => new byte[] { 0xb3, 0x04 },
        IRSensitivity.LevelThree => new byte[] { 0x63, 0x03 },
        IRSensitivity.LevelFour => new byte[] { 0x35, 0x03 },
        IRSensitivity.LevelFive => new byte[] { 0x1f, 0x03 },
        _ => new byte[] { 0x63, 0x03 },
    };

    private int SendIRCameraEnabled(Wiimote remote, bool enabled)
    {
        byte[] mask = new byte[] { (byte)(enabled ? 0x04 : 0x00) };

        int first = remote.SendWithType(OutputDataType.IR_CAMERA_ENABLE, mask);
        if (first < 0) { return first; }

        int second = remote.SendWithType(OutputDataType.IR_CAMERA_ENABLE_2,mask);
        if(second < 0) { return second; }

        return first + second; // success
    }

    [ContextMenu("Force IR Camera Set Up")]
    public void ForceIRCameraReset()
    {
        if (remote != null)
        {
            SetUpIRCamera(remote, irSensitivity);
            remote.SendPlayerLED(Random.value > 0.5f, Random.value > 0.5f, Random.value > 0.5f, Random.value > 0.5f);
        }
    }

    [ContextMenu("Debug Test Rumble")]
    public void DebugRumble()
    {
        RumbleRemote(debugRumbleMotorTime);
    }

    public void RumbleRemote(float time)
    {
        StopAllCoroutines();
        StartCoroutine(RumbleMotor(time));
    }

    private IEnumerator RumbleMotor(float time)
    {
        remote.RumbleOn = true;
        remote.SendStatusInfoRequest();
        yield return new WaitForSeconds(time);
        remote.RumbleOn = false;
        remote.SendStatusInfoRequest();
    }

    private void OnApplicationQuit()
    {
        Debug.Log("quit");
        lock (WiimoteManager.Wiimotes)
        {
            for (int i = 0; i < WiimoteManager.Wiimotes.Count; i++)
            {
                Wiimote remote = WiimoteManager.Wiimotes[i];
                
                WiimoteManager.Cleanup(remote);
            }
        }
    }
}
