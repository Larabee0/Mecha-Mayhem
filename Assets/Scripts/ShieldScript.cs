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
            StartCoroutine(ShieldRecharge());
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
            Debug.Log("unfier");
            shieldObject.SetActive(false);
            shieldReady = false;
            currentShieldHealth = 0;
            StartCoroutine(ShieldRecharge());
            

        }

        public override void Fire()
        {
            if (shieldReady)
            {
                shieldObject.SetActive(true);
                Debug.Log("fier");
                currentShieldHealth += shieldHealth;
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

        // private void OnCollisionEnter(Collision collision)
        // {
        //     if (collision.gameObject.CompareTag("Projectile"))
        //     {
        //         DamageShield();
        //     }
        // }

        public void DamageShield()
        {
            currentShieldHealth--;
            if (currentShieldHealth <= 0)
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
           
        }
    }
}
