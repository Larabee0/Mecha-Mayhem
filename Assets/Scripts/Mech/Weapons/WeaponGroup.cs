using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.Mech
{
    /// <summary>
    /// Provides a way of grouping weapons to fire them in order with a time delay between each firing.
    /// </summary>
    public class WeaponGroup : MonoBehaviour
    {
        [SerializeField] private CentralMechComponent CMC;
        [SerializeField] private ControlBinding binding;
        [SerializeField] private ControlBehaviour behaviour;
        private bool UnboundFromControls = true;
        [SerializeField] private WeaponCore[] weaponsInGroup;

        [SerializeField][Range(0f, 1f)] float groupFireIntervalMin = 0.01f;
        [SerializeField][Range(0f, 1f)] float groupFireIntervalMax = 0.05f;

        private float fireInterval = 0f;
        private int currentWeaponIndex = 0;

        void Awake()
        {
            CMC = GetComponentInParent<CentralMechComponent>();
            if(weaponsInGroup == null || weaponsInGroup.Length == 0)
            {
                enabled = false;
                return;
            }
            for (int i = 0; i < weaponsInGroup.Length; i++)
            {
                weaponsInGroup[i].Grouped= true;
            }

            BindtoControls();
        }

        private void OnEnable()
        {
            if(UnboundFromControls)
            {
                BindtoControls();
            }
        }

        private void OnDisable()
        {
            if (!UnboundFromControls)
            {
                UnBindControls();
            }
        }

        private void BindtoControls()
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
            UnboundFromControls = false;
        }

        protected virtual void UnBindControls()
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
            UnboundFromControls = true;
        }

        private void Fire()
        {
            fireInterval -= Time.deltaTime;
            switch(fireInterval)
            {
                case <= 0f:
                    fireInterval = Random.Range(groupFireIntervalMin, groupFireIntervalMax);
                    weaponsInGroup[currentWeaponIndex].GroupFire();
                    currentWeaponIndex = (currentWeaponIndex + 1) % weaponsInGroup.Length;
                    break;
            }
        }
    }
}