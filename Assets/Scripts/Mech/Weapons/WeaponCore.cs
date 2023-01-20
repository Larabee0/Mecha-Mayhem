using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XInput;
using UnityEngine.PlayerLoop;
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
        [Range(1, 100)] public int damage = 10;

        [SerializeField] protected ControlBinding binding;
        [SerializeField] protected ControlBehaviour behaviour;
        [SerializeField] protected ProjectileCore projectilePrefab;
        [SerializeField] protected Transform projectileSpawnPoint;
        [SerializeField] protected Transform animationCentre;
        protected Transform targetObject;
        protected virtual Vector3 TargetPos => targetObject.position;
        protected virtual Vector3 TargetForward => targetObject.forward;

        [HideInInspector] public bool Grouped;

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

        protected virtual void Update()
        {
            Vector3 lookTarget = targetObject.position;
            lookTarget.y = animationCentre.position.y;
            animationCentre.LookAt(lookTarget);
        }

        public abstract void Fire();

        public abstract void GroupFire();
    }
}