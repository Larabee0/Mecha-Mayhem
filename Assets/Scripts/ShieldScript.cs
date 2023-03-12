using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RedButton.Mech
{
    public class ShieldScript : WeaponCore
    {
        [SerializeField] int shieldCD;
        [SerializeField] private int shieldHealth;
        [HideInInspector] public int currentShieldHealth;
        [SerializeField] bool shieldReady;
        [SerializeField] private GameObject shieldObject;
        public bool ShieldActive => shieldObject.activeSelf;
        protected override void Awake()
        {
            base.Awake();
            StartCoroutine(ShieldRecharge(shieldCD));
            shieldObject.transform.SetParent(null, true);
            UnFire();
        }

        protected override void Start()
        {
            targetObject = CMC.MechMovementCore.TargetPoint;
        }

        protected override void BindtoControls()
        {
            ButtonEventContainer buttonEventContainer = controlBinding switch
            {
                ControlBinding.Fire1 => CMC.MechInputController.fireOneButton,
                ControlBinding.Fire2 => CMC.MechInputController.fireTwoButton,
                _ => null,
            };

            buttonEventContainer.OnButtonPressed += Fire;
            buttonEventContainer.OnButtonReleased += UnFire;
        }

        protected override void UnBindControls()
        {
            ButtonEventContainer buttonEventContainer = controlBinding switch
            {
                ControlBinding.Fire1 => CMC.MechInputController.fireOneButton,
                ControlBinding.Fire2 => CMC.MechInputController.fireTwoButton,
                _ => null,
            };

            buttonEventContainer.OnButtonPressed -= Fire;
            buttonEventContainer.OnButtonReleased -= UnFire;
        }

        private void UnFire()
        {
            shieldObject.SetActive(false);
            currentShieldHealth = 0;
        }

        public override void Fire()
        {
            if (shieldReady && !shieldObject.activeSelf)
            {
                shieldObject.SetActive(true);
                currentShieldHealth += shieldHealth;
            }

        }
        IEnumerator ShieldRecharge(float time)
        {
            yield return new WaitForSeconds(time);
            shieldReady = true;
        }

        public void DamageShield()
        {
            currentShieldHealth--;
            if (currentShieldHealth <= 0)
            {
                shieldObject.SetActive(false);
                shieldReady = false;
                StartCoroutine(ShieldRecharge(shieldCD * 2));
            }
        }

        public override void GroupFire()
        {
            
        }

        protected override void Update()
        {
            shieldObject.transform.position = transform.position;
            shieldObject.transform.forward = transform.forward;
        }
    }
}
