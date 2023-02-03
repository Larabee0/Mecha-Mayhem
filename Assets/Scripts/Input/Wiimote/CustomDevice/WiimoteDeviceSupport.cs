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
    /// <summary>
    /// Provides a way of alerting the Input System when a Wiimote is connect or disconnected from the computer.
    /// </summary>
    [ExecuteInEditMode]
    public class WiimoteDeviceSupport : MonoBehaviour
    {
        /// <summary>
        /// Used to register the Wiimote Device Layerout with the Input System and set its interface for device description matching.
        /// </summary>
        private void Awake()
        {
            InputSystem.RegisterLayout<WiimoteDevice>(matches: new InputDeviceMatcher().WithInterface("WiimoteAPI"));
        }

        /// <summary>
        /// Subscribe to the device added/removed events provided by the ControlArbiter.
        /// </summary>
        private void OnEnable()
        {
            ControlArbiter.wiimoteAdded += OnDeviceAdded;
            ControlArbiter.wiimoteRemoved += OnDeviceRemoved;
        }

        /// <summary>
        /// Un-Subscribe to the device added/removed events provided by the ControlArbiter.
        /// </summary>
        private void OnDisable()
        {
            ControlArbiter.wiimoteAdded -= OnDeviceAdded;
            ControlArbiter.wiimoteRemoved -= OnDeviceRemoved;
        }

        /// <summary>
        /// When a new wiimote is added this method is run, this adds the wiimote to the InputSystem
        /// It does this with an InputDevice Description struct, that sets the name and interface type of the device.
        /// </summary>
        /// <param name="name">Wiimote device name (HID ID)</param>
        private void OnDeviceAdded(string name)
        {
            InputSystem.AddDevice(new InputDeviceDescription
            {
                interfaceName = "WiimoteAPI",
                product = name
            });
        }

        /// <summary>
        /// When a wiimote is removed this method is run, this makes sure to remove the right
        /// wiimote from the InputSystem.
        /// This is done by searching all devices for the first device that matches the InputDeviceDescription.
        /// (the control arbiter keeps track of hte HID ID for this purposes.
        /// </summary>
        /// <param name="name">Target Wiimote HID ID</param>
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

    }
}