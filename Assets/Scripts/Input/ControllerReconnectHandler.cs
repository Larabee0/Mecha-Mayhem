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
            if (arbiter.newDevices.Contains(obj.control.device))
            {
                arbiter.newDevices.Remove(obj.control.device);
                controller.AssignDevice(obj.control.device,controller.Player);
                controller.Enable();
                reconnectListerner.performed -= AnyButtonPressed;
                arbiter.controllerReconnectors.Remove(this);
                arbiter.toDispose.Enqueue(this);
            }
        }
    }
}