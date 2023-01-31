using RedButton.Core;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
/*
 * Built using https://docs.unity3d.com/Packages/com.unity.inputsystem@1.4/manual/Devices.html#creating-custom-devices
 * as a guide
 */
namespace RedButton.Core.WiimoteSupport
{
    [ExecuteInEditMode]
    public class WiimoteDeviceSupport : MonoBehaviour
    {
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

            if (device != null)
            {
                InputSystem.RemoveDevice(device);
            }
        }

        private void Awake()
        {
            InputSystem.RegisterLayout<WiimoteDevice>(matches: new InputDeviceMatcher().WithInterface("WiimoteAPI"));
        }
    }
}