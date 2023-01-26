using RedButton.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

[ExecuteInEditMode]
public class WiimoteDeviceSupport : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnEnable()
    {
        ControlArbiter.wiimoteAdded += OnDeviceAdded;
        ControlArbiter.wiimoteRemoved += OnDeviceRemoved;
    }

    private void OnDisable()
    {
        ControlArbiter.wiimoteAdded -= OnDeviceAdded;
        ControlArbiter.wiimoteRemoved -= OnDeviceRemoved;
    }

    private void OnDeviceAdded(string name)
    {
        InputSystem.AddDevice(new InputDeviceDescription
        {
            interfaceName = "WiimoteAPI",
            product = name
        });
    }

    private void OnDeviceRemoved(string name)
    {
        var device = InputSystem.devices.FirstOrDefault(x => x.description == new InputDeviceDescription
        {
            interfaceName = "WiimoteAPI",
            product = name
        });

        if(device != null)
        {
            InputSystem.RemoveDevice(device);
        }
    }

    private void Awake()
    {
        InputSystem.RegisterLayout<WiimoteDevice>(matches: new InputDeviceMatcher().WithInterface("WiimoteAPI"));
    }
}
