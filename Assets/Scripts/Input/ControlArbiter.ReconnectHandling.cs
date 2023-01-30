using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RedButton.Core
{
    public partial class ControlArbiter : MonoBehaviour
    {
        #region Controller connect/disconnect handlers
        /// <summary>
        /// When a device is plugged in or unplugged, this gets called.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="change"></param>
        private void OnDeviceChanged(InputDevice device, InputDeviceChange change)
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    HandleDeviceConnected(device);
                    break;
                case InputDeviceChange.Disconnected:
                    HandleDeviceLost(device);
                    break;
                case InputDeviceChange.Reconnected:
                    HandleDeviceConnected(device);
                    break;
            }
        }

        /// <summary>
        /// Backend to handle a new controller being plugged in.
        /// </summary>
        /// <param name="device"></param>
        private void HandleDeviceConnected(InputDevice device)
        {
            newDevices.Add(device);
            if (devicelessMaps.Count > 0)
            {
                controllerReconnectors.Add(new(devicelessMaps.Dequeue(), this));
                mainUIController.FlashControllerConnected(device.displayName);
            }
        }

        /// <summary>
        /// Backend to handle a controller being disconnected.
        /// </summary>
        /// <param name="device"></param>
        private void HandleDeviceLost(InputDevice device)
        {
            if (newDevices.Contains(device))
            {
                newDevices.Remove(device);
            }
            string path = device.path;
            if (controllerMap.ContainsKey(path) && controllerMap[path].DeviceConnected)
            {
                PlayerInput player = controllerMap[path];
                player.Disable();
                player.DeviceConnected = false;
                devicelessMaps.Enqueue(player);
                int playerNum = Mathf.Clamp(((int)player.Player) + 1, 1, 4);
                mainUIController.HandleControllerDisconnect(playerNum, player.playerColour);
            }
        }
        #endregion
    }
}