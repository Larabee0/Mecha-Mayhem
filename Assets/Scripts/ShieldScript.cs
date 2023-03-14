using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

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
            ShieldColour();
        }

        
        private void ShieldColour()
        {
            MeshRenderer shieldColour = GetComponent<MeshRenderer>();
            if (shieldHealth > 3)
            {
                shieldColour.material.SetColor("_FrontColor", Color.blue);
                shieldColour.material.SetColor("_BackColor", Color.blue);
                //Make barreir blue
            }
            else if(shieldHealth == 3)
            {
                shieldColour.material.SetColor("_FrontColor", Color.green);
                shieldColour.material.SetColor("_BackColor", Color.green);
                //Make barrier green
            }
            else if(shieldHealth == 2)
            {
                shieldColour.material.SetColor("_FrontColor", Color.yellow);
                shieldColour.material.SetColor("_BackColor", Color.yellow);
                //Make barrier yellow
            }
            else if(shieldHealth == 1)
            {
                shieldColour.material.SetColor("_FrontColor", Color.red);
                shieldColour.material.SetColor("_BackColor", Color.red);
                //Make barrier red
            }
        }
    }
}
