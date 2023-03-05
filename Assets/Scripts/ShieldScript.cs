using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RedButton.Mech
{
    public class ShieldScript : WeaponCore
    {
        [SerializeField] int shieldCD;
        int shieldHealth;
        [SerializeField] bool shieldReady;
        [SerializeField] private GameObject shieldObject;
        public bool ShieldActive => shieldObject.activeSelf;
        protected override void Awake()
        {
            base.Awake();
            StartCoroutine(ShieldRecharge());
            shieldObject.transform.SetParent(null, true);
            UnFire();
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
            // shieldReady = false;
            // StartCoroutine(ShieldRecharge());
        }

        public override void Fire()
        {
            if (shieldReady && !shieldObject.activeSelf)
            {
                shieldObject.SetActive(true);
                shieldHealth = 3;
            }

        }
        IEnumerator ShieldRecharge()
        {
            yield return new WaitForSeconds(shieldCD);
            shieldReady = true;
        }

        IEnumerator ShieldDestroyed()
        {
            yield return new WaitForSeconds(shieldCD * 2);
            shieldReady = true;
        }

        public void DamageShield()
        {
            shieldHealth--;
            if (shieldHealth <= 0)
            {
                shieldObject.SetActive(false);
                shieldReady = false;
                StartCoroutine(ShieldDestroyed());
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
