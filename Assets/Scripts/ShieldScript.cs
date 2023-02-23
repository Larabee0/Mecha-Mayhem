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
        bool shieldReady;
        [SerializeField] private GameObject shield;

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
            shield.SetActive(false);
            shieldReady = false;
            StartCoroutine("ShieldRecharge");

        }

        public override void Fire()
        {
            if (shieldReady)
            {
                shield.SetActive(true);
                Debug.Log("fier");
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
        private void ShieldACtivated()
        {
            
            
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Projectile"))
            {
                shieldHealth--;
                if (shieldHealth <= 0)
                {
                    shield.SetActive(false);
                    shieldReady = false;
                    StartCoroutine("ShieldDestroyed");
                }
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
