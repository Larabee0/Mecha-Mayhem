using UnityEngine.InputSystem;

namespace RedButton.Core
{
    public class ControllerReconnectHandler
    {
        public InputAction reconnectListerner;
        public PlayerInput controller;
        public ControlArbiter arbiter;

        public ControllerReconnectHandler() { }

        public ControllerReconnectHandler(PlayerInput controller, ControlArbiter arbiter)
        {
            reconnectListerner = new InputAction(binding: "/*/<button>");
            this.controller = controller;
            this.arbiter = arbiter;

            reconnectListerner.performed += AnyButtonPressed;
            reconnectListerner.Enable();
        }

        private void AnyButtonPressed(InputAction.CallbackContext obj)
        {
            InputDevice[] devices = ProcessDevice(obj.control.device);
            if (arbiter.newDevices.IsSupersetOf(devices))
            {
                arbiter.newDevices.ExceptWith(devices);
                controller.AssignDevice(devices, controller.Player);
                controller.Enable();
                reconnectListerner.performed -= AnyButtonPressed;
                arbiter.controllerReconnectors.Remove(this);
                arbiter.toDispose.Enqueue(this);
            }
        }

        private InputDevice[] ProcessDevice(InputDevice device)
        {
            if (device is Keyboard || device is Mouse)
            {
                return new InputDevice[] { Keyboard.current, Mouse.current };
            }
            else
            {
                return new InputDevice[] { device };
            }
        }
    }
}