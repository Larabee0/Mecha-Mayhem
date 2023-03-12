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
        protected CentralMechComponent CMC;
        protected Transform targetObject; // gotten by MovementCore this is what the weapon will try and look at, aiming it.
        
        // these properties are both overrideable
        protected virtual Vector3 TargetPos => targetObject.position;
        protected virtual Vector3 TargetForward => targetObject.forward;

        [HideInInspector] public bool Grouped; // if the weapon is part of a group this is set to true by the group.

        [Header("Base Weapon Settings")]

        [SerializeField, Range(1, 100)] protected int damage = 10; // damage property for the weapon, to be provided to the projectile

        [SerializeField] protected ControlBinding controlBinding = ControlBinding.Fire2;
        [SerializeField] protected ControlBehaviour controlBehaviour = ControlBehaviour.OnHeld;

        [SerializeField] protected ProjectileCore projectilePrefab; // optional projectileCore prefab slot

        protected Transform muzzleOriginPoint; // point at which projectiles are spawned from or the ray cast is cast from.
        [SerializeField] protected Transform animationCentre; // centre point of the visual portion of the weapon. this will be made to look at the targetObject

        protected virtual void Awake()
        {
            CMC = GetComponentInParent<CentralMechComponent>();
            
            if (CMC == null)
            {
                Debug.LogErrorFormat("Weapon attached to {0}, was unable to find the CMC! of its mech", gameObject.name);
                enabled = false;
                return;
            }
            muzzleOriginPoint = CMC.GetNextWeaponOrigin();
            if (!Grouped)
            {
                BindtoControls();
            }
        }

        protected virtual void Start()
        {
            targetObject = CMC.MechMovementCore.TargetPoint;

        }

        protected virtual void OnDestroy()
        {
            UnBindControls();
        }

        /// <summary>
        /// This method binds the weapon to the control setting set in the editor
        /// This is not called if the weapon is in a group
        /// </summary>
        protected virtual void BindtoControls()
        {
            ButtonEventContainer buttonEventContainer = controlBinding switch
            {
                ControlBinding.Fire1 => CMC.MechInputController.fireOneButton,
                ControlBinding.Fire2 => CMC.MechInputController.fireTwoButton,
                _ => null,
            };

            switch (controlBehaviour)
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

        protected virtual void UnBindControls()
        {
            ButtonEventContainer buttonEventContainer = controlBinding switch
            {
                ControlBinding.Fire1 => CMC.MechInputController.fireOneButton,
                ControlBinding.Fire2 => CMC.MechInputController.fireTwoButton,
                _ => null,
            };

            switch (controlBehaviour)
            {
                case ControlBehaviour.OnPress when buttonEventContainer != null:
                    buttonEventContainer.OnButtonPressed -= Fire;
                    break;
                case ControlBehaviour.OnRelease when buttonEventContainer != null:
                    buttonEventContainer.OnButtonReleased -= Fire;
                    break;
                case ControlBehaviour.OnHeld when buttonEventContainer != null:
                    buttonEventContainer.OnButtonHeld -= Fire;
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// rotates center of the animation point to look at the target point (aiming the weapon)
        /// </summary>
        protected virtual void Update()
        {
            Vector3 lookTarget = TargetPos;
            lookTarget.y = animationCentre.position.y;
            animationCentre.LookAt(lookTarget);
        }

        /// <summary>
        /// Fire must be implemeneted. This is where a weapon should work out when it should fire.
        /// it gets called whenever the fire button this has bound to is pressed, unless the weapon is grouped,
        /// in which case GroupFire() is called.
        /// </summary>
        public abstract void Fire();

        /// <summary>
        /// GroupFire() should be a pass through directly to firing the weapon (spawning the prefab or making the raycast).
        /// Timing is handled by the WeaponGroup component.
        /// </summary>
        public abstract void GroupFire();
    }
}