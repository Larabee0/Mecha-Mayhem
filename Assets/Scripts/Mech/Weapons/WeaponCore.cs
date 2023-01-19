using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XInput;
using UnityEngine.Scripting;

namespace RedButton.Mech
{
    /// <summary>
    /// Base class all weapons are derived from
    /// </summary>
    public abstract class WeaponCore : MonoBehaviour
    {
        [Header("Base Weapon Settings")]
        [SerializeField] protected CentralMechComponent CMC;

        [Tooltip("Which fire button triggers this weapon\nIf group this is controlled by the Weapon Group")]
        [SerializeField] protected ControlBinding binding;
        [SerializeField] protected ControlBehaviour behaviour;

        [SerializeField] protected Transform targetObject;
        protected virtual Vector3 TargetPos => targetObject.position;
        protected virtual Vector3 TargetForward => targetObject.forward;

        public bool Grouped;

        protected virtual void Awake()
        {
            CMC = GetComponentInParent<CentralMechComponent>();

            if (CMC == null)
            {
                Debug.LogErrorFormat("Weapon attached to {0}, was unable to find the CMC! of its mech", gameObject.name);
                enabled = false;
                return;
            }

            if (!Grouped)
            {
                BindtoControls();
            }
        }

        protected virtual void Start()
        {
            targetObject = CMC.MechMovementCore.TargetPoint;
        }

        protected virtual void BindtoControls()
        {
            ButtonEventContainer buttonEventContainer = binding switch
            {
                ControlBinding.Fire1 => CMC.MechInputController.fireOneButton,
                ControlBinding.Fire2 => CMC.MechInputController.fireTwoButton,
                _ => null,
            };

            switch (behaviour)
            {
                case ControlBehaviour.OnPress when buttonEventContainer != null:
                    buttonEventContainer.OnButtonPressed += Fire;
                    break;
                case ControlBehaviour.OnRelease when buttonEventContainer != null:
                    buttonEventContainer.OnButtonReleased += Fire;
                    break;
                case ControlBehaviour.OnHeld when buttonEventContainer != null:
                    buttonEventContainer.OnButtonHeld += Fire;
                    break;
                default:
                    return;
            }
        }

        [RequiredMember]
        public virtual void Fire() { }

        [RequiredMember]
        public virtual void GroupFire() { }
    }
}